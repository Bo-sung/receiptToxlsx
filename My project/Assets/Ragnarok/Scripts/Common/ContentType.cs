namespace Ragnarok
{
    public enum ContentType
    {
        Stage            = 1, // 모험
        Skill            = 2, // 메뉴, [메뉴]스킬
        Dungeon          = 3, // [메뉴]던전
        Cupet            = 4, // [메뉴]큐펫
        Make             = 5, // 제작
        SecretShop       = 6, // [상점]비밀상점
        Rebirth          = 7, // [영웅]전승
        JobChange        = 8, // 전직
        CombatAgent      = 9, // 전투 동료
        Sharing          = 10, // 셰어
        [System.Obsolete]
        ZenyDungeon      = 11, // 제니 던전
        [System.Obsolete]
        ExpDungeon       = 12, // 경험치 던전
        Buff             = 13, // 버프
        TradeTown        = 14, // 마을(거래소)
        ItemEnchant      = 15, // 장비 강화
        Duel             = 16, // 듀얼
        Explore          = 17, // 파견
        Maze             = 18, // 미로
        Boss             = 19, // 보스 도전
        Pvp              = 20, // 대전
        Guild            = 21, // 길드
        ShareControl     = 22, // 셰어 컨트롤
        ShareLevelUp     = 23, // 셰어 레벨업
        ChangeElement    = 24, // 속성 부여
        TierUp           = 25, // 장비 초월
        ManageCard       = 26, // 카드 관리
        [System.Obsolete]
        FreeFight        = 27, // 난전
        StoryBook8Open   = 28, // 시나리오 북 8 오픈
        StoryBook9Open   = 29, // 시나리오 북 9 오픈
        ShareHope        = 30, // 희망의 영웅
        StoryBook11Open  = 31, // 시나리오 북 11 오픈
        StoryBook12Open  = 32, // 시나리오 북 12 오픈
        StoryBook13Open  = 33, // 시나리오 북 13 오픈
        ShareVice2ndOpen = 34, // 2세대 쉐어바이스 오픈
        PronteraScenario = 35, // 프론테라 미궁 시나리오

        // <!-- 클라 전용 --!>
        AchieveMultiMaze = 101, // 업적
    }

    public static class ContentTypeExtensions
    {
        public static string ToText(this ContentType type)
        {
            switch (type)
            {
                case ContentType.Stage:
                    return LocalizeKey._10601.ToText(); // 모험
                case ContentType.Skill:
                    return LocalizeKey._10602.ToText(); // 스킬
                case ContentType.Dungeon:
                    return LocalizeKey._10603.ToText(); // 던전
                case ContentType.Cupet:
                    return LocalizeKey._10604.ToText(); // 큐펫
                case ContentType.Make:
                    return LocalizeKey._10605.ToText(); // 제작
                case ContentType.SecretShop:
                    return LocalizeKey._10606.ToText(); // 비밀상점
                case ContentType.Rebirth:
                    return LocalizeKey._10607.ToText(); // 전승
                case ContentType.JobChange:
                    return LocalizeKey._10608.ToText(); // 전직
                case ContentType.CombatAgent:
                    return LocalizeKey._47306.ToText(); // PVP 동료
                case ContentType.Sharing:
                    return LocalizeKey._10610.ToText(); // 셰어
                case ContentType.ZenyDungeon:
                    return LocalizeKey._10611.ToText(); // 제니 던전
                case ContentType.ExpDungeon:
                    return LocalizeKey._10612.ToText(); // 경험치 던전
                case ContentType.Buff:
                    return LocalizeKey._10613.ToText(); // 버프
                case ContentType.TradeTown:
                    return LocalizeKey._10614.ToText(); // 거래소
                case ContentType.ItemEnchant:
                    return LocalizeKey._49002.ToText(); // 장비 강화
                case ContentType.Duel:
                    return LocalizeKey._2103.ToText(); // 듀얼
                case ContentType.Explore:
                    return LocalizeKey._2105.ToText(); // 파견
                case ContentType.Maze:
                    return LocalizeKey._2104.ToText(); // 카드 미로
                case ContentType.Boss:
                    return LocalizeKey._2303.ToText(); // 보스도전
                case ContentType.Pvp:
                    return LocalizeKey._2506.ToText(); // 대전
                case ContentType.Guild:
                    return LocalizeKey._3009.ToText(); // 길드
                case ContentType.ShareControl:
                    return LocalizeKey._54300.ToText(); // 셰어 조작
                case ContentType.ShareLevelUp:
                    return LocalizeKey._54301.ToText(); // 셰어 레벨업
                case ContentType.ChangeElement:
                    return LocalizeKey._54302.ToText(); // 속성 부여
                case ContentType.TierUp:
                    return LocalizeKey._54303.ToText(); // 장비 초월
                case ContentType.ManageCard:
                    return LocalizeKey._54304.ToText(); // 카드 관리
                case ContentType.FreeFight:
                    return LocalizeKey._2515.ToText(); // 난전
                case ContentType.ShareHope:
                    return LocalizeKey._54313.ToText(); // 희망의 영웅
                case ContentType.ShareVice2ndOpen:
                    return LocalizeKey._48249.ToText(); // 쉐어 포스

                case ContentType.AchieveMultiMaze:
                    return LocalizeKey._5422.ToText(); // 미궁 정복자
            }

            return string.Empty;
        }

        public static bool IsContentsUnlockViaTutorial(this ContentType type)
        {
            switch (type)
            {
                case ContentType.Maze:
                case ContentType.ShareControl:
                case ContentType.ShareLevelUp:
                case ContentType.ChangeElement:
                case ContentType.TierUp:
                case ContentType.ManageCard:
                case ContentType.FreeFight:
                case ContentType.AchieveMultiMaze:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 항시 개방형 컨텐츠
        /// 잠금컨텐츠: {NAME} 컨텐츠가 오픈되었습니다.
        /// 항시개방컨텐츠: {NAME} 컨텐츠를 이용할 수 있습니다.
        /// </summary>
        public static bool IsAlwaysOpend(this ContentType type)
        {
            switch (type)
            {
                case ContentType.AchieveMultiMaze:
                    return true;
            }

            return false;
        }
    }
}