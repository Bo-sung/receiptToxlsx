namespace Ragnarok
{
    /// <summary>
    /// 패키지 타입
    /// </summary>
    public enum PackageType
    {
        /// <summary>
        /// 패키지 아님
        /// </summary>
        None = 0,

        /// <summary>
        /// 일반 패키지
        /// </summary>
        GeneralPackage = 1,

        /// <summary>
        /// 팝업 상품 패키지 (직업 레벨 달성 패키지)
        /// </summary>
        PopUpProductPackage = 2,

        /// <summary>
        /// 공유 보상 UP 패키지
        /// </summary>
        SharePackage = 3,

        /// <summary>
        /// 카프라 패키지
        /// </summary>
        KafraPackage = 4,

        /// <summary>
        /// 냥다래 패키지
        /// </summary>
        CatCoinPackage = 5,

        /// <summary>
        /// 레벨 달성 패키지
        /// </summary>
        LevelAchievePackage = 6,

        /// <summary>
        /// 시나리오 패키지
        /// </summary>
        ScenarioPackage = 7,

        /// <summary>
        /// 첫결제 보상(구매하는 상품 아님) 캐쉬 첫결제시 추가 지급 상품
        /// </summary>
        FirstPaymentReward = 8,

        /// <summary>
        /// 금빛 영양제 패키지
        /// </summary>
        TreePackage = 9,

        /// <summary>
        /// 배틀 패스 패키지
        /// </summary>
        BattlePassPackage = 10,

        /// <summary>
        /// 온버프 패스 패키지
        /// </summary>
        OnBuffPassPackage = 11,
    }

    public static class PackageTypeExtension
    {
        /// <summary>
        /// 1회만 구매할수 있는 패키지
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBuyLimitPackage(this PackageType type)
        {
            switch (type)
            {
                // 1회만 구매할수 있는 상품
                case PackageType.PopUpProductPackage:
                case PackageType.SharePackage:
                case PackageType.LevelAchievePackage:
                case PackageType.ScenarioPackage:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 기간제 패키지
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsPeriodProductPackage(this PackageType type)
        {
            switch (type)
            {
                // 28일 보상 받은 후에 재구매 가능 상품
                case PackageType.KafraPackage:
                case PackageType.CatCoinPackage:

                // 버프 시간 종료 후 재구매 가능
                case PackageType.TreePackage:
                    return true;
            }
            return false;
        }
    }
}