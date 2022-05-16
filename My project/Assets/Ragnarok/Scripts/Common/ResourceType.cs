namespace Ragnarok
{
    /// <summary>
    /// 테이블 목록
    /// 리소스 다운로드 주소이름과 매칭되어 있어 이름 변경시 서버도 같이 변경해야한다.
    /// http:{주소}/{타입}
    /// </summary>
    public enum ResourceType : byte
    {
        LanguageDataDB             = 0,  // 언어
        BasisDataDB                = 1,  // 기초데이터
        BasisDetailDataDB          = 2,  // 기초데이터 상세
        StageDataDB                = 3,  // 스테이지
        MonsterDataDB              = 4,  // 몬스터 
        DungeonDataDB              = 5,  // 던전
        JobDataDB                  = 6,  // 직업
        SkillDataDB                = 7,  // 스킬
        SkillEffectDataDB          = 8,  // 스킬연출
        ItemDataDB                 = 9,  // 아이템 
        CupetDataDB                = 10, // 큐펫
        BoxDataDB                  = 11, // 상자
        ExpDataDB                  = 12, // 경험치
        StatDataDB                 = 13, // 스탯
        SmeltRateDataDB            = 14, // 제련확률
        SmeltCoefficientDataDB     = 15, // 제련계수
        BotCoefficientDataDB       = 16, // 전옵타제련계수
        RandomTableDataDB          = 17, // 랜덤데이터
        CupetSetBuffDataDB         = 18, // 큐펫세트버프
        MakeDataDB                 = 19, // 제작 테이블
        LoginBonusDataDB           = 20, // 로그인 보너스
        QuestDataDB                = 21, // 퀘스트
        ShopDataDB                 = 22, // 상점
        GachaDataDB                = 23, // 가챠
        DisassembleItemDataDB      = 24, // 분해 
        RewardGroupDataDB          = 25, // 보상그룹
        PanelBuffDataDB            = 26, // 패널버프
        CrowdControlDataDB         = 27, // 상태이상
        CupetPositionDataDB        = 28, // 큐펫포지션
        WorldBossDataDB            = 29, // 월드보스
        DefenceDungeonDataDB       = 30, // 디펜스던전
        PvETierDataDB              = 31, // Pve티어
        PvERankRewardDataDB        = 32, // Pve랭킹
        ItemTierUpDataDB           = 33, // 티어업
        MazeMapDataDB              = 34, // 미로맵
        AgentDataDB                = 35, // 동료
        TamingDataDB               = 36, // 테이밍
        MultiMazeDataDB            = 37, // 멀티미로
        ScenarioMazeDataDB         = 38, // 시나리오
        MvpDataDB                  = 39, // MVP
        AdventureDataDB            = 40, // 모험
        CardOptionDataDB           = 41, // 카드옵션
        CardOptionProbDataDB       = 42, // 카드옵션 확률
        AgentBookDataDB            = 43, // 동료세트효과
        MultiMazeWaitingRoomDataDB = 44, // 멀티미로대기실
        ClickerDungeonDataDB       = 45, // 클리커던전
        DuelRewardDataDB           = 46, // 듀얼보상
        MvpRewardUIDataDB          = 47, // MVP 보상 정보 (UI 표시용)
        EquipItemLevelupDataDB     = 48, // 장비 강화 테이블
        EventLoginBonusDataDB      = 49, // 특별로그인보너스
        NickFilterDataDB           = 50, // 닉네임필터
        ChatFilterDataDB           = 51, // 채팅필터
        AutoNickDataDB             = 52, // 자동생성닉네임
        ShareDataDB                = 53, // 셰어
        CostumeDataDB              = 54, // 코스튬 (머리 오프셋)
        DungeonInfoDataDB          = 55, // 던전인포
        EventDualRewardDataDB      = 56, // 이벤트 듀얼 보상
        PaymentRewardDataDB        = 57, // 누적결제 보상
        FreeFightRewardDataDB      = 58, // 난전 보상
        BookDataDB                 = 59, // 도감 보상
        BingoDataDB                = 60, // 빙고 보상
        CLabDataDB                 = 61, // 중앙실험실
        CLabMonsterDataDB          = 62, // 중앙실험실 몬스터그룹
        CLabSkillDataDB            = 63, // 중앙실험실 스킬
        EventDualBuffDataDB        = 64, // 이벤트듀얼버프
        GuideDataDB                = 65, // 가이드
        JobLevelRewardDataDB       = 66, // 잡레벨 보상
        ElementDataDB              = 67, // 속성
        EventQuizDataDB            = 68, // 퀴즈
        GuildAttackMonsterDataDB   = 69, // 길드습격 몬스터그룹
        RPSDataDB                  = 70, // 가위바위보
        DiceRewardDataDB           = 71, // 주사위완주보상테이블
        DiceDataDB                 = 72, // 주사위기묘한사건테이블
        ChallengeRewardDataDB      = 73, // 챌린지 보상 테이블
        EndlessDungeonDataDB       = 74, // 엔들리스 타워
        DarkTreeRewardDB           = 75, // 어둠의 나무 테이블
        ForestBaseDataDB           = 76, // 미궁숲 테이블
        ForestMonDataDB            = 77, // 미궁숲 몬스터 그룹 테이블
        ForestRewardDataDB         = 78, // 미궁숲 선택보상 테이블
        TimePatrolStageDataDB      = 79, // TP스테이지 테이블
        TimePatrolBossDataDB       = 80, // TP보스 테이블
        TPCostumeLevelDataDB       = 81, // 합체 코스튭 강화 테이블
        KafExchangeDataDB          = 82, // 카프라교환테이블
        ProfileDataDB              = 83, // 프로필테이블
        GuildBattleRewardDB        = 84, // 길드전 보상 테이블
        ShareStatBuildUpDataDB     = 85, // 쉐어스텟강화 테이블
        PassDataDB                 = 86, // 패스 테이블
        GateDataDB                 = 87, // 게이트 데이블
        DuelArenaDataDB            = 88, // 듀얼 아레나 테이블
        DuelArenaRankDataDB        = 89, // 듀얼 아레나 순위 테이블
        NabihoDataDB               = 90, // 나비호 테이블
        NabihoIntimacyDataDB       = 91, // 나비호 친밀도 테이블
        FindAlphabetDataDB         = 92, // 글자찾기테이블
        OnBuffPassDataDB           = 93, // 온버프 패스 테이블
    }
}