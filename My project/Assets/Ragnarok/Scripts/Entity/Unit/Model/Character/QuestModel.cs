using MEC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 퀘스트 정보
    /// </summary>
    public sealed class QuestModel : CharacterEntityModel, IEqualityComparer<ContentType>
    {
        private const string TAG = nameof(QuestModel);

        /// <summary>
        /// 이벤트 호출 주기
        /// </summary>
        private const float CALL_WAIT_TIME = 30f;

        private readonly BattleQuestInfo battleQuestInfo;
        private readonly ScenarioMazeDataManager scenarioMazeDataRepo;
        private readonly KafExchangeDataManager kafExchangeDataRepo;

        /// <summary>
        /// 해당 컨텐츠가 오픈될 때에 추가 (해당 컨텐츠에 new 아이콘 띄우기 위함)
        /// </summary>
        private readonly HashSet<ContentType> newOpenContentHashSet;

        /// <summary>
        /// 길드 퀘스트 하루 최대 보상 횟수
        /// </summary>
        public int GuildQuestRewardMaxCount => battleQuestInfo.GuildQuestRewardMaxCount;

        /// <summary>
        /// 카프라 운송 상태 타입
        /// </summary>
        public KafraCompleteType KafraCompleteType { get; private set; }

        /// <summary>
        /// 카프라 운송 목록
        /// </summary>
        public List<KafraDeliveryPacket> KafraDeliveryList;

        /// <summary>
        /// 진행중인 카프라 타입
        /// </summary>
        public KafraType CurKafraType { get; private set; }

        // <!-- Event --!>

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수 변경시 호출
        /// </summary>
        public event Action OnStandByReward;

        public event Action OnEventQuest;

        public event Action OnUpdateMainQuest
        {
            add { GetMaintQuest().OnUpdateEvent += value; }
            remove { GetMaintQuest().OnUpdateEvent -= value; }
        }

        public event Action OnUpdateTimePatrolQuest
        {
            add { GetTimePatrolQuest().OnUpdateEvent += value; }
            remove { GetTimePatrolQuest().OnUpdateEvent -= value; }
        }

        /// <summary>
        /// 의뢰 퀘스트 받기 여부 체크 이벤트
        /// </summary>
        public event Action OnNormalQuestFree;

        public event Action OnUpdateNewOpenContent;

        public event Action<bool> OnReqeustReward;

        /// <summary>
        /// 카프라 운송 상태 변경 시 호출
        /// </summary>
        public event Action OnUpdateKafra;

        /// <summary>
        /// 이벤트 퀘스트 스킵 시 호출 (퀘스트타입 103 한정)
        /// </summary>
        public event Action OnUpdateEventQuestSkip;

        public QuestModel()
        {
            battleQuestInfo = new BattleQuestInfo();
            scenarioMazeDataRepo = ScenarioMazeDataManager.Instance;
            kafExchangeDataRepo = KafExchangeDataManager.Instance;
            newOpenContentHashSet = new HashSet<ContentType>();
            KafraDeliveryList = new List<KafraDeliveryPacket>();
        }

        public override void AddEvent(UnitEntityType type)
        {
            battleQuestInfo.Initialize();
            battleQuestInfo.OnStandByRewards += OnUpdateStandByReward;

            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.AddEvent(OnReceiveCharDailyCalc);
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            Timing.KillCoroutines(TAG);

            battleQuestInfo.OnStandByRewards -= OnUpdateStandByReward;
            battleQuestInfo.Dispose();

            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.RemoveEvent(OnReceiveCharDailyCalc);
            }
        }

        internal void Initialize(QuestPacket questPacket)
        {
            battleQuestInfo.Reset(); // 초기화
            newOpenContentHashSet.Clear();

            if (questPacket != null)
            {
                // 1. 일퀘 퀘스트 목록 및 보상 수령 정보
                foreach (var item in questPacket.charQuestDailies.OrEmptyIfNull())
                {
                    battleQuestInfo.AddDailyQuest(item.quest_id, item.receive);
                }

                // 2. 일일 퀘스트 진행 정보
                foreach (var item in questPacket.charQuestDailyProgresses.OrEmptyIfNull())
                {
                    battleQuestInfo.SetDailyQuestProgress(item.quest_type, item.condition_value, item.progress);
                }

                // 3. 업적 퀘스트 진행 정보
                foreach (var item in questPacket.charAchieveProgresses.OrEmptyIfNull())
                {
                    battleQuestInfo.SetAchievementProgress(item.quest_type, item.condition_value, item.progress);
                }

                // 4. 업적 퀘스트 보상 수령 퀘스트 아이디
                foreach (var item in questPacket.charAchieveIds.OrEmptyIfNull())
                {
                    battleQuestInfo.SetReceivedAchievement(item);
                }

                // eg 이벤트 퀘스트 그룹 목록 세팅
                foreach (var item in questPacket.eventquestGroupInfos.OrEmptyIfNull())
                {
                    battleQuestInfo.AddEventQuestGroup(item);
                }

                // 5. 이벤트 퀘스트 목록 및 보상 수령 정보, 진행도 세팅
                foreach (var item in questPacket.eventQuestInfos.OrEmptyIfNull())
                {
                    battleQuestInfo.AddEventQuest(item.eventInfo.event_no, item.eventInfo.end_dt, item.eventInfo.quest_id, item.progress, item.receive, item.eventInfo.eventGroupId, item.eventInfo.initType);
                }

                // 7. 메인 퀘스트 목록
                foreach (var item in questPacket.charMainQuests.OrEmptyIfNull())
                {
                    battleQuestInfo.SetMainQuestProgress(item.quest_id, item.receive, item.progress);
                }

                // 6. 진행중인 메인 퀘스트 시퀀스
                battleQuestInfo.SetMainQuest(questPacket.mainQuestSeq);

                // 8. 의뢰 퀘스트 목록
                foreach (var item in questPacket.charNormalProgresses.OrEmptyIfNull())
                {
                    battleQuestInfo.SetNormalQuestProgress(item.quest_id, item.receive, item.progress);
                }

                //// 9. 다음 의뢰 퀘스트까지 남은 시간
                //nextNormalQuestTime = questPacket.nextNormalQuestTime;

                // 10-1. 길드 퀘스트 목록
                foreach (var item in QuestDataManager.Instance.Get(QuestCategory.Guild))
                {
                    battleQuestInfo.AddGuildQuest(item.id);
                }

                // 10-2. 길드 퀘스트 진행도 
                foreach (var item in questPacket.charQuestGuilds.OrEmptyIfNull())
                {
                    battleQuestInfo.SetGuildQuestProgress(item.quest_type, item.condition_value, item.progress);

                    battleQuestInfo.SetGuildQuestReceived(item.quest_type, item.condition_value, item.received);
                }

                // 12. 진행중인 타임패트롤 퀘스트 시퀀스
                battleQuestInfo.SetTimePatrolQuest(questPacket.timePatrolSeq);

                // 13. 타임패트롤 퀘스트 정보
                battleQuestInfo.SetTimePatrolQuestProgress(questPacket.timePatrolQuest.quest_id, questPacket.timePatrolQuest.receive, questPacket.timePatrolQuest.progress);

                // 14. 패스 일일 퀘스트 목록 및 보상 수령 정보
                foreach (var item in questPacket.passDailyQuests.OrEmptyIfNull())
                {
                    battleQuestInfo.AddPassDailyQuest(item.quest_id, item.receive);
                }

                // 15. 패스 일일 퀘스트 진행 정보
                foreach (var item in questPacket.passDailyQuestProgresses.OrEmptyIfNull())
                {
                    battleQuestInfo.SetPassDailyQuestProgress(item.quest_type, item.condition_value, item.progress);
                }

                // 16. 패스 퀘스트 진행도 
                foreach (var item in questPacket.passSeasonQuests.OrEmptyIfNull())
                {
                    battleQuestInfo.SetPassSeaonQuestProgress(item.quest_type, item.condition_value, item.progress);

                    battleQuestInfo.SetPassSeasonQuestReceived(item.quest_type, item.condition_value, item.received);
                }

                // 17. 온버프패스 일일 퀘스트 목록 및 보상 수령 정보
                foreach (var item in questPacket.onBuffPassDailyQuests.OrEmptyIfNull())
                {
                    battleQuestInfo.AddOnBuffPassDailyQuest(item.quest_id, item.receive);
                }

                // 18. 온버프패스 일일 퀘스트 진행 정보
                foreach (var item in questPacket.onBuffPassDailyQuestProgresses.OrEmptyIfNull())
                {
                    battleQuestInfo.SetOnBuffPassDailyQuestProgress(item.quest_type, item.condition_value, item.progress);
                }
            }

            battleQuestInfo.Reload(); // 다시 로드

            battleQuestInfo.InvokeStandByRewards();

            Timing.KillCoroutines(TAG);
            Timing.RunCoroutine(UpdateNormalQuest(), TAG);
        }

        private void OnReceiveCharDailyCalc(Response response)
        {
            if (response.isSuccess)
            {
                battleQuestInfo.ResetDaily();
                OnEventQuest?.Invoke();
                if (response.ContainsKey("3"))
                {
                    battleQuestInfo.ResetPassDaily();
                    var packets = response.GetPacketArray<PassDailyQuest>("3");
                    foreach (var item in packets.OrEmptyIfNull())
                    {
                        battleQuestInfo.AddPassDailyQuest(item.quest_id, item.receive);
                    }
                }

                if (response.ContainsKey("4"))
                {
                    battleQuestInfo.ResetOnBuffPassDaily();
                    var packets = response.GetPacketArray<PassDailyQuest>("4");
                    foreach (var item in packets.OrEmptyIfNull())
                    {
                        battleQuestInfo.AddOnBuffPassDailyQuest(item.quest_id, item.receive);
                    }
                }
            }
            else
            {
                response.ShowResultCode();
            }
        }

        /// <summary>
        /// 메인 퀘스트 반환
        /// </summary>
        public QuestInfo GetMaintQuest()
        {
            return battleQuestInfo.GetMainQuest();
        }

        /// <summary>
        /// 메인 퀘스트 반환
        /// </summary>
        public QuestInfo GetMaintQuest(int seq)
        {
            return battleQuestInfo.GetMainQuest(seq);
        }

        /// <summary>
        /// 타임패트롤 퀘스트 반환
        /// </summary>
        public QuestInfo GetTimePatrolQuest()
        {
            return battleQuestInfo.GetTimePatrolQuest();
        }

        /// <summary>
        /// 그룹별 일일 퀘스트 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetDailyQuests()
        {
            return battleQuestInfo.GetDailyQuests();
        }

        /// <summary>
        /// 길드 퀘스트 반환
        /// </summary>
        public QuestInfo[] GetGuildQuests()
        {
            return battleQuestInfo.GetGuildQuests();
        }

        /// <summary>
        /// 패스 일일 퀘스트 목록
        /// </summary>
        public QuestInfo[] GetPassDailyQuests(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return battleQuestInfo.GetPassDailyQuests();

                case PassType.OnBuff:
                    return battleQuestInfo.GetOnBuffPassDailyQuests();
            }

            throw new InvalidOperationException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
        }

        /// <summary>
        /// 패스 시즌 퀘스트 목록
        /// </summary>
        public QuestInfo[] GetPassSeasonQuests(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return battleQuestInfo.GetPassSeasonQuests();

                case PassType.OnBuff:
                    return null; // 시즌 정보 음슴
            }

            throw new InvalidOperationException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
        }

        /// <summary>
        /// 완료된 일일 길드 퀘스트 수 반환
        /// </summary>
        /// <returns></returns>
        public int GetGuildQuestRewardCount()
        {
            return battleQuestInfo.GetGuildQuestRewardCount();
        }

        /// <summary>
        /// 보상 받을 수 있는 길드 퀘스트 있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool HasGuildQuestReward()
        {
            return battleQuestInfo.HasGuildQuestReward();
        }

        /// <summary>
        /// 모든 일일 퀘스트 클리어 정보 반환
        /// </summary>
        public QuestInfo GetDailyTotalClearQuest()
        {
            return battleQuestInfo.GetDailyTotalClearQuest();
        }

        /// <summary>
        /// 모든 일일 퀘스트 클리어 완료 여부
        /// </summary>
        /// <returns></returns>
        public bool IsTotalClearQuest()
        {
            QuestInfo dailyTotalClearQuest = GetDailyTotalClearQuest();
            if (dailyTotalClearQuest == null)
                return false;

            return dailyTotalClearQuest.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward;
        }

        /// <summary>
        /// 그룹별 업적 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetAchievements()
        {
            return battleQuestInfo.GetAchievements();
        }

        /// <summary>
        /// 업적 진행도
        /// </summary>
        public float GetAchievementProgress()
        {
            return battleQuestInfo.GetAchievementProgress();
        }

        /// <summary>
        /// 그룹별 의뢰 퀘스트 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetNormalQuests()
        {
            return battleQuestInfo.GetNormalQuests();
        }

        /// <summary>
        /// 노멀 퀘스트 완료 가능
        /// </summary>
        public bool CanRewardNormalQuest()
        {
            return battleQuestInfo.CanRewardNormalQuest();
        }

        /// <summary>
        /// 의뢰 퀘스트 진행 가능
        /// </summary>
        public bool HasRemainNormalQuest()
        {
            return battleQuestInfo.HasRemainNormalQuest();
        }

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수
        /// </summary>
        public Dictionary<QuestCategory, int> GetStandByRewards()
        {
            return battleQuestInfo.GetStandByRewards();
        }

        /// <summary>
        /// 메인 퀘스트 보상 획득 가능여부
        /// </summary>
        public bool IsMainQuestReward()
        {
            var main = GetMaintQuest();
            if (main.IsInvalidData)
                return false;

            return main.CompleteType == QuestInfo.QuestCompleteType.StandByReward;
        }

        /// <summary>
        /// 타임패트롤 퀘스트 보상 획득 가능 여부
        /// </summary>
        public bool IsTimePatrolReward()
        {
            var timePatrol = GetTimePatrolQuest();
            if (timePatrol.IsInvalidData)
                return false;

            return timePatrol.CompleteType == QuestInfo.QuestCompleteType.StandByReward;
        }

        /// <summary>
        /// 보상 받을수 있는 퀘스트가 있는지 여부
        /// </summary>
        public bool IsStandByReward()
        {
            return battleQuestInfo.IsStandByReward();
        }

        /// <summary>
        /// 보상 받을수 있는 이벤트 퀘스트가 있는지 여부
        /// </summary>
        public bool IsEventQuestStandByReward()
        {
            return battleQuestInfo.IsEventQuestStandByReward();
        }

        /// <summary>
        /// 보상 받을수 있는 이벤트 퀘스트가 있는지 여부
        /// </summary>
        public bool IsEventQuestStandByReward(int groupId)
        {
            return battleQuestInfo.IsEventQuestStandByReward(groupId);
        }

        /// <summary>
        /// 가이드퀘스트가 클리어 가능한지 여부
        /// </summary>
        public bool IsGuideQuestStandByReward()
        {
            QuestInfo currentGuideQuestInfo = battleQuestInfo.GetMainQuest();
            if (currentGuideQuestInfo == null)
                return false;

            return currentGuideQuestInfo.CompleteType == QuestInfo.QuestCompleteType.StandByReward;
        }

        /// <summary>
        /// 보상 받을수 있는 패스 퀘스트가 있는지 여부
        /// </summary>
        public bool IsPassQuestStandByReward(PassType passType)
        {
            switch (passType)
            {
                case PassType.Labyrinth:
                    return battleQuestInfo.IsPassQuestStandByReward();

                case PassType.OnBuff:
                    return battleQuestInfo.IsOnBuffPassQuestStandByReward();
            }

            throw new System.InvalidOperationException($"유효하지 않은 처리: {nameof(passType)} = {passType}");
        }

        /// <summary>
        /// 이벤트 퀘스트 그룹 반환
        /// </summary>
        public EventQuestGroupInfo[] GetEventQuestGroupInfos()
        {
            return battleQuestInfo.GetEventQuestGroupInfos();
        }

        public EventQuestGroupInfo GetEventQuestByShortCut(ShortCutType shortCutType)
        {
            return battleQuestInfo.GetEventQuestByShortCut(shortCutType);
        }

        /// <summary>
        /// 이벤트 퀘스트 반환
        /// </summary>
        public QuestInfo[] GetEventQuests(int groupId, bool isSort = true)
        {
            return battleQuestInfo.GetEventQuests(groupId, isSort);
        }

        /// <summary>
        /// 보상받을수 있는 퀘스트 수 변경
        /// </summary>
        private void OnUpdateStandByReward()
        {
            OnStandByReward?.Invoke();
        }

        /// <summary>
        /// 퀘스트 보상 수령 
        /// </summary>
        public async Task<Response> RequestQuestRewardAsync(QuestInfo info, bool isShowRewardPopup = false)
        {
            // 가방 무게 체크
            if (info.IsWeight && !UI.CheckInvenWeight())
                return default;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", info.ID); // quest_id

            // 이벤트 퀘스트 No
            if (info.Category == QuestCategory.Event)
                sfs.PutInt("3", info.EventNo);

            OnReqeustReward?.Invoke(true);
            var response = await Protocol.QUEST_REWARD.SendAsync(sfs);
            OnReqeustReward?.Invoke(false);

            if (response.isSuccess)
            {
                SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess); // 퀘스트 보상 SFX

                // cud. 캐릭터 업데이트 데이터
                if (response.ContainsKey("cud"))
                {
                    CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                    Notify(charUpdateData);

                    if (info.Category == QuestCategory.Main || info.Category == QuestCategory.TimePatrol)
                    {
                        // 보상 팝업 띄우기
                        if (info != null)
                        {
                            // 이미 표시중인 UI 가 있다면 지워줍니다.
                            UIPowerUpdate uiPowerUpdate = UI.GetUI<UIPowerUpdate>();
                            if (uiPowerUpdate && uiPowerUpdate.IsVisible)
                                uiPowerUpdate.Hide();

                            // 타임슈트 획득 처리
                            if (info.Category == QuestCategory.TimePatrol)
                            {
                                List<int> questSeqList = BasisType.TIME_SUIT_QUEST_ID.GetKeyList();
                                if (questSeqList.Contains(info.Group))
                                {
                                    int timeSuit = BasisType.TIME_SUIT_QUEST_ID.GetInt(info.Group);
                                    if (timeSuit != 0)
                                    {
                                        NotifyTimeSuit(timeSuit.ToEnum<ShareForceType>(), 0);
                                    }
                                }
                            }

                            UI.Show<UIQuestReward>(new UIQuestReward.Input(info));

                            if (info.Category == QuestCategory.Main)
                            {
                                Analytics.TrackEvent($"Quest{info.Group}");

                                if (info.OpenContent == ContentType.ShareControl)
                                {
                                    Analytics.TrackEvent(TrackType.ShareControl);
                                }
                            }
                        }
                    }
                    else if (info.Category == QuestCategory.PassDaily || info.Category == QuestCategory.PassSeason || info.Category == QuestCategory.OnBuffPassDaily || isShowRewardPopup)
                    {
                        UI.RewardInfo(charUpdateData.rewards);
                    }
                    else
                    {
                        UI.RewardToast(charUpdateData.rewards, isExceptGoods: false); // 획득 보상 보여줌 (토스트팝업)
                    }
                }

                battleQuestInfo.ProcessReceiveReward(info); // 보상 받기 처리
            }
            else
            {
                response.ShowResultCode();
            }

            return response;
        }

        /// <summary>
        /// 이벤트 퀘스트 정보 요청
        /// </summary>
        /// <returns></returns>
        public async Task RequestEventQuest()
        {
            if (battleQuestInfo.GetEndEventQuestCount() == 0)
                return;

            var response = await Protocol.REQUEST_EVENT_QUEST.SendAsync();
            if (response.isSuccess)
            {
                EventQuestGroupPacket[] eventquestGroupInfos = null;
                EventQuestInfo[] eventQuestInfos = null;

                if (response.ContainsKey("eg"))
                {
                    eventquestGroupInfos = response.GetPacketArray<EventQuestGroupPacket>("eg");
                    Array.Sort(eventquestGroupInfos, (a, b) => a.sort.CompareTo(b.sort));
                }

                if (response.ContainsKey("5"))
                    eventQuestInfos = response.GetPacketArray<EventQuestInfo>("5");

                battleQuestInfo.ResetEventQuest();

                // eg 이벤트 퀘스트 그룹 목록 세팅
                foreach (var item in eventquestGroupInfos.OrEmptyIfNull())
                {
                    battleQuestInfo.AddEventQuestGroup(item);
                }

                // 5. 이벤트 퀘스트 목록 및 보상 수령 정보, 진행도 세팅
                foreach (var item in eventQuestInfos.OrEmptyIfNull())
                {
                    battleQuestInfo.AddEventQuest(item.eventInfo.event_no, item.eventInfo.end_dt, item.eventInfo.quest_id, item.progress, item.receive, item.eventInfo.eventGroupId, item.eventInfo.initType);
                }

                OnEventQuest?.Invoke();

                battleQuestInfo.InvokeStandByRewards();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public QuestInfo GetNeedQuest(ContentType contentType)
        {
            return battleQuestInfo.GetMainQuest(contentType);
        }

        /// <summary>
        /// 현재 진행중인 퀘스트
        /// </summary>
        public bool IsCurrentQuest(QuestType questType)
        {
            QuestInfo currentMainQuest = battleQuestInfo.GetMainQuest();
            if (currentMainQuest == null)
                return false;

            QuestInfo find = battleQuestInfo.FindMainQuestByQuestType(questType);
            if (find == null)
                return false;

            return currentMainQuest.Group == find.Group && currentMainQuest.CompleteType == QuestInfo.QuestCompleteType.InProgress;
        }

        /// <summary>
        /// 현재 완료한 퀘스트
        /// </summary>
        public bool IsStandByRewardQuest(QuestType questType)
        {
            QuestInfo currentMainQuest = battleQuestInfo.GetMainQuest();
            if (currentMainQuest == null)
                return false;

            QuestInfo find = battleQuestInfo.FindMainQuestByQuestType(questType);
            if (find == null)
                return false;

            return currentMainQuest.Group == find.Group && currentMainQuest.CompleteType == QuestInfo.QuestCompleteType.StandByReward;
        }

        public ScenarioMazeData GetNeedScenario(ContentType contentType)
        {
            return scenarioMazeDataRepo.GetByContents(contentType);
        }

        public ScenarioMazeData GetScenarioData(int id)
        {
            return scenarioMazeDataRepo.Get(id);
        }

        /// <summary>
        /// 가이드 퀘스트에 따른 컨텐츠 오픈 여부 체크
        /// </summary>
        public bool IsOpenContent(ContentType contentType, bool isShowPopup = true)
        {
            if (Cheat.All_OPEN_CONTENT)
                return true;

            QuestInfo needQuest = GetNeedQuest(contentType);
            ScenarioMazeData needScenario = GetNeedScenario(contentType);

            if (needQuest == null && needScenario == null)
            {
                return true;
            }
            else if (needScenario != null)
            {
                var res = Entity.Dungeon.IsOpenContent(contentType);

                if (res)
                    return true;

                if (isShowPopup)
                    UI.ShowToastPopup(LocalizeKey._54305.ToText().Replace(ReplaceKey.NAME, needScenario.name_id.ToText()));

                return false;
            }
            else
            {
                if (needQuest.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward)
                    return true;

                if (isShowPopup)
                {
                    string description = LocalizeKey._90083.ToText() // 가이드 퀘스트 [{NUMBER}.{NAME}] 클리어 해야합니다.
                        .Replace("{NUMBER}", needQuest.Group.ToString())
                        .Replace("{NAME}", needQuest.Name.ToString());

                    UI.ShowToastPopup(description);
                }

                return false;
            }
        }

        public string OpenContentText(ContentType contentType)
        {
            QuestInfo needQuest = GetNeedQuest(contentType);
            ScenarioMazeData needScenario = GetNeedScenario(contentType);

            if (needQuest == null && needScenario == null)
            {
                return string.Empty;
            }
            else if (needScenario != null)
            {
                return LocalizeKey._54305.ToText().Replace(ReplaceKey.NAME, needScenario.name_id.ToText());
            }
            else
            {
                return LocalizeKey._90177.ToText() // 메인 퀘스트 [37BCFF][c][{NUMBER}.{NAME}][/c][-] 클리어 해야합니다.
                    .Replace("{NUMBER}", needQuest.Group.ToString())
                    .Replace("{NAME}", needQuest.Name.ToString());
            }
        }

        /// <summary>
        /// 길드 가입 상태 세팅
        /// </summary>
        /// <param name="hasGuild"></param>
        public void SetHasGuild(bool hasGuild)
        {
            battleQuestInfo.SetHaveGuild(hasGuild);
        }

        /// <summary>
        /// 새로운 오픈 컨텐츠 알림 추가
        /// </summary>
        public void AddNewOpenContent(ContentType contentType)
        {
            if (contentType == default)
                return;

            if (!newOpenContentHashSet.Add(contentType))
                return;

            OnUpdateNewOpenContent?.Invoke();
        }

        /// <summary>
        /// 새로운 오픈 컨텐츠 알림 제거
        /// </summary>
        public void RemoveNewOpenContent(ContentType contentType)
        {
            if (!newOpenContentHashSet.Remove(contentType))
                return;

            OnUpdateNewOpenContent?.Invoke();
        }

        /// <summary>
        /// 새로운 오픈 컨텐츠 알림 확인
        /// </summary>
        public bool HasNewOpenContent(ContentType contentType)
        {
            return newOpenContentHashSet.Contains(contentType);
        }

        /// <summary>
        /// 30초에 한번씩 의뢰 퀘스트 받을 수 있는지 체크
        /// </summary>
        private IEnumerator<float> UpdateNormalQuest()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(CALL_WAIT_TIME);
                OnNormalQuestFree?.Invoke();
            }
        }

        /// <summary>
        /// 쉐어포스 획득 가능한 퀘스트 시퀀스
        /// </summary>
        public int OpenShareForceQuestSeq(ShareForceType type)
        {
            List<int> questSeqList = BasisType.TIME_SUIT_QUEST_ID.GetKeyList();
            foreach (var seq in questSeqList)
            {
                int shareForceType = BasisType.TIME_SUIT_QUEST_ID.GetInt(seq);

                if (type == shareForceType.ToEnum<ShareForceType>())
                {
                    return seq;
                }
            }
            return default;
        }

        internal void Initialize(KafraCompleteType kafraCompleteType)
        {
            KafraCompleteType = kafraCompleteType;
            Debug.Log($"[카프라 운송 상태] {KafraCompleteType}, {(int)KafraCompleteType}");
        }

        internal void Initialize(KafraDeliveryPacket[] packets)
        {
            KafraDeliveryList.Clear();
            foreach (KafraDeliveryPacket packet in packets)
            {
                KafraDeliveryList.Add(packet);
                KafExchangeData data = kafExchangeDataRepo.Get(packet.Id);
                if (data != null && CurKafraType != data.type.ToEnum<KafraType>())
                {
                    CurKafraType = data.type.ToEnum<KafraType>();
                    Debug.Log($"[카프라 운송 타입] {nameof(CurKafraType)}={CurKafraType}");
                }
                Debug.Log($"[카프라 운송 목록] {nameof(packet.Id)}={packet.Id}, {nameof(packet.Count)}={packet.Count}");
            }
        }

        /// <summary>
        /// [카프라 운송] 수락 요청
        /// </summary>
        public async Task RequestKafraDelivery((int id, int count)[] items)
        {
            var sfs = Protocol.NewInstance();
            var sfsArray = Protocol.NewArrayInstance();
            foreach (var item in items)
            {
                var element = Protocol.NewInstance();
                element.PutInt("1", item.id);
                element.PutInt("2", item.count);
                sfsArray.AddSFSObject(element);
            }
            sfs.PutSFSArray("1", sfsArray);

            var response = await Protocol.REQUEST_KAF_DELIVERY.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }

            // 1. 카프라 운송 목록
            if (response.ContainsKey("1"))
            {
                Initialize(KafraCompleteType.InProgress);
                Initialize(response.GetPacketArray<KafraDeliveryPacket>("1"));
            }

            OnUpdateKafra?.Invoke();
        }

        /// <summary>
        /// [카프라 운송] 완료 요청
        /// </summary>
        public async Task RequestKafraDeliveryAccept()
        {
            var response = await Protocol.REQUEST_KAF_DELIVERY_ACCEPT.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            Initialize(KafraCompleteType.StandByReward);

            OnUpdateKafra?.Invoke();
        }

        /// <summary>
        /// [카프라 운송] 보상 요청
        /// </summary>
        public async Task RequestKafraDeliveryReward()
        {
            var response = await Protocol.REQUEST_KAF_DELIVERY_REWARD.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
            Notify(charUpdateData);

            RewardData[] rewards = UI.ConvertRewardData(charUpdateData == null ? null : charUpdateData.rewards); // 획득 보상
            SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess); // 퀘스트 보상 SFX
            UI.Show<UIQuestReward>(new UIQuestReward.Input(CurKafraType, rewards));

            Initialize(KafraCompleteType.None);
            KafraDeliveryList.Clear();
            CurKafraType = KafraType.Exchange;

            OnUpdateKafra?.Invoke();
        }

        /// <summary>
        /// 냥다래 나무 모든보상 획득 퀘스트(103) 스킵
        /// </summary>
        public async Task RequestEventConnectQuestSkip()
        {
            var response = await Protocol.REQUEST_EVENT_CONNECT_QUEST_SKIP.SendAsync();
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            Quest.QuestProgress(QuestType.CONNECT_TIME_ALL_REWARD);

            CharUpdateData charUpdateData = response.ContainsKey("cud") ? response.GetPacket<CharUpdateData>("cud") : null;
            Notify(charUpdateData);

            OnUpdateEventQuestSkip?.Invoke();
        }

        bool IEqualityComparer<ContentType>.Equals(ContentType x, ContentType y)
        {
            return x == y;
        }

        int IEqualityComparer<ContentType>.GetHashCode(ContentType obj)
        {
            return obj.GetHashCode();
        }
    }
}