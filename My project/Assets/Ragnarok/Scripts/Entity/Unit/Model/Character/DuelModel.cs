using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{

    public class DuelModel : CharacterEntityModel
    {
        private DuelRewardDataManager duelRewardDataRepo;
        private readonly Buffer<RewardData> rewardBuffer;
        private readonly ProfileDataManager profileDataRepo;

        private ChapterDuelState[] chapterDuelStates;
        private List<CharDuelHistory> duelHistoryList;

        public DuelInfo CurDuelInfo { get; private set; }
        public ObscuredInt DuelPoint { get; private set; } // 듀얼 포인트
        public int DuelPointBuyCount { get { return duelPointBuyCount; } }
        public int RecentlyChapter { get; set; } // UI에서 가장 최근에 확인한 챕터
        public UIDuel.State SelectedState { get; set; } // UI에서 가장 최근에 이벤트를 선택했는지..
        private ObscuredInt duelPointBuyCount; // 듀얼 포인트 구매횟수 (일일 초기화)

        public IEnumerable<CharDuelHistory> GetDuelHistoryList() => duelHistoryList;

        public event Action<int> OnDualPointChanged;

        public ChapterDuelState CurProgressingBattleState { get; private set; }
        public int CurBattleTargetingAlphabet { get; private set; }

        public bool IsDuelInfoInitialized { get; private set; }

        private RemainTime duelCharListResetTime; // 듀얼 대전 상대 리스트 변경 쿨타임
        private RemainTime eventDuelCharListResetTime; // 이벤트듀얼 대전 상대 리스트 변경 쿨타임
        private RemainTime arenaDuelCharListResetTime; // 아레나 대전 상대 리스트 변경 쿨타임
        private DuelCharList[] duelCharList; // 듀얼 대전 상대 리스트
        /// <summary>
        /// 듀얼 대전 상대 리스트 업데이트 이벤트
        /// </summary>
        public Action<DuelCharList[]> OnUpdateDuelCharList;

        /// <summary>
        /// 듀얼 리스트 쿨타임 변경 알림..
        /// </summary>
        public event Action OnResetWaitingTime;

        /// <summary>
        /// 이벤트듀얼 리스트 쿨타임 변경 알림..
        /// </summary>
        public event Action OnResetEventWaitingTime;

        /// <summary>
        /// 아레나듀얼 리스트 쿨타임 변경 알림..
        /// </summary>
        public event Action OnResetArenaWaitingTime;

        /// <summary>
        /// 듀얼 포인트 구매 횟수 변경 시 호출
        /// </summary>
        public event Action OnUpdateDuelPointBuyCount;

        /// <summary>
        /// 듀얼 정보 업데이트
        /// </summary>
        public event Action OnUpdateDuelInfo;

        private bool openDuelUI;
        private UIDuel.State duelState;

        public DuelModel()
        {
            duelRewardDataRepo = DuelRewardDataManager.Instance;
            profileDataRepo = ProfileDataManager.Instance;
            rewardBuffer = new Buffer<RewardData>();
            duelHistoryList = new List<CharDuelHistory>();
            duelCharListResetTime = 0;
            eventDuelCharListResetTime = 0;
            arenaDuelCharListResetTime = 0;
            RecentlyChapter = -1;
            SelectedState = UIDuel.State.Chapter;
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                BattleManager.OnStart += OpenDuelUIIfNeed;
                if (Entity.Daily != null)
                    Entity.Daily.OnResetDailyEvent += ResetDualPointChargeCount;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                BattleManager.OnStart -= OpenDuelUIIfNeed;
                if (Entity.Daily != null)
                    Entity.Daily.OnResetDailyEvent -= ResetDualPointChargeCount;
            }
        }

        internal void Initialize(CharacterPacket characterPacket)
        {
            DuelPoint = characterPacket.duelPoint;
            duelPointBuyCount = characterPacket.duelPointBuyCount;
        }

        internal void SetDuelPointBuyCount(int duelPointBuyCount)
        {
            bool isDirty = this.duelPointBuyCount.Replace(duelPointBuyCount);

            if (isDirty)
                OnUpdateDuelPointBuyCount?.Invoke();
        }

        public override void ResetData()
        {
            base.ResetData();
            chapterDuelStates = null;
            duelHistoryList.Clear();
            CurDuelInfo = null;
            DuelPoint = 0;
            duelPointBuyCount = 0;
            CurProgressingBattleState = null;
            CurBattleTargetingAlphabet = 0;
            IsDuelInfoInitialized = false;
            RecentlyChapter = -1;
            SelectedState = UIDuel.State.Chapter;
        }

        public ChapterDuelState GetDuelState(int chapter)
        {
            if (chapter > chapterDuelStates.Length)
            {
                Debug.LogError($"[듀얼] 챕터에 대한 데이터가 없음  챕터={chapter}");
                return null;
            }

            return chapterDuelStates[chapter - 1];
        }

        public void SetDualPoint(int point)
        {
            DuelPoint = point;
            OnDualPointChanged?.Invoke(point);
        }

        /// <summary>
        /// 듀얼 포인트 구매 횟수
        /// </summary>
        /// <returns></returns>
        public int GetDuelPointBuyCount() => duelPointBuyCount;

        /// <summary>
        /// 드랍으로 획득 가능한 듀얼 입장권 최대치
        /// </summary>
        /// <returns></returns>
        public int GetMaxDuelPoint()
        {
            return BasisType.DUEL_POINT_DROP_MAX.GetInt();
        }

        /// <summary>
        /// 듀얼 시 소모되는 입장권 수
        /// </summary>
        /// <returns></returns>
        public int GetEnterDuelPoint()
        {
            return BasisType.ENTER_DUEL_POINT.GetInt();
        }

        /// <summary>
        /// 듀얼 입장 가능 여부
        /// </summary>
        public bool CanDuel()
        {
            return DuelPoint >= GetEnterDuelPoint();
        }

        /// <summary>
        /// 듀얼 정보 요청
        /// </summary>
        public async Task RequestDuelInfo()
        {
            Response response = await Protocol.REQUEST_DUEL_INFO.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            DuelInfo duelInfo = response.GetPacket<DuelInfo>("1");
            CurDuelInfo = duelInfo;

            InitDuelStateCollectionIfNeed();

            // 듀얼 조각 & 보상 상태 세팅
            int chapter = 1;
            foreach (var item in duelInfo.GetChapterStates())
            {
                if (chapter > chapterDuelStates.Length)
                    continue;

                var state = chapterDuelStates[chapter - 1];
                state.InitOwningAndRewardState(item.curOwningBit, item.curRewardedCount);
                ++chapter;
            }

            // 듀얼 기록 세팅
            duelHistoryList.Clear();
            if (response.ContainsKey("2"))
            {
                duelHistoryList.AddRange(response.GetPacketArray<CharDuelHistory>("2"));
                for (int i = 0; i < duelHistoryList.Count; i++)
                {
                    duelHistoryList[i].Initialize(profileDataRepo);
                }
            }
            duelHistoryList.Sort((a, b) =>
            {
                return a.InsertDate > b.InsertDate ? -1 : 1;
            });

            IsDuelInfoInitialized = true;

            OnUpdateDuelInfo?.Invoke();
        }

        private void InitDuelStateCollectionIfNeed()
        {
            if (chapterDuelStates != null)
                return;

            var datas = duelRewardDataRepo.GetDatas();

            // 챕터별 보상 목록
            Dictionary<int, List<DuelRewardData>> buffer = new Dictionary<int, List<DuelRewardData>>();
            foreach (var item in datas)
            {
                if (item.check_type == 1)
                {
                    if (!buffer.ContainsKey(item.check_value))
                        buffer.Add(item.check_value, new List<DuelRewardData>());

                    buffer[item.check_value].Add(item);
                }
            }

            int count = buffer.Count;
            chapterDuelStates = new ChapterDuelState[count];

            int chapter;
            for (int i = 0; i < count; i++)
            {
                chapter = i + 1;
                buffer[chapter].Sort((a, b) => a.step_value - b.step_value);
                chapterDuelStates[i] = new ChapterDuelState(buffer[chapter].ToArray());
            }
        }

        public async Task<bool> RequestDuelReward(int chapter)
        {
            var duelState = GetDuelState(chapter);

            if (duelState == null)
                return false;

            if (duelState.RewardedCount == 2)
                return false;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", chapter);
            Response response = await Protocol.REQUEST_DUEL_GET_REWARD.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            Quest.QuestProgress(QuestType.CHAPTER_DUEL_CLEAR_COUNT, conditionValue: chapter); // 특정 챕터 듀얼 보상 받기 횟수

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null; // cud. 캐릭터 업데이트 데이터
            Notify(charUpdateData);

            if (charUpdateData != null)
            {
                foreach (var item in charUpdateData.rewards)
                {
                    rewardBuffer.Add(new RewardData(item.rewardType, item.rewardValue, item.rewardCount, item.rewardOption));
                }
            }

            string name = LocalizeKey._30602.ToText(); // 듀얼 보상
            string desc = LocalizeKey._30603.ToText(); // 듀얼 보상을 획득하였습니다.
            UI.Show<UIRewardDuel>().SetReward(rewardBuffer.GetBuffer(isAutoRelease: true), name, desc);

            duelState.InitOwningAndRewardState(0, duelState.RewardedCount + 1);

            return true;
        }

        public void UpdateDuelState(int chapter, int owningAlphabetBit)
        {
            if (chapterDuelStates == null)
                return;

            if (chapter > chapterDuelStates.Length)
            {
                Debug.LogError($"[듀얼] 챕터에 대한 데이터가 없음  챕터={chapter}");
                return;
            }

            chapterDuelStates[chapter - 1].AddOwningBit(owningAlphabetBit);
        }

        public async Task RequestDuelCharList(int chapter, int alphabet)
        {
            //Debug.Log($"[듀얼리스트 남은시간]={duelCharListResetTime.ToRemainTime()}");
            // 쿨타임이 있을경우 기존의 리스트로 보여줌
            if (duelCharListResetTime.ToRemainTime() > 0)
            {
                OnUpdateDuelCharList?.Invoke(duelCharList);
                OnResetWaitingTime?.Invoke();
                return;
            }

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", chapter);
            sfs.PutInt("2", 1 << alphabet);
            Response response = await Protocol.REQUEST_DUEL_CHAR_LIST.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // 초기화 남은 쿨타임 갱신
            duelCharListResetTime = BasisType.DUEL_LIST_CHANGE_TIME.GetInt();// * 0.001f;
            //Debug.Log($"[듀얼리스트 갱신]={duelCharListResetTime.ToRemainTime()}");
            OnResetWaitingTime?.Invoke();

            if (response.ContainsKey("1"))
            {
                duelCharList = response.GetPacketArray<DuelCharList>("1");
            }
            else
            {
                duelCharList = new DuelCharList[0];
            }

            for (int i = 0; i < duelCharList.Length; i++)
            {
                duelCharList[i].Initialize(profileDataRepo);
            }

            OnUpdateDuelCharList?.Invoke(duelCharList);
        }

        public void StartCooldownEventDuelCharList()
        {
            eventDuelCharListResetTime = BasisType.DUEL_LIST_CHANGE_TIME.GetInt();// * 0.001f;
            OnResetEventWaitingTime?.Invoke();
        }

        public void StartCooldownArenaDuelCharList()
        {
            arenaDuelCharListResetTime = BasisType.DUEL_LIST_CHANGE_TIME.GetInt();// * 0.001f;
            OnResetArenaWaitingTime?.Invoke();
        }

        public void RequestStartDuel(ChapterDuelState duel, int alphabetIndex, int targetCID, int targetUID)
        {
            if (DuelPoint < BasisType.ENTER_DUEL_POINT.GetInt())
                return;

            CurProgressingBattleState = duel;
            CurBattleTargetingAlphabet = alphabetIndex;
            var input = new BattleInputDuel(targetCID, targetUID, duel.Chapter, 1 << alphabetIndex);
            BattleManager.Instance.StartBattle(BattleMode.Duel, input);
        }

        public void RequestStartEventDuel(int serverId, int targetCid, int targetUid)
        {
            if (DuelPoint < BasisType.ENTER_DUEL_POINT.GetInt())
                return;

            var input = new BattleInputDuel(targetCid, targetUid, serverId);
            BattleManager.Instance.StartBattle(BattleMode.Duel, input);
        }

        public void RequestStartArenaDuel(int targetCid, int targetUid)
        {
            if (DuelPoint < BasisType.ENTER_DUEL_POINT.GetInt())
                return;

            var input = new BattleInputDuel(targetCid, targetUid);
            BattleManager.Instance.StartBattle(BattleMode.Duel, input);
        }

        public async Task<bool> RequestEventReward(int step)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", step);
            Response response = await Protocol.REQUEST_DUELWORLD_GET_REWARD.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                string name = LocalizeKey._30602.ToText(); // 듀얼 보상
                string desc = LocalizeKey._30603.ToText(); // 듀얼 보상을 획득하였습니다.
                UI.Show<UIRewardDuel>().SetReward(UI.ConvertRewardData(charUpdateData.rewards), name, desc);
            }

            return true;
        }

        /// <summary>
        /// 아레나 깃발 구매 (실패시 -1 리턴)
        /// </summary>
        public async Task<int> RequestArenaPointBuy()
        {
            Response response = await Protocol.REQUEST_ARENA_POINT_BUY.SendAsync();
            if (!response.isSuccess)
            {
                if (response.resultCode == ResultCode.NOT_IN_PROGRESS)
                {
                    string message = LocalizeKey._47881.ToText(); // 아레나가 종료되었습니다.
                    UI.ConfirmPopup(message);
                }
                else
                {
                    response.ShowResultCode();
                }

                return -1;
            }

            int arenaPoint = response.GetInt("1");

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            return arenaPoint;
        }

        /// <summary>
        /// 아레나 보상 받기
        /// </summary>
        public async Task<int> RequestArenaReward(int step)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", step);
            Response response = await Protocol.REQUEST_ARENA_POINT_REWARD_GET.SendAsync(sfs);
            if (!response.isSuccess)
            {
                // 진행중이 아니다.
                if (response.resultCode == ResultCode.NOT_IN_PROGRESS)
                {
                    string message = LocalizeKey._47881.ToText(); // 아레나가 종료되었습니다.
                    UI.ConfirmPopup(message);
                }
                // 최대 횟수를 초과하였습니다.
                else if (response.resultCode == ResultCode.NOT_ENOUGHT_COUNT)
                {
                    string message = LocalizeKey._47882.ToText(); // 아레나 깃발이 부족합니다.
                    UI.ConfirmPopup(message);
                }
                else
                {
                    response.ShowResultCode();
                }

                return -1;
            }

            int arenaPoint = response.GetInt("1");

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);

                string name = LocalizeKey._30602.ToText(); // 듀얼 보상
                string desc = LocalizeKey._30603.ToText(); // 듀얼 보상을 획득하였습니다.
                UI.Show<UIRewardDuel>().SetReward(UI.ConvertRewardData(charUpdateData.rewards), name, desc);
            }

            return arenaPoint;
        }

        public RemainTime GetDuelListWaitingTime(UIDuel.State state)
        {
            switch (state)
            {
                default:
                case UIDuel.State.Chapter:
                    return duelCharListResetTime;

                case UIDuel.State.Event:
                    return eventDuelCharListResetTime;

                case UIDuel.State.Arena:
                    return arenaDuelCharListResetTime;
            }
        }

        /// <summary>
        /// 이벤트 진행 중 여부
        /// </summary>
        public bool IsCheckEventTime()
        {
            return true;
        }

        /// <summary>
        /// 듀얼 결과 세팅
        /// </summary>
        public void SetDuelResult(bool isShowDuelAlphabet, UIDuel.State duelState)
        {
            if (isShowDuelAlphabet)
                CurProgressingBattleState.AddOwningIndex(CurBattleTargetingAlphabet);

            openDuelUI = true;
            this.duelState = duelState;
        }

        public async Task<Response> RequestBuyDuelPoint()
        {
            Response response = await Protocol.REQUEST_DUEL_POINT_BUY.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return response;
            }

            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            SetDualPoint(response.GetInt("1"));
            SetDuelPointBuyCount(DuelPointBuyCount + 1);

            return response;
        }

        private void OpenDuelUIIfNeed(BattleMode mode)
        {
            if (mode == BattleMode.Stage && openDuelUI)
                UI.Show<UIDuel>(new UIDuel.Input { needRequestDuelInfo = true, duelState = duelState });

            openDuelUI = false;
            duelState = UIDuel.State.Chapter;
        }

        private void ResetDualPointChargeCount()
        {
            SetDuelPointBuyCount(0);
        }
    }
}
