namespace Ragnarok
{
    public interface IBattleTrapInput
    {
        int Id { get; }
        byte State { get; }
        short IndexX { get; }
        short IndexZ { get; }
    }
}