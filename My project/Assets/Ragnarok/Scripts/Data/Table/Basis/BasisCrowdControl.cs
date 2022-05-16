namespace Ragnarok
{
    public enum BasisCrowdControl
    {
        /// <summary>
        /// 수면 - 물리방어력 감소율
        /// </summary>
        Sleep_DefDecreaseRate = 1,

        /// <summary>
        /// 수면 - 마법방어력 감소율
        /// </summary>
        Sleep_MDefDecreaseRate = 2,

        /// <summary>
        /// 출혈 - 전체 체력 퍼센트 도트대미지
        /// </summary>
        Bleeding_DotDamageRate = 3,

        /// <summary>
        /// 출혈 - 물리방어력 감소율
        /// </summary>
        Bleeding_DefDecreaseRate = 4,

        /// <summary>
        /// 화상 - 전체 체력 퍼센트 도트대미지
        /// </summary>
        Burning_DotDamageRate = 5,

        /// <summary>
        /// 중독 - 전체 체력 퍼센트 도트대미지
        /// </summary>
        Poison_DotDamageRate = 6,

        /// <summary>
        /// 중독 - 마법방어력 감소율
        /// </summary>
        Poison_MDefDecreaseRate = 7,

        /// <summary>
        /// 저주 - STR 감소율
        /// </summary>
        Curse_StrDecreaseRate = 8,

        /// <summary>
        /// 저주 - LUK 감소율
        /// </summary>
        Curse_LukDecreaseRate = 9,

        /// <summary>
        /// 빙결 - 이동속도 감소율
        /// </summary>
        Freezing_MoveSpdDecreaseRate = 9,

        /// <summary>
        /// 빙결 - 공격속도 감소율
        /// </summary>
        Freezing_AtkSpdDecreaseRate = 9,

        /// <summary>
        /// 동빙 - 물리방어력 감소율
        /// </summary>
        Frozen_DefDecreaseRate = 9,

        /// <summary>
        /// 동빙 - 마법방어력 감소율
        /// </summary>
        Frozen_MDefDecreaseRate = 9,
    }
}