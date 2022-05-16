namespace Ragnarok.View
{
    public class UIBattleCupetSlot : UIBattleUnitSlot
    {
        protected CupetEntity cupetEntity => unitEntity as CupetEntity;

        protected override string GetThumbnailName()
        {
            return cupetEntity.Cupet.ThumbnailName;
        }
    }
}