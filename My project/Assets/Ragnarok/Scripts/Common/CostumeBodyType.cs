namespace Ragnarok
{
    public enum CostumeBodyType
    {
        /// <summary>
        /// 남여 공용
        /// </summary>
        Unisex = 0,

        /// <summary>
        /// 남성
        /// </summary>
        Male = 1,

        /// <summary>
        /// 여성
        /// </summary>
        Female = 2,
    }

    public static class CostumeBodyTypeExtensions
    {
        public static bool IsInvisible(this CostumeBodyType type, Gender gender)
        {
            switch (type)
            {
                case CostumeBodyType.Unisex:
                    return true;

                case CostumeBodyType.Male:
                    return gender == Gender.Male;

                case CostumeBodyType.Female:
                    return gender == Gender.Female;

                default:
                    return false;
            }
        }
    }
}
