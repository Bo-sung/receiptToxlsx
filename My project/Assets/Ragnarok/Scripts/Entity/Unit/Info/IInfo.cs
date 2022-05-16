namespace Ragnarok
{
    public interface IInfo
    {
        bool IsInvalidData { get; }

        event System.Action OnUpdateEvent;
    }
}