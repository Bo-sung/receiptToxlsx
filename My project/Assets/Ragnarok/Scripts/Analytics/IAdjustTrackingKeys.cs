namespace Ragnarok
{
    public interface IAdjustTrackingKeys
    {
        string this[TrackType type] { get; }
    }
}