using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIAdventureMap"/>
    /// </summary>
    public sealed class AdventureMapPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;
        private readonly QuestModel questModel;
        private readonly CharacterModel characterModel;
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly AdventureDataManager adventureDataRepo;
        private readonly StageDataManager stageDataRepo;
        private readonly MonsterDataManager monsterDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly BoxDataManager boxDataRepo;
        private readonly Dictionary<int, ChapterElement[]> chapterDic;
        private readonly Dictionary<int, StageElement[]> stageDic;
        public readonly int maxChapter;
        public readonly int eventStageMaxLevel;
        public readonly int eventStageFreeEnterCount;
        public readonly int eventStageClearCountLimit;
        public readonly int eventStageTicketItemId;
        public readonly string eventTicketItemIcon;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action OnUpdateEventStageInfo
        {
            add => dungeonModel.OnUpdateEventStageInfo += value;
            remove => dungeonModel.OnUpdateEventStageInfo -= value;
        }

        public event System.Action OnUpdateEventStageCount
        {
            add => dungeonModel.OnUpdateEventStageCount += value;
            remove => dungeonModel.OnUpdateEventStageCount -= value;
        }

        public event System.Action OnUpdateClearedStage;

        private bool isStartBattle;
        private bool isEventMode;

        public AdventureMapPresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            questModel = Entity.player.Quest;
            characterModel = Entity.player.Character;
            inventoryModel = Entity.player.Inventory;

            adventureDataRepo = AdventureDataManager.Instance;
            stageDataRepo = StageDataManager.Instance;
            monsterDataRepo = MonsterDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            boxDataRepo = BoxDataManager.Instance;
            chapterDic = new Dictionary<int, ChapterElement[]>(IntEqualityComparer.Default);
            stageDic = new Dictionary<int, StageElement[]>(IntEqualityComparer.Default);

            battleManager = BattleManager.Instance;

            int serverIndex = ConnectionManager.Instance.GetSelectServerGroupId();
            int maxStageId = BasisType.ENTERABLE_MAXIMUM_STAGE_BY_SERVER.GetInt(serverIndex);
            maxChapter = GetChapter(maxStageId);
            eventStageMaxLevel = BasisType.HARD_CHELLENGE_MAX_LEVEL.GetInt();
            eventStageFreeEnterCount = BasisType.CHELLENGE_FREE_ENTER_COUNT.GetInt();
            eventStageClearCountLimit = BasisType.CHELLENGE_CLEAR_COUNT_LIMIT.GetInt();
            eventStageTicketItemId = BasisItem.EventStageTicket.GetID();
            eventTicketItemIcon = GetItemIcon(eventStageTicketItemId);
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
            dungeonModel.OnUpdateClearedStage += InvokeClearedStage;
            characterModel.OnUpdateProfile += OnUpdateProfile;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
            dungeonModel.OnUpdateClearedStage -= InvokeClearedStage;
            characterModel.OnUpdateProfile -= OnUpdateProfile;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (!isStartBattle)
                return;

            UI.Close<UIAdventureMap>();
        }

        void InvokeClearedStage()
        {
            RefreshChapter();
            RefreshStage();
            OnUpdateClearedStage?.Invoke();
        }

        void OnUpdateProfile()
        {
            string thumbnailName = characterModel.GetThumbnailName();
            foreach (var values in stageDic.Values)
            {
                foreach (var item in values)
                {
                    item.SetPlayerThumbnail(thumbnailName);
                }
            }
        }

        /// <summary>
        /// 챕터 정보 반환
        /// </summary>
        public UIChapterElement.IInput[] GetChapterData(int chapter)
        {
            int adventureGroup = GetAdventureGroup(chapter);
            if (!chapterDic.ContainsKey(adventureGroup))
            {
                AdventureData[] arrData = adventureDataRepo.GetChapters(adventureGroup);
                int length = arrData == null ? 0 : arrData.Length;
                ChapterElement[] chapters = new ChapterElement[length];
                for (int i = 0; i < length; i++)
                {
                    chapters[i] = new ChapterElement(arrData[i], maxChapter);
                }
                chapterDic.Add(adventureGroup, chapters);
                RefreshChapter(adventureGroup);
            }

            return chapterDic[adventureGroup];
        }

        /// <summary>
        /// 마지막 입장한 챕터
        /// </summary>
        public int GetCurrentChapter()
        {
            return GetChapter(dungeonModel.LastEnterStageId);
        }

        /// <summary>
        /// Stgae 에 해당하는 Chapter 반환 (챕터는 최소 1 이상)
        /// </summary>
        public int GetChapter(int stageId)
        {
            StageData data = stageDataRepo.Get(stageId);
            if (data == null)
                return 1;

            return data.chapter;
        }

        /// <summary>
        /// 이벤트 모드 여부
        /// </summary>
        public bool GetCurrentEventMode()
        {
            return dungeonModel.StageMode == StageMode.Event || dungeonModel.StageMode == StageMode.Challenge;
        }

        /// <summary>
        /// 챕터에 해당하는 모험 Group
        /// </summary>
        public int GetAdventureGroup(int chapter)
        {
            AdventureData data = adventureDataRepo.GetChapterData(chapter);
            if (data == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"해당 Chapter에 해당하는 AdventureData가 존재하지 않음: {nameof(chapter)} = {chapter}");
#endif
                return 0;
            }

            return data.scenario_id;
        }

        /// <summary>
        /// 모험 Group 에 해당하는 첫번째 Chapter
        /// </summary>
        public int GetFirstChapter(int adventureGroup)
        {
            return adventureDataRepo.GetFirstChapter(adventureGroup);
        }

        /// <summary>
        /// 모험 Group 에 해당하는 마지막 Chapter
        /// </summary>
        public int GetLastChapter(int adventureGroup)
        {
            return adventureDataRepo.GetLastChapter(adventureGroup);
        }

        /// <summary>
        /// 모험 Group 에 위치할 Chapter 반환
        /// </summary>
        public int GetDisplayChapter(int adventureGroup)
        {
            int currentChapter = GetCurrentChapter();
            int lastGroup = GetAdventureGroup(currentChapter);
            // 현재 진행중인 Group 일 경우에는 진행중인 Chapter 반환
            if (lastGroup == adventureGroup)
                return currentChapter;

            // 가장 많이 간 Chapter 반환
            int clearedChapter = GetChapter(dungeonModel.FinalStageId);
            int firstChapter = GetFirstChapter(adventureGroup);
            int lastChapter = GetLastChapter(adventureGroup);
            return MathUtils.Clamp(clearedChapter, firstChapter, lastChapter);
        }

        /// <summary>
        /// 챕터에 해당하는 스테이지 정보 반환
        /// </summary>
        public UIStageElement.IInput[] GetStageData(int chapter)
        {
            if (!stageDic.ContainsKey(chapter))
            {
                AdventureData[] arrData = adventureDataRepo.GetStages(chapter);
                int length = arrData == null ? 0 : arrData.Length;
                StageElement[] stages = new StageElement[length];
                for (int i = 0; i < length; i++)
                {
                    stages[i] = new StageElement(arrData[i], stageDataRepo, monsterDataRepo);
                }
                stageDic.Add(chapter, stages);
                RefreshStage(chapter);
            }

            return stageDic[chapter];
        }

        /// <summary>
        /// 챕터에 해당하는 알림표시 여부
        /// </summary>
        public bool HasNotice(int chapter)
        {
            // 이벤트스테이지 중이 아님
            if (!dungeonModel.IsOpendEventStage())
                return false;

            // 보스 오픈컨텐츠 조건 체크
            if (!questModel.IsOpenContent(ContentType.Boss, isShowPopup: false))
                return false;

            StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Challenge, chapter);
            if (find == null)
                return false;

            // 입장가능 챕터 확인
            if (find.challenge_return_stage > dungeonModel.FinalStageId)
                return false;

            // 최대 레벨 도달 체크
            if (GetEventLevel(find.id) > eventStageMaxLevel)
                return false;

            return GetChallengeClearCount(find.id) < eventStageFreeEnterCount;
        }

        /// <summary>
        /// 이벤트스테이지 남은 시간 반환
        /// </summary>
        public RemainTime GetEventStageRemainTime()
        {
            return dungeonModel.EventStageRemainTime;
        }

        /// <summary>
        /// 챕터 선택
        /// </summary>
        public void SelectChapter(int chapter)
        {
            foreach (var values in chapterDic.Values)
            {
                foreach (var item in values)
                {
                    item.SetSelect(item.Chapter == chapter);
                }
            }
        }

        /// <summary>
        /// 이벤트 모드 선택
        /// </summary>
        public void SelectEventMode(bool isEvent)
        {
            isEventMode = isEvent;

            int cleardStageId = dungeonModel.FinalStageId;
            int currentStageId = GetCurrentStageId();
            int eventStageLevel;
            foreach (var values in stageDic.Values)
            {
                foreach (var item in values)
                {
                    eventStageLevel = dungeonModel.GetEventStageLevel(item.StageId);

                    item.SetComplete(isEventMode ? eventStageLevel > eventStageMaxLevel : item.StageId < cleardStageId);
                    item.SetPlayerOnHere(item.StageId == currentStageId);
                    item.SetEventMode(isEventMode);
                }
            }

            // 이벤트 스테이지 정보 호출
            if (isEventMode)
            {
                dungeonModel.RequestEventStageInfo().WrapNetworkErrors(); // 이벤트스테이지 정보 호출
            }
        }

        /// <summary>
        /// 스테이지 선택 가능 여부
        /// </summary>
        public bool IsCheckSelectStage(int stageId)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                UI.ShowToastPopup(LocalizeKey._90231.ToText()); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                return false;
            }

            // 스테이지 모드에서 같은 곳으로 이동
            if (stageId == GetCurrentStageId())
            {
                UI.ShowToastPopup(LocalizeKey._48237.ToText()); // 현재 진행중인 던전입니다.
                return false;
            }

            if (isEventMode)
            {
                // 이벤트 스테이지 진행중이 아닐 경우
                if (!dungeonModel.IsOpendEventStage())
                {
                    UI.ShowToastPopup(LocalizeKey._48223.ToText()); // 이벤트 기간이 아닙니다.
                    return false;
                }

                // 보스 오픈컨텐츠 조건 체크
                if (!questModel.IsOpenContent(ContentType.Boss, isShowPopup: true))
                    return false;

                // 최대 레벨 도달 체크
                int stageLevel = dungeonModel.GetEventStageLevel(stageId);
                if (stageLevel > eventStageMaxLevel)
                {
                    UI.ShowToastPopup(LocalizeKey._48222.ToText()); // 최대 레벨에 도달하였습니다.
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 챌린지 스테이지 선택 가능 여부
        /// </summary>
        public bool IsCheckSelectChallenge(int chapter)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                UI.ShowToastPopup(LocalizeKey._90231.ToText()); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                return false;
            }

            StageData stageData = stageDataRepo.FindWithChapter(StageChallengeType.Challenge, chapter);
            if (stageData == null)
                return false;

            int stageId = stageData.id;

            // 스테이지 모드에서 같은 곳으로 이동
            if (stageId == GetCurrentStageId())
            {
                UI.ShowToastPopup(LocalizeKey._48237.ToText()); // 현재 진행중인 던전입니다.
                return false;
            }

            // 이벤트 스테이지 진행중이 아닐 경우
            if (!dungeonModel.IsOpendEventStage())
            {
                UI.ShowToastPopup(LocalizeKey._48223.ToText()); // 이벤트 기간이 아닙니다.
                return false;
            }

            // 보스 오픈컨텐츠 조건 체크
            if (!questModel.IsOpenContent(ContentType.Boss, isShowPopup: true))
                return false;

            // 입장가능 챕터 확인
            if (stageData.challenge_return_stage > dungeonModel.FinalStageId)
            {
                int checkStageId = stageData.challenge_return_stage - 1;
                StageData checkStageData = stageDataRepo.Get(checkStageId);
                if (checkStageData == null)
                {
                    UI.ShowToastPopup(LocalizeKey._90227.ToText()); // 전 필드의 보스를 클리어해 주세요.
                    return false;
                }

                string message = LocalizeKey._48206.ToText() // 시나리오 {NAME} 던전 클리어 후에 이용 가능합니다.
                    .Replace(ReplaceKey.NAME, checkStageData.name_id.ToText());
                UI.ShowToastPopup(message);
                return false;
            }

            // 최대 레벨 도달 체크
            int stageLevel = dungeonModel.GetEventStageLevel(stageId);
            if (stageLevel > eventStageMaxLevel)
            {
                UI.ShowToastPopup(LocalizeKey._48222.ToText()); // 최대 레벨에 도달하였습니다.
                return false;
            }

            return true;
        }

        /// <summary>
        /// 스테이지에 해당하는 챌린지 클리어 횟수
        /// </summary>
        public int GetChallengeClearCount(int stageId)
        {
            return dungeonModel.GetChallengeClearCount(stageId);
        }

        /// <summary>
        /// 해당 이벤트스테이지 레벨
        /// </summary>
        public int GetEventLevel(int stageId)
        {
            return dungeonModel.GetEventStageLevel(stageId);
        }

        /// <summary>
        /// 해당 스테이지에서 획득 가능한 이벤트 포인트
        /// </summary>
        public int GetEventPoint(int stageId)
        {
            StageData data = stageDataRepo.Get(stageId);
            int level = GetEventLevel(stageId);
            return data.challenge_rank_value + ((level - 1) * data.challenge_rank_increase_value);
        }

        /// <summary>
        /// 해당 챕터에 해당하는 StageId 반환
        /// </summary>
        public int GetChallengeStageId(int chapter)
        {
            StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Challenge, chapter);
            if (find == null)
                return 0;

            return find.id;
        }

        /// <summary>
        /// 이벤트 보상
        /// </summary>
        public RewardData GetEventReward(int stageId)
        {
            StageData stageData = stageDataRepo.Get(stageId);
            int bossDrop = stageData.challenge_boss_drop;
            if (bossDrop == 0)
                return null;

            ItemData itemData = itemDataRepo.Get(bossDrop);
            if (itemData == null)
                return null;

            if (itemData.ItemType != ItemType.Box)
                return null;

            BoxData boxData = boxDataRepo.Get(itemData.event_id);
            if (boxData == null)
                return null;

            BoxType boxType = boxData.box_type.ToEnum<BoxType>();
            if (boxType != BoxType.DirectOpen)
                return null;

            foreach (var item in boxData.rewards)
            {
                if (item == null)
                    continue;

                if (item.RewardType == RewardType.CatCoin || item.RewardType == RewardType.CatCoinFree || item.RewardType == RewardType.ROPoint)
                {
                    RewardData eventReward = new RewardData(item.RewardType, item.RewardValue, item.RewardCount);
                    eventReward.SetIsEvent(true); // 이벤트 보상 처리
                    return eventReward;
                }
            }

            return null;
        }

        /// <summary>
        /// 이벤트스테이지 입장티켓개수
        /// </summary>
        public int GetEventTicketItemCount()
        {
            return inventoryModel.GetItemCount(eventStageTicketItemId);
        }

        /// <summary>
        /// 스테이지 시작
        /// </summary>
        public void StartStage(int stageId)
        {
            // 이벤트 기간 중복 체크 (팝업 후에 이벤트 기간이 지날 수 있다)
            if (isEventMode && !dungeonModel.IsOpendEventStage())
            {
                UI.ShowToastPopup(LocalizeKey._48223.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }

            dungeonModel.StartBattleStageMode(isEventMode ? StageMode.Event : StageMode.Normal, stageId);
            isStartBattle = true;
        }

        /// <summary>
        /// 챌린지 시작
        /// </summary>
        public void StartChallenge(int chapter)
        {
            // 이벤트 기간 중복 체크 (팝업 후에 이벤트 기간이 지날 수 있다)
            if (!dungeonModel.IsOpendEventStage())
            {
                UI.ShowToastPopup(LocalizeKey._48223.ToText()); // 이벤트 기간이 아닙니다.
                return;
            }

            int stageId = GetChallengeStageId(chapter);
            dungeonModel.StartBattleStageMode(StageMode.Challenge, stageId);
            isStartBattle = true;
        }

        /// <summary>
        /// 챕터 정보 업데이트
        /// </summary>
        private void RefreshChapter()
        {
            foreach (var item in chapterDic)
            {
                RefreshChapter(item.Key);
            }
        }

        /// <summary>
        /// 챕터 정보 업데이트
        /// </summary>
        private void RefreshChapter(int adventureGroup)
        {
            int clearedChapter = GetChapter(dungeonModel.FinalStageId);
            foreach (var item in chapterDic[adventureGroup])
            {
                item.SetOpen(item.Chapter <= clearedChapter);
                item.SetNotice(HasNotice(item.Chapter));
            }
        }

        /// <summary>
        /// 스테이지 정보 업데이트
        /// </summary>
        private void RefreshStage()
        {
            foreach (var item in stageDic)
            {
                RefreshStage(item.Key);
            }
        }

        /// <summary>
        /// 스테이지 정보 업데이트
        /// </summary>
        private void RefreshStage(int chapter)
        {
            int currentGuideQuestStageId = GetCurrentGuideQuestStageId();
            int cleardStageId = dungeonModel.FinalStageId;
            int currentStageId = GetCurrentStageId();
            string thumbnailName = characterModel.GetThumbnailName();
            int eventStageLevel;
            foreach (var item in stageDic[chapter])
            {
                eventStageLevel = dungeonModel.GetEventStageLevel(item.StageId);

                item.SetCurrentGuideQuest(item.StageId == currentGuideQuestStageId);
                item.SetOpen(item.StageId <= cleardStageId);
                item.SetComplete(isEventMode ? eventStageLevel > eventStageMaxLevel : item.StageId < cleardStageId);
                item.SetPlayerOnHere(item.StageId == currentStageId);
                item.SetPlayerThumbnail(thumbnailName);
                item.SetEventMode(isEventMode);
                item.SetEventStageLevel(Mathf.Min(eventStageMaxLevel, eventStageLevel));
            }
        }

        /// <summary>
        /// 현재 가이드퀘스트에 해당하는 StageId
        /// </summary>
        private int GetCurrentGuideQuestStageId()
        {
            QuestInfo guideQuest = questModel.GetMaintQuest();
            if (guideQuest == null || guideQuest.IsInvalidData)
                return 0;

            // 시나리오 타입이 아니거나 진행중이 아닐 경우에는 없는 것으로 간주
            if (guideQuest.ShortCutType != ShortCutType.AdventureScenario || guideQuest.CompleteType != QuestInfo.QuestCompleteType.InProgress)
                return 0;

            return guideQuest.ShortCutValue;
        }

        /// <summary>
        /// 현재 진행중인 스테이지 id
        /// </summary>
        private int GetCurrentStageId()
        {
            // 스테이지 모드가 아닐 경우
            if (battleManager.Mode != BattleMode.Stage)
                return 0;

            // 이벤트 모드가 다를 경우
            if (isEventMode != GetCurrentEventMode())
                return 0;

            return dungeonModel.LastEnterStageId; // 스테이지 이면서
        }

        /// <summary>
        /// 아이템 id에 해당하는 icon 이름 반환
        /// </summary>
        private string GetItemIcon(int itemId)
        {
            ItemData itemData = itemDataRepo.Get(itemId);
            if (itemData == null)
                return string.Empty;

            return itemData.icon_name;
        }

        private class ChapterElement : UIChapterElement.IInput
        {
            public int Chapter { get; }
            public int LocalKey { get; }
            public string IconName { get; }
            public bool IsLock { get; }
            public bool IsSelected { get; private set; }
            public bool IsOpened { get; private set; }
            public bool HasNotice { get; private set; }

            public event System.Action OnUpdateSelect;
            public event System.Action OnUpdateOpen;
            public event System.Action OnUpdateNotice;

            public ChapterElement(AdventureData data, int maxChapter)
            {
                Chapter = data.chapter;
                LocalKey = data.name_id;
                IconName = StringBuilderPool.Get()
                    .Append("Ui_Texture_Chapter_").Append(Chapter)
                    .Release();
                IsLock = Chapter > maxChapter;
            }

            public void SetSelect(bool isSelect)
            {
                if (IsSelected == isSelect)
                    return;

                IsSelected = isSelect;
                OnUpdateSelect?.Invoke();
            }

            public void SetOpen(bool isOpen)
            {
                if (IsOpened == isOpen)
                    return;

                IsOpened = isOpen;
                OnUpdateOpen?.Invoke();
            }

            public void SetNotice(bool hasNotice)
            {
                if (HasNotice == hasNotice)
                    return;

                HasNotice = hasNotice;
                OnUpdateNotice?.Invoke();
            }
        }

        private class StageElement : UIStageElement.IInput
        {
            public int StageId { get; }
            public int LocalKey { get; }
            public string MonsterIconName { get; }
            public int MonsterLocalKey { get; }
            public string MapIconName { get; }
            public RewardData Reward { get; }

            public bool IsCurrentGuideQuest { get; private set; }
            public bool IsOpened { get; private set; }
            public bool IsCompleted { get; private set; }
            public bool IsPlayerOnHere { get; private set; }
            public string PlayerThumbnailName { get; private set; }
            public bool IsEventMode { get; private set; }
            public int EventStageLevel { get; private set; }

            public event System.Action OnUpdateCurrentQuest;
            public event System.Action OnUpdateOpen;
            public event System.Action OnUpdateComplete;
            public event System.Action OnUpdatePlayerOnHere;
            public event System.Action OnUpdatePlayerThumbnail;
            public event System.Action OnUpdateMode;
            public event System.Action OnUpdateEventStageLevel;

            public StageElement(AdventureData data, StageDataManager.IStageDataRepoImpl stageDataRepoImpl, MonsterDataManager.IImpl monsterDataRepoImpl)
            {
                StageId = data.link_value;
                LocalKey = data.name_id;
                MonsterIconName = data.icon_name;
                MapIconName = StringBuilderPool.Get()
                    .Append("Ui_Texture_Field_").Append(data.chapter)
                    .Release();

                StageData stageData = stageDataRepoImpl.Get(StageId);
#if UNITY_EDITOR
                if (stageData == null)
                {
                    Debug.LogError($"스테이지 데이터 에러: {nameof(data.id)} = {data.id}, {nameof(data.link_value)} = {data.link_value}");
                }
#endif
                MonsterData monsterData = monsterDataRepoImpl.Get(stageData.normal_monster_id_1);
#if UNITY_EDITOR
                if (monsterData == null)
                {
                    Debug.LogError($"몬스터 데이터 에러: {nameof(data.id)} = {data.id}, {nameof(stageData.normal_monster_id_1)} = {stageData.normal_monster_id_1}");
                }
#endif

                MonsterLocalKey = monsterData.name_id;
                int dropItemId = stageData.normal_drop_1;
                Reward = dropItemId > 0 ? new RewardData(RewardType.Item, dropItemId, 1) : null;
            }

            public void SetCurrentGuideQuest(bool isCurrentGuideQuest)
            {
                if (IsCurrentGuideQuest == isCurrentGuideQuest)
                    return;

                IsCurrentGuideQuest = isCurrentGuideQuest;
                OnUpdateCurrentQuest?.Invoke();
            }

            public void SetOpen(bool isOpen)
            {
                if (IsOpened == isOpen)
                    return;

                IsOpened = isOpen;
                OnUpdateOpen?.Invoke();
            }

            public void SetComplete(bool isCompleted)
            {
                if (IsCompleted == isCompleted)
                    return;

                IsCompleted = isCompleted;
                OnUpdateComplete?.Invoke();
            }

            public void SetPlayerOnHere(bool isPlayerOnHere)
            {
                if (IsPlayerOnHere == isPlayerOnHere)
                    return;

                IsPlayerOnHere = isPlayerOnHere;
                OnUpdatePlayerOnHere?.Invoke();
            }

            public void SetPlayerThumbnail(string playerThumbnailName)
            {
                if (string.Equals(PlayerThumbnailName, playerThumbnailName))
                    return;

                PlayerThumbnailName = playerThumbnailName;
                OnUpdatePlayerThumbnail?.Invoke();
            }

            public void SetEventMode(bool isEventMode)
            {
                if (IsEventMode == isEventMode)
                    return;

                IsEventMode = isEventMode;
                OnUpdateMode?.Invoke();
            }

            public void SetEventStageLevel(int eventStageLevel)
            {
                if (EventStageLevel == eventStageLevel)
                    return;

                EventStageLevel = eventStageLevel;
                OnUpdateEventStageLevel?.Invoke();
            }
        }
    }
}