namespace Ragnarok
{
    public sealed class ShareviceItem
    {
        private readonly IShareviceItemImpl impl;

        public readonly int itemId;
        public readonly int itemCount;
        public readonly int itemExp;
        public readonly string itemIcon;

        public int SelectedCount { get; private set; }

        public ShareviceItem(IShareviceItemImpl impl, ItemInfo info)
        {
            this.impl = impl;

            itemId = info.ItemId;
            itemCount = info.ItemCount;
            itemExp = info.ExpValue;
            itemIcon = info.IconName;
        }

        /// <summary>
        /// 선택된 경험치
        /// </summary>
        /// <returns></returns>
        public int GetSelectedExp()
        {
            return itemExp * SelectedCount;
        }

        /// <summary>
        /// 선택 갯수 증가. 선택 갯수가 보유수량을 넘을수는 없음
        /// </summary>
        public void PlusSelectCount(int count)
        {
            if (itemCount < (SelectedCount + count))
            {
                SelectedCount = itemCount;
            }
            else
            {
                SelectedCount += count;
            }
        }

        /// <summary>
        /// 선택갯수 증가. 만약 선택한 아이템의 총 경험치량이 레벨업까지 남은 경험치량보다 크면 레벨업까지만 선택하도록 수정됨.
        /// </summary>
        public void PlusSelectCountBeforeOverload(int count)
        {
            if (count <= 0)
                return;

            PlusSelectCount(count - 1);
            while (impl.IsMaxExp())
            {
                SelectedCount--;
            }
            PlusSelectCount(1);
        }

        /// <summary>
        /// 아이템 선택 가능 여부
        /// </summary>
        public bool CanSelect()
        {
            if (impl.IsImpossibleViceLevelUp())
            {
                UI.ShowToastPopup(LocalizeKey._10256.ToText()); // 셰어바이스 레벨은 유저의 직업 레벨을 넘을 수 없습니다.
                return false;
            }

            if (impl.IsMaxExp())
            {
                UI.ShowToastPopup(LocalizeKey._10257.ToText()); // 이미 게이지의 수치가 최대입니다.
                return false;
            }

            return SelectedCount < itemCount;
        }
    }
}