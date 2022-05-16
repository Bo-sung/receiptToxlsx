using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public sealed class MailInfo : IInfo
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt cid; // 캐릭터ID
        public readonly ObscuredInt uid; // 유저ID
        private readonly string message; // <T> 문자열이 있을경우 언어테이블 ID 참조       
        private readonly long insertTime;
        private readonly RewardData rewardData;
        private readonly ObscuredInt rewardEventId; // == 1 : 광고메일, == 2 : 페북(국내: 네이버라운지)링크, else : 일반메일
        private readonly int shopId;

        // 거래소 메일을 위한 필드
        public long seq_no; // 보상 수령을 위한 key
        long mail_uid;
        public bool isGetItem; // 아이템 혹은 제니의 수령 여부 (0: 미수령, 1: 수령)
        public TradeShopTradeSellType sellType; // (0 : 개인상점 판매 1 : 개인상점 구매 2 : 거래소 판매 3 : 거래소 구매)

        public bool IsWeight => rewardData.TotalWeight() > 0;

        /// <summary>
        /// 메일 구분
        /// </summary>
        public MailGroup MailGroup => rewardEventId.ToEnum<MailGroup>();

        bool IInfo.IsInvalidData => false;

        event System.Action IInfo.OnUpdateEvent
        {
            add { }
            remove { }
        }

        public MailInfo(MailPacket data)
        {
            id = data.id;
            cid = data.cid;
            uid = data.uid;
            message = data.message;
            rewardData = new RewardData(data.rewardType, data.rewardValue, data.rewardCount, data.rewardOption);
            rewardEventId = data.rewardEventId;
            insertTime = data.insertTime;
            shopId = data.rewardOption;
        }

        /// <summary>
        /// 거래소/개인상점 메일 전용 생성자
        /// </summary>
        public MailInfo(long seq_no, long mail_uid, int cid, int itemID, int tier_per, byte item_level, int item_count,
            long card_id1, long card_id2, long card_id3, long card_id4, long insertTime, byte sellType, int sellValue, byte isGetItem, int itemTranscend, int changedElement, int elementLevel)
        {
            this.seq_no = seq_no;
            this.mail_uid = mail_uid;
            this.cid = cid;
            this.uid = 0;
            this.insertTime = insertTime;
            this.isGetItem = isGetItem == 0 ? false : true;
            this.sellType = sellType.ToEnum<TradeShopTradeSellType>();

            bool isZeny = this.sellType == TradeShopTradeSellType.PRIVATE_SELL || this.sellType == TradeShopTradeSellType.TRADE_SELL;
            RewardType rewardType = isZeny ? RewardType.Zeny : RewardType.Item;

            if (isZeny)
            {
                rewardData = new RewardData(rewardType.ToByteValue(), sellValue * item_count - GetChargeRate(this.sellType, sellValue * item_count), item_count); // 수수료를 제한 금액 표기
            }
            else
            {
                rewardData = new RewardData(rewardType.ToByteValue(), itemID, item_count);
                rewardData.SetTranscend(itemTranscend);
                rewardData.SetChangedElement(changedElement);
                rewardData.SetElementLevel(elementLevel);
            }
        }

        /// <summary>
        /// 우편에 보여줄 내용
        /// </summary>
        public string GetMessage()
        {
            if (rewardData.ShopId > 0 && rewardData.RewardValue > 0) // 상점우편에서 추가보상이 있을 때
            {
                float rate = MathUtils.ToPermilleValue(rewardData.RewardValue); // 추가 보상 비율 1000분률
                string persent = MathUtils.GetPercentText(rate);

                return string.Concat(FilterUtils.GetServerMessage(message),
                    string.Format(" (+{0}) ", persent));
            }
            else
            {
                return FilterUtils.GetServerMessage(message);
            }
        }

        public string GetMessageTrade()
        {
            switch (sellType)
            {
                case TradeShopTradeSellType.PRIVATE_BUY:
                case TradeShopTradeSellType.TRADE_BUY:
                    return LocalizeKey._12010.ToText(); // 거래소에서 구매한 물품 입니다.

                case TradeShopTradeSellType.PRIVATE_SELL:
                case TradeShopTradeSellType.TRADE_SELL:
                    int originZeny = GetOriginZeny(sellType, rewardData.RewardValue);
                    return LocalizeKey._12009.ToText().Replace(ReplaceKey.VALUE, GetChargeRate(sellType, originZeny)); // 거래소에서 수수료 {VALUE}제니를 차감한 판매금액 입니다.
            }
            return string.Empty;
        }

        /// <summary>
        /// 수수료 반환
        /// </summary>
        private int GetChargeRate(TradeShopTradeSellType sellType, int originZeny)
        {
            switch (sellType)
            {
                case TradeShopTradeSellType.PRIVATE_SELL:
                    return (BasisType.STALL_ZENY_SALES_CHARGE_RATE.GetFloat() * originZeny).ToIntValue();

                case TradeShopTradeSellType.TRADE_SELL:
                    return (BasisType.MARKET_ZENY_SALES_CHARGE_RATE.GetFloat() * originZeny).ToIntValue();
            }

            return 0;
        }

        private int GetOriginZeny(TradeShopTradeSellType sellType, int chargedZeny)
        {
            switch (sellType)
            {
                case TradeShopTradeSellType.PRIVATE_SELL:
                    return (chargedZeny / (1f - BasisType.STALL_ZENY_SALES_CHARGE_RATE.GetFloat())).ToIntValue();

                case TradeShopTradeSellType.TRADE_SELL:
                    return (chargedZeny / (1f - BasisType.MARKET_ZENY_SALES_CHARGE_RATE.GetFloat())).ToIntValue();
            }

            return 0;
        }

        public string GetTitle()
        {
            if (MailGroup == MailGroup.PolicyViolation)
                return LocalizeKey._12025.ToText(); // [운영 정책 위반 경고 메일]

            if (MailGroup == MailGroup.OnlyMessage)
                return LocalizeKey._12026.ToText(); // [메일]

            RewardType rewardType = rewardData.RewardType;

            switch (rewardType)
            {
                case RewardType.Item:
                    {
                        switch (rewardData.ItemGroupType)
                        {
                            case ItemGroupType.Equipment:
                            case ItemGroupType.Card:
                                return LocalizeKey._12014.ToText().Replace(ReplaceKey.NAME, rewardData.ItemName); // {NAME} // 장비 이름만
                            default:
                                return LocalizeKey._12013.ToText().Replace(ReplaceKey.NAME, rewardData.ItemName).Replace(ReplaceKey.VALUE, rewardData.Count); // {NAME} {VALUE}개
                        }
                    }

                case RewardType.DefDungeonTicket:
                case RewardType.WorldBossTicket:
                case RewardType.PveTicket:
                case RewardType.StageBossTicket:
                case RewardType.CharacterShareChargeItem1:
                case RewardType.CharacterShareChargeItem2:
                case RewardType.CharacterShareChargeItem3:
                case RewardType.MultiMazeTicket:
                case RewardType.SummonMvpTicket:
                case RewardType.ZenyDungeonTicket:
                case RewardType.ExpEungeonTicket:
                case RewardType.EventMultiMazeTicket:
                    return LocalizeKey._12013.ToText().Replace(ReplaceKey.NAME, rewardData.ItemName).Replace(ReplaceKey.VALUE, rewardData.Count); // {NAME} {VALUE}개

                default:
                    return LocalizeKey._12012.ToText().Replace(ReplaceKey.VALUE, rewardData.Count).Replace(ReplaceKey.NAME, rewardType.GetItemName()); // {VALUE} {NAME} // 예 : 5000 제니
            }
        }

        /// <summary>
        /// 우편 날짜
        /// </summary>
        public string GetDate()
        {
            return insertTime.ToDateTime().ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 상점 ID 반환
        /// </summary>
        public int GetShopId()
        {
            return shopId;
        }

        /// <summary>
        /// 우편함 보상
        /// </summary>
        public RewardData GetReward()
        {
            return rewardData;
        }

        /// <summary>
        /// 우편함 보상 무게
        /// </summary>
        public int GetTotalWeight()
        {            
            return GetReward().TotalWeight();
        }
    }
}
