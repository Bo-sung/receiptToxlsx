using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleShare2nd : UICanvas, TutorialShareVice2ndOpen.IOpenCharacterShareImpl
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonWithLock btnShare;

        private BattleShare2ndPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BattleShare2ndPresenter();
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.AddEvent();

            EventDelegate.Add(btnShare.OnClick, OnClickedBtnShare);
        }

        protected override void OnClose()
        {
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.RemoveEvent();

            EventDelegate.Remove(btnShare.OnClick, OnClickedBtnShare);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Sharing();
            UpdateNotice();
            UpdateNewIcon();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnShare.LocalKey = LocalizeKey._48256; // 쉐어
        }

        /// <summary>
        /// 셰어 아이콘 클릭
        /// </summary>
        void OnClickedBtnShare()
        {
            presenter.ClickedBtnShare();
        }

        void UpdateNotice()
        {           
            btnShare.SetNotice(presenter.GetHasNotice());
        }

        public void UpdateNewIcon()
        {
            btnShare.SetActiveNew(presenter.HasNewIcon());
        }

        public UISprite GetShare2ndIcon()
        {
            return btnShare.GetIcon();
        }

        [SerializeField] UIWidget tutorialWidget;

        UIWidget TutorialShareVice2ndOpen.IOpenCharacterShareImpl.GetShareWidget()
        {
            return tutorialWidget;
        }       
    }
}