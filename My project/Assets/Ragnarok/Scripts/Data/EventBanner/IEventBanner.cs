namespace Ragnarok
{
    public interface IEventBanner
    {
        int Seq { get; }
        string Url { get; }
        ShortCutType ShortcutType { get; }
        int ShortcutValue { get; }
        string Description { get; }
        System.DateTime StartTime { get; }
        System.DateTime EndTime { get; }
        string TextureName { get; }
        int Pos { get; }
        RemainTime RemainTime { get; }
        string TextureUrl { get; }
    }
}