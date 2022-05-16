using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class TutorialItemEnchant : TutorialStep
    {
        public TutorialItemEnchant() : base(TutorialType.ItemEnchant)
        {
        }

        public override bool IsCheckCondition()
        {
            if (Entity.player.Sharing.GetSharingState() != SharingModel.SharingState.None)
                return false;

            if (!Entity.player.Quest.IsOpenContent(ContentType.ItemEnchant, false))
                return false;

            if (Entity.player.Inventory.itemList.Find(v => v.ItemGroupType == ItemGroupType.Equipment) == null)
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
            HideTutorial();

            yield return WaitUntilHideDailyCheck();

            // UIQuestReward UI가 보이지 않을 때까지 기다린다.
            yield return WaitUntilHideCanvas<UIQuestReward>();

            yield return WaitUntilHideCanvas<UIContentsUnlock>();

            // 튜토리얼 화면
            ShowTutorial();

            var uiMain = UI.GetUI<UIMain>();

            UIInven.tabType = UIInven.TabType.Equipment;

            yield return Show(LocalizeKey._26035.ToText(), UIWidget.Pivot.Top, uiMain.InvenWidget, () => IsVisibleCanvas<UIInven>());
            
            var uiInven = UI.GetUI<UIInven>();

            yield return Show(uiInven.FirstItemWidget, () => IsVisibleCanvas<UIEquipmentInfo>());

            var uiEquipmentInfo = UI.GetUI<UIEquipmentInfo>();

            if (uiEquipmentInfo.CanEquipmentLevelUp())
            {
                yield return Show(LocalizeKey._26036.ToText(), UIWidget.Pivot.Top, uiEquipmentInfo.ItemEnchantWidget, uiEquipmentInfo.IsRequestEquipmentLevelUp);
            }
            else
            {
                yield return Show(LocalizeKey._26036.ToText(), UIWidget.Pivot.Top, uiEquipmentInfo.ItemEnchantWidget);
            }

            yield return RequestFinish(); // 서버로 완료 프로토콜 보냄
        }
    }
}