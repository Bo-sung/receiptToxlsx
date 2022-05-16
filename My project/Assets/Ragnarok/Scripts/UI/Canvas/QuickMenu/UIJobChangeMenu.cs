using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobChangeMenu : UICanvas, TurotialJobChange.IOpenJobChangeImpl
    {
        public enum MenuContent
        {
            JobChange = 1,
            SpecialEvent = 2,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonWithLock btnChangeJob;
        [SerializeField] UIButtonWithLock btnSpecialEvent;

        JobChangeMenuPresenter presenter;

        protected override void OnInit()
        {
            presenter = new JobChangeMenuPresenter();

            EventDelegate.Add(btnChangeJob.OnClick, OnClickedBtnChangeJob);
            EventDelegate.Add(btnSpecialEvent.OnClick, OnClickedBtnSpecialEvent);

            presenter.OnChangeJobLevel += UpdateOpenContent;
            presenter.OnChangeJob += UpdateOpenContent;
            presenter.OnUpdateGuideQuest += UpdateOpenContent;
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.OnUpdateQuizInfo += UpdateOpenContent;
            presenter.OnCatCoinGiftReward += UpdateOpenContent;
            presenter.OnStandByReward += UpdateOpenContent;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnChangeJob.OnClick, OnClickedBtnChangeJob);
            EventDelegate.Remove(btnSpecialEvent.OnClick, OnClickedBtnSpecialEvent);

            presenter.OnChangeJobLevel -= UpdateOpenContent;
            presenter.OnChangeJob -= UpdateOpenContent;
            presenter.OnUpdateGuideQuest -= UpdateOpenContent;
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.OnUpdateQuizInfo -= UpdateOpenContent;
            presenter.OnCatCoinGiftReward -= UpdateOpenContent;
            presenter.OnStandByReward -= UpdateOpenContent;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateOpenContent();

            // UpdateOpenContent 에서 활성화 상태일 때 갱신해줌.
            return;
            UpdateNotice();
            UpdateNewIcon();
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

        /// <summary>
        /// 전직 버튼
        /// </summary>
        void OnClickedBtnChangeJob()
        {
            isClickedJobChange = true;
            UI.Show<UIJobChange>();
        }

        void OnClickedBtnSpecialEvent()
        {
            // 퀴즈퀴즈, 냥다래선물 이벤트
            UI.Show<UISpecialEvent>();
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
            UIButtonWithLock button = GetButton(content);
            if (button == null)
                return;

            // 버튼 활성화일때
            if (presenter.IsOpenContent(content))
            {
                button.SetActive(true);
                button.SetNotice(presenter.GetHasNotice(content));
                button.SetActiveNew(presenter.GetHasNewIcon(content));
            }
            else
            {
                button.SetActive(false);
            }
        }

        private void UpdateNotice(MenuContent content)
        {
            UIButtonWithLock button = GetButton(content);
            if (button == null)
                return;

            button.SetNotice(presenter.GetHasNotice(content));
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

        private UIButtonWithLock GetButton(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.JobChange:
                    return btnChangeJob;

                case MenuContent.SpecialEvent:
                    return btnSpecialEvent;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private string GetButtonText(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.JobChange:
                    return LocalizeKey._3018.ToText(); // 전직

                case MenuContent.SpecialEvent:
                    return LocalizeKey._5413.ToText(); // 스페셜
            }

            Debug.LogError($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            return string.Empty;
        }

        #region Tutorial
        [SerializeField] UIWidget tutorialWidget;
        bool isClickedJobChange;

        UIWidget TurotialJobChange.IOpenJobChangeImpl.GetBtnJobChange()
        {
            return tutorialWidget;
        }

        bool TurotialJobChange.IOpenJobChangeImpl.IsClickedBtnJobChange()
        {
            if (isClickedJobChange)
            {
                isClickedJobChange = false;
                return true;
            }

            return false;
        }
        #endregion
    }
}