namespace Ragnarok
{
    /// <summary>
    /// <see cref="DefenceDungeonData"/>
    /// <see cref="WorldBossData"/>
    /// <see cref="ClickerDungeonData"/>
    /// <see cref="CentralLabData"/>
    /// </summary>
    public interface IDungeonGroup : IOpenConditional
    {
        /// <summary>
        /// 던전 타입
        /// </summary>
        DungeonType DungeonType { get; }

        /// <summary>
        /// 난이도
        /// </summary>
        int Difficulty { get; }

        /// <summary>
        /// 던전 이름
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 아이디
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 몬스터 정보
        /// </summary>
        (int monsterId, MonsterType type, int monsterLevel)[] GetMonsterInfos();

        /// <summary>
        /// 보상 정보
        /// </summary>
        (RewardInfo info, bool isBoss)[] GetRewardInfos();
    }
}