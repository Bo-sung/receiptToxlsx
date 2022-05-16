namespace Ragnarok
{
    [System.Flags]
    public enum JobFilter : int
    {
        None     = 0,      // 0
        Knight   = 1 << 0, // 1
        Crusader = 1 << 1, // 2
        Wizard   = 1 << 2, // 4
        Sage     = 1 << 3, // 8
        Assassin = 1 << 4, // 16
        Rogue    = 1 << 5, // 32
        Hunter   = 1 << 6, // 64
        Dancer   = 1 << 7, // 128
    }
}