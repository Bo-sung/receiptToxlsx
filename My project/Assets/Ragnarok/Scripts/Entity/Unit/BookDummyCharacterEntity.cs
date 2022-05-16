namespace Ragnarok
{
    /// <summary>
    /// 도감 전용의 경우에는 코스튬이 무기에 관계없이 그대로 보여주어야 한다.
    /// </summary>
    public class BookDummyCharacterEntity : DummyCharacterEntity
    {
        protected override UnitActor SpawnEntityActor()
        {
            return unitActorPool.SpawnBookDummyCharacter();
        }
    }
}