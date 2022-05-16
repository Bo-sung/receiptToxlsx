namespace Ragnarok
{
    public interface IBattleItemInput
    {
        int Id { get; }
        byte State { get; }
        short IndexX { get; }
        short IndexZ { get; }
    }
}