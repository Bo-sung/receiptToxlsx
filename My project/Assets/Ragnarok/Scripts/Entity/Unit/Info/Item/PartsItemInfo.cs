namespace Ragnarok
{
    public class PartsItemInfo : ItemInfo
    {
        public override bool IsStackable => true;
        public override bool IsSmeltMaterial => MaxSmelt != 0;
        public override int MaxSmelt => data.atk_max;
        public override int WaringSmeltLevel => data.atk_min;

        public override ElementType ElementType => GetElementType();
        public override int ElementLevel => GetElementLevel();
        public override bool IsElementStone => GetIsElementStone();
        public override int DarkTreePoint => GetDarkTreePoint();
        public override int CupetExpPoint => GetCupetExpPoint();
        public override int GuildBattleBuffExpPoint => GetGuildBattleBuffExpPoint();

        public override void SetItemInfo(int tier, int itemLevel, byte itemPos, long equippedCardNo1, long equippedCardNo2, long equippedCardNo3, long equippedCardNo4, bool isLock, int itemTranscend = 0, int itemChangedElement = 0, int itemElementLevel = 0)
        {
        }

        public override void Reload(bool isEquipCard)
        {
        }

        private ElementType GetElementType()
        {
            return data.element_type.ToEnum<ElementType>();
        }

        private int GetElementLevel()
        {
            return data.GetElementStoneLevel();
        }

        private bool GetIsElementStone()
        {
            return GetElementType() != ElementType.None;
        }

        private int GetDarkTreePoint()
        {
            return data.skill_rate;
        }

        private int GetCupetExpPoint()
        {
            return data.matk_min;
        }

        private int GetGuildBattleBuffExpPoint()
        {
            return data.matk_max;
        }
    }
}