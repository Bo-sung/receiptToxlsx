namespace Ragnarok
{
    /// <summary>
    /// 보스 몬스터 소환 정보
    /// </summary>
    public interface IBossMonsterSpawnData
    {
        /// <summary>
        /// 소환 아이디
        /// </summary>
        int BossMonsterId { get; }

        /// <summary>
        /// 소환 레벨
        /// </summary>
        int Level { get; }

        /// <summary>
        /// 소환 크기
        /// </summary>
        float Scale { get; }
    }
}