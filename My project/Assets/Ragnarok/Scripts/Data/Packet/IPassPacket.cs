namespace Ragnarok
{
    public interface IPassPacket
    {
        long PayPassEndTime { get; }
        string PassFreeStep { get; }
        string PassPayStep { get; }
        int PassExp { get; }
    }
}