using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class BattleQuestInfo : IEqualityComparer<BattleQuestInfo.ProgressKey>, IEqualityComparer<QuestCategory>, IEventQuestGroupInfoImpl
    {
        private struct ProgressKey
        {
            public readonly ObscuredShort questType;
            public readonly ObscuredInt conditionValue;

            public ProgressKey(short questType, int conditionValue)
            {
                this.questType = questType;
                this.conditionValue = conditionValue;
            }

            public override bool Equals(object obj)
            {
                if (obj is ProgressKey)
                    return Equals((ProgressKey)obj);

                return false;
            }

            public override int GetHashCode()
            {
                int hash = 17;

                hash = hash * 29 + questType.GetHashCode();
                hash = hash * 29 + conditionValue.GetHashCode();
                return hash;
            }

            public bool Equals(ProgressKey obj)
            {
                return obj.questType == questType && obj.conditionValue == conditionValue;
            }
        }

        private readonly QuestDataManager questDataRepo;

        /// <summary>
        /// 진행중인 메인 퀘스트 
        /// </summary>
        private readonly QuestInfo mainQuest;

        /// <summary>
        /// 메인 퀘스트 목록
        /// </summary>
        private readonly Dictionary<ObscuredInt, QuestInfo> mainQuestDic;

        /// <summary>
        /// "모든 일일 퀘스트 클리어" 정보
        /// </summary>
        private readonly QuestInfo dailyTotalClearQuest;

        /// <summary>
        /// 퀘스트 리스트 (완료된 퀘스트 포함)
        /// </summary>
        private readonly List<QuestInfo> dailyQuestList, achievementList, eventQuestList, normalQuestList, guildQuestList, passDailyQuestList, passSeasonQuestList, onBuffPassDailyQuestList;

        /// <summary>
        /// 진행중인 퀘스트
        /// </summary>
        private readonly Dictionary<ProgressKey, ObscuredInt> dailyQuestProgressDic, achievementProgressDic, guildQuestProgressDic, passDailyProgressDic, passSeasonProgressDic, onBuffPassDailyProgressDic;

        /// <summary>
        /// 카테고리별 보상받을수 있는 퀘스트 수(New 표시에 사용)
        /// </summary>
        private readonly Dictionary<QuestCategory, int> standByRewardDic;

        /// <summary>
        /// 이벤트 퀘스트 그룹 정보(배너용)
        /// </summary>
        private readonly List<EventQuestGroupInfo> eventQuestGroupInfoList;

        /// <summary>
        /// 진행중인 타임패트롤 퀘스트 
        /// </summary>
        private readonly QuestInfo timePatrolQuest;

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수 변경시 호출
        /// </summary>
        public event Action OnStandByRewards;

        private bool hasGuild;

        /// <summary>
        /// 길드 퀘스트 하루 최대 보상 횟수
        /// </summary>
        public int GuildQuestRewardMaxCount => BasisType.GUILD_QUEST_MAX_REWARD_COUNT.GetInt();

        private readonly ConnectionManager connectionManager;

        public BattleQuestInfo()
        {
            questDataRepo = QuestDataManager.Instance;

            mainQuest = new QuestInfo();

            mainQuestDic = new Dictionary<ObscuredInt, QuestInfo>(ObscuredIntEqualityComparer.Default);

            dailyTotalClearQuest = new QuestInfo();
            dailyQuestList = new List<QuestInfo>();
            dailyQuestProgressDic = new Dictionary<ProgressKey, ObscuredInt>(this);

            achievementList = new List<QuestInfo>();
            achievementProgressDic = new Dictionary<ProgressKey, ObscuredInt>(this);

            eventQuestList = new List<QuestInfo>();

            normalQuestList = new List<QuestInfo>();

            standByRewardDic = new Dictionary<QuestCategory, int>(this);

            eventQuestGroupInfoList = new List<EventQuestGroupInfo>();

            guildQuestList = new List<QuestInfo>();
            guildQuestProgressDic = new Dictionary<ProgressKey, ObscuredInt>(this);

            timePatrolQuest = new QuestInfo();

            passDailyQuestList = new List<QuestInfo>();
            passDailyProgressDic = new Dictionary<ProgressKey, ObscuredInt>(this);

            passSeasonQuestList = new List<QuestInfo>();
            passSeasonProgressDic = new Dictionary<ProgressKey, ObscuredInt>(this);

            onBuffPassDailyQuestList = new List<QuestInfo>();
            onBuffPassDailyProgressDic = new Dictionary<ProgressKey, ObscuredInt>(this);

            connectionManager = ConnectionManager.Instance;
        }

        public void Initialize()
        {
            Quest.OnProgress += OnProgress;
        }

        public void Dispose()
        {
            Quest.OnProgress -= OnProgress;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Reset()
        {
            // 데이터 초기화
            mainQuestDic.Clear();
            dailyQuestList.Clear();
            achievementList.Clear();
            eventQuestList.Clear();
            normalQuestList.Clear();
            standByRewardDic.Clear();
            eventQuestGroupInfoList.Clear();
            guildQuestList.Clear();
            passDailyQuestList.Clear();
            passSeasonQuestList.Clear();
            onBuffPassDailyQuestList.Clear();

            // 메인 퀘스트 정보 (캐릭터 생성 후 첫번째 접속시 프로그래스 정보가 없다)
            List<QuestData> mainDataList = questDataRepo.Get(QuestCategory.Main);
            for (int i = 0; i < mainDataList.Count; i++)
            {
                QuestInfo info = new QuestInfo();
                info.SetData(mainDataList[i]);
                mainQuestDic.Add(info.ID, info);
            }

            // 모든 일일 퀘스트 클리어
            dailyTotalClearQuest.SetData(questDataRepo.Get(QuestType.DAILY_QUEST_CLEAR));

            // 업적 정보 (업적은 서버에서 보내지 않기 때문에 데이터에 있는 모든 업적을 미리 가져옴)
            List<QuestData> achievementDataList = questDataRepo.Get(QuestCategory.Achieve);
            for (int i = 0; i < achievementDataList.Count; i++)
            {
                QuestInfo info = new QuestInfo();
                info.SetData(achievementDataList[i]);
                achievementList.Add(info);
            }

            // 미궁 정복자 퀘스트 세팅
            List<QuestData> normalDataList = questDataRepo.Get(QuestCategory.Normal);
            if (normalDataList == null)
            {
                // Do Nothing
            }
            else
            {
                for (int i = 0; i < normalDataList.Count; i++)
                {
                    QuestInfo info = new QuestInfo();
                    info.SetData(normalDataList[i]);
                    normalQuestList.Add(info);
                }
            }

            // 패스 시즌 정보 (패스 시즌은 서버에서 보내지 않기 때문에 데이터에 있는 패스 시즌을 미리 가져옴)
            List<QuestData> passSeasonDataList = questDataRepo.Get(QuestCategory.PassSeason);
            if (passSeasonDataList != null)
            {
                for (int i = 0; i < passSeasonDataList.Count; i++)
                {
                    QuestInfo info = new QuestInfo();
                    info.SetData(passSeasonDataList[i]);
                    passSeasonQuestList.Add(info);
                }
            }

            // 보상받을수 있는 퀘스트 수 초기화
            foreach (QuestCategory item in Enum.GetValues(typeof(QuestCategory)))
            {
                standByRewardDic.Add(item, 0);
            }
        }

        /// <summary>
        /// 다시 로드
        /// </summary>
        public void Reload()
        {
            // 모든 일일퀘스트 클리어의 MaxValue는 모든 일일 퀘스트 개수
            // dailyQuestDic 에는 모든 일일퀘스트 클리어 Data 정보가 빠져있기 때문에
            // dailyQuestDic.Count -1 로 처리하지 않고 그냥 dailyQuestDic.Count 값을 넣어준다
            int dailyReceivedCount = 0;
            for (int i = 0; i < dailyQuestList.Count; i++)
            {
                if (dailyQuestList[i].CompleteType == QuestInfo.QuestCompleteType.ReceivedReward)
                    ++dailyReceivedCount;
            }

            // Max를 먼저 세팅해줘야 한다!!
            dailyTotalClearQuest.SetMaxValue(dailyQuestList.Count);
            dailyTotalClearQuest.SetCurrentValue(dailyReceivedCount);

            // 보상받을수 있는 일일 퀘스트 수(일일퀘 모두 클리어)
            SetStandByReward(dailyTotalClearQuest, isAdd: true);

            // 일일 퀘스트 Progress 세팅
            for (int i = 0; i < dailyQuestList.Count; i++)
            {
                QuestInfo info = dailyQuestList[i];

                ProgressKey key = new ProgressKey(info.QuestTypeValue, info.ConditionValue);

                if (!dailyQuestProgressDic.ContainsKey(key))
                    continue;

                info.SetCurrentValue(dailyQuestProgressDic[key]);

                // 보상받을수 있는 일일 퀘스트 수
                SetStandByReward(info, isAdd: true);
            }

            // 업적 Progress 세팅
            for (int i = 0; i < achievementList.Count; i++)
            {
                QuestInfo info = achievementList[i];

                ProgressKey key = new ProgressKey(info.QuestTypeValue, info.ConditionValue);

                if (!achievementProgressDic.ContainsKey(key))
                    continue;

                info.SetCurrentValue(achievementProgressDic[key]);

                // 보상받을수 있는 업적 퀘스트 수
                SetStandByReward(info, isAdd: true);
            }

            // 길드퀘스트 Progress 세팅
            for (int i = 0; i < guildQuestList.Count; ++i)
            {
                QuestInfo info = guildQuestList[i];
                ProgressKey key = new ProgressKey(info.QuestTypeValue, info.ConditionValue);

                if (!guildQuestProgressDic.ContainsKey(key))
                    continue;

                info.SetCurrentValue(guildQuestProgressDic[key]);

                // 보상받을수 있는 업적 퀘스트 수
                SetStandByReward(info, isAdd: true);
            }

            // 패스 시즌 Progress 세팅
            for (int i = 0; i < passDailyQuestList.Count; i++)
            {
                QuestInfo info = passDailyQuestList[i];

                ProgressKey key = new ProgressKey(info.QuestTypeValue, info.ConditionValue);

                if (!passDailyProgressDic.ContainsKey(key))
                    continue;

                info.SetCurrentValue(passDailyProgressDic[key]);

                // 보상받을수 있는 업적 퀘스트 수
                SetStandByReward(info, isAdd: true);
            }

            // 패스 시즌 Progress 세팅
            for (int i = 0; i < passSeasonQuestList.Count; i++)
            {
                QuestInfo info = passSeasonQuestList[i];

                ProgressKey key = new ProgressKey(info.QuestTypeValue, info.ConditionValue);

                if (!passSeasonProgressDic.ContainsKey(key))
                    continue;

                info.SetCurrentValue(passSeasonProgressDic[key]);

                // 보상받을수 있는 업적 퀘스트 수
                SetStandByReward(info, isAdd: true);
            }

            // 온버프패스 Progress 세팅
            for (int i = 0; i < onBuffPassDailyQuestList.Count; i++)
            {
                QuestInfo info = onBuffPassDailyQuestList[i];

                ProgressKey key = new ProgressKey(info.QuestTypeValue, info.ConditionValue);

                if (!onBuffPassDailyProgressDic.ContainsKey(key))
                    continue;

                info.SetCurrentValue(onBuffPassDailyProgressDic[key]);

                // 보상받을수 있는 업적 퀘스트 수
                SetStandByReward(info, isAdd: true);
            }

            dailyQuestProgressDic.Clear();
            achievementProgressDic.Clear();
            guildQuestProgressDic.Clear();
            passDailyProgressDic.Clear();
            passSeasonProgressDic.Clear();
            onBuffPassDailyProgressDic.Clear();

            InvokeStandByRewards();
        }

        #region 메인 퀘스트

        /// <summary>
        /// 진행중인 메인 퀘스트 정보
        /// </summary>
        /// <returns></returns>
        public QuestInfo GetMainQuest()
        {
            return mainQuest;
        }

        /// <summary>
        /// 메인 퀘스트 
        /// </summary>
        /// <param name="questID"></param>
        /// <param name="isReceived"></param>
        /// <param name="progress"></param>
        public void SetMainQuestProgress(int questID, bool isReceived, int progress)
        {
            QuestData data = questDataRepo.Get(questID);
            if (data == null)
                return;

            if (!mainQuestDic.ContainsKey(questID))
                return;

            mainQuestDic[questID].SetReceived(isReceived);
            mainQuestDic[questID].SetCurrentValue(progress);
        }

        /// <summary>
        /// 진행중인 메인 퀘스트 세팅
        /// </summary>
        /// <param name="mainQuestSeq"></param>
        public void SetMainQuest(int mainQuestSeq)
        {
            QuestData data = questDataRepo.GetMainQuest(mainQuestSeq);

            if (data == null)
            {
                mainQuest.SetData(null);
                return;
            }

            mainQuest.SetData(data);
            mainQuest.SetReceived(false);

            foreach (var item in mainQuestDic.Values.OrEmptyIfNull())
            {
                if (item.Group == mainQuestSeq)
                {
                    item.SetWait(false);
                    mainQuest.SetCurrentValue(item.CurrentValue);
                    mainQuest.SetWait(false);
                }
                else if (item.Group > mainQuestSeq)
                {
                    item.SetWait(true);
                }
                else
                {
                    item.SetWait(false);
                    item.SetReceived(true);
                }
            }
        }

        /// <summary>
        /// 컨텐츠 오픈을 위해 클리어해야하는 메인 퀘스트
        /// </summary>
        public QuestInfo GetMainQuest(ContentType contentType)
        {
            return mainQuestDic.Values.FirstOrDefault(x => x.OpenContent == contentType);
        }

        /// <summary>
        /// 메인 퀘스트
        /// </summary>
        public QuestInfo GetMainQuest(int seq)
        {
            return mainQuestDic.Values.FirstOrDefault(x => x.Group == seq);
        }

        /// <summary>
        /// 완료조건으로 메인 퀘스트 찾기
        /// </summary>
        public QuestInfo FindMainQuestByQuestType(QuestType questType)
        {
            return mainQuestDic.Values.FirstOrDefault(x => x.QuestType == questType);
        }

        #endregion

        #region 일일 퀘스트

        /// <summary>
        /// 일일 퀘스트 초기화
        /// </summary>
        public void ResetDaily()
        {
            // 일일 퀘스트 초기화
            dailyTotalClearQuest.SetCurrentValue(0);
            foreach (var item in dailyQuestList)
            {
                item.SetReceived(false);
                item.SetCurrentValue(0);
            }
            standByRewardDic[QuestCategory.DailyStart] = 0;

            // 이벤트 퀘스트 초기화
            foreach (var item in eventQuestList)
            {
                if (item.InitType != 1)
                    continue;

                item.SetReceived(false);
                item.SetCurrentValue(0);
            }
            standByRewardDic[QuestCategory.Event] = 0;

            // 길드 퀘스트 초기화
            foreach (var item in guildQuestList)
            {
                item.SetReceived(false);
                item.SetCurrentValue(0);
            }
            standByRewardDic[QuestCategory.Guild] = 0;

            InvokeStandByRewards();
        }

        /// <summary>
        /// 그룹별 일일 퀘스트 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetDailyQuests()
        {
            return GetQuests(dailyQuestList);
        }

        /// <summary>
        /// 모든 일일 퀘스트 클리어 정보 반환
        /// </summary>
        public QuestInfo GetDailyTotalClearQuest()
        {
            return dailyTotalClearQuest;
        }

        /// <summary>
        /// 서버 일일 퀘스트 추가
        /// </summary>
        public void AddDailyQuest(int id, bool isReceived)
        {
            QuestData data = questDataRepo.Get(id);

            if (data == null)
            {
                Debug.LogError("퀘스트 추가 불가");
                return;
            }

            QuestInfo info;

            // 모든 일일 퀘스트 클리어일 경우에는 dailyQuestDic 에 포함히시지 않음
            QuestType questType = data.quest_type.ToEnum<QuestType>();
            if (questType == QuestType.DAILY_QUEST_CLEAR)
            {
                info = dailyTotalClearQuest;
            }
            else
            {
                info = new QuestInfo();
                info.SetData(data);
                dailyQuestList.Add(info); // 퀘스트 추가
            }

            info.SetReceived(isReceived); // 보상 받음 정보
        }

        /// <summary>
        /// 일일 퀘스트 진행도 세팅
        /// </summary>
        public void SetDailyQuestProgress(short questType, int conditionValue, int progress)
        {
            var key = new ProgressKey(questType, conditionValue);
            if (!dailyQuestProgressDic.ContainsKey(key))
            {
                dailyQuestProgressDic.Add(key, progress);
            }
            else
            {
                dailyQuestProgressDic[key] = progress;
            }
        }

        #endregion

        #region 의뢰 퀘스트

        /// <summary>
        /// 그룹별 의뢰 퀘스트 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetNormalQuests()
        {
            normalQuestList.Sort(SortByGroup); // 정렬 (보상 순서)
            return normalQuestList.ToArray();
        }

        /// <summary>
        /// 의뢰 퀘스트 진행도 세팅
        /// </summary>
        public void SetNormalQuestProgress(int questID, bool isReceived, int progress)
        {
            foreach (var item in normalQuestList)
            {
                if (item.ID == questID)
                {
                    item.SetReceived(isReceived);
                    item.SetCurrentValue(progress);
                    break;
                }
            }
        }

        /// <summary>
        /// 의뢰 퀘스트 진행 가능
        /// </summary>
        public bool HasRemainNormalQuest()
        {
            for (int i = 0; i < normalQuestList.Count; i++)
            {
                if (normalQuestList[i].CompleteType != QuestInfo.QuestCompleteType.ReceivedReward)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 보상 받기 처리
        /// </summary>
        public void ProcessReceiveReward(QuestInfo info)
        {
            // 보상 받을수 있는 퀘스트 수량 처리
            SetStandByReward(info, isAdd: false);

            QuestCategory questCategory = info.Category;
            QuestType questType = info.QuestType;

            if (questCategory == QuestCategory.Main)
            {
                info.SetReceived(true); // 보상 받음 처리
                QuestInfo curGuideQuest = GetMainQuest();
                curGuideQuest?.SetReceived(true); // 현재 가이드 퀘스트도 보상 받음 처리

                // 특정 메인 퀘스트 점핑
                if (info.Group == Constants.Quest.MAIN_QUEST_JUMP_START_GROUP_ID)
                {
                    SetMainQuest(Constants.Quest.MAIN_QUEST_JUMP_DESTINATION_GROUP_ID);
                }
                else
                {
                    SetMainQuest(info.Group + 1);
                }
            }
            else if (questCategory == QuestCategory.TimePatrol)
            {
                info.SetReceived(true); // 보상 받음 처리
                QuestInfo curQuest = GetTimePatrolQuest();
                curQuest?.SetReceived(true); // 현재 퀘스트도 보상 받음 처리
                SetTimePatrolQuest(info.Group + 1);
            }
            else
            {
                info.SetReceived(true); // 보상 받음 처리
            }

            // ===========================================================================
            // 경고. SetMainQuest 메서드에 의해, 인자로 주어진 info 의 상태가 바뀔 수 있으므로,
            // 만약 바뀌기 이전의 info 의 상태에 대한 참조가 필요하다면 미리 저장해둡시다.
            // ===========================================================================

            if (questCategory == QuestCategory.DailyStart)
                Quest.QuestProgress(QuestType.DAILY_QUEST_CLEAR); // 일일 퀘스트 클리어     

            if (questType == QuestType.DAILY_QUEST_CLEAR)
                Quest.QuestProgress(QuestType.QUESTE_TYPE_CLEAR_COUNT_2); // 일일 퀘스트 모두 클리어 

            Quest.QuestProgress(QuestType.QUESTE_TYPE_CLEAR_COUNT, (int)questCategory); // 특정 퀘스트 완료     

            InvokeStandByRewards();
        }

        /// <summary>
        /// 의뢰 퀘스트 포기
        /// </summary>
        public void ProcessDropNormalQuest(QuestInfo info)
        {
            // 보상 받을수 있는 퀘스트 포기
            SetStandByReward(info, isAdd: false);

            normalQuestList.Remove(info);
            InvokeStandByRewards();
        }

        /// <summary>
        /// 의뢰퀘스트 완료 가능
        /// </summary>
        public bool CanRewardNormalQuest()
        {
            foreach (var item in normalQuestList)
            {
                if (item.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
                    return true;
            }

            return false;
        }

        #endregion

        #region 업적 퀘스트

        /// <summary>
        /// 업적 진행도
        /// </summary>
        public float GetAchievementProgress()
        {
            int completedCount = 0;
            int maxCount = achievementList.Count;

            for (int i = 0; i < achievementList.Count; i++)
            {
                if (achievementList[i].CompleteType == QuestInfo.QuestCompleteType.ReceivedReward)
                    ++completedCount;
            }

            return (float)completedCount / maxCount;
        }

        /// <summary>
        /// 그룹별 업적 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetAchievements()
        {
            return GetQuests(achievementList);
        }

        /// <summary>
        /// 보상 받은 업적 id 추가
        /// </summary>
        public void SetReceivedAchievement(int id)
        {
            for (int i = 0; i < achievementList.Count; i++)
            {
                QuestInfo info = achievementList[i];
                if (info.ID == id)
                {
                    info.SetReceived(true);
                    break;
                }
            }
        }

        /// <summary>
        /// 업적 진행도 세팅
        /// </summary>
        public void SetAchievementProgress(short questType, int conditionValue, int progress)
        {
            var key = new ProgressKey(questType, conditionValue);
            if (!achievementProgressDic.ContainsKey(key))
            {
                achievementProgressDic.Add(key, progress);
            }
            else
            {
                achievementProgressDic[key] = progress;
            }
        }

        #endregion

        #region 길드 퀘스트

        public void SetHaveGuild(bool hasGuild)
        {
            this.hasGuild = hasGuild;
            if (!hasGuild)
            {
                // 길드 탈퇴, 강퇴시 길드퀘스트 카운트 초기화
                foreach (var item in guildQuestList)
                {
                    item.SetCurrentValue(0);
                }
            }
        }

        /// <summary>
        /// 길드 퀘스트 반환
        /// </summary>
        public QuestInfo[] GetGuildQuests()
        {
            return GetQuests(guildQuestList);
        }

        /// <summary>
        /// 완료된 일일 길드 퀘스트 수 반환
        /// </summary>
        /// <returns></returns>
        public int GetGuildQuestRewardCount()
        {
            return guildQuestList.Count(e => e.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward);
        }

        /// <summary>
        /// 길드 퀘스트 추가
        /// </summary>
        public void AddGuildQuest(int quest_id)
        {
            QuestData data = questDataRepo.Get(quest_id);

            if (data == null)
                return;

            QuestInfo info = new QuestInfo();
            info.SetData(data);

            guildQuestList.Add(info); // 퀘스트 추가
        }

        /// <summary>
        /// 길드 퀘스트 진행도 세팅
        /// </summary>
        public void SetGuildQuestProgress(short questType, int conditionValue, int progress)
        {
            var key = new ProgressKey(questType, conditionValue);
            if (!guildQuestProgressDic.ContainsKey(key))
            {
                guildQuestProgressDic.Add(key, progress);
            }
            else
            {
                guildQuestProgressDic[key] = progress;
            }
        }

        /// <summary>
        /// 길드 퀘스트 보상 수령 여부 세팅
        /// </summary>
        public void SetGuildQuestReceived(short questType, int conditionValue, bool received)
        {
            QuestType thisQuestType = questType.ToEnum<QuestType>();
            QuestInfo questInfo = guildQuestList.Find(e => e.QuestType == thisQuestType && e.ConditionValue == conditionValue);

            if (questInfo == null)
                return;

            questInfo.SetReceived(received);
        }

        /// <summary>
        /// 보상 받을 수 있는 길드 퀘스트 있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool HasGuildQuestReward()
        {
            if (!hasGuild)
                return false;

            if (GetGuildQuestRewardCount() >= GuildQuestRewardMaxCount)
                return false;

            return standByRewardDic[QuestCategory.Guild] > 0;
        }

        #endregion

        #region 이벤트 퀘스트

        /// <summary>
        /// 보상 받을수 있는 이벤트 퀘스트가 있는지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsEventQuestStandByReward()
        {
            return standByRewardDic[QuestCategory.Event] > 0;
        }

        /// <summary>
        /// 보상 받을수 있는 이벤트 퀘스트가 있는지 여부
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public bool IsEventQuestStandByReward(int groupId)
        {
            if (!IsEventQuestStandByReward())
                return false;

            return eventQuestList.
                Count(x => x.GroupId == groupId
                && x.RemainTime.ToRemainTime() > 0
                && x.CompleteType == QuestInfo.QuestCompleteType.StandByReward) > 0;
        }

        /// <summary>
        /// 이벤트 퀘스트 그룹 반환
        /// </summary>
        /// <returns></returns>
        public EventQuestGroupInfo[] GetEventQuestGroupInfos()
        {
            return eventQuestGroupInfoList.Where(x => x.RemainTime.ToRemainTime() > 0).ToArray();
        }

        public EventQuestGroupInfo GetEventQuestByShortCut(ShortCutType shortCutType)
        {
            return eventQuestGroupInfoList.Find(x => x.ShortcutType == shortCutType);
        }

        /// <summary>
        /// 이벤트 퀘스트 반환
        /// </summary>
        public QuestInfo[] GetEventQuests(int groupId, bool isSort = true)
        {
            var result = eventQuestList.FindAll(x => x.GroupId == groupId && x.RemainTime.ToRemainTime() > 0);
            return isSort ? GetQuests(result) : result.ToArray();
        }

        /// <summary>
        /// 종료된 이벤트 퀘스트 수
        /// </summary>
        /// <returns></returns>
        public int GetEndEventQuestCount()
        {
            return eventQuestGroupInfoList.Count(x => x.RemainTime <= 0);
        }

        /// <summary>
        /// 이벤트 퀘스트 리셋
        /// </summary>
        public void ResetEventQuest()
        {
            eventQuestGroupInfoList.Clear();
            eventQuestList.Clear();
        }

        /// <summary>
        /// 이벤트 퀘스트 그룹 추가 
        /// </summary>
        public void AddEventQuestGroup(EventQuestGroupPacket packet)
        {
            EventQuestGroupInfo info = new EventQuestGroupInfo();
            string imageUrl = connectionManager.GetResourceUrl(packet.imageName);
            info.Initialize(packet, imageUrl);
            info.Set(this);
            eventQuestGroupInfoList.Add(info);
        }

        /// <summary>
        /// 이벤트 퀘스트 추가
        /// </summary>       
        public void AddEventQuest(int event_no, long end_dt, int questID, int progress, bool isReceived, int eventGroupdId, byte initType)
        {
            QuestData data = questDataRepo.Get(questID);

            if (data == null)
            {
                Debug.LogError("퀘스트 추가 불가");
                return;
            }

            // 퀘스트 추가
            QuestInfo info = new QuestInfo();
            info.SetData(data);
            info.SetEventNo(event_no);
            info.SetReceived(isReceived);
            info.SetEventGroupId(eventGroupdId);
            info.SetInitType(initType);
            info.SetRemainTime(end_dt);
            info.SetCurrentValue(progress); // 진행도 세팅
            SetStandByReward(info, isAdd: true);
            eventQuestList.Add(info);
        }

        #endregion

        #region 타임패트롤 퀘스트

        /// <summary>
        /// 진행중인 타임패트롤 퀘스트 정보
        public QuestInfo GetTimePatrolQuest()
        {
            return timePatrolQuest;
        }

        /// <summary>
        /// 진행중인 타임패트롤 퀘스트 세팅
        /// </summary>
        public void SetTimePatrolQuest(int timePatrolQuestSeq)
        {
            QuestData data = questDataRepo.GetTimePatrolQuest(timePatrolQuestSeq);

            if (data == null)
            {
                timePatrolQuest.SetData(null);
                return;
            }

            timePatrolQuest.SetData(data);
            timePatrolQuest.SetReceived(false);
            timePatrolQuest.SetWait(false);
            timePatrolQuest.SetCurrentValue(0);
        }

        /// <summary>
        /// 타임패트롤 퀘스트 
        /// </summary>
        public void SetTimePatrolQuestProgress(int questID, bool isReceived, int progress)
        {
            QuestData data = questDataRepo.Get(questID);
            if (data == null)
                return;

            if (timePatrolQuest.ID != questID)
                return;

            timePatrolQuest.SetReceived(isReceived);
            timePatrolQuest.SetCurrentValue(progress);
        }
        #endregion

        #region 패스 퀘스트

        /// <summary>
        /// 일일 퀘스트 초기화
        /// </summary>
        public void ResetPassDaily()
        {
            passDailyQuestList.Clear();
            passDailyProgressDic.Clear();
            standByRewardDic[QuestCategory.PassDaily] = 0;

            InvokeStandByRewards();
        }

        /// <summary>
        /// 그룹별 패스 일일 퀘스트 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetPassDailyQuests()
        {
            passDailyQuestList.Sort(SortByGroup); // 정렬 (보상 순서)
            return passDailyQuestList.ToArray();
        }

        /// <summary>
        /// 그룹별 패스 시즌 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetPassSeasonQuests()
        {
            passSeasonQuestList.Sort(SortByGroup); // 정렬 (보상 순서)
            return passSeasonQuestList.ToArray();
        }

        /// <summary>
        /// 패스 일일 퀘스트 추가
        /// </summary>
        public void AddPassDailyQuest(int id, bool isReceived)
        {
            QuestData data = questDataRepo.Get(id);

            if (data == null)
            {
                Debug.LogError("퀘스트 추가 불가");
                return;
            }

            QuestInfo info = new QuestInfo();
            info.SetData(data);
            passDailyQuestList.Add(info); // 퀘스트 추가

            info.SetReceived(isReceived); // 보상 받음 정보
        }

        /// <summary>
        /// 패스 일일 퀘스트 진행도 세팅
        /// </summary>
        public void SetPassDailyQuestProgress(short questType, int conditionValue, int progress)
        {
            var key = new ProgressKey(questType, conditionValue);
            if (!dailyQuestProgressDic.ContainsKey(key))
            {
                passDailyProgressDic.Add(key, progress);
            }
            else
            {
                passDailyProgressDic[key] = progress;
            }
        }

        /// <summary>
        /// 패스 퀘스트 진행도 세팅
        /// </summary>
        public void SetPassSeaonQuestProgress(short questType, int conditionValue, int progress)
        {
            var key = new ProgressKey(questType, conditionValue);
            if (!passSeasonProgressDic.ContainsKey(key))
            {
                passSeasonProgressDic.Add(key, progress);
            }
            else
            {
                passSeasonProgressDic[key] = progress;
            }
        }

        /// <summary>
        /// 패스 퀘스트 보상 수령 여부 세팅
        /// </summary>
        public void SetPassSeasonQuestReceived(short questType, int conditionValue, bool received)
        {
            QuestType thisQuestType = questType.ToEnum<QuestType>();
            QuestInfo questInfo = passSeasonQuestList.Find(e => e.QuestType == thisQuestType && e.ConditionValue == conditionValue);

            if (questInfo == null)
                return;

            questInfo.SetReceived(received);
        }

        /// <summary>
        /// 보상 받을수 있는 패스 퀘스트가 있는지 여부
        /// </summary>
        public bool IsPassQuestStandByReward()
        {
            return standByRewardDic[QuestCategory.PassDaily] > 0 || standByRewardDic[QuestCategory.PassSeason] > 0;
        }

        #endregion

        #region 온버프패스 퀘스트

        /// <summary>
        /// 온버프패스 일일 퀘스트 초기화
        /// </summary>
        public void ResetOnBuffPassDaily()
        {
            onBuffPassDailyQuestList.Clear();
            onBuffPassDailyProgressDic.Clear();
            standByRewardDic[QuestCategory.OnBuffPassDaily] = 0;

            InvokeStandByRewards();
        }

        /// <summary>
        /// 그룹별 온버프패스 일일 퀘스트 반환 (전체 정보가 아닌, UI에 보일 그룹별 정보)
        /// </summary>
        public QuestInfo[] GetOnBuffPassDailyQuests()
        {
            onBuffPassDailyQuestList.Sort(SortByGroup); // 정렬 (보상 순서)
            return onBuffPassDailyQuestList.ToArray();
        }

        /// <summary>
        /// 온버프패스 일일 퀘스트 추가
        /// </summary>
        public void AddOnBuffPassDailyQuest(int id, bool isReceived)
        {
            QuestData data = questDataRepo.Get(id);

            if (data == null)
            {
                Debug.LogError("퀘스트 추가 불가");
                return;
            }

            QuestInfo info = new QuestInfo();
            info.SetData(data);
            onBuffPassDailyQuestList.Add(info); // 퀘스트 추가

            info.SetReceived(isReceived); // 보상 받음 정보
        }

        /// <summary>
        /// 온버프패스 일일 퀘스트 진행도 세팅
        /// </summary>
        public void SetOnBuffPassDailyQuestProgress(short questType, int conditionValue, int progress)
        {
            var key = new ProgressKey(questType, conditionValue);
            if (!dailyQuestProgressDic.ContainsKey(key))
            {
                onBuffPassDailyProgressDic.Add(key, progress);
            }
            else
            {
                onBuffPassDailyProgressDic[key] = progress;
            }
        }

        /// <summary>
        /// 보상 받을수 있는 온버프패스 퀘스트가 있는지 여부
        /// </summary>
        public bool IsOnBuffPassQuestStandByReward()
        {
            return standByRewardDic[QuestCategory.OnBuffPassDaily] > 0;
        }

        #endregion

        /// <summary>
        /// 현재 적용중인 데이터 반환
        /// </summary>
        private QuestInfo[] GetQuests(List<QuestInfo> questList)
        {
            Dictionary<int, List<QuestInfo>> groupQuestDic = new Dictionary<int, List<QuestInfo>>(IntEqualityComparer.Default); // 그룹 정렬
            for (int i = 0; i < questList.Count; i++)
            {
                QuestInfo info = questList[i];

                int group = info.Group;
                if (!groupQuestDic.ContainsKey(group))
                    groupQuestDic.Add(group, new List<QuestInfo>());

                groupQuestDic[group].Add(info);
            }

            List<QuestInfo> list = new List<QuestInfo>();
            foreach (var item in groupQuestDic.Values)
            {
                item.Sort(SortByLowMaxValue); // 완료 조건이 낮은 것으로 정렬              
                int index = item.FindIndex(FindByInProgress); // 진행중인 퀘스트 index

                if (index == -1)
                    index = item.Count - 1;

                list.Add(item[index]);
            }

            list.Sort(SortByGroup);
            return list.ToArray();
        }

        /// <summary>
        /// 진행중인 퀘스트
        /// </summary>
        private bool FindByInProgress(QuestInfo info)
        {
            return info.CompleteType != QuestInfo.QuestCompleteType.ReceivedReward;
        }

        /// <summary>
        /// 완료 조건이 낮은 것으로 정렬
        /// </summary>
        private int SortByLowMaxValue(QuestInfo x, QuestInfo y)
        {
            return x.MaxValue.CompareTo(y.MaxValue);
        }

        /// <summary>
        /// 퀘스트 정렬
        /// </summary>
        private int SortByGroup(QuestInfo x, QuestInfo y)
        {
            int result1 = x.CompleteType.CompareTo(y.CompleteType); // completeType
            int result2 = result1 == 0 ? x.Group.CompareTo(y.Group) : result1; // group
            return result2;
        }

        /// <summary>
        /// 퀘스트 진행 시 호출
        /// </summary>
        private void OnProgress(QuestType type, int conditionValue, int questValue)
        {
            // 메인 퀘스트            
            QuestProgress(mainQuest, type, conditionValue, questValue);
            foreach (var item in mainQuestDic.Values)
            {
                QuestProgress(item, type, conditionValue, questValue);
            }

            // 일일 퀘스트
            QuestProgress(dailyTotalClearQuest, type, conditionValue, questValue);
            for (int i = 0; i < dailyQuestList.Count; i++)
            {
                QuestProgress(dailyQuestList[i], type, conditionValue, questValue);
            }

            // 업적
            for (int i = 0; i < achievementList.Count; i++)
            {
                QuestProgress(achievementList[i], type, conditionValue, questValue);
            }

            // 의뢰 퀘스트
            for (int i = 0; i < normalQuestList.Count; i++)
            {
                QuestProgress(normalQuestList[i], type, conditionValue, questValue);
            }

            // 이벤트 퀘스트
            for (int i = 0; i < eventQuestList.Count; i++)
            {
                QuestProgress(eventQuestList[i], type, conditionValue, questValue);
            }

            if (hasGuild)
            {
                // 길드 퀘스트
                for (int i = 0; i < guildQuestList.Count; ++i)
                {
                    QuestProgress(guildQuestList[i], type, conditionValue, questValue);
                }
            }

            // 타임패트롤 퀘스트
            QuestProgress(timePatrolQuest, type, conditionValue, questValue);

            // 패스 일일 퀘스트
            for (int i = 0; i < passDailyQuestList.Count; i++)
            {
                QuestProgress(passDailyQuestList[i], type, conditionValue, questValue);
            }

            // 패스 시즌 퀘스트
            for (int i = 0; i < passSeasonQuestList.Count; i++)
            {
                QuestProgress(passSeasonQuestList[i], type, conditionValue, questValue);
            }

            // 온버프패스 시즌 퀘스트
            for (int i = 0; i < onBuffPassDailyQuestList.Count; i++)
            {
                QuestProgress(onBuffPassDailyQuestList[i], type, conditionValue, questValue);
            }
        }

        /// <summary>
        /// 진행도 증가
        /// </summary>
        private void QuestProgress(QuestInfo info, QuestType type, int conditionValue, int questValue)
        {
            // 유효하지 않은 데이터일 경우
            if (info.IsInvalidData)
                return;

            // 진행중인 퀘스트가 아닐 경우
            if (info.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward ||
                info.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
                return;

            // 퀘스트 타입이 동일하지 않음
            if (info.QuestType != type)
                return;

            // 퀘스트 조건이 동일하지 않음
            if (info.ConditionValue != conditionValue)
                return;

            if (type.IsMaxCondition())
            {
                info.MaxCurrentValue(questValue);
            }
            else
            {
                info.PlusCurrentValue(questValue);
            }

            if (SetStandByReward(info, true))
            {
                InvokeStandByRewards();
            }

            if (info.Category == QuestCategory.Main && info.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
            {
                // 장비 강화퀘스트 완료 시에는 모든 UI 닫음
                // 특정 제련도 도달 퀘스트 완료 시에는 모든 UI 닫음
                if (info.QuestType == QuestType.ITEM_UPGRADE || info.QuestType == QuestType.ITEM_LEVEL_UPGRADE)
                    UIManager.Instance.ShortCut();

                UI.ShowToastPopup(LocalizeKey._90085.ToText()); // 퀘스트를 클리어하였습니다.
            }

            //Debug.Log($"[퀘스트 진행도 증가] {nameof(info.ID)} = {info.ID}, {nameof(info.CurrentValue)} = {info.CurrentValue}, {nameof(info.MaxValue)} = {info.MaxValue}");
        }

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수량 변경
        /// </summary>
        /// <param name="info"></param>
        /// <param name="isAdd"></param>
        private bool SetStandByReward(QuestInfo info, bool isAdd)
        {
            if (info.CompleteType == QuestInfo.QuestCompleteType.StandByReward)
            {
                if (isAdd)
                {
                    standByRewardDic[info.Category]++;
                }
                else
                {
                    standByRewardDic[info.Category]--;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 보상 받을수 있는 퀘스트 수
        /// </summary>
        public Dictionary<QuestCategory, int> GetStandByRewards()
        {
            return standByRewardDic;
        }

        /// <summary>
        /// 보상 받을수 있는 퀘스트가 있는지 여부(일일, 업적, 길드)
        /// </summary>
        public bool IsStandByReward()
        {
            bool isStandByReward = standByRewardDic.Where(x =>
            x.Key == QuestCategory.DailyStart ||
            x.Key == QuestCategory.Achieve).Sum(x => x.Value) > 0;

            return isStandByReward || HasGuildQuestReward();
        }

        public void InvokeStandByRewards()
        {
            OnStandByRewards?.Invoke();
        }

        bool IEqualityComparer<ProgressKey>.Equals(ProgressKey x, ProgressKey y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ProgressKey>.GetHashCode(ProgressKey obj)
        {
            return obj.GetHashCode();
        }

        bool IEqualityComparer<QuestCategory>.Equals(QuestCategory x, QuestCategory y)
        {
            return x == y;
        }

        int IEqualityComparer<QuestCategory>.GetHashCode(QuestCategory obj)
        {
            return (int)obj;
        }
    }
}