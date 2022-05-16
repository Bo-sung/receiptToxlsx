using System.Collections.Generic;

namespace Ragnarok
{
    class MainShortcutPresenter : ViewPresenter
    {
        private readonly QuestModel questModel;
        private readonly GoodsModel goodsModel;
        private readonly UserModel userModel;
        private readonly AlarmModel alarmModel;
        private readonly DungeonModel dungeonModel;
        private readonly BingoModel bingoModel;
        private readonly EventModel eventModel;
        private readonly InventoryModel inventoryModel;
        private readonly CharacterModel characterModel;

        /// <summary>
        /// 룰렛 뽑기 제한 레벨
        /// </summary>
        private int jobLevelLimit;

        /// <summary>
        /// 단어 수집 아이템 목록
        /// </summary>
        private List<int> wordCollectionItems;

        public event System.Action OnUpdateQuestMainRewardState;
        public event System.Action OnUpdateTreeRewardState;
        public event System.Action OnUpdateAlarm;
        public event System.Action OnUpdateQuestNotice;
        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public event System.Action OnUpdateBingoQuestRewardState
        {
            add { bingoModel.OnMissionStateChanged += value; }
            remove { bingoModel.OnMissionStateChanged -= value; }
        }

        public event System.Action OnUpdateEventNotice;

        public event System.Action OnUpdateEventStageInfo
        {
            add { dungeonModel.OnUpdateEventStageInfo += value; }
            remove { dungeonModel.OnUpdateEventStageInfo -= value; }
        }

        public event System.Action OnUpdateEventStageCount
        {
            add { dungeonModel.OnUpdateEventStageCount += value; }
            remove { dungeonModel.OnUpdateEventStageCount -= value; }
        }

        public event System.Action OnUpdateClearedStage
        {
            add { dungeonModel.OnUpdateClearedStage += value; }
            remove { dungeonModel.OnUpdateClearedStage -= value; }
        }

        public event System.Action OnUpdateDarkTree
        {
            add { inventoryModel.DarkTree.OnUpdate += value; }
            remove { inventoryModel.DarkTree.OnUpdate -= value; }
        }

        public event System.Action OnUpdateAttendEventReward
        {
            add { eventModel.OnUpdateAttendEventReward += value; }
            remove { eventModel.OnUpdateAttendEventReward -= value; }
        }

        public MainShortcutPresenter()
        {
            questModel = Entity.player.Quest;
            goodsModel = Entity.player.Goods;
            userModel = Entity.player.User;
            alarmModel = Entity.player.AlarmModel;
            dungeonModel = Entity.player.Dungeon;
            bingoModel = Entity.player.Bingo;
            eventModel = Entity.player.Event;
            inventoryModel = Entity.player.Inventory;
            characterModel = Entity.player.Character;
            jobLevelLimit = BasisType.SPECIAL_ROULETTE_JOB_LEVEL_LIMIT.GetInt();
            wordCollectionItems = BasisType.WORD_COLLECTION_ITEMS.GetKeyList();
        }

        public override void AddEvent()
        {
            questModel.OnStandByReward += InvokeUpdateQuestMainRewardState;
            userModel.OnTreeReward += InvokeUpdateTreeRewardState;
            alarmModel.OnAlarm += InvokeUpdateAlarm;
            questModel.OnStandByReward += InvokeOnQuestNoticeUpdate;
            questModel.OnNormalQuestFree += InvokeOnQuestNoticeUpdate;
            goodsModel.OnUpdateNormalQuestCoin += InvokeOnQuestNoticeUpdate;
            characterModel.OnUpdateJobLevel += InvokeOnEventNoticeUpdate;

            foreach (var item in wordCollectionItems)
            {
                inventoryModel.AddItemEvent(item, OnWordCollectionItems);
            }
        }

        public override void RemoveEvent()
        {
            questModel.OnStandByReward -= InvokeUpdateQuestMainRewardState;
            userModel.OnTreeReward -= InvokeUpdateTreeRewardState;
            alarmModel.OnAlarm -= InvokeUpdateAlarm;
            questModel.OnStandByReward -= InvokeOnQuestNoticeUpdate;
            questModel.OnNormalQuestFree -= InvokeOnQuestNoticeUpdate;
            goodsModel.OnUpdateNormalQuestCoin -= InvokeOnQuestNoticeUpdate;
            characterModel.OnUpdateJobLevel -= InvokeOnEventNoticeUpdate;

            foreach (var item in wordCollectionItems)
            {
                inventoryModel.RemoveItemEvent(item, OnWordCollectionItems);
            }
        }

        void InvokeUpdateQuestMainRewardState()
        {
            OnUpdateQuestMainRewardState?.Invoke();
        }

        void InvokeUpdateTreeRewardState()
        {
            OnUpdateTreeRewardState?.Invoke();
        }

