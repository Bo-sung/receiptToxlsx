namespace Ragnarok
{
    [System.Flags]
    public enum ShopState : int
    {
        New  = 1 << 0, // 1
        Best = 1 << 1, // 2
        Hot  = 1 << 2, // 4
    }
}