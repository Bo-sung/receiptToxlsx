using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using Ragnarok.View.CharacterShare;
using System.Globalization;
using Ragnarok.View;

namespace Ragnarok
{
    public sealed class ShopInfo : DataInfo<ShopData>, CharacterShareChargePopupView.IInitializer, AddGoodsView.IInput
    {
        private ObscuredInt buyCount; // 구매 횟수

        public int ID => data.id;
        /// <summary>
        /// 상품 이름
        /// </summary>
        public string Name => data.name_id.ToText();
        /// <summary>
        /// 상품 설명
        /// </summary>
        public string Description => data.des_id.ToText();
        /// <summary>
        /// 아이콘 이름
        /// </summary>
        public string IconName => GetIconName();
        /// <summary>
        /// 구매 재화 타입
        /// </summary>
        public CoinType CoinType => data.pay_type.ToEnum<CoinType>();
        /// <summary>
        /// 보상 타입
        /// </summary>
        public RewardType RewardType => data.goods_type.ToEnum<RewardType>();
        /// <summary>
        /// 구매 비용
        /// </summary>
        public int Cost => data.pay_value;
        /// <summary>
        /// 구매 비용 표시
        /// </summary>
        public string CostText => GetCostText();
        /// <summary>
        /// 보상 정보
        /// </summary>
        public RewardData Reward { get; private set; }
        /// <summary>
        /// 탭
        /// </summary>
        public int Tab => data.shop_tab;
        /// <summary>
        /// 스토어 타입
        /// </summary>
        public StoreType StoreType => BuildSettings.Instance.StoreType;
        /// <summary>
        /// 현금 구매 결제 ID
        /// </summary>
        public string ProductId => GetProductId();
        /// <summary>
        /// 상품 정렬 순서
        /// </summary>
        public int SortIndex => data.tab_order;
        /// <summary>
        /// 판매 타입 
        /// </summary>
        public SellType SellType => data.sell_type.ToEnum<SellType>();
        /// <summary>
        /// 판매 타입 값
        /// </summary>
        public int SellValue => data.sell_value;
        /// <summary>
        /// 횟수 제한 초기화 타입
        /// </summary>
        public LimitDayType LimitDayType => data.limit_day_type.ToEnum<LimitDayType>();
        /// <summary>
        /// 첫 구매 시 2배 기간 타입
        /// </summary>
        public FirstDayType FirstDayType => data.first_day_type.ToEnum<FirstDayType>();
        /// <summary>
        /// 구매 횟수
        /// </summary>
        public int BuyCount => buyCount;
        /// <summary>
        /// 구매 제한 횟수
        /// </summary>
        public int BuyLimitCount => data.limit_day_count;
        /// <summary>
        /// 구매 제한 상품 타입 여부
        /// </summary>
        public bool IsBuyLimit => GetIsBuyLimit();
        /// <summary>
        /// 아이템 구매 가능 여부
        /// </summary>
        public bool CanBuyLimit => GetCanBuy();
        /// <summary>
        /// 첫구매까지 남은시간
        /// </summary>
        public RemainTime FirstRemainTime;
        /// <summary>
        /// 첫구매시 추가상품 비율
        /// </summary>
        public int FirstRate => data.first_rate;
        /// <summary>
        /// 다중 구매 가능 여부
        /// </summary>
        public bool IsMultiBuy => GetIsMultiBuy();
        /// <summary>
        /// New, Best, Hot Flag
        /// </summary>
        public ShopState ShopState => data.state.ToEnum<ShopState>();
        /// <summary>
        /// 기간제 상품 구매 제한 시간
        /// </summary>
        public RemainTime RemainTime { get; private set; }
        /// <summary>
        /// 기간 단위 판매 아이템 초기화까지 남은시간
        /// </summary>
        public RemainTime ResetRemainTime { get; private set; }
        /// <summary>
        /// 상점 구매 제한 타입
        /// </summary>
        public ShopLimitType ShopLimitType => data.limit_type.ToEnum<ShopLimitType>();
        /// <summary>
        /// 상점 표시 타입
        /// </summary>
        public ShopVisableType ShopVisableType => data.visable_type.ToEnum<ShopVisableType>();
        /// <summary>
        /// 상점 표시 타입 중 직업레벨 제한 체크
        /// </summary>
        public int JobLevel => data.visable_value;
        /// <summary>
        /// 구매 가능 캐릭터 직업(클래스 비트 타입 형식으로 사용)
        /// </summary>
        public JobFilter JobFilter => data.job.ToEnum<JobFilter>();
        /// <summary>
        /// 추가 보상 아이템 타입
        /// </summary>
        public RewardType AddGoodsType => data.add_goods_type.ToEnum<RewardType>();
        /// <summary>
        /// 추가 보상 아이템이 있는지 여부
        /// </summary>
        public bool IsAddGoods => AddGoodsType != RewardType.None;
        /// <summary>
        /// 상점 패키지 관련 타입
        /// </summary>
        public ShopConditionType ShopConditionType => data.condition_type.ToEnum<ShopConditionType>();
        /// <summary>
        /// 기간 한정 상품 초기화까지 남은 시간 세팅여부
        /// </summary>
        public bool IsSetResetRemainTime { get; private set; }
        /// <summary>
        /// 기간제 상품 다음 첫구매 보상까지 남은 시간 세팅여부
        /// </summary>
        public bool IsSetFirstItemRemainTime { get; private set; }
        /// <summary>
        /// 상점 메일 아이콘 이름
        /// </summary>
        public string MailIconName => data.mail_icon;
        /// <summary>
        /// 패키지 타입
        /// </summary>
        public PackageType PackageType => data.package_type.ToEnum<PackageType>();
        /// <summary>
        /// 결제시 추가되는 마일리지
        /// </summary>
        public int Mileage => data.mileage;