        void InvokeUpdateAlarm(AlarmType alarmType)
        {
            OnUpdateAlarm?.Invoke();
        }

        /// <summary>
        /// 알림 표시 여부
        /// </summary>
        public bool GetHasNotice(UIMainShortcut.MenuContent content)
        {
            switch (content)
            {
                case UIMainShortcut.MenuContent.Event:
                    return questModel.IsEventQuestStandByReward()
                        || IsTreeReward()
                        || bingoModel.IsBingoQuestStandByReward()
                        || IsSpecialRouletteNotice()
                        || IsAttendEventStandByReward()
                        || IsWordCollectionStandByReward();

                case UIMainShortcut.MenuContent.Quest:
                    return HasQuestNotice();

                case UIMainShortcut.MenuContent.Mail:
                    return alarmModel.HasAlarm(AlarmType.MailAccount)
                        || alarmModel.HasAlarm(AlarmType.MailCharacter)
                        || alarmModel.HasAlarm(AlarmType.MailShop)
                        || alarmModel.HasAlarm(AlarmType.MailTrade)
                        || alarmModel.HasAlarm(AlarmType.MailOnBuff);

                case UIMainShortcut.MenuContent.Map:
                    return HasMapNotice();

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIMainShortcut.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 신규 컨텐츠 여부
        /// </summary>
        public bool GetHasNewIcon(UIMainShortcut.MenuContent content)
        {
            switch (content)
            {
                case UIMainShortcut.MenuContent.Event:
                    return false;

                case UIMainShortcut.MenuContent.Quest:
                    return false;

                case UIMainShortcut.MenuContent.Mail:
                    return false;

                case UIMainShortcut.MenuContent.Map:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIMainShortcut.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private bool HasQuestNotice()
        {
            // 메인 보상
            if (questModel.IsMainQuestReward())
                return true;

            // 메인, 이벤트 퀘스트를 제외한 모든 보상 (일일, 업적, 길드)
            if (questModel.IsStandByReward())
                return true;

            return false;
        }

        private void InvokeOnQuestNoticeUpdate()
        {
            OnUpdateQuestNotice?.Invoke();
        }

        private void InvokeOnQuestNoticeUpdate(int obj)
        {
            OnUpdateQuestNotice?.Invoke();
        }

        private void InvokeOnEventNoticeUpdate(int joblevel)
        {
            OnUpdateEventNotice?.Invoke();
        }

        /// <summary>
        /// 스페셜 룰렛 뽑기 가능 여부
        /// </summary>
        /// <returns></returns>
        private bool IsSpecialRouletteNotice()
        {
            if (IsJobLevelLimit())
                return false;

            if (!eventModel.IsRemainTimeRoulette())
                return false;

            if (eventModel.IsSpecialRouletteMaxUsed())
                return false;

            int count = inventoryModel.GetItemCount(eventModel.SpecialRouletteItemId);

            return count >= eventModel.GetSpecialRouletteNeedCount();
        }

        /// <summary>
        /// 스페셜 룰렛 직업 레벨 제한 여부
        /// </summary>
        private bool IsJobLevelLimit()
        {
            return characterModel.JobLevel < jobLevelLimit;
        }

        /// <summary>
        /// 모험 알림표시 여부
        /// </summary>
        private bool HasMapNotice()
        {
            // 이벤트스테이지 중이 아님
            if (!dungeonModel.IsOpendEventStage())
                return false;

            // 보스 오픈컨텐츠 조건 체크
            if (!questModel.IsOpenContent(ContentType.Boss, isShowPopup: false))
                return false;

            return dungeonModel.HasFreeChallengeTicket();
        }

        /// <summary>
        /// 나무 보상 여부
        /// </summary>
        private bool IsTreeReward()
        {
            if (userModel.IsCatCoinReward)
                return true;

            if (userModel.IsZenyTreeReward)
                return true;

            if (userModel.IsMaterialTreeReward)
                return true;

            return inventoryModel.DarkTree.HasStandByReward();
        }

        private bool IsAttendEventStandByReward()
        {
            EventQuestGroupInfo eventData = questModel.GetEventQuestByShortCut(ShortCutType.AttendEvent);
            if (eventData == null || eventData.RemainTime.ToRemainTime() <= 0)
                return false;

            return eventModel.IsAttendEventStandByReward();
        }

        private bool IsWordCollectionStandByReward()
        {
            EventQuestGroupInfo eventData = questModel.GetEventQuestByShortCut(ShortCutType.WordCollectionEvent);
            if (eventData == null || eventData.RemainTime.ToRemainTime() <= 0)
                return false;

            return eventModel.IsWordCollectionStandByReward();
        }

        /// <summary>
        /// 단어 수집 아이템 수량 변경 이벤트
        /// </summary>
        private void OnWordCollectionItems()
        {
            OnUpdateEventNotice?.Invoke();
        }
    }
}