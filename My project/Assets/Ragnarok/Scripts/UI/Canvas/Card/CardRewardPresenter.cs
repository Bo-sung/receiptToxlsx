namespace Ragnarok
{
    public class CardRewardPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        /******************** Models ********************/

        /******************** Repositories ********************/

        /******************** Event ********************/

        private readonly IView view;

        private CardItemInfo.ICardInfoSimple info;

        public CardRewardPresenter(IView view)
        {
            this.view = view;
        }

        public override void AddEvent()
        {            
        }

        public override void RemoveEvent()
        {            
        }

        public void SetData(CardItemInfo.ICardInfoSimple info)
        {
            this.info = info;
            view.Refresh();
        }

        public int GetNameId() => info.NameId;
        public int GetRating() => info.Rating;
        public string GetIconName() => info.IconName;
    }
}