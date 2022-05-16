namespace Ragnarok
{
    /// <summary>
    /// 일반 몬스터 소환 정보
    /// </summary>
    public interface INormalMonsterSpawnData
    {
        /// <summary>
        /// 소환 가능한 최대 인덱스
        /// </summary>
        int MaxIndex { get; }

        /// <summary>
        /// 소환 비용
        /// </summary>
        int Cost { get; }

        /// <summary>
        /// 소환 레벨
        /// </summary>
        int Level { get; }

        /// <summary>
        /// 인덱스에 해당하는 소환 정보
        /// </summary>
        (int monsterId, int spawnRate) GetSpawnInfo(int index);
    }
}