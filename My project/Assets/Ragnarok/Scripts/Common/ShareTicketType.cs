using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public enum ShareTicketType
    {
        /// <summary>
        /// 클라 전용
        /// </summary>
        None = -1,

        /// <summary>
        /// 일일무료
        /// </summary>
        DailyFree = 0,
        /// <summary>
        /// 캐릭터셰어 충전 아이템 1
        /// </summary>
        ChargeItem1 = 1,
        /// <summary>
        /// 캐릭터셰어 충전 아이템 2
        /// </summary>
        ChargeItem2 = 2,
        /// <summary>
        /// 캐릭터셰어 충전 아이템 3
        /// </summary>
        ChargeItem3 = 3,
    }

    public static class ShareTicketTypeExtensions
    {
        /// <summary>
        /// 충전 시간 반환
        /// </summary>
        public static System.TimeSpan ToTimeSpan(this ShareTicketType shareTicketType)
        {
            switch (shareTicketType)
            {
                case ShareTicketType.DailyFree:
                    return System.TimeSpan.FromMilliseconds(BasisType.FREE_USE_CHAR_SHARE_TIME.GetInt());

                case ShareTicketType.ChargeItem1:
                    return System.TimeSpan.FromMilliseconds(BasisType.CHAR_SHARE_CHARGE_ITEM_1.GetInt());

                case ShareTicketType.ChargeItem2:
                    return System.TimeSpan.FromMilliseconds(BasisType.CHAR_SHARE_CHARGE_ITEM_2.GetInt());

                case ShareTicketType.ChargeItem3:
                    return System.TimeSpan.FromMilliseconds(BasisType.CHAR_SHARE_CHARGE_ITEM_3.GetInt());
            }

            return System.TimeSpan.Zero;
        }
    }
}