namespace Ragnarok
{
    public enum BoxType
    {
        None = 0,
        /// <summary>
        /// 랜덤 박스
        /// </summary>
        RateCheck = 1,
        /// <summary>
        /// 패키지 박스
        /// </summary>
        Reward = 2,      
        /// <summary>
        /// 뽑기 상자
        /// </summary>
        Gacha = 6,
        /// <summary>
        /// 즉시 오픈
        /// </summary>
        DirectOpen = 8,

        /// <summary>
        /// 직업 참조 보상 (노비스는 열수 없다)
        /// </summary>
        JobRef = 11,

        /// <summary>
        /// 2차 전직 이상부터 오픈 할수 있는 상자
        /// </summary>
        JobGradeRef = 12,
    }

    public static class BoxTypeExtensions
    {       
        /// <summary>
        /// 아이템 획득처 표시
        /// </summary>
        /// <param name="boxType"></param>
        /// <returns></returns>
        public static bool IsItemSourceCategoryType(this BoxType boxType)
        {
            switch (boxType)
            {
                case BoxType.RateCheck:
                case BoxType.Reward:
                case BoxType.Gacha:
                    return true;
            }
            return false;
        }
    }
}