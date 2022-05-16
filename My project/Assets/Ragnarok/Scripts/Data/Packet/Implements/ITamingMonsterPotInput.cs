namespace Ragnarok
{
    public interface ITamingMonsterPotInput : ISpawnMonster
    {
        int Index { get; }
        byte State { get; }
    }
}
