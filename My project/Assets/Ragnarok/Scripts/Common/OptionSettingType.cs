namespace Ragnarok
{
    /// <summary>
    /// <see cref="Protocol.REQUEST_OPTION_SETTING"/>
    /// PutByte("1", type)
    /// </summary>
    public enum OptionSettingType
    {
        /// <summary>
        /// 푸시
        /// </summary>
        Push =1, 

        /// <summary>
        /// 야간 푸시
        /// </summary>
        NightPush = 2,
        
        /// <summary>
        /// 언어
        /// </summary>
        Language = 3,

        /// <summary>
        /// 셰어 푸시
        /// </summary>
        SharePush = 4,
    }
}