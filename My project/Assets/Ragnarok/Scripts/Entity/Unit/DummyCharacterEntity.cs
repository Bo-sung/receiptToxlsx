namespace Ragnarok
{
    public class DummyCharacterEntity : CharacterEntity
    {
        public override UnitEntityType type => UnitEntityType.UI;

        protected override DamagePacket.UnitKey GetDamageUnitKey()
        {
            throw new System.NotImplementedException("Dummy Character는 대미지 체크가 음슴");
        }

        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnDummyCharacter();
        }
    }
}