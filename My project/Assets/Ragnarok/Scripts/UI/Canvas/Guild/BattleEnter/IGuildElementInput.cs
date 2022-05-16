namespace Ragnarok.View
{
    public interface IGuildElementInput
    {
        int GuildId { get; }
        int GuildLevel { get; }
        string GuildName { get; }
        int Emblem { get; }
    }
}