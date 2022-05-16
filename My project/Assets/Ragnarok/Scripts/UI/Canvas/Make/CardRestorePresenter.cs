namespace Ragnarok
{
    public class CardRestorePresenter : ViewPresenter
    {
        private UICardRestore view;

        public CardRestorePresenter(UICardRestore view)
        {
            this.view = view;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public async void RequestRestore(CardItemInfo cardItem)
        {
            if (!await UI.CostPopup(CoinType.CatCoin, BasisType.CARD_RESTORING_COST.GetInt(cardItem.Rating), LocalizeKey._28035.ToText(), LocalizeKey._90211.ToText()))
                return;

            if (!await Entity.player.Inventory.RequestCardRestore(cardItem))
                return;

            UI.ShowToastPopup(LocalizeKey._28053.ToText());
            view.ResetSelection();
        }

        public bool IsContentsOpen()
        {
            return Entity.player.Quest.IsOpenContent(ContentType.ManageCard, false);
        }
    }
}
