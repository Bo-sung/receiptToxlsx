namespace Ragnarok
{
    /// <summary>
    /// 오픈 조건이 존재하는 던전 데이터
    /// </summary>
    public interface IOpenConditional
    {
        /// <summary>
        /// 오픈 조건
        /// </summary>
        DungeonOpenConditionType ConditionType { get; }

        /// <summary>
        /// 오픈 조건값
        /// </summary>
        int ConditionValue { get; }
    }
}