namespace Ragnarok
{
    public enum DataUnit : byte
    {
        Boolean        = 0, 
        Integer        = 1,
        Percent        = 2,     // 백분율
        PerMille       = 3,     // 천분율
        PerTenThousand = 4,     // 만분율
        String         = 5,     
        DetailRef      = 6,     // 상세참조
    }
}