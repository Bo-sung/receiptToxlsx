namespace Ragnarok
{
    public enum BasisItem
    {
        /// <summary>
        /// 무기 제련 재료
        /// </summary>
        WeaponSmelt = 1,

        /// <summary>
        /// 방어구 제련 재료
        /// </summary>
        ArmorSmelt = 2,

        /// <summary>
        /// 무기 오버 제련 재료
        /// </summary>
        WeaponOverSmelt = 3,

        /// <summary>
        /// 방어구 오버 제련 재료
        /// </summary>
        ArmorOverSmelt = 4,

        /// <summary>
        /// 강력접착제
        /// </summary>
        SuperGlue = 5,

        /// <summary>
        /// 던전 소탕권
        /// </summary>
        DungeonClearTicket = 6,

        /// <summary>
        /// 소드맨 전직 완료 상자
        /// </summary>
        Swordman = 7,

        /// <summary>
        /// 매지션 전직 완료 상자
        /// </summary>
        Magician = 8,

        /// <summary>
        /// 씨프 전직 완료 상자
        /// </summary>
        Thief = 9,

        /// <summary>
        /// 헌터 전직 완료 상자
        /// </summary>
        Archer = 10,

        /// <summary>
        /// 리그전 티켓
        /// </summary>
        LeagueTicket = 11,

        /// <summary>
        /// 미로 입장권
        /// </summary>
        MazeEnterTicket = 12,

        /// <summary>
        /// 미로 소탕권
        /// </summary>
        [System.Obsolete("제거 예정")]
        MapFastClearTicket = 13,

        /// <summary>
        /// 스테이지 소탕권
        /// </summary>
        [System.Obsolete("제거 예정")]
        BossFastClearTicket = 14,

        /// <summary>
        /// 무속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Neutral = 18,

        /// <summary>
        /// 화속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Fire = 19,

        /// <summary>
        /// 수속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Water = 20,

        /// <summary>
        /// 풍속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Wind = 21,

        /// <summary>
        /// 지속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Earth = 22,

        /// <summary>
        /// 독속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Poison = 23,

        /// <summary>
        /// 성속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Holy = 24,

        /// <summary>
        /// 암속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Shadow = 25,

        /// <summary>
        /// 염속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Ghost = 26,

        /// <summary>
        /// 사속성 속성석
        /// </summary>
        [System.Obsolete("제거 예정")]
        ElementStone_Undead = 27,

        /// <summary>
        /// 발키리힘의 파편 (거래 불가)
        /// </summary>
        RebirthMaterial = 28,

        /// <summary>
        /// 발키리힘의 파편 (거래 가능)
        /// </summary>
        RebirthMaterial_CanTrade = 29,

        /// <summary>
        /// 셰어 부스트
        /// </summary>
        ShareBoost = 30,

        /// <summary>
        /// 비밀 상점 초기화권
        /// </summary>
        SecretShopReset = 31,

        /// <summary>
        /// 제니 던전 무료 보상(즉시 오픈 상자 아이템)
        /// </summary>
        ZenyDungeonFree = 32,

        /// <summary>
        /// 비공정 습격 무료 보상(즉시 오픈 상자 아이템)
        /// </summary>
        DefenceDungeonFree = 33,

        /// <summary>
        /// 경험치 던전 무료 보상(즉시 오픈 상자 아이템)
        /// </summary>
        ExpDungeonFree = 34,

        /// <summary>
        /// 카프라 버프 아이템 (셰어 보상 10% 증가)
        /// </summary>
        KafraBuff = 35,

        /// <summary>
        /// 엠펠리움 결정
        /// </summary>
        Emperium = 37,

        /// <summary>
        /// 이벤트 주화 아이템(가위바위보/사다리타기/주사위게임에 사용)
        /// </summary>
        EventCoin = 39,

        /// <summary>
        /// 이벤트모드 스테이지 도전권
        /// </summary>
        EventStageTicket = 40,

        /// <summary>
        /// 쉐도우 장비 카드 슬롯 오픈 시 필요 아이템 (영혼의 송곳)
        /// </summary>
        ShadowCardSlotOpen = 41,

        /// <summary>
        /// 엔들리스 타워 입장권 (엔들리스 타워 추가 입장 아이템)
        /// </summary>
        EndlessTowerTicket = 42,

        /// <summary>
        /// 어둠의 재 (엔들리스 타워 층 스킵 재료 아이템)
        /// </summary>
        EndlessTowerSkipItem = 43,

        /// <summary>
        /// 미궁숲 임장권 (미궁숲 추가 입장 아이템)
        /// </summary>
        ForestMazeTicket = 44,

        /// <summary>
        /// 초월 무기 재료 아이템
        /// </summary>
        BeyondSword = 45,

        /// <summary>
        /// 초월 방어구 재료 아이템
        /// </summary>
        BeyondArmor = 46,

        /// <summary>
        /// 프론테라의 축복 버프 아이템 (84037)
        /// </summary>
        PronteraBless = 47,
        
        /// <summary>
        /// 40만제니 상자
        /// </summary>
        ZenyBox = 48,

        /// <summary>
        /// 직업 변경권 (84038)
        /// </summary>
        JobChangeTicket = 49,

        /// <summary>
        /// 초월 분해 아이템 (84039)
        /// </summary>
        TranscendenceDisasemble = 50,

        /// <summary>
        /// 나비호 친밀도 증가 아이템 (94239)
        /// </summary>
        NabihoExp = 51,
    }
}