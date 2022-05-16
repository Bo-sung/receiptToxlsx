namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleDescription"/>
    /// </summary>
    public sealed class BattleDescriptionPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly ItemDataManager itemDataRepo;

        public BattleDescriptionPresenter()
        {
            itemDataRepo = ItemDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public string GetItemIconName(int itemId)
        {
            if (itemId == 0)
                return string.Empty;

            ItemData itemData = itemDataRepo.Get(itemId);
            if (itemData == null)
                return string.Empty;

            return itemData.icon_name;
        }
    }
}