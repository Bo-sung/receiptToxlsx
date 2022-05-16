using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="QuickExpandMenuPresenter"/>
    /// </summary>
    public sealed class UIQuickExpandMenu : UICanvas<QuickExpandMenuPresenter>, TutorialSharingCharacterEquip.IOpenCharacterShareImpl
    {
        public delegate void ShareClickEvent(int index, int cid);

        public enum MenuContent
        {
            Share = 1,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] GameObject goShareBase;
        [SerializeField] UIButtonWithLock btnShare;
        [SerializeField] ExpandMenuView shareExpandView;
        [SerializeField] GameObject goShareNotice;
        [SerializeField] GameObject goShareEmpty;
        [SerializeField] GameObject goShareTime;
        [SerializeField] UILabel labelShareTime;
        [SerializeField] UIWidget widgetExpandView;

        public delegate void ExpandMenuSlotInitEvent(int index);

        public event ShareClickEvent OnClickItem; // 클릭된 셰어 캐릭터의 CID 반환
        public event ExpandMenuSlotInitEvent OnSlotInit; // Slot.SetData될 때 Fire

        private RemainTimeStopwatch remainTimeForShare;
        private int sharingCharacterCount;
        private bool isShareNoticeMode;

        protected override void OnInit()
        {
            presenter = new QuickExpandMenuPresenter();
            remainTimeForShare = new RemainTimeStopwatch();

            // 셰어
            shareExpandView.OnUpdate += Refresh;
            shareExpandView.OnSelect += OnSelectSlot;

            EventDelegate.Add(btnShare.OnClick, OnClickedBtnShare);

            presenter.OnUpdateSharingCharacters += Refresh;
            presenter.OnUpdateShareFreeTicket += UpdateNotice;
            //presenter.OnUpdateGuideQuest += Refresh;
            presenter.OnUpdateSharingCharacters += UpdateShareNotice;
            presenter.OnUpdateSharingCharacters += UpdateRemainTimeForSharing;
            presenter.OnUpdateRemainTimeForShare += UpdateRemainTimeForSharing;
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.OnUpdateChangeJob += Refresh;
            presenter.OnShareForceLevelUp += UpdateNotice;
            presenter.OnUpdateShareForceStatus += UpdateNotice;
            presenter.AddEvent();

            ExpandShare();
        }

        protected override void OnClose()
        {
            // 셰어
            shareExpandView.OnUpdate -= Refresh;
            shareExpandView.OnSelect -= OnSelectSlot;

            EventDelegate.Remove(btnShare.OnClick, OnClickedBtnShare);

            presenter.OnUpdateSharingCharacters -= Refresh;
            presenter.OnUpdateShareFreeTicket -= UpdateNotice;
            //presenter.OnUpdateGuideQuest -= Refresh;
            presenter.OnUpdateSharingCharacters -= UpdateShareNotice;
            presenter.OnUpdateSharingCharacters -= UpdateRemainTimeForSharing;
            presenter.OnUpdateRemainTimeForShare -= UpdateRemainTimeForSharing;
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.OnUpdateChangeJob -= Refresh;
            presenter.OnShareForceLevelUp -= UpdateNotice;
            presenter.OnUpdateShareForceStatus -= UpdateNotice;
            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateOpenContent();
            UpdateNotice();
            UpdateShareNotice();
            UpdateRemainTimeForSharing();
            UpdateNewIcon();

            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UIButtonHelper button = GetButton(item);
                if (button == null)
                    continue;

                button.Text = GetButtonText(item);
            }
        }

        void Update()
        {
            if (remainTimeForShare.IsFinished())
                return;

            if (RefreshTime())
                return;

            // Finished
        }

        /// <summary>
        /// 셰어 아이콘 클릭
        /// </summary>
        void OnClickedBtnShare()
        {
            SelectContent(MenuContent.Share);
            isClickedBtnSharing = true;
        }

        private void Refresh()
        {
            shareExpandView.SetJobGrade(presenter.GetJobGrade(), presenter.GetOpenedSlotCount(), presenter.GetOpenedCloneCount());

            if (shareExpandView.CurState == ExpandMenuView.State.Collapsed)
                return;

            for (int i = 0; i < shareExpandView.ListSize; ++i)
            {
                string thumbnailName = presenter.GetShareThumbnailName(i);
                shareExpandView.SetThumnailName(i, thumbnailName);

                if (string.IsNullOrEmpty(thumbnailName))
                {
                    // Do Nothing
                }
                else
                {
                    OnSlotInit?.Invoke(i);
                }
            }
        }

        public void ExpandShare()
        {
            shareExpandView.SetExpansionState(ExpandMenuView.State.Expanded);
        }

        /// <summary>
        /// 셰어 Slot 클릭이벤트
        /// </summary>
        void OnSelectSlot(int index)
        {
            int cid = presenter.GetSharingCharacterCid(index);
            if (cid == 0)
            {
                UI.Show<UICharacterShare>();
                return;
            }

            OnClickItem?.Invoke(index, cid);
        }

        private void SelectContent(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Share:
                    UI.Show<UICharacterShare>();
                    break;
            }
        }

        public void SetShareNoticeMode(bool isShareNoticeMode)
        {
            this.isShareNoticeMode = isShareNoticeMode;
            UpdateShareNotice();

            for (int i = 0; i < shareExpandView.ListSize; ++i)
            {
                shareExpandView.SetNoticeMode(i, isShareNoticeMode);
            }
        }

        /// <summary>
        /// 현재 체력 % 설정
        /// </summary>
        public void SetShareCharacterCurrentHp(int index, int cur, int max, bool skipAnim = false)
        {
            shareExpandView.SetShareCharacterCurrentHp(index, cur, max, skipAnim);
        }

        /// <summary>
        /// 셰어 캐릭터 부활 대기시간 설정
        /// </summary>
        public void SetShareCharacterReviveTime(int index, float reviveTime)
        {
            shareExpandView.SetShareCharacterReviveTime(index, reviveTime);
        }

        /// <summary>
        /// 셰어캐릭터 조종 아이콘 설정
        /// </summary>
        public void SetShareCharacterSelectState(int index, bool isSelect)
        {
            shareExpandView.SetShareCharacterSelectState(index, isSelect);
        }

        private void UpdateOpenContent()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UpdateOpenContent(item);
            }
        }

        private void UpdateNotice()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UpdateNotice(item);
            }
        }

        private void UpdateNewIcon()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UIButtonWithLock button = GetButton(item);
                if (button == null)
                    return;

                button.SetActiveNew(presenter.GetHasNewIcon(item));
            }
        }

        public void UpdateOpenContent(MenuContent content)
        {
            GameObject go = GetBaseObject(content);
            if (go == null)
                return;

            NGUITools.SetActive(go, presenter.IsOpenContent(content, isShowPopup: false));
        }

        private void UpdateNotice(MenuContent content)
        {
            UIButtonWithLock button = GetButton(content);
            if (button == null)
                return;

            button.SetNotice(presenter.GetHasNotice(content));
        }

        private GameObject GetBaseObject(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Share:
                    return goShareBase;
            }

            return null;
        }

        private void UpdateShareNotice()
        {
            bool isEmptyShare = presenter.IsEmptySharingCharacter();
            if (isEmptyShare)
            {
                goShareNotice.SetActive(false);
                goShareEmpty.SetActive(true);
            }
            else
            {
                goShareNotice.SetActive(true);
                goShareEmpty.SetActive(false);
            }
        }

        private void UpdateRemainTimeForSharing()
        {
            sharingCharacterCount = presenter.GetSharingCharacterCount();
            float remainTime = presenter.GetRemainTimeForShare();
            remainTimeForShare.Set(remainTime);

            if (sharingCharacterCount == 0)
            {
                remainTimeForShare.Pause();
                btnShare.SetActiveLabel(true);
                NGUITools.SetActive(goShareTime, false);
            }
            else
            {
                remainTimeForShare.Resume();
                btnShare.SetActiveLabel(false);
                NGUITools.SetActive(goShareTime, true);
            }

            RefreshTime();
        }

        private bool RefreshTime()
        {
            float remainTime = remainTimeForShare.ToRemainTime();

            // Apply TimeScale
            if (sharingCharacterCount > 1)
                remainTime *= sharingCharacterCount;

            var timeSpan = remainTime.ToTimeSpan();
            int totalHours = (int)timeSpan.TotalHours;
            labelShareTime.text = StringBuilderPool.Get()
                .Append(totalHours.ToString()).Append(":").Append(timeSpan.Minutes.ToString("00")).Append(":").Append(timeSpan.Seconds.ToString("00"))
                .Release();
            return remainTimeForShare.IsFinished();
        }

        public UISprite GetMenuIcon(MenuContent content)
        {
            UIButtonWithLock button = GetButton(content);

#if UNITY_EDITOR
            if (button == null)
            {
                Debug.LogError($"{nameof(MenuContent)}에 해당하는 Button이 음슴: {nameof(content)} = {content}");
                return null;
            }
#endif

            return button.GetIcon();
        }

        public int GetSlotCount()
        {
            return shareExpandView.ListSize;
        }

        private UIButtonWithLock GetButton(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Share:
                    return btnShare;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private string GetButtonText(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Share:
                    return LocalizeKey._3101.ToText(); // 셰어
            }

            Debug.LogError($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            return string.Empty;
        }

        #region Tutorial
        [SerializeField] UIWidget tutorialWidget;
        private bool isClickedBtnSharing;

        UIWidget TutorialSharingCharacterEquip.IOpenCharacterShareImpl.GetBtnSharing()
        {
            return tutorialWidget;
        }

        bool TutorialSharingCharacterEquip.IOpenCharacterShareImpl.IsClickedBtnSharing()
        {
            if (isClickedBtnSharing)
            {
                isClickedBtnSharing = false;
                return true;
            }

            return false;
        }

        UIWidget TutorialSharingCharacterEquip.IOpenCharacterShareImpl.GetWidgetExpand()
        {
            return widgetExpandView;
        }
        #endregion
    }
}