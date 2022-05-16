using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEnemyFocus : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] GameObject goNeedleFX;
        [SerializeField] GameObject goSpreadFX;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            goNeedleFX.SetActive(false);
            goSpreadFX.SetActive(false);
            Hide();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Show(bool showNeedleFX, bool showSpreadFX)
        {
            goNeedleFX.SetActive(showNeedleFX);
            goSpreadFX.SetActive(showSpreadFX);
            gameObject.SetActive(true);
        }
    }
}