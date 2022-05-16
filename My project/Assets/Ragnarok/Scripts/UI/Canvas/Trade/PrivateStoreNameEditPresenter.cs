namespace Ragnarok
{
    public class PrivateStoreNameEditPresenter : ViewPresenter
    {
        private readonly TradeModel tradeModel;

        public PrivateStoreNameEditPresenter()
        {
            tradeModel = Entity.player.Trade;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public string GetCurrentMyStoreComment()
        {
            if (string.IsNullOrWhiteSpace(tradeModel.StallName))
            {
                return LocalizeKey._45008.ToText() // {NAME}의 상점
                    .Replace(ReplaceKey.NAME, Entity.player.GetName());
            }
            return tradeModel.StallName;
        }

        public bool IsPrivateStoreSelling()
        {
            return (tradeModel.SellingState == PrivateStoreSellingState.SELLING);
        }

        public void SetPersonalStoreTitle(string newTitle)
        {
            tradeModel.SetStallName(newTitle);
        }

        public void RequestTitleChange(string newTitle)
        {
            // 판매중이면 이름 변경 프로토콜을 전송
            // 판매중이 아니면 그냥 값만 변경한다.
            if (IsPrivateStoreSelling())
            {
                tradeModel.RequestPrivateItemRegister(null, newTitle).WrapErrors();
            }
            else
            {
                tradeModel.SetStallName(newTitle);
            }
        }        
    }
}