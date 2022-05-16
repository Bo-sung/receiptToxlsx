using UnityEngine;

namespace Ragnarok
{
    public enum ShortCutType
    {
        None = 0,
        Stage = 1,
        Dungeon = 2,
        Skill = 3,
        Cupet = 4,
        Character = 5,
        Inventory = 6,
        Make = 7,
        Shop = 8,
        SecretShop = 9,
        Main = 10,
        JobChange = 11,
        Quest = 12,
        MakeItem = 13,
        Event = 14,
        AdventureScenario = 15,
        Maze = 16,
        AgentExplore = 17,
        Sharing = 18,
        UseItem = 19,
        WorldBoss = 20,
        League = 21,
        MultiMaze = 22,
        Duel = 23,
        LoginBonus = 24,
        EventRoulette = 25,
        Empty = 26,
        MvpTutorial = 27,
        Coupon = 28,
        Bingo = 29,
        SpecialRoulette = 30, // 특수 룰렛 숏컷
        MagicEngineering = 31, // 마도공학
        EventRps = 32, // 가위바위보
        EventDice = 33, // 주사위게임
        EndlessTower = 34, // 엔들리스 타워
        FacebookLikePage = 35, // 페이스북 좋아요 페이지
        ForestMaze = 36, // 미궁숲
        TutorialGuide = 37, // 튜토리얼 가이드
        GuildBattleEvent = 38, // 길드전 이벤트
        TimePatrol = 39, // 타임패트롤
        AgentCompose = 40, // 동료합성
        AttendEvent = 41, // 출석 이벤트 (14일)
        SummerEvent = 42, // 여름방학 이벤트 (포도송이)
        WordCollectionEvent = 43, // 단어 수집 이벤트 (추석이벤트)
        OnBuffEvent = 44, // 온버프 이벤트
    }

    public static class ShortCutTypeExtension
    {
        public static void GoShortCut(this ShortCutType type, int value = 0, RemainTime remainTime = default)
        {
            switch (type)
            {
                case ShortCutType.Dungeon:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Dungeon, isShowPopup: true))
                        return;

                    UIDungeon.viewType = (UIDungeon.ViewType)value;
                    UI.ShortCut<UIDungeon>();
                    break;

