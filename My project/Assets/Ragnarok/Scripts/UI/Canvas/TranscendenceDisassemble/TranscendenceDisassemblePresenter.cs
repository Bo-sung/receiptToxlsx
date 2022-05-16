using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UITranscendenceDisassemble"/>
    /// </summary>
    public class TranscendenceDisassemblePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly DisassembleItemDataManager disassembleItemDataRepo;

        // <!-- Event --!>
        public event System.Action OnDisassemble
        {
            add { inventoryModel.OnDisassemble += value; }
            remove { inventoryModel.OnDisassemble -= value; }
        }

        public TranscendenceDisassemblePresenter()
        {
            inventoryModel = Entity.player.Inventory;
            disassembleItemDataRepo = DisassembleItemDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 분해 장비 목록
        /// </summary>
        public UIDisassembleElement.IInput[] GetInputs()
        {
            List<ItemInfo> result = inventoryModel.itemList.FindAll(a =>
            a.ItemType == ItemType.Equipment && // 장비
            a.ItemTranscend > 0 && // 초월 여부
            a.IsEquipped == false); // 장착 여부
            result.Sort(SortByCustom);

            DisassembleElement[] elements = new DisassembleElement[result.Count];

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = new DisassembleElement(result[i]);
            }

            return elements;
        }

        private int SortByCustom(ItemInfo x, ItemInfo y)
        {
            int result0 = x.IsLock.CompareTo(y.IsLock);
            int result1 = result0 == 0 ? x.ItemTranscend.CompareTo(y.ItemTranscend) : result0;
            int result2 = result1 == 0 ? x.Smelt.CompareTo(y.Smelt) : result1;
            int result3 = result2 == 0 ? x.ItemId.CompareTo(y.ItemId) : result2;
            return result3;
        }

        /// <summary>
        /// 분해 할 아이템 정보
        /// </summary>
        public ItemInfo GetItemInfo(long itemNo)
        {
            return inventoryModel.GetItemInfo(itemNo);
        }

        /// <summary>
        /// 분해 결과 보상 정보
        /// </summary>
        public RewardData[] GetRewards(int type, int rating)
        {
            DisassembleItemData data = disassembleItemDataRepo.Get(type, rating);
            return data == null ? null : data.rewards;
        }

        /// <summary>
        /// 초월 장비 분해 요청
        /// </summary>
        public void RequestDisassemble(long itemNo)
        {
            inventoryModel.RequestItemDisassemble(new long[] { itemNo }, type: 2, ItemGroupType.Equipment).WrapNetworkErrors();
        }

        private class DisassembleElement : UIDisassembleElement.IInput
        {
            public ItemInfo itemInfo { get; }

            public DisassembleElement(ItemInfo itemInfo)
            {
                this.itemInfo = itemInfo;
            }
        }
    }
}