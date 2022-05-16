namespace Ragnarok
{
    public class CardResetPresenter : ViewPresenter
    {
        private UICardReset view;

        public CardResetPresenter(UICardReset view)
        {
            this.view = view;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public async void RequestReset(CardItemInfo cardItem)
        {
            if (!await UI.CostPopup(CoinType.CatCoin, BasisType.CARD_RESET_COST.GetInt(cardItem.Rating), LocalizeKey._28036.ToText(), LocalizeKey._90210.ToText()))
                return;

            if (!await Entity.player.Inventory.RequestCardReset(cardItem))
                return;

            UI.ShowToastPopup(LocalizeKey._28050.ToText());
            UI.Show<UICardInfo>(new UICardInfo.Input(cardItem).ShowBtnConfirm());
        }

        public bool IsContentsOpen()
        {
            return Entity.player.Quest.IsOpenContent(ContentType.ManageCard, false);
        }
    }
}
