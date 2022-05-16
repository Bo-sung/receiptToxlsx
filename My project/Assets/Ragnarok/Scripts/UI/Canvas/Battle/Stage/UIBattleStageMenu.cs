using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleStageMenu : UICanvas, TutorialBossSummon.IImpl
    {
        public enum MenuContent
        {
            MvpSummon = 1,
            BossSummon,
            Assemble,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonWithLock btnMvpSummon;
        [SerializeField] UIButtonWithLock btnBossSummon;
        [SerializeField] UIButtonWithLock btnAssemble;
        [SerializeField] UILabel labelBossCoolTime;
        [SerializeField] UISprite bossSummonSprite;
        [SerializeField] UILabel labelMvpSummonCount;
        [SerializeField] UIWidget assembleWidget;
        [SerializeField] GameObject goBossIcon;
        [SerializeField] GameObject goEventBossIcon;

        public event System.Action OnSelectMvpSummon;
        public event System.Action OnSelectBossSummon;
        public event System.Action OnSelectAssemble;

        private bool isActiveBtnMvpSummon;
        private bool isActiveBtnBossSummon;
        private bool isActiveBtnAssemble;
        private bool forceShowAssemble;

        BattleStageMenuPresenter presenter;
        private RemainTime bossCoolTime;
        private float timer = 0;

        public UIWidget AssembleWidget => assembleWidget;

        protected override void OnInit()
        {
            presenter = new BattleStageMenuPresenter();

            EventDelegate.Add(btnMvpSummon.OnClick, OnClickedBtnMvpSummon);
            EventDelegate.Add(btnBossSummon.OnClick, OnClickedBtnSummonBoss);
            EventDelegate.Add(btnAssemble.OnClick, OnClickedBtnAssemble);

            presenter.OnUpdateSummonMvpTicket += UpdateMvpSummonTicketCount;
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnMvpSummon.OnClick, OnClickedBtnMvpSummon);
            EventDelegate.Remove(btnBossSummon.OnClick, OnClickedBtnSummonBoss);
            EventDelegate.Remove(btnAssemble.OnClick, OnClickedBtnAssemble);

            presenter.OnUpdateSummonMvpTicket -= UpdateMvpSummonTicketCount;
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateOpenContent();
            UpdateNotice();
            UpdateNewIcon();
            UpdateMvpSummonTicketCount();
            UpdateBossCoolTimeLabel();
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
            timer -= Time.deltaTime;

            if (timer > 0)
                return;

            timer = 1f;
            UpdateBossCoolTimeLabel();
        }

        void OnClickedBtnMvpSummon()
        {
            SelectContent(MenuContent.MvpSummon);
        }

        void OnClickedBtnSummonBoss()
        {
            SelectContent(MenuContent.BossSummon);
        }

        void OnClickedBtnAssemble()
        {
            SelectContent(MenuContent.Assemble);
        }

        public void SetBossCoolTimeIfGreater(int bossCoolTime)
        {           
            this.bossCoolTime = (int)bossCoolTime;
            UpdateBossCoolTimeLabel();
        }

        public void SetActiveMvpSummon(bool isActive)
        {
            isActiveBtnMvpSummon = isActive;
            UpdateOpenContent(MenuContent.MvpSummon);
        }

        public void SetActiveBossSummon(bool isActive)
        {
            isActiveBtnBossSummon = isActive;
            UpdateOpenContent(MenuContent.BossSummon);
        }

        public void SetActiveAssemble(bool isActive)
        {
            isActiveBtnAssemble = isActive;
            UpdateOpenContent(MenuContent.Assemble);
        }

        private void SelectContent(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.MvpSummon:
                    OnSelectMvpSummon?.Invoke();
                    break;

                case MenuContent.BossSummon:
                    OnSelectBossSummon?.Invoke();
                    break;

                case MenuContent.Assemble:
                    OnSelectAssemble?.Invoke();
                    break;
            }
        }

        public void UpdateOpenContent()
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

            button.SetActive(presenter.IsOpenContent(content, isShowPopup: false) && IsActive(content));
        }

        private void UpdateNotice(MenuContent content)
        {
            UIButtonWithLock button = GetButton(content);
            if (button == null)
                return;

            button.SetNotice(presenter.GetHasNotice(content));
        }

        private void UpdateMvpSummonTicketCount()
        {
            int mvpSummonTicketCount = presenter.GetMvpSummonTicketCount();
            labelMvpSummonCount.text = mvpSummonTicketCount > 99 ? "99+" : mvpSummonTicketCount.ToString();
        }

        private void UpdateBossCoolTimeLabel()
        {
            labelBossCoolTime.text = bossCoolTime.ToStringTime(@"mm\:ss");
            labelBossCoolTime.gameObject.SetActive(bossCoolTime.ToRemainTime() > 0);
            btnBossSummon.IsEnabled = bossCoolTime.ToRemainTime() <= 0;
            bossSummonSprite.color = bossCoolTime.ToRemainTime() <= 0 ? Color.white : Color.grey;
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
                case MenuContent.MvpSummon:
                    return btnMvpSummon;

                case MenuContent.BossSummon:
                    return btnBossSummon;

                case MenuContent.Assemble:
                    return btnAssemble;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private bool IsActive(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.MvpSummon:
                    return isActiveBtnMvpSummon;

                case MenuContent.BossSummon:
                    return isActiveBtnBossSummon;

                case MenuContent.Assemble:
                    return forceShowAssemble || isActiveBtnAssemble;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private string GetButtonText(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.MvpSummon:
                    return LocalizeKey._2300.ToText(); // 소환권

                case MenuContent.BossSummon:
                    return LocalizeKey._2303.ToText(); // 보스도전

                case MenuContent.Assemble:
                    return LocalizeKey._2301.ToText(); // 집결!
            }

            Debug.LogError($"[올바르지 않은 {nameof(MenuContent)}] {nameof(content)} = {content}");
            return string.Empty;
        }

        public void SetActiveEventBossIcon(bool isActive)
        {
            goEventBossIcon.SetActive(isActive);
            goBossIcon.SetActive(!isActive);
        }

        #region Tutorial
        UIWidget TutorialBossSummon.IImpl.GetBtnSummonWidget()
        {
            return btnBossSummon.GetComponent<UIWidget>();
        }
        #endregion
    }
}