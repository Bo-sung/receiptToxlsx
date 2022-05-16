namespace Ragnarok
{
    /// <summary>
    /// 보상 타입
    /// </summary>
    public enum RewardType
    {
        None                      = 0,
        Zeny                      = 1,
        CatCoin                   = 2,
        CatCoinFree               = 3,
        SkillPoint                = 4,
        StatPoint                 = 5,
        Item                      = 6,

        JobExp                    = 8,
        LevelExp                  = 9,

        CentralLabTicket          = 11,

        GuildCoin                 = 24,
        ROPoint                   = 25,
        DefDungeonTicket          = 26,
        WorldBossTicket           = 27,
        PveTicket                 = 28,
        [System.Obsolete]
        StageBossTicket           = 29,

        [System.Obsolete]
        QuestCoin                 = 32,
        Agent                     = 33,
        CharacterShareChargeItem1 = 34,
        CharacterShareChargeItem2 = 35,
        CharacterShareChargeItem3 = 36,
        MultiMazeTicket           = 37,
        SummonMvpTicket           = 38,
        ZenyDungeonTicket         = 39,
        ExpEungeonTicket          = 40,
        EventMultiMazeTicket      = 45,
        EndlessTowerTicket        = 46,
        DefenceDungeonTicket      = 47,
        PassExp                   = 48, // 라비린스 패스
        OnBuffPassExp             = 49, // 온버프 패스 경험치
        OnBuffPoint               = 50, // 온버프 포인트
        OnBuffMvpPoint            = 51, // 온버프 Mvp 포인트
        OnBuffPointMail           = 52, // 온버프 포인트 (우편함)

        InviteReward              = 78, // 페이스북 친구초대 보상

        RefRewardGroup            = 98, // 보상그룹 테이블 참조
        RefGacha                  = 99, // 뽑기 테이블 참조

        #region 클라 전용

        // 클라용
        DuelAlphabet              = 200, // 듀얼 조각. 클라이언트에서만 사용
        ShareReward               = 201, // 셰어 정산 보상 +20%
        InvenWeight               = 202, // 가방 무게 확장 +20
        TreeReward                = 203, // 나무 보상 증가

        ShareForce1               = 204, // 쉐어포스1
        ShareForce2               = 205, // 쉐어포스2
        ShareForce3               = 206, // 쉐어포스3
        ShareForce4               = 207, // 쉐어포스4
        ShareForce5               = 208, // 쉐어포스5
        ShareForce6               = 209, // 쉐어포스6

        BattlePass                = 210, // 배틀 패스
        OnBuffPass                = 211, // 온버프 패스

        #endregion
    }

    public static class RewardTypeExtensions
    {
        public static string GetItemName(this RewardType type)
        {
            switch (type)
            {
                case RewardType.Zeny:
                    return LocalizeKey._58000.ToText(); // 제니
                case RewardType.CatCoin:
                case RewardType.CatCoinFree:
                    return LocalizeKey._58001.ToText(); // 냥다래
                case RewardType.SkillPoint:
                    return LocalizeKey._58016.ToText(); // 스킬 포인트
                case RewardType.StatPoint:
                    return LocalizeKey._58017.ToText(); // 스탯 포인트

                case RewardType.JobExp:
                    return LocalizeKey._58002.ToText(); // 직업 경험치
                case RewardType.LevelExp:
                    return LocalizeKey._58003.ToText(); // 일반 경험치

                case RewardType.GuildCoin:
                    return LocalizeKey._58004.ToText(); // 길드코인
                case RewardType.ROPoint:
                    return LocalizeKey._58010.ToText(); // ROPoint
                case RewardType.DefDungeonTicket:
                    return LocalizeKey._58018.ToText(); // 디팬스 던전 티켓
                case RewardType.WorldBossTicket:
                    return LocalizeKey._58019.ToText(); // 월드보스 입장 티켓
                case RewardType.PveTicket:
                    return LocalizeKey._58020.ToText(); // 대전 입장 티켓
                case RewardType.StageBossTicket:
                    return LocalizeKey._58021.ToText(); // 스테이지 보스 도전권

                case RewardType.QuestCoin:
                    return LocalizeKey._58006.ToText(); // 퀘스트 코인
                case RewardType.Agent:
                    return LocalizeKey._3102.ToText(); // 동료
                case RewardType.CharacterShareChargeItem1:
                    return LocalizeKey._58007.ToText(); // 인연의 모래시계(10분)
                case RewardType.CharacterShareChargeItem2:
                    return LocalizeKey._58008.ToText(); // 인연의 모래시계(30분)
                case RewardType.CharacterShareChargeItem3:
                    return LocalizeKey._58009.ToText(); // 인연의 모래시계(60분)
                case RewardType.MultiMazeTicket:
                    return LocalizeKey._58011.ToText(); // 멀티미로 티켓
                case RewardType.SummonMvpTicket:
                    return LocalizeKey._58012.ToText(); // MVP 소환 티켓
                case RewardType.ZenyDungeonTicket:
                    return LocalizeKey._58013.ToText(); // 제니던전 티켓
                case RewardType.ExpEungeonTicket:
                    return LocalizeKey._58014.ToText(); // 경험치던전 티켓

                case RewardType.ShareReward:
                    return LocalizeKey._12016.ToText(); // (영구) 셰어 정산 보상 +20%
                case RewardType.InvenWeight:
                    return LocalizeKey._12019.ToText(); // (영구) 가방 무게 확장 +20
                case RewardType.TreeReward:
                    return LocalizeKey._12020.ToText(); // (28일) 나무 보상 +100%

                case RewardType.EventMultiMazeTicket:
                    return LocalizeKey._58022.ToText(); // 이벤트 멀티미궁 티켓

                case RewardType.ShareForce1:
                    return ShareForceType.ShareForce1.GetNameId().ToText(); // 쉐어 포스 타임 실린더

                case RewardType.ShareForce2:
                    return ShareForceType.ShareForce2.GetNameId().ToText(); // 쉐어 포스 타임 베어링

                case RewardType.ShareForce3:
                    return ShareForceType.ShareForce3.GetNameId().ToText(); // 쉐어 포스 타임 나이프

                case RewardType.ShareForce4:
                    return ShareForceType.ShareForce4.GetNameId().ToText(); // 쉐어 포스 타임 실드

                case RewardType.ShareForce5:
                    return ShareForceType.ShareForce5.GetNameId().ToText(); // 쉐어 포스 타임 키

                case RewardType.ShareForce6:
                    return ShareForceType.ShareForce6.GetNameId().ToText();  // 쉐어 포스 타임 커터

                case RewardType.CentralLabTicket:
                    return LocalizeKey._58023.ToText(); // 중앙실험실 티켓

                case RewardType.PassExp:
                    return LocalizeKey._58024.ToText(); // 라비린스 패스 경험치

                case RewardType.BattlePass:
                    return LocalizeKey._58025.ToText(); // 라비린스 패스

                case RewardType.OnBuffPassExp:
                    return LocalizeKey._58026.ToText(); // 온버프 패스 경험치

                case RewardType.OnBuffPoint:
                case RewardType.OnBuffPointMail:
                    return LocalizeKey._58027.ToText(); // 온버프 포인트

                case RewardType.OnBuffPass:
                    return LocalizeKey._58028.ToText(); // 온버프 패스

                case RewardType.OnBuffMvpPoint:
                    return LocalizeKey._58029.ToText(); // MVP 사냥 포인트
            }
            return string.Empty;
        }

        public static string GetDesc(this RewardType type)
        {
            switch (type)
            {
                case RewardType.Zeny:
                    return LocalizeKey._46300.ToText();
                case RewardType.CatCoin:
                case RewardType.CatCoinFree:
                    return LocalizeKey._46301.ToText();
                case RewardType.SkillPoint:
                    return LocalizeKey._46314.ToText(); // 스킬 레벨을 올릴 수 있다.
                case RewardType.StatPoint:
                    return LocalizeKey._46315.ToText(); // 스탯 레벨을 올릴 수 있다.

                case RewardType.JobExp:
                    return LocalizeKey._46302.ToText();
                case RewardType.LevelExp:
                    return LocalizeKey._46303.ToText();

                case RewardType.GuildCoin:
                    return LocalizeKey._46313.ToText(); // 길드의 명예를 드높인 자에게만 지급되는 코인.
                case RewardType.ROPoint:
                    return LocalizeKey._46304.ToText();
                case RewardType.DefDungeonTicket:
                    return LocalizeKey._46316.ToText(); // 디팬스 던전 입장을 할 수 있다.
                case RewardType.WorldBossTicket:
                    return LocalizeKey._46317.ToText(); // 월드보스 입장을 할 수 있다.
                case RewardType.PveTicket:
                    return LocalizeKey._46318.ToText(); // 대전 입장을 할 수 있다.
                case RewardType.StageBossTicket:
                    return LocalizeKey._46319.ToText(); // 스테이지 보스 도전을 할 수 있다.

                case RewardType.QuestCoin:
                    return LocalizeKey._46320.ToText(); // 퀘스트 코인이다.
                case RewardType.Agent:
                    return LocalizeKey._46321.ToText(); // 대전 및 듀얼에서 사용하는 지원군
                case RewardType.CharacterShareChargeItem1:
                    return LocalizeKey._46305.ToText(); // 모래시계1 설명
                case RewardType.CharacterShareChargeItem2:
                    return LocalizeKey._46306.ToText(); // 모래시계2 설명
                case RewardType.CharacterShareChargeItem3:
                    return LocalizeKey._46307.ToText(); // 모래시계3 설명
                case RewardType.MultiMazeTicket:
                    return LocalizeKey._46308.ToText(); // 멀티미로티켓 설명
                case RewardType.SummonMvpTicket:
                    return LocalizeKey._46309.ToText(); // MVP보스 소환 티켓 설명
                case RewardType.ZenyDungeonTicket:
                    return LocalizeKey._46322.ToText(); // 제니던전 입장을 할 수 있다.
                case RewardType.ExpEungeonTicket:
                    return LocalizeKey._46323.ToText(); // 경험치던전 입장을 할 수 있다.

                case RewardType.ShareReward:
                    return LocalizeKey._46310.ToText(); // 셰어 보상 20% 설명
                case RewardType.InvenWeight:
                    return LocalizeKey._46311.ToText(); // 가방 무게 증가 보상 설명
                case RewardType.TreeReward:
                    return LocalizeKey._46312.ToText(); // 28일간 나무에서 획득하는 보상이 양이 2배가 된다.

                case RewardType.EventMultiMazeTicket:
                    return LocalizeKey._46324.ToText(); // 이벤트 멀티 미궁에 입장할 수 있게 해주는 티켓.\n자정(GMT+8)이 되면 보유량이 초기화 된다.

                case RewardType.CentralLabTicket:
                    return LocalizeKey._46325.ToText(); // 중앙실험실 입장을 할 수 있다.

                case RewardType.PassExp:
                    return LocalizeKey._46326.ToText(); // 라비린스 패스 경험치 설명

                case RewardType.BattlePass:
                    return LocalizeKey._46327.ToText(); // 라비린스 패스 설명

                case RewardType.OnBuffPassExp:
                    return LocalizeKey._46328.ToText(); // 온버프 패스 설명

                case RewardType.OnBuffPoint:
                case RewardType.OnBuffPointMail:
                    return LocalizeKey._46329.ToText(); // 온버프 포인트 설명

                case RewardType.OnBuffPass:
                    return LocalizeKey._46330.ToText(); // 온버프 패스 설명

                case RewardType.OnBuffMvpPoint:
                    return LocalizeKey._46331.ToText(); // MVP를 사냥할 때 얻을 수 있는 포인트
            }

            return "";
        }

        public static bool IsRewardToast(this RewardType type)
        {
            switch (type)
            {
                case RewardType.Item:
                    return true;
            }
            return false;
        }

        public static string IconName(this RewardType type)
        {
            switch (type)
            {
                case RewardType.Zeny:
                    return "Zeny";

                case RewardType.CatCoin:
                case RewardType.CatCoinFree:
                    return "CatCoin";

                case RewardType.ROPoint:
                    return "Ropoint";

                case RewardType.DefDungeonTicket:
                    return "DefDungeonTicket";

                case RewardType.JobExp:
                    return "JobExp";

                case RewardType.LevelExp:
                    return "BaseExp";

                case RewardType.QuestCoin:
                    return "QuestCoin";

                case RewardType.GuildCoin:
                    return "GuildCoin";

                case RewardType.CharacterShareChargeItem1:
                    return "ShareTimer_1";
                case RewardType.CharacterShareChargeItem2:
                    return "ShareTimer_2";
                case RewardType.CharacterShareChargeItem3:
                    return "ShareTimer_3";

                case RewardType.MultiMazeTicket:
                    return "MultiMazeTicket";
                case RewardType.SummonMvpTicket:
                    return "SummonMvpTicket";
                case RewardType.ZenyDungeonTicket:
                    return "ZenyDungeonTicket";
                case RewardType.ExpEungeonTicket:
                    return "ExpEungeonTicket";

                case RewardType.ShareReward:
                    return "icon_specialbuff_1";

                case RewardType.InvenWeight:
                    return "icon_specialbuff_3";

                case RewardType.TreeReward:
                    return "icon_specialbuff_4";

                case RewardType.SkillPoint:
                    return "SkillPoint";

                case RewardType.EventMultiMazeTicket:
                    return "EventMultiMazeTicket";

                case RewardType.ShareForce1:
                    return ShareForceType.ShareForce1.GetTextureName();

                case RewardType.ShareForce2:
                    return ShareForceType.ShareForce2.GetTextureName();

                case RewardType.ShareForce3:
                    return ShareForceType.ShareForce3.GetTextureName();

                case RewardType.ShareForce4:
                    return ShareForceType.ShareForce4.GetTextureName();

                case RewardType.ShareForce5:
                    return ShareForceType.ShareForce5.GetTextureName();

                case RewardType.ShareForce6:
                    return ShareForceType.ShareForce6.GetTextureName();

                case RewardType.CentralLabTicket:
                    return "CentralLabTicket";

                case RewardType.PassExp:
                    return "PassExp";

                case RewardType.BattlePass:
                    return "BattlePass";

                case RewardType.OnBuffPassExp:
                    return "OnBuffPassExp";

                case RewardType.OnBuffPoint:
                case RewardType.OnBuffPointMail:
                    return "OnBuffPoint";

                case RewardType.OnBuffPass:
                    return "OnBuffPass";

                case RewardType.OnBuffMvpPoint:
                    return "MvpPoint";
            }
            return string.Empty;
        }

        public static CoinType ToCoinType(this RewardType type)
        {
            switch (type)
            {
                case RewardType.Zeny:
                    return CoinType.Zeny;

                case RewardType.CatCoin:
                case RewardType.CatCoinFree:
                    return CoinType.CatCoin;

                case RewardType.QuestCoin:
                    return CoinType.QuestCoint;

                case RewardType.GuildCoin:
                    return CoinType.GuildCoin;

                case RewardType.OnBuffPoint:
                case RewardType.OnBuffPointMail:
                    return CoinType.OnBuffPoint;
            }
            return default;
        }
    }
}