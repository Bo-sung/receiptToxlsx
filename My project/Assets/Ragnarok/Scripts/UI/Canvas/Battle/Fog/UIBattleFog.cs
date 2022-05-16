using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleFog : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] BattleFogView battleFogView;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            ShowFog();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void ShowFog()
        {
            battleFogView.Show();
        }

        public void ClearFog()
        {
            battleFogView.Hide();
        }

        public void PlusFog()
        {
            battleFogView.Plus();
        }

        public void MinusFog()
        {
            battleFogView.Minus();
        }
    }
}