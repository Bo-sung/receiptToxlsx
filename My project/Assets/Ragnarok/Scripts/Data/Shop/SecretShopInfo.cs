using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public sealed class SecretShopInfo : DataInfo<GachaData>, IInitializable<CharSecretShop>
    {        
        private ObscuredShort buyCount;

        /// <summary>
        /// 뽑기 아이디
        /// </summary>
        public int GachaID => data.id;

        /// <summary>
        /// 구매 수량
        /// </summary>
        public short BuyCount => buyCount;              

        public RewardData reward { get; private set; }

        /// <summary>
        /// 구매 비용 타입
        /// </summary>
        public CoinType CoinType => data.pay_type.ToEnum<CoinType>();

        /// <summary>
        /// 구매 비용
        /// </summary>
        public int Cost => data.pay_value;

        /// <summary>
        /// [비밀상점] 매진 여부
        /// </summary>
        public bool IsSoldOut => buyCount > 0;

        /// <summary>
        /// 해금 시 필요한 도달 챕터
        /// </summary>
        public int OpenChapter => data.group_id;

        public void Initialize(CharSecretShop packet)
        {
            buyCount = packet.buy_count;
        }

        public void SetReward(RewardData reward)
        {
            this.reward = reward;
        }

        /// <summary>
        /// 상점 구매
        /// </summary>
        public void BuyShop()
        {
            buyCount++;
        }
    }
}
