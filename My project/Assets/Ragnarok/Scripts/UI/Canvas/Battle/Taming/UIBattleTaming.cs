using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIBattleTaming : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] BattleTamingRewardView battleTamingRewardView;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void Set(int guildCoin, int cupetPice)
        {
            battleTamingRewardView.SetGuildCoin(guildCoin);
            battleTamingRewardView.SetCupetPiece(cupetPice);
        }
    }
}