namespace Ragnarok
{
    public enum RankType : byte
    {
        All                 = 0, // 전체 직업레벨
        Swordman            = 1, // 검사
        Magician            = 2, // 마법사
        Thief               = 3, // 도둑
        Archer              = 4, // 궁수
        StageClear          = 5, // 스테이지 진행도 랭킹
        Guild               = 6, // 길드
        CupetSumRank        = 7, // 큐펫 등급 합산 랭킹
        League              = 8, // 리그
        BattleScore         = 9, // 전투력
        KillMvp             = 10, // mvp 소탕
        CardUp              = 11, // 카드 강화
        ItemMake            = 12, // 제작
        ItemDisassemble     = 13, // 분해
        ItemSell            = 14, // 거래소 아이템 판매
        ItemBuy             = 15, // 거래소 아이템 구매
        RockPaperScissors   = 16, // 가위바위보
        EventStagePoint     = 17, // 이벤트스테이지 점수
        EventStageKillCount = 18, // 이벤트스테이지 보스처치
        SingleLeague        = 19, // 싱글 대전 랭킹
        DuelArena           = 20, // 듀얼 아레나

        MazeClear           = 99, // 미로맵 클리어 랭킹
        GuildBattle         = 100, // 길드전
        EventGuildBattle    = 101, // 이벤트길드전
        MyGuildBattle       = 102, // 길드전 (내 길드원 랭킹)
    }
}