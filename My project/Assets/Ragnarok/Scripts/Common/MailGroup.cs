namespace Ragnarok
{
    public enum MailGroup
    {
        /// <summary>
        /// 보상있는 일반 메일
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 광고 메일 (광고 시청 후 보상 받을 수 있음)
        /// </summary>
        Advertisement = 1,

        /// <summary>
        /// 페이스북 (링크 이동 후 보상 받을 수 있음)
        /// </summary>
        Facebook = 2,

        /// <summary>
        /// [운영 정책 위반 경고 메일] (보상 없이 메시지만 있는 메일)
        /// </summary>
        PolicyViolation = 3,

        /// <summary>
        /// 메시지 전용 (보상 없이 메시지만 있는 메일)
        /// </summary>
        OnlyMessage = 4,
    }
}
