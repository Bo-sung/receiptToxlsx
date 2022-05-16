namespace Ragnarok.View
{
    public class UIBattleCharacterSlot : UIBattleUnitSlot
    {
        protected CharacterEntity characterEntity => unitEntity as CharacterEntity;

        protected override string GetThumbnailName()
        {
            return characterEntity.Character.GetAgentProfileName();
        }
    }
}