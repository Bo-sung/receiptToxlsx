using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIFreeFight"/>
    /// </summary>
    public class FreeFightPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;

        // <!-- Repositories --!>
        public readonly int roundCount;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnEnterFreeFight;

        // <!-- Temps --!>
        private FreeFightElement[] elements;

        public FreeFightPresenter()
        {
            goodsModel = Entity.player.Goods;
            roundCount = BasisType.FF_ROUNT_COUNT.GetInt();
        }

        public void Initialize()
        {
            foreach (var item in elements)
            {
                item.Initialize();
            }
        }

        public void Dispose()
        {
            if (elements == null)
                return;

            foreach (var item in elements)
            {
                item.Dispose();
            }
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnStartBattle(BattleMode mode)
        {
            switch (mode)
            {
                case BattleMode.FreeFight:
                case BattleMode.WaterBombFreeFight:
                    OnEnterFreeFight?.Invoke();
                    break;
            }
        }

        public UIFreeFightElement.IInput[] GetElements(FreeFightEventType[] types)
        {
            if (elements == null)
            {
                int length = types == null ? 0 : types.Length;
                elements = new FreeFightElement[length];
                for (int i = 0; i < elements.Length; i++)
                {
                    elements[i] = new FreeFightElement(types[i]);
                }
            }

            return elements;
        }

        public long GetZeny()
        {
            return goodsModel.Zeny;
        }

        public long GetCatCoin()
        {
            return goodsModel.CatCoin;
        }

        private class FreeFightElement : UIFreeFightElement.IInput
        {
            private readonly FreeFightEventType type;
            private readonly string tag;

            private readonly QuestModel questModel;
            private readonly BattleManager battleManager;
            private readonly FreeFightRewardDataManager freeFightRewardDataRepo;
            private readonly SkillDataManager skillDataRepo;
            private readonly int playMilliSecondsTime;

            public int NameId { get; }
            public string ImageName { get; }
            public string LockMessage { get; }

            public event System.Action<bool, string> OnUpdateTime;

            private string[] times;
            private SkillData[] skills;

            private bool isRequestFreeFightInfo;
            private bool canEnterFreeFight = true;

            public FreeFightElement(FreeFightEventType type)
            {
                this.type = type;
                tag = string.Concat(nameof(FreeFightElement), this.type.ToString());

                questModel = Entity.player.Quest;
                battleManager = BattleManager.Instance;
                freeFightRewardDataRepo = FreeFightRewardDataManager.Instance;
                skillDataRepo = SkillDataManager.Instance;

                FreeFightConfig config = FreeFightConfig.GetByKey(this.type);
                playMilliSecondsTime = config.playTimeBasisType.GetInt();
                NameId = config.NameId;
                ImageName = config.imageName;

                if (config.openContentType == default || questModel.IsOpenContent(config.openContentType, isShowPopup: false))
                {
                    LockMessage = string.Empty;
                }
                else
                {
                    int conditionValue = GetMainQuestId(config.openContentType);
                    LockMessage = LocalizeKey._40020.ToText() // QUEST {VALUE}
                        .Replace(ReplaceKey.VALUE, conditionValue);
                }
            }

            public void Initialize()
            {
                SetCanEnterFreeFight(false);
                RequestFreeFightInfo();
            }

            public void Dispose()
            {
                Timing.KillCoroutines(tag);
            }

            public string[] GetTimes()
            {
                if (times == null)
                {
                    FreeFightConfig config = FreeFightConfig.GetByKey(type);
                    var list = config.openTimeBasisType.GetKeyList();
                    times = new string[list.Count];
                    for (int i = 0; i < times.Length; i++)
                    {
                        // 참조: https://www.freeformatter.com/epoch-timestamp-to-date-converter.html
                        int epochTime = config.openTimeBasisType.GetInt(list[i]);
                        System.TimeSpan startTime = System.TimeSpan.FromSeconds(epochTime); // UTC 시작시간 (시,분,초 만 사용)
                        System.DateTime startLocalTime = ((long)startTime.TotalMilliseconds).ToDateTime();
                        System.DateTime endLocalTime = startLocalTime.AddMilliseconds(playMilliSecondsTime);

                        times[i] = StringBuilderPool.Get()
                            .Append(startLocalTime.ToString("HH:mm"))
                            .Append(" ~ ")
                            .Append(endLocalTime.ToString("HH:mm"))
                            .Release();
                    }
                }

                return times;
            }

            public UIFreeFightReward.IInput[] GetRewards()
            {
                return freeFightRewardDataRepo.GetArrayData(type);
            }

            public UIEventMazeSkill.IInput[] GetSkills()
            {
                if (skills == null)
                {
                    FreeFightConfig config = FreeFightConfig.GetByKey(type);
                    int length = config.useSkills == null ? 0 : config.useSkills.Length;
                    skills = new SkillData[length];
                    for (int i = 0; i < skills.Length; i++)
                    {
                        skills[i] = skillDataRepo.Get(config.useSkills[i], level: 1);
                    }
                }

                return skills;
            }

            public void StartBattle()
            {
                if (!IsOpenContent())
                    return;

                switch (type)
                {
                    case FreeFightEventType.Normal:
                        StartBattle(BattleMode.FreeFight).WrapUIErrors();
                        break;

                    case FreeFightEventType.WaterBomb:
                        StartBattle(BattleMode.WaterBombFreeFight).WrapUIErrors();
                        break;
                }
            }

            public void RequestFreeFightInfo()
            {
                if (isRequestFreeFightInfo)
                    return;

                isRequestFreeFightInfo = true;

                switch (type)
                {
                    case FreeFightEventType.Normal:
                        AsyncRequestFreeFightInfo().WrapNetworkErrors();
                        break;

                    case FreeFightEventType.WaterBomb:
                        AsyncRequestEventFreeFightInfo().WrapNetworkErrors();
                        break;
                }
            }

            private async Task StartBattle(BattleMode battleMode)
            {
                if (!CheckOpenTime())
                    return;

                if (UIBattleMatchReady.IsMatching)
                {
                    string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                    UI.ShowToastPopup(message);
                    return;
                }

                if (battleManager.Mode == BattleMode.MultiMazeLobby)
                {
                    UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                    return;
                }

                // 해당 던전에 입장하시겠습니까?
                if (!await UI.SelectPopup(LocalizeKey._7000.ToText()))
                    return;

                if (!CheckOpenTime())
                    return;

                battleManager.StartBattle(battleMode);
            }

            private async Task AsyncRequestFreeFightInfo()
            {
                Response response = await Protocol.REQUEST_FF_NOTICETIME.SendAsync();
                if (!response.isSuccess)
                {
                    response.ShowResultCode();
                    return;
                }

                long serverTime = response.GetLong("1"); // 서버 현재 시각
                long startTime = response.GetLong("2"); // 서버 시작 시각
                SetTime(serverTime, startTime);
            }

            private async Task AsyncRequestEventFreeFightInfo()
            {
                Response response = await Protocol.REQUEST_FF_EVENTNOTICETIME.SendAsync();
                if (!response.isSuccess)
                {
                    response.ShowResultCode();
                    return;
                }

                long serverTime = response.GetLong("1"); // 서버 현재 시각
                long startTime = response.GetLong("2"); // 서버 시작 시각
                SetTime(serverTime, startTime);
            }

            private void SetTime(long serverTime, long startTime)
            {
                ServerTime.Initialize(serverTime); // 서버 시간 세팅
                System.DateTime startDateTime = startTime.ToDateTime();
                System.DateTime endDateTime = startDateTime.AddMilliseconds(playMilliSecondsTime);
                Timing.RunCoroutine(YieldRefreshTime(startDateTime, endDateTime), tag);
            }

            /// <summary>
            /// 입장 남은 시간 업데이트
            /// </summary>
            IEnumerator<float> YieldRefreshTime(System.DateTime startTime, System.DateTime endTime)
            {
                // 서버 시간이 존재하지 않을 경우
                if (!ServerTime.IsInitialize)
                    yield break;

                while (true)
                {
                    System.DateTime now = ServerTime.Now;

                    // 이미 시즌이 지났을 경우
                    if (now > endTime)
                    {
                        SetCanEnterFreeFight(false);
                        break;
                    }

                    isRequestFreeFightInfo = false; // 시간 업데이트

                    System.TimeSpan timeSpan;
                    if (now < startTime)
                    {
                        timeSpan = startTime - now;
                        SetCanEnterFreeFight(false);
                    }
                    else
                    {
                        timeSpan = endTime - now;
                        SetCanEnterFreeFight(true);
                    }

                    UpdateFreeFightTimeText(timeSpan);
                    yield return Timing.WaitForSeconds(1f);
                }

                RequestFreeFightInfo(); // 서버 시간 다시 요청
            }

            private void SetCanEnterFreeFight(bool value)
            {
                if (canEnterFreeFight == value)
                    return;

                canEnterFreeFight = value;
                UpdateFreeFightTimeText(System.TimeSpan.Zero);
            }

            private void UpdateFreeFightTimeText(System.TimeSpan timeSpan)
            {
                OnUpdateTime?.Invoke(canEnterFreeFight, timeSpan.ToString(@"hh\:mm\:ss"));
            }

            private bool CheckOpenTime()
            {
                if (canEnterFreeFight)
                    return true;

                string message = LocalizeKey._49506.ToText(); // 입장 가능 시간이 아닙니다.
                UI.ShowToastPopup(message);
                return false;
            }

            private bool IsOpenContent()
            {
                FreeFightConfig config = FreeFightConfig.GetByKey(type);
                if (config.openContentType == default)
                    return true;

                return questModel.IsOpenContent(config.openContentType, isShowPopup: true);
            }

            private int GetMainQuestId(ContentType content)
            {
                QuestInfo info = questModel.GetNeedQuest(content);
                if (info == null)
                    return 0;

                return info.GetMainQuestGroup();
            }
        }
    }
}