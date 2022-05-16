using System.Collections.Generic;

namespace Ragnarok
{
    public class TierUpViewPresenter : ViewPresenter
    {
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;
        private readonly CharacterModel characterModel;

        /// <summary> 
        /// 장비 초월 이벤트
        /// </summary>
        public event System.Action<bool> OnTierUp
        {
            add { inventoryModel.OnTierUp += value; }
            remove { inventoryModel.OnTierUp -= value; }
        }

        public TierUpViewPresenter()
        {
            inventoryModel = Entity.player.Inventory;
            questModel = Entity.player.Quest;
            characterModel = Entity.player.Character;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void RequestTierUp(ItemInfo targetItem, List<ItemInfo> selectedEquipments)
        {
            inventoryModel.RequestItemTierUp(targetItem, selectedEquipments).WrapNetworkErrors();
        }

        public bool IsContentsOpen()
        {
            return questModel.IsOpenContent(ContentType.TierUp, false);
        }

        /// <summary>
        /// 초월 필요 Job레벨
        /// </summary>
        public int NeedJobLevel(int tierIndex)
        {
            int needJobLevel = BasisType.ITEM_TRANSCEND_JOB_LEVEL.GetInt(tierIndex);

            // JobLevel 부족
            if (characterModel.JobLevel < needJobLevel)
                return needJobLevel;

            return 0;
        }
    }
}