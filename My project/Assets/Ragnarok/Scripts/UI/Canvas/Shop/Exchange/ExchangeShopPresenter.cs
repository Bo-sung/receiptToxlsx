using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIExchangeShop"/>
    /// </summary>
    public sealed class ExchangeShopPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GoodsModel goodsModel;
        private readonly ShopModel shopModel;

        // <!-- Repositories --!>
        private readonly BetterList<ExchangeElement> list;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }
        public event System.Action<long> OnUpdateRoPoint
        {
            add { goodsModel.OnUpdateRoPoint += value; }
            remove { goodsModel.OnUpdateRoPoint -= value; }
        }

        public ExchangeShopPresenter()
        {
            goodsModel = Entity.player.Goods;
            shopModel = Entity.player.ShopModel;

            list = new BetterList<ExchangeElement>();
            KafExchangeData[] arrayData = KafExchangeDataManager.Instance.GetArrayData(KafraType.Exchange);
            if (arrayData != null)
            {
                foreach (var item in arrayData)
                {
                    list.Add(new ExchangeElement(item));
                }
            }
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public UIExchangeElement.IInput[] GetArrayData()
        {
            return list.ToArray();
        }

        public void RequestExchange(int id, int count)
        {
            shopModel.RequestExcnage(id, count).WrapNetworkErrors();
        }

        private class ExchangeElement : UIExchangeElement.IInput
        {
            private readonly KafExchangeData data;

            int UIExchangeElement.IInput.Id => data.id;
            RewardData UIExchangeElement.IInput.Result => data.result;
            RewardData UIExchangeElement.IInput.Material => data.material;

            private int count = 1;
            public int Count
            {
                get { return count; }
                set
                {
                    if (count == value)
                        return;

                    count = value;
                    OnUpdateSelectedCount?.Invoke();
                }
            }

            public event System.Action OnUpdateSelectedCount;

            public ExchangeElement(KafExchangeData data)
            {
                this.data = data;
            }
        }
    }
}