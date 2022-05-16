namespace Ragnarok
{
    /// <summary>
    /// int: 1 << 30 까지 사용 가능
    /// long: 1 << 62 까지 사용 가능
    /// </summary>
    [System.Flags]
    public enum TutorialType : long
    {
        None                  = 0,
        //--------------------
        Agent                 = 1L << 0,  /// 동료 <see cref="TutorialAgent"/>
        FirstStage            = 1L << 1,  /// 첫번째스테이지 <see cref="TutorialFirstStage"/>
        SharingCharacterEquip = 1L << 2,  /// 셰어링캐릭터장착 <see cref="TutorialSharingCharacterEquip"/>
        JobChange             = 1L << 3,  /// 전직 <see cref="TurotialJobChange"/>
        SkillLearn            = 1L << 4,  /// 스킬배우기 <see cref="TutorialSkillLearn"/>
        SkillEquip            = 1L << 5,  /// 스킬장착 <see cref="TutorialSkillEquip"/>
        BossSummon            = 1L << 6,  /// 보스도전 <see cref="TutorialBossSummon"/>

        ItemEnchant           = 1L << 7,  /// 장비 강화 <see cref="TutorialItemEnchant"/>
        CardEquip             = 1L << 8,  /// 카드 장착 <see cref="TutorialCardEquip"/>
        CardEnchant           = 1L << 9,  /// 카드 강화 <see cref="TutorialCardEnchant"/>
        Duel                  = 1L << 10, /// 듀얼 <see cref="TutorialDuel"/>
        Trade                 = 1L << 11, /// 거래소 <see cref="TutorialTrade"/>
        Mvp                   = 1L << 12, /// MVP, 집결 <see cref="TutorialMvp"/>

        MazeOpen              = 1L << 13, /// 미로 오픈 <see cref="TutorialMazeOpen">
        PronteraScenario      = 1L << 14, /// 프론테라 시나리오 <see cref="TutorialPronteraScenario">
        ShareControl1         = 1L << 15, /// 셰어 조작1 <see cref="TutorialShareControl1">
        ShareControl2         = 1L << 16, /// 셰어 조작2 <see cref="TutorialShareControl2">
        ShareLevelUp          = 1L << 17, /// 셰어 레벨업 <see cref="TutorialShareLevelUp">
        ChangeElement         = 1L << 18, /// 속성 부여 <see cref="TutorialChangeElement">
        TierUp                = 1L << 19, /// 장비 초월 <see cref="TutorialTierUp">
        ManageCard            = 1L << 20, /// 카드 관리 <see cref="TutorialManageCard">
        StoryBook8Open        = 1L << 21, /// 시나리오 북 8 오픈 <see cref="TutorialStoryBook8Open">
        StoryBook9Open        = 1L << 22, /// 시나리오 북 9 오픈 <see cref="TutorialStoryBook9Open">
        ShareClone            = 1L << 23, /// 셰어 클론 오픈 <see cref="TutorialShareClone">
        StoryBook11Open       = 1L << 24, /// 시나리오 북 11 오픈 <see cref="TutorialStoryBook11Open">
        StoryBook12Open       = 1L << 25, /// 시나리오 북 12 오픈 <see cref="TutorialStoryBook12Open">
        StoryBook13Open       = 1L << 26, /// 시나리오 북 13 오픈 <see cref="TutorialStoryBook13Open">
        TimePatrolOpen        = 1L << 27, /// 타임패트롤 오픈 <see cref="TutorialTimePatrolOpen">
        TimePatrolFirstEnter  = 1L << 28, /// 타임패트롤 첫 진입 <see cref="TimePatrolEntry">
        ShareVice2ndOpen      = 1L << 29, /// 타임패트롤 셰어바이스 오픈 <see cref="TutorialShareVice2ndOpen"/>
        MazeEnter             = 1L << 30, /// 미로 입장 <see cref="TutorialMazeEnter"/>
        SingleMazeEnter       = 1L << 31, /// 싱글미로 입장 <see cref="TutorialSingleMazeEnter"/>
        MultiMazeEnter        = 1L << 32, /// 멀티미로 입장 <see cref="TutorialMultiMazeEnter"/>
        MazeExit              = 1L << 33, /// 미로 퇴장 <see cref="TutorialMazeExit"/>
        GateOpen              = 1L << 34, /// 게이트 오픈 <see cref="TutorialGateOpen"/>
        GateEnter             = 1L << 35, /// 게이트 입장 <see cref="TutorialGateEnter"/>

        //--------------------
        All = -1,
    }
}