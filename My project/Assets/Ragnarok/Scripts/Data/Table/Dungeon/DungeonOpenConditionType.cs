namespace Ragnarok
{
    /// <summary>
    /// 던전 오픈 조건
    /// </summary>
    public enum DungeonOpenConditionType
    {
        /// <summary>
        /// 특정 직업 레벨값 도달
        /// </summary>
        JobLevel = 1,

        /// <summary>
        /// 메인 퀘스트
        /// </summary>
        MainQuest = 2,

        /// <summary>
        /// 시나리오 미로
        /// </summary>
        ScenarioMaze = 3,

        /// <summary>
        /// 업데이트 예정
        /// </summary>
        UpdateLater = 99,
    }
}