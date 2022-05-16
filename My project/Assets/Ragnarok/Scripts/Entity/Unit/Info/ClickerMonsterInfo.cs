namespace Ragnarok
{
    public class ClickerMonsterInfo : UIMonsterIcon.IInput
    {
        private readonly DungeonType dungeonType;

        public string IconName => GetIconName();

        public string ElementIconName => string.Empty;

        public UnitSizeType UnitSizeType => UnitSizeType.None;

        public bool IsBoss => false;

        public ClickerMonsterInfo(DungeonType dungeonType)
        {
            this.dungeonType = dungeonType;
        }

        private string GetIconName()
        {
            switch (dungeonType)
            {
                case DungeonType.ZenyDungeon:
                    return "Zeny";

                case DungeonType.ExpDungeon:
                    return "BaseExp";
            }

            return string.Empty;
        }
    }
}