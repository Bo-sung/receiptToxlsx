namespace Ragnarok
{
    public enum RpsResultType : byte
    {
        // 승리의 경우 레디상태(단계가 올라감)
        Ready = 0,
        
        // 무승부
        Draw,
        
        // 패배
        Defeat,
    }
}