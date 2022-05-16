namespace Ragnarok
{
    public interface ISpawnMonster
    {
        MonsterType Type { get; }
        int Id { get; }
        int Level { get; }
        float Scale { get; }
    }
}