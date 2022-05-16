namespace Ragnarok
{
    public interface IMazeCubeStateInfo
    {
        int Index { get; }
        float PosX { get; }
        float PosY { get; }
        float PosZ { get; }
        MazeCubeState State { get; }
    }
}