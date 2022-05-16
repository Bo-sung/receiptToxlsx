using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialCardEnchant : TutorialStep
    {
        public TutorialCardEnchant() : base(TutorialType.CardEnchant)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!IsVisibleCanvas<UIEquipmentInfo>())
                return false;

            UIEquipmentInfo ui = UI.GetUI<UIEquipmentInfo>();

            if (!ui.CurItem.IsEquippedCard)
                return false;

            if (ui.FirstCardEnchantWidget == null)
                return false;

            return true;
        }

        protected override bool HasSkip()
        {
            return false;
        }

        protected override Npc GetNpc()
        {
            return Npc.HOLORUCHI;
        }

        protected override IEnumerator<float> Run()
        {
            var uiEquipmentInfo = UI.GetUI<UIEquipmentInfo>();

            yield return Show(LocalizeKey._26040.ToText(), UIWidget.Pivot.Center);

            yield return Show(LocalizeKey._26041.ToText(), UIWidget.Pivot.Center, uiEquipmentInfo.FirstCardEnchantWidget);

            yield return Show(LocalizeKey._26042.ToText(), UIWidget.Pivot.Center, uiEquipmentInfo.FirstCardEnchantWidget);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}