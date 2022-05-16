namespace Ragnarok
{
    /// <summary>
    /// 미로맵 슬롯 정보
    /// <see cref="MazeMapData"/>
    /// </summary>
    public interface IMazeMapSlotData
    {
        int MazeMapId { get; }
        int MazeMapNameId { get; }
    }
}
