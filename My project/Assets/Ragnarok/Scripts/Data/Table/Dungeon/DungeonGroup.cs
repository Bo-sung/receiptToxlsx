namespace Ragnarok
{
    public class DungeonGroup : IDungeonGroup, IOpenConditional
    {
        public DungeonType DungeonType { get; }
        public string Name => DungeonType.ToText();
        public DungeonOpenConditionType ConditionType { get; }
        public int ConditionValue { get; }

        public int Difficulty => throw new System.NotImplementedException();
        public int Id => throw new System.NotImplementedException();

        public DungeonGroup(DungeonType dungeonType, DungeonOpenConditionType conditionType, int conditionValue)
        {
            DungeonType = dungeonType;
            ConditionType = conditionType;
            ConditionValue = conditionValue;
        }

        public (int monsterId, MonsterType type, int monsterLevel)[] GetMonsterInfos()
        {
            throw new System.NotImplementedException();
        }

        public (RewardInfo info, bool isBoss)[] GetRewardInfos()
        {
            throw new System.NotImplementedException();
        }
    }
}