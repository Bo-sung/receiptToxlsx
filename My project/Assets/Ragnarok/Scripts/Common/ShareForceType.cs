namespace Ragnarok
{
    /// <summary>
    /// 쉐어포스 타입
    /// </summary>
    public enum ShareForceType
    {
        /// <summary>
        /// 쉐어포스1
        /// </summary>
        ShareForce1 = 1,

        /// <summary>
        /// 쉐어포스2
        /// </summary>
        ShareForce2 = 2,

        /// <summary>
        /// 쉐어포스3
        /// </summary>
        ShareForce3 = 3,

        /// <summary>
        /// 쉐어포스4
        /// </summary>
        ShareForce4 = 4,

        /// <summary>
        /// 쉐어포스5
        /// </summary>
        ShareForce5 = 5,

        /// <summary>
        /// 쉐어포스6
        /// </summary>
        ShareForce6 = 6,
    }

    public static class ShareForceTypeExtensions
    {
        public static string GetTextureName(this ShareForceType type)
        {
            switch (type)
            {
                case ShareForceType.ShareForce1:
                    return "ShareForce1";

                case ShareForceType.ShareForce2:
                    return "ShareForce2";

                case ShareForceType.ShareForce3:
                    return "ShareForce3";

                case ShareForceType.ShareForce4:
                    return "ShareForce4";

                case ShareForceType.ShareForce5:
                    return "ShareForce5";

                case ShareForceType.ShareForce6:
                    return "ShareForce6";

                default:
                    return default;
            }
        }

        public static int GetNameId(this ShareForceType type)
        {
            switch (type)
            {
                case ShareForceType.ShareForce1:
                    return LocalizeKey._48257; // 쉐어 포스 타임 실린더

                case ShareForceType.ShareForce2:
                    return LocalizeKey._48258; // 쉐어 포스 타임 베어링

                case ShareForceType.ShareForce3:
                    return LocalizeKey._48259; // 쉐어 포스 타임 나이프

                case ShareForceType.ShareForce4:
                    return LocalizeKey._48260; // 쉐어 포스 타임 실드

                case ShareForceType.ShareForce5:
                    return LocalizeKey._48261; // 쉐어 포스 타임 키

                case ShareForceType.ShareForce6:
                    return LocalizeKey._48262; // 쉐어 포스 타임 커터

                default:
                    return default;
            }
        }
    }

}
