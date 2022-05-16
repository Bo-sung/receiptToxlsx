using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleReady : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelNotice;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void ShowNotice(string notice)
        {
            Show();

            labelNotice.Text = notice;
        }
    }
}