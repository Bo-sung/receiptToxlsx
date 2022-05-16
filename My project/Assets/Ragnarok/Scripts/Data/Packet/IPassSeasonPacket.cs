namespace Ragnarok
{
    public interface IPassSeasonPacket
    {
        long PassEndTime { get; }
        int SeasonNo { get; }
    }
}