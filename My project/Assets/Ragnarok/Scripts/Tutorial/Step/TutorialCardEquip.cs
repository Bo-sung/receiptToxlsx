using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialCardEquip : TutorialStep
    {
        public TutorialCardEquip() : base(TutorialType.CardEquip)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!IsVisibleCanvas<UIEquipmentInfo>())
                return false;

            UIEquipmentInfo ui = UI.GetUI<UIEquipmentInfo>();

            if (ui.CurItem.Smelt < 10)
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

            yield return Show(LocalizeKey._26037.ToText(), UIWidget.Pivot.Center);

            yield return Show(LocalizeKey._26038.ToText(), UIWidget.Pivot.Center, uiEquipmentInfo.CardEquipWidget);

            yield return Show(LocalizeKey._26039.ToText(), UIWidget.Pivot.Center, uiEquipmentInfo.CardAutoEquipWidget);

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}