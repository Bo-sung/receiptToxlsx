namespace Ragnarok
{
    public enum ForestMazeState
    {
        /// <summary>
        /// 아무 상태 아님
        /// </summary>
        None,

        /// <summary>
        /// 미로 진행
        /// </summary>
        Maze,

        /// <summary>
        /// 중간보스 전투
        /// </summary>
        MiddleBossBattle,

        /// <summary>
        /// 최종보스 전투
        /// </summary>
        BossBattle,
    }
}