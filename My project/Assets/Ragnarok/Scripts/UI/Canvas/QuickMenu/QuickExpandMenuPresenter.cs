using Ragnarok.View.CharacterShare;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIQuickExpandMenu"/>
    /// </summary>
    public class QuickExpandMenuPresenter : ViewPresenter
    {
        private readonly SharingModel sharingModel;
        private readonly QuestModel questModel;
        private readonly BattleManager battleManager;
        private readonly DungeonModel dungeonModel;
        private readonly CharacterModel characterModel;

        public event System.Action OnUpdateShareFreeTicket
        {
            add { sharingModel.OnUpdateShareFreeTicket += value; }
            remove { sharingModel.OnUpdateShareFreeTicket -= value; }
        }

        public event System.Action OnUpdateSharingCharacters
        {
            add { sharingModel.OnUpdateSharingCharacters += value; }
            remove { sharingModel.OnUpdateSharingCharacters -= value; }
        }

        public event System.Action OnUpdateRemainTimeForShare
        {
            add { sharingModel.OnUpdateRemainTimeForShare += value; }
            remove { sharingModel.OnUpdateRemainTimeForShare -= value; }
        }

        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public event System.Action OnUpdateChangeJob;

        public event System.Action OnShareForceLevelUp
        {
            add { characterModel.OnShareForceLevelUp += value; }
            remove { characterModel.OnShareForceLevelUp -= value; }
        }

        public event System.Action OnUpdateShareForceStatus
        {
            add { characterModel.OnUpdateShareForceStatus += value; }
            remove { characterModel.OnUpdateShareForceStatus -= value; }
        }

        public QuickExpandMenuPresenter()
        {
            sharingModel = Entity.player.Sharing;
            questModel = Entity.player.Quest;
            dungeonModel = Entity.player.Dungeon;
            characterModel = Entity.player.Character;
            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            characterModel.OnChangedJob += OnChangeJob;
        }

        public override void RemoveEvent()
        {
            characterModel.OnChangedJob -= OnChangeJob;
        }

        void OnChangeJob(bool isInitialize)
        {
            OnUpdateChangeJob?.Invoke();
        }

        public string GetShareThumbnailName(int index)
        {
            UISimpleCharacterShareBar.IInput info = GetShareInfo(index);
            return GetThumbnailName(info);
        }

        /// <summary>
        /// 현재 적용 중인 셰어 캐릭터 정보 반환
        /// </summary>
        public int GetSharingCharacterCid(int index)
        {
            UISimpleCharacterShareBar.IInput info = GetShareInfo(index);
            return info == null ? 0 : info.Cid;
        }

        /// <summary>
        /// 컨텐츠 오픈 여부
        /// </summary>
        public bool IsOpenContent(UIQuickExpandMenu.MenuContent content, bool isShowPopup)
        {
            switch (content)
            {
                case UIQuickExpandMenu.MenuContent.Share:
                    return questModel.IsOpenContent(ContentType.Sharing, isShowPopup);

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIQuickExpandMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 알림 표시 여부
        /// </summary>
        public bool GetHasNotice(UIQuickExpandMenu.MenuContent content)
        {
            switch (content)
            {
                case UIQuickExpandMenu.MenuContent.Share:
                    return HasCharacterShareNotice() || HasNoticeShareForceStat();

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIQuickExpandMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 신규 컨텐츠 여부
        /// </summary>
        public bool GetHasNewIcon(UIQuickExpandMenu.MenuContent content)
        {
            switch (content)
            {
                case UIQuickExpandMenu.MenuContent.Share:
                    return questModel.HasNewOpenContent(ContentType.Sharing);

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIQuickExpandMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 셰어링 캐릭터 비어있음 여부
        /// </summary>
        public bool IsEmptySharingCharacter()
        {
            return !sharingModel.HasSharingCharacters();
        }

        public bool IsFirstQuestComplete()
        {
            QuestInfo curQuest = questModel.GetMaintQuest();
            if (curQuest.IsInvalidData)
                return false;

            bool isReward = curQuest.CompleteType == QuestInfo.QuestCompleteType.StandByReward;
            return isReward && curQuest.Group == 1; // 첫번째 퀘스트 완료
        }

        /// <summary>
        /// 현재 사용중인 셰어링 캐릭터 수 반환
        /// </summary>
        public int GetSharingCharacterCount()
        {
            return sharingModel.GetSharingCharacterSize();
        }

        /// <summary>
        /// 공유 가능 남은시간
        /// </summary>
        public float GetRemainTimeForShare()
        {
            return sharingModel.GetRemainTimeForShare();
        }

        public int GetJobGrade()
        {
            return characterModel.JobGrade();
        }

        public int GetOpenedSlotCount()
        {
            return sharingModel.GetShareSlotCount(GetJobGrade(), questModel.IsOpenContent(ContentType.ShareHope, false));
        }

        public int GetOpenedCloneCount()
        {
            return sharingModel.GetCloneSlotCount(GetJobGrade());
        }

        private UISimpleCharacterShareBar.IInput GetShareInfo(int index)
        {
            if (index < Constants.Size.SHARE_SLOT_SIZE)
            {
                UISimpleCharacterShareBar.IInput[] shareCharacterList = sharingModel.GetSharingCharacters();
                if (index < shareCharacterList.Length)
                {
                    return shareCharacterList[index];
                }

                return null;
            }

            SharingModel.CloneCharacterType cloneCharacterType = sharingModel.GetCloneCharacterType(index);
            return sharingModel.GetCloneCharacter(cloneCharacterType);
        }

        private string GetThumbnailName(UISimpleCharacterShareBar.IInput info)
        {
            if (info == null)
                return string.Empty;

            return info.ThumbnailName;
        }

        /// <summary>
        /// 캐릭터 셰어 빨콩 여부
        /// </summary>
        private bool HasCharacterShareNotice()
        {
            return sharingModel.GetShareTicketCount(ShareTicketType.DailyFree) > 0; // 무료 티켓이 존재할 경우
        }

        /// <summary>
        /// 쉐어포스 스탯 빨콩 여부
        /// </summary>
        private bool HasNoticeShareForceStat()
        {
            return characterModel.CanShareForceStatusUpgrade();
        }
    }
}