namespace Ragnarok.View
{
    public class DungeonElement : IInfo
    {
        protected readonly IDungeonGroup dungeonGroup;
        private readonly IDungeonImpl impl;

        public bool IsInvalidData => false;
        public event System.Action OnUpdateEvent;

        public DungeonType DungeonType => dungeonGroup.DungeonType;
        public virtual string Name => dungeonGroup.Name;
        public int Id => dungeonGroup.Id;
        public DungeonOpenConditionType ConditionType => dungeonGroup.ConditionType;
        public int ConditionValue => dungeonGroup.ConditionValue;

        public DungeonElement(IDungeonGroup dungeonGroup, IDungeonImpl impl)
        {
            this.dungeonGroup = dungeonGroup;
            this.impl = impl;
        }

        public void InvokeUpdateEvent()
        {
            OnUpdateEvent?.Invoke();
        }

        public bool IsOpenedDungeon()
        {
            return impl.IsOpend(DungeonType);
        }

        public string GetOpenConditionalSimpleText()
        {
            return impl.GetOpenConditionalSimpleText(dungeonGroup);
        }

        public int GetFreeCount()
        {
            return impl.GetFreeEntryCount(DungeonType);
        }

        public int GetFreeMaxCount()
        {
            return impl.GetFreeEntryMaxCount(DungeonType);
        }

        public bool PossibleFreeReward()
        {
            return impl.PossibleFreeReward(DungeonType);
        }
    }
}