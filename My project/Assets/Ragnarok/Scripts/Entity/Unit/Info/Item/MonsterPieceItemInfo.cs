namespace Ragnarok
{
    public class MonsterPieceItemInfo : ItemInfo
    {
        public override bool IsStackable => true;

        public override void SetItemInfo(int tier, int itemLevel, byte itemPos, long equippedCardNo1, long equippedCardNo2, long equippedCardNo3, long equippedCardNo4, bool isLock, int itemTranscend = 0, int itemChangedElement = 0, int itemElementLevel = 0)
        {
        }

        public override void Reload(bool isEquipCard)
        {
        }
    }
}