                case ShortCutType.Skill:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Skill, isShowPopup: true))
                        return;

                    UI.ShortCut<UISkill>();
                    break;

                case ShortCutType.Cupet:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Cupet, isShowPopup: true))
                        return;

                    UI.ShortCut<UICupet>();
                    break;

                case ShortCutType.Character:
                    UICharacterInfo.tabType = value.ToEnum<UICharacterInfo.TabType>();
                    UI.ShortCut<UICharacterInfo>();
                    break;

                case ShortCutType.Inventory:
                    UIInven.tabType = value.ToEnum<UIInven.TabType>();
                    UI.ShortCut<UIInven>();
                    break;

                case ShortCutType.Make:
                    UICraft.curMainTab = value;
                    UI.ShortCut<UIMake>();
                    break;

                case ShortCutType.Shop:
                    UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default, value.ToEnum<ShopTabType>());
                    break;

                case ShortCutType.SecretShop:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.SecretShop, isShowPopup: true))
                        return;

                    UI.ShortCut<UIShop>().Set(UIShop.ViewType.Secret);
                    break;

                case ShortCutType.Main:
                    UI.ShortCut<UIMain>();
                    UI.Show<UIMainTop>();
                    UI.Show<UIMainShortcut>();
                    break;

                case ShortCutType.JobChange:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.JobChange, isShowPopup: true))
                        return;

                    if (!Entity.player.Character.CanChangeJob())
                    {
                        UI.ShowToastPopup(LocalizeKey._90167.ToText()); // 직업 레벨이 부족하여 전직이 불가능합니다.
                        return;
                    }

                    UI.ShortCut<UIJobChange>();
                    break;

                case ShortCutType.Quest:
                    UIQuest.view = value.ToEnum<UIQuest.ViewType>();
                    UI.ShortCut<UIQuest>();
                    break;

                case ShortCutType.MakeItem:
                    UI.ShortCut<UIMake>(new UIMake.Input(value));
                    break;

                case ShortCutType.Event:
                    UI.ShortCut<UIDailyCheck>();
                    break;

                case ShortCutType.AdventureScenario:
                    if (BattleManager.Instance.GetCurrentEntry().mode == BattleMode.Stage && Entity.player.Dungeon.LastEnterStageId == value)
                    {
                        UI.ShowToastPopup(LocalizeKey._48600.ToText()); // 현재 위치에서 진행 가능한 퀘스트입니다.
                        return;
                    }

                    UI.ShortCut<UIAdventureMap>().SetStage(value);
                    break;

                case ShortCutType.Maze:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Maze, isShowPopup: true))
                        return;

                    if (BattleManager.Instance.GetCurrentEntry().mode == BattleMode.MultiMazeLobby)
                    {
                        UI.ShowToastPopup(LocalizeKey._48600.ToText()); // 현재 위치에서 진행 가능한 퀘스트입니다.
                        return;
                    }

                    UI.ShortCut<UIAdventureMazeSelect>().SelectId(value);
                    break;

                case ShortCutType.AgentExplore:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Explore, isShowPopup: true))
                        return;

                    UIAgent.view = value.ToEnum<UIAgent.ViewType>();
                    UI.ShortCut<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.ExploreAgent });
                    break;

                case ShortCutType.Sharing:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Sharing, isShowPopup: true))
                        return;

                    UI.ShortCut<UICharacterShare>();
                    break;

                case ShortCutType.UseItem:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Buff, isShowPopup: true))
                        return;

                    UI.ShortCut<UIBuffInfo>();
                    break;

                case ShortCutType.WorldBoss:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Dungeon, isShowPopup: true))
                        return;

                    Entity.player.Dungeon.SetSelectWorldBoss(value);
                    UI.ShortCut<UIWorldBoss>();
                    break;

                case ShortCutType.League:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Pvp, isShowPopup: true))
                        return;

                    UI.ShortCut<UILeague>();
                    break;

                case ShortCutType.MultiMaze:
                    if (!Entity.player.Quest.IsOpenContent(ContentType.Maze, isShowPopup: true))
                        return;

                    if (BattleManager.Instance.GetCurrentEntry().mode == BattleMode.MultiMazeLobby)
                    {
                        UI.ShowToastPopup(LocalizeKey._48600.ToText()); // 현재 위치에서 진행 가능한 퀘스트입니다.
                        return;
                    }

                    UI.ShortCut<UIAdventureMazeSelect>().SelectId(value);
                    break;

                case ShortCutType.Duel:
                    var duel = UI.ShortCut<UIDuel>();
                    duel.RequestShowChapter(value);
                    break;

                case ShortCutType.LoginBonus:
                    UI.Show<UILoginBonus>(new UILoginBonus.Input() { groupToShow = value, showUIDailyCheck = false });
                    break;

                case ShortCutType.EventRoulette:
                    UI.Show<UIRouletteEvent>();
                    break;

                case ShortCutType.MvpTutorial:
                    if (BattleManager.Instance.GetCurrentEntry().mode != BattleMode.Stage)
                        return;

                    Tutorial.ForceRun(TutorialType.Mvp);
                    break;

                case ShortCutType.Bingo:
                    UI.Show<UIBingo>();
                    break;

                case ShortCutType.SpecialRoulette:
                    UISpecialRoulette.ShowByConfig();
                    break;

                case ShortCutType.MagicEngineering:
                    UI.ShortCut<UIMake>(new UIMake.Input(UIMake.InputType.ShortCut, value));
                    break;

                case ShortCutType.EventRps:
                    var uiRps = UI.ShortCut<UIRpsEvent>();
                    uiRps.SetRemainTime(remainTime); // 이벤트 남은시간 셋팅
                    break;

                case ShortCutType.EventDice:
                    UI.ShortCut<UIDiceEvent>();
                    break;

                case ShortCutType.EndlessTower:
                    if (!BasisOpenContetsType.EndlessTower.IsOpend())
                    {
                        string message = LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                        UI.ShowToastPopup(message);
                        return;
                    }

                    if (!Entity.player.Quest.IsOpenContent(ContentType.Dungeon, isShowPopup: true))
                        return;

                    if (!Entity.player.Dungeon.IsOpened(DungeonType.EnlessTower, isShowPopup: true))
                        return;

                    UI.ShortCut<UIEndlessTower>();
                    break;

                case ShortCutType.FacebookLikePage:
                    // 방어코드 (한국의 경우는 처리하지 않음)
                    if (GameServerConfig.IsKorea())
                        return;

                    bool isTaiwan = ConnectionManager.Instance.IsTaiwan();

                    if (isTaiwan)
                    {
                        BasisUrl.TaiwanFaceBookHompage.OpenUrl(); // 페이스북 공식 페이지 이동
                    }
                    else
                    {
                        BasisUrl.FaceBookHomepage.OpenUrl(); // 페이스북 공식 페이지 이동
                    }

                    Entity.player.User.LikedFacebook().WrapNetworkErrors(); // 좋아요 보상
                    break;

                case ShortCutType.ForestMaze:
                    if (!BasisOpenContetsType.ForestMaze.IsOpend())
                    {
                        string message = LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                        UI.ShowToastPopup(message);
                        return;
                    }

                    if (!Entity.player.Quest.IsOpenContent(ContentType.Maze, isShowPopup: true))
                        return;

                    if (!Entity.player.Dungeon.IsOpened(DungeonType.ForestMaze, isShowPopup: true))
                        return;

                    if (BattleManager.Instance.GetCurrentEntry().mode == BattleMode.MultiMazeLobby)
                    {
                        UI.ShowToastPopup(LocalizeKey._48600.ToText()); // 현재 위치에서 진행 가능한 퀘스트입니다.
                        return;
                    }

                    UI.ShortCut<UIAdventureMazeSelect>().SelectId(value);
                    break;

                case ShortCutType.TutorialGuide:
                    UITutorialGuide.GuideType guideType = value.ToEnum<UITutorialGuide.GuideType>();

                    switch (guideType)
                    {
                        default:
                            Debug.LogError($"정의되지 않은 {nameof(guideType)} = {guideType}");
                            return;

                        case UITutorialGuide.GuideType.CardSmelt:
                            UIInven.tabType = value.ToEnum<UIInven.TabType>();
                            UI.ShortCut<UIInven>();
                            break;
                    }

                    UI.Show<UITutorialGuide>().SetGuideType(guideType);
                    break;

                case ShortCutType.GuildBattleEvent:
                    UI.ShortCut<UIGuildBattleRankEvent>();
                    break;

                case ShortCutType.TimePatrol:
                    if (!Entity.player.Dungeon.IsOpenTimePatrol(isShowPopup: true))
                        return;

                    UI.ShortCut<UIAdventureMazeSelect>().ShowTimePatrol();
                    break;

                case ShortCutType.AgentCompose:
                    UIAgent.view = UIAgent.ViewType.Compose;
                    UI.ShortCut<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
                    break;

                case ShortCutType.AttendEvent:
                    UI.ShortCut<UIAttendEvent>();
                    break;

                case ShortCutType.SummerEvent:
                    UI.ShortCut<UISummerEvent>();
                    break;

                case ShortCutType.WordCollectionEvent:
                    UI.ShortCut<UIWordCollectionEvent>();
                    break;
            }
        }
    }
}