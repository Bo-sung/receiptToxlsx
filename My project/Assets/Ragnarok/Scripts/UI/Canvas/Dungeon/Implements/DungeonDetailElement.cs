using UnityEngine;

namespace Ragnarok.View
{
    public class DungeonDetailElement : DungeonElement
    {
        private readonly IDungeonDetailImpl impl;

        public override string Name => LocalizeKey._7030.ToText() // {DUNGEON} 난이도{DIFFICULTY}
            .Replace(ReplaceKey.DUNGEON, base.Name)
            .Replace(ReplaceKey.DIFFICULTY, Difficulty);

        UIMonsterIcon.IInput[] monsterInfos;
        (RewardInfo info, bool isBoss)[] rewardInfos;

        public int Difficulty => dungeonGroup.Difficulty;

        public DungeonDetailElement(IDungeonGroup dungeonGroup, IDungeonDetailImpl impl) : base(dungeonGroup, impl)
        {
            this.impl = impl;
        }

        public bool IsOpenedDungeon(bool isShowNotice)
        {
            return impl.IsOpend(DungeonType, Id, isShowNotice);
        }

        public int GetClearTicketCount()
        {
            return impl.GetClearTicketCount(DungeonType);
        }

        public bool IsCleardDungeon()
        {
            return impl.IsCleared(DungeonType, Id);
        }

        public bool IsCurrentPlaying()
        {
            return impl.IsCurrentDungeonPlaying();
        }

        public int GetClearedDifficulty()
        {
            return impl.GetClearedDifficulty(DungeonType);
        }

        public bool CanEnter(bool isShowPopup)
        {
            return impl.CanEnter(DungeonType, Id, isShowPopup);
        }

        public UIMonsterIcon.IInput[] GetMonsterInfos()
        {
            return monsterInfos ?? (monsterInfos = impl.GetMonsterInfos(DungeonType, dungeonGroup.GetMonsterInfos()));
        }

        public (RewardInfo info, bool isBoss)[] GetRewardInfos()
        {
            return rewardInfos ?? (rewardInfos = dungeonGroup.GetRewardInfos());
        }
    }
}