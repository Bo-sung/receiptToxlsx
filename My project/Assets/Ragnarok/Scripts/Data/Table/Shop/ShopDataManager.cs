using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public sealed class ShopDataManager : Singleton<ShopDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, ShopData> dataDic;

        public ResourceType DataType => ResourceType.ShopDataDB;

        public ShopDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, ShopData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ShopData data = new ShopData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public ShopData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
                throw new System.ArgumentException($"상점 데이터가 존재하지 않습니다: id = {id}");

            return dataDic[id];
        }

        public Dictionary<ObscuredInt, ShopData> GetShopDatas()
        {
            return dataDic;
        }

        public IEnumerable<ObscuredString> GetProductIds()
        {
            StoreType storeType = BuildSettings.Instance.StoreType;
            switch (storeType)
            {
                case StoreType.GOOGLE:
                    return dataDic.Values
                                  .Where(x => x.pay_type == 3 && !string.IsNullOrEmpty(x.google_product_id))
                                  .Select(x => x.google_product_id);
                case StoreType.APPLE:
                    return dataDic.Values
                                  .Where(x => x.pay_type == 3 && !string.IsNullOrEmpty(x.apple_product_id))
                                  .Select(x => x.apple_product_id);
                case StoreType.TSTORE:
                    return dataDic.Values
                                  .Where(x => x.pay_type == 3 && !string.IsNullOrEmpty(x.tstore_product_id))
                                  .Select(x => x.tstore_product_id);
            }
            return default;
        }

        public ShopData GetByProductId(string productId)
        {
            StoreType storeType = BuildSettings.Instance.StoreType;
            switch (storeType)
            {
                case StoreType.GOOGLE:
                    return dataDic.Values.FirstOrDefault(x => x.pay_type == 3 && x.google_product_id == productId);
                case StoreType.APPLE:
                    return dataDic.Values.FirstOrDefault(x => x.pay_type == 3 && x.apple_product_id == productId);
                case StoreType.TSTORE:
                    return dataDic.Values.FirstOrDefault(x => x.pay_type == 3 && x.tstore_product_id == productId);
            }
            return default;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

            int chapterCount = 0;
            int JobLevelCount = 0;
            int shareUpCount = 0;
            int firstRewardCount = 0;
            int treeRewardCount = 0;

            foreach (var item in dataDic.Values)
            {
                if (item.goods_type.ToEnum<RewardType>() == RewardType.Item)
                {
                    if (ItemDataManager.Instance.Get(item.goods_value) == null)
                    {
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 없는 보상 아이템({nameof(item.goods_value)})={item.goods_value}");
                    }
                }

                bool isSale = item.sale_pay_value > 0;
                bool isFirstBuyItem = item.first_day_type.ToEnum<FirstDayType>() != FirstDayType.None;
                bool isAddGoods = item.add_goods_type.ToEnum<RewardType>() != RewardType.None;

                // 냥다래 구매시 예외적으로 첫구매 추가보상과 구매 추가보상이 중복으로 사용가능하다
                // UI에 표시할때 구매 추가보상은 표시하지 않고 첫구매 추가보상만 표시해준다.
                if (item.goods_type.ToEnum<RewardType>() == RewardType.CatCoin)
                {
                    if (isSale)
                    {
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 냥다래 구매는 세일을 할 수 없다.");
                    }
                }
                else
                {
                    // 세일, 첫구매 추가보상, 구매 추가보상 검증(중복해서 사용 불가)                   
                    int addCount = 0;
                    if (isSale)
                        addCount++;

                    if (isFirstBuyItem)
                        addCount++;

                    if (isAddGoods)
                        addCount++;

                    if (addCount > 1)
                    {
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 세일, 첫구매 추가보상, 구매 추가보상(중복해서 사용 불가)");
                    }
                }

                CoinType payType = item.pay_type.ToEnum<CoinType>();
                RewardType rewardType = item.goods_type.ToEnum<RewardType>();

                // 첫구매 검증 (재화 타입만 추가 보상을 받을수 있다)
                if (isFirstBuyItem)
                {
                    if (IsFirstItemLimit())
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 첫구매 추가보상에 사용 불가 타입, 보상타입={rewardType}");

                    bool IsFirstItemLimit()
                    {
                        switch (rewardType)
                        {
                            case RewardType.Zeny:
                            case RewardType.CatCoin:
                            case RewardType.CatCoinFree:
                                return false;
                        }
                        return true;
                    }
                }

                // 현금결제의 경우 무료 냥다래를 보상으로 줄수 없다.
                if (payType == CoinType.Cash && rewardType == RewardType.CatCoinFree)
                {
                    throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 현금결제의 경우 무료 냥다래를 보상으로 줄수 없다.");
                }

                ShopConditionType shopConditionType = item.condition_type.ToEnum<ShopConditionType>();
                if (shopConditionType == ShopConditionType.JobLevel)
                {
                    if (JobLevelCount > 1)
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 직업레벨 패키지는 중복 테이터 불가");

                    JobLevelCount++;
                }

                if (shopConditionType == ShopConditionType.Scenario)
                {
                    if (chapterCount > 1)
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 시나리오 패키지는 중복 테이터 불가");

                    chapterCount++;
                }

                if (shopConditionType == ShopConditionType.ShareUp)
                {
                    if (shareUpCount > 1)
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 공유 보상 UP 패키지는 중복 테이터 불가");

                    shareUpCount++;
                }

                if (shopConditionType == ShopConditionType.FirstPurchaseReward)
                {
                    if (firstRewardCount > 1)
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 첫결제 보상은 중복 테이터 불가");

                    firstRewardCount++;
                }

                if (shopConditionType == ShopConditionType.TreeReward)
                {
                    if (treeRewardCount > 1)
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 금빛 영양제 패키지는 중복 테이터 불가");

                    treeRewardCount++;
                }

                // 미사용 판매 타입 체크
                SellType sellType = item.sell_type.ToEnum<SellType>();

                if (sellType == SellType.ComebackUser || sellType == SellType.NewUser)
                {
                    throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 미사용 판매 타입 {nameof(item.sell_type)}={item.sell_type}");
                }

                if (sellType == SellType.SellAlways && item.limit_day_type > 0)
                {
                    throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 상시판매 상품은 구매횟수 제한 불가 {nameof(item.sell_type)}={item.sell_type}, {nameof(item.limit_day_type)}={item.limit_day_type}");
                }

                // 특수 상점 타입일때 구매 제한 타입 사용 불가
                if (shopConditionType != ShopConditionType.None)
                {
                    if (item.limit_day_count != 0 || item.limit_day_type != 0)
                    {
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 특수패키지 상품 구매제한타입 사용불가 {nameof(shopConditionType)}={shopConditionType}, {nameof(item.limit_day_type)}={item.limit_day_type}, {nameof(item.limit_day_count)}={item.limit_day_count}");
                    }
                }

                PackageType packageType = item.package_type.ToEnum<PackageType>();

                // 패키지 상품 캐쉬 결제 체크
                switch (packageType)
                {
                    case PackageType.GeneralPackage:
                    case PackageType.PopUpProductPackage:
                    case PackageType.SharePackage:
                    case PackageType.KafraPackage:
                    case PackageType.CatCoinPackage:
                    case PackageType.LevelAchievePackage:
                    case PackageType.ScenarioPackage:
                    case PackageType.TreePackage:
                        if (payType != CoinType.Cash)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 캐시로만 결제가능한 상품 {nameof(packageType)}={packageType}");
                        }
                        break;
                }

                // 패키지 별 shopConditionType 검증
                switch (packageType)
                {
                    case PackageType.GeneralPackage:
                    case PackageType.PopUpProductPackage:
                        if (shopConditionType != ShopConditionType.None)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, ConditionType 0만 사용가능한 패키지 {nameof(packageType)}={packageType}, {nameof(shopConditionType)}={shopConditionType}");
                        }
                        break;
                    case PackageType.FirstPaymentReward:
                        if (shopConditionType != ShopConditionType.FirstPurchaseReward)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, ConditionType 4만 사용가능한 패키지 {nameof(packageType)}={packageType}, {nameof(shopConditionType)}={shopConditionType}");
                        }
                        break;
                    case PackageType.SharePackage:
                        if (shopConditionType != ShopConditionType.ShareUp)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, ConditionType 4만 사용가능한 패키지 {nameof(packageType)}={packageType}, {nameof(shopConditionType)}={shopConditionType}");
                        }
                        break;
                    case PackageType.KafraPackage:
                    case PackageType.CatCoinPackage:
                        if (shopConditionType != ShopConditionType.EveryDay)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, ConditionType 1만 사용가능한 패키지 {nameof(packageType)}={packageType}, {nameof(shopConditionType)}={shopConditionType}");
                        }
                        break;
                    case PackageType.LevelAchievePackage:
                        if (shopConditionType != ShopConditionType.JobLevel)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, ConditionType 2만 사용가능한 패키지 {nameof(packageType)}={packageType}, {nameof(shopConditionType)}={shopConditionType}");
                        }
                        break;
                    case PackageType.ScenarioPackage:
                        if (shopConditionType != ShopConditionType.Scenario)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, ConditionType 3만 사용가능한 패키지 {nameof(packageType)}={packageType}, {nameof(shopConditionType)}={shopConditionType}");
                        }
                        break;
                    case PackageType.TreePackage:
                        if (shopConditionType != ShopConditionType.TreeReward)
                        {
                            throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, ConditionType 6만 사용가능한 패키지 {nameof(packageType)}={packageType}, {nameof(shopConditionType)}={shopConditionType}");
                        }
                        break;
                }

                // 보상 그룹 테이블 참조하는 패키지
                if (IsRwardGroupPackage())
                {
                    if (rewardType != RewardType.RefRewardGroup)
                    {
                        throw new System.Exception($"23.상점 테이블 오류 ID={item.id}, 보상 그룹 테이블(98)만 사용할수 있는 패키지  {nameof(packageType)}={packageType}, {nameof(rewardType)}={rewardType}");
                    }
                }

                // 보상 그룹 테이블 참조하는 패키지
                bool IsRwardGroupPackage()
                {
                    switch (packageType)
                    {
                        case PackageType.GeneralPackage:
                        case PackageType.PopUpProductPackage:
                        case PackageType.KafraPackage:
                        case PackageType.CatCoinPackage:
                        case PackageType.LevelAchievePackage:
                        case PackageType.ScenarioPackage:
                            return true;
                    }
                    return false;
                }
            }
#endif
        }
    }
}