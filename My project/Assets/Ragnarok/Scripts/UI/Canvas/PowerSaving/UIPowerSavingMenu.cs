using UnityEngine;

namespace Ragnarok
{
    public sealed class UIPowerSavingMenu : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIButtonHelper btnPowerSaving;

        protected override void OnInit()
        {
            EventDelegate.Add(btnPowerSaving.OnClick, ShowPowerSavingUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnPowerSaving.OnClick, ShowPowerSavingUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnPowerSaving.LocalKey = LocalizeKey._10700; // 절전모드
        }

        private void ShowPowerSavingUI()
        {
            UI.Show<UIPowerSaving>();
        }
    }
}