        /// <summary>
        /// 캐쉬 상품 가격
        /// </summary>
        private string price;

        public void SetReward()
        {
            Reward = new RewardData(data.goods_type, data.goods_value, data.goods_count);
        }

        private string GetDefaultCost()
        {
            return MathUtils.ToPercentValue(data.usd_price).ToString("C", CultureInfo.GetCultureInfo("en-US"));
        }

        private string GetCostText()
        {
            switch (CoinType)
            {
                case CoinType.Cash:

                    if (Application.isEditor)
                        return GetDefaultCost();

                    if (string.IsNullOrEmpty(price))
                        return GetDefaultCost();

                    return price;

                case CoinType.Ad:
                    return LocalizeKey._58005.ToText(); // 광고

                case CoinType.Free:
                    return LocalizeKey._58015.ToText(); // 무료
            }
            return $"{Cost:N0}";
        }

        private string GetProductId()
        {
            switch (StoreType)
            {
                case StoreType.GOOGLE:
                    return data.google_product_id;
                case StoreType.APPLE:
                    return data.apple_product_id;
                case StoreType.TSTORE:
                    return data.tstore_product_id;
            }
            return default;
        }

        private string GetIconName()
        {
            // 아이콘 이름이 있는 경우
            if(IsValueIconName())
                return data.icon_name;

            if (RewardType == RewardType.Item)
                return Reward.IconName;

            return data.icon_name;
        }

        public bool IsValueIconName()
        {
            return !string.IsNullOrEmpty(data.icon_name) && !data.icon_name.Equals("0");
        }

        public bool GetCanBuy(int buyCount = 1)
        {
            if (!IsBuyLimit)
                return true;

            return this.buyCount + buyCount <= BuyLimitCount;
        }

        /// <summary>
        /// 첫구매 보상 삳품인지 여부
        /// </summary>
        /// <returns></returns>
        public bool IsFirstBuyItem()
        {
            return FirstDayType != FirstDayType.None;
        }

        public void Reset()
        {
            buyCount = 0;
            IsSetResetRemainTime = false;
            IsSetFirstItemRemainTime = false;
        }

        public void AddBuyCount(int addCount = 1)
        {
            SetBuyCount(buyCount + addCount);
        }

        public void SetBuyCount(int buyCount)
        {
            this.buyCount = buyCount;
        }

        public void SetFirstRemainTime(long remainTime)
        {
            FirstRemainTime = remainTime;
            IsSetFirstItemRemainTime = true; // 남은 시간이 있어 추가 보상이 없다.
        }

        /// <summary>
        /// 다중 구매 가능한지 여부
        /// </summary>
        /// <returns></returns>
        bool GetIsMultiBuy()
        {
            switch (CoinType)
            {
                case CoinType.Cash:
                case CoinType.Ad:
                case CoinType.Free:
                    return false;
            }

            if (RewardType == RewardType.Item)
                return true;

            return false;
        }

        bool GetIsBuyLimit()
        {
            // 상시 판매 & 구매 횟수 제한 없음
            if (SellType == SellType.SellAlways)
                return false;

            // 단위 기간 상품 & 구매횟수 제한
            if (SellType == SellType.ResetPeriodProduct && BuyLimitCount > 0)
                return true;

            // 기간제 상품 & 구매횟수 제한
            if (SellType == SellType.SellPeriodProduct && BuyLimitCount > 0)
                return true;

            return false;
        }

        /// <summary>
        /// 캐쉬 상품 가격 세팅
        /// </summary>
        /// <param name="price"></param>
        public void SetPrice(string price)
        {
            this.price = price;
        }

        public string GetLimitDayTypeToText()
        {
            string limitText = ShopLimitType == ShopLimitType.Account ? LocalizeKey._8054.ToText() : string.Empty; // (계정)

            if (SellType == SellType.SellAlways)
                return LocalizeKey._8035.ToText(); // 구매 횟수 제한 없음

            if (SellType == SellType.SellPeriodProduct)
                return LocalizeKey._8036.ToText().Replace(ReplaceKey.NAME, limitText); // 기간 한정{NAME}

            switch (LimitDayType)
            {
                case LimitDayType.NoLimit:
                    return LocalizeKey._8037.ToText().Replace(ReplaceKey.NAME, limitText); // 구매 한정{NAME}

                case LimitDayType.DailyLimit:
                    return LocalizeKey._8038.ToText().Replace(ReplaceKey.NAME, limitText); // 일일 한정{NAME}

                case LimitDayType.WeeklyLimit:
                    return LocalizeKey._8039.ToText().Replace(ReplaceKey.NAME, limitText); // 주간 한정{NAME}

                case LimitDayType.MonthlyLimit:
                    return LocalizeKey._8040.ToText().Replace(ReplaceKey.NAME, limitText); // 월간 한정{NAME}
            }
            return string.Empty;
        }

        public bool IsSale()
        {
            return data.sale_pay_value > 0;
        }

        /// <summary>
        /// 세일 이전 가격 표시용
        /// </summary>
        /// <returns></returns>
        public string GetSalePrice()
        {
            return $"[s]{data.sale_pay_value:N0}";
        }

        public string GetSalePersent()
        {
            return $"{data.sale}%";
        }

        /// <summary>
        /// 구매 제한 시간
        /// </summary>
        /// <param name="remainTime"></param>
        public void SetBuyRemainTime(long remainTime)
        {
            RemainTime = remainTime;
        }

        /// <summary>
        /// 기간단위 상품 초기화 남은시간
        /// </summary>
        /// <param name="resetRemainTime"></param>
        public void SetResetRemainTime(long resetRemainTime)
        {
            ResetRemainTime = resetRemainTime;
            IsSetResetRemainTime = true;
        }

        /// <summary>
        /// 기간제 아이템 여부
        /// </summary>
        /// <returns></returns>
        public bool IsPeriedItem()
        {
            return SellType == SellType.SellPeriodProduct;
        }

        public RewardData GetAddGoodsReward()
        {
            return new RewardData(data.add_goods_type, data.add_goods_value, data.add_goods_count);
        }

        /// <summary>
        /// 패키지 상품 여부
        /// </summary>
        /// <returns></returns>
        public bool IsPackage()
        {
            // 탭으로 패키지 여부 구분
            return data.shop_tab == 0;
        }

        public int GetGroupId()
        {
            return data.goods_value;
        }

        /// <summary>
        /// 구매 직후부터 구매 제한시간이 시작되는 상품
        /// </summary>
        /// <returns></returns>
        public bool IsBuyLimitResetType()
        {
            switch (LimitDayType)
            {
                case LimitDayType.Lmit_6H:
                case LimitDayType.Lmit_12H:
                case LimitDayType.Lmit_24H:
                case LimitDayType.Lmit_36H:
                case LimitDayType.Lmit_48H:
                case LimitDayType.Lmit_60H:
                    return true;
            }

            return false;

        }

        public bool IsSortCanbuyLimit()
        {
            if (IsBuyLimitResetType())
                return true;

            return CanBuyLimit;
        }

        /// <summary>
        /// 직업 레벨 패키지 UI 표시 타입 (0 : 표기 없음, 1 : productvalue_value배 가치, 2: productvalue_value% 할인
        /// </summary>
        /// <returns></returns>
        public ShopProductType GetProductValueType()
        {
            return data.productvalue_type.ToEnum<ShopProductType>();
        }

        public int GetProductValue()
        {
            return data.productvalue_value;
        }

        public string GetSubDescription()
        {
            return data.des_id_2.ToText();
        }        
    }
}