using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIItemSource"/>
    /// </summary>
    public class ItemSourcePresenter : ViewPresenter
    {
        public interface IView
        {
            ItemInfo GetItemInfo();
        }

        private readonly IView view;
        private readonly QuestModel questModel;
        private readonly DungeonModel dungeonModel;
        private readonly GuildModel guildModel;
        private readonly BattleManager battleManager;
        private readonly ConnectionManager connectionManager;

        public ItemSourcePresenter(IView view)
        {
            this.view = view;

            questModel = Entity.player.Quest;
            dungeonModel = Entity.player.Dungeon;
            guildModel = Entity.player.Guild;
            battleManager = BattleManager.Instance;
            connectionManager = ConnectionManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 해당 아이템의 사용처/획득처 카테고리를 얻는다.
        /// </summary>
        public ItemSourceCategoryData[] GetCategoryTypeArray(ItemInfo itemInfo, UIItemSource.Mode mode)
        {
            // 모드를 구분해서 사용처/획득처를 구분해 카테고리를 받아온다.
            ItemSourceCategoryType categoryData = ItemSourceCategoryType.None;
            if (mode == UIItemSource.Mode.GetSource)
            {
                categoryData = itemInfo.Get_ClassBitType;
            }
            else if (mode == UIItemSource.Mode.Use)
            {
                categoryData = itemInfo.Use_ClassBitType;
            }

            // 카테고리 Flag Enum을 각각 리스트로 변환. (27) -> (1 | 2 | 8 | 16) -> [1, 2, 8, 16]
            //ItemSourceCategoryType[] retArray = (ItemSourceCategoryType.DisassembleCard.ToIntValue() * 2 - 1).ToEnum<ItemSourceCategoryType>().FlagsToArray(); // 테스트
            ItemSourceCategoryType[] retArray = categoryData.FlagsToArray();
            List<ItemSourceCategoryData> retList = new List<ItemSourceCategoryData>();
            foreach (var category in retArray)
            {
                if (category == ItemSourceCategoryType.None)
                    continue;

                // 나비호 컨텐츠 비활성화시 목록에서 제거
                if(category == ItemSourceCategoryType.Nabiho || category == ItemSourceCategoryType.UseNabiho)
                {
                    if(BasisType.NABIHO_OPEN_BY_SERVER.GetInt(connectionManager.GetSelectServerGroupId()) == 1) // 컨텐츠 비활성화
                        continue;
                }

                ItemSourceCategoryData newData = new ItemSourceCategoryData(category, itemInfo);
                retList.Add(newData);
            }

            retList.Sort((x, y) => x.categoryType.Sort().CompareTo(y.categoryType.Sort()));
            return retList.ToArray();
        }

        /// <summary>
        /// 특정 UI로 이동
        /// </summary>
        public void Move(ItemSourceCategoryType categoryType, ItemInfo itemInfo)
        {
            switch (categoryType)
            {
                case ItemSourceCategoryType.Make: // 제작 UI의 해당 아이템으로 이동
                    UI.ShortCut<UIMake>(new UIMake.Input(itemInfo.ItemId));
                    break;

                case ItemSourceCategoryType.SecretShop: // 비밀 상점 UI로 이동
                    UI.ShortCut<UIShop>().Set(UIShop.ViewType.Secret);
                    break;

                case ItemSourceCategoryType.ConsumableShop: // 소모품 상점으로 이동
                    UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.Consumable);
                    break;

                case ItemSourceCategoryType.BoxShop: // 상자 상점으로 이동
                    UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.Box);
                    break;

                case ItemSourceCategoryType.EveryDayShop: // 매일매일 상점으로 이동
                    UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.EveryDay);
                    break;

                case ItemSourceCategoryType.DailyQuest: // 일일 퀘스트
                    UI.ShortCut<UIQuest>(new UIQuest.Input(UIQuest.ViewType.Daily));
                    break;

                case ItemSourceCategoryType.GuildQuest: // 길드 퀘스트
                    UI.ShortCut<UIQuest>(new UIQuest.Input(UIQuest.ViewType.Guild));
                    break;

                case ItemSourceCategoryType.Event: // 이벤트 UI
                case ItemSourceCategoryType.UseEvent:
                    UI.ShortCut<UIDailyCheck>();
                    break;

                case ItemSourceCategoryType.League: // 대전
                case ItemSourceCategoryType.UseLeague: // 대전
                    UI.ShortCut<UILeague>();
                    break;

                case ItemSourceCategoryType.DungeonDrop: // 던전 UI
                    UI.ShortCut<UIDungeon>();
                    break;

                case ItemSourceCategoryType.WorldBoss: // 월드보스 UI
                    UI.ShortCut<UIWorldBoss>();
                    break;

                case ItemSourceCategoryType.Maze: // 카드 미로 UI
                    UI.ShortCut<UIAdventureMazeSelect>();
                    break;

                case ItemSourceCategoryType.AgentExplore: // 탐험 UI
                    UIAgent.view = UIAgent.ViewType.Explore;
                    UI.ShortCut<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.ExploreAgent });
                    break;

                case ItemSourceCategoryType.FreeFight: // 난전
                    UI.ShortCut<UIFreeFight>();
                    break;

                case ItemSourceCategoryType.EndlessTower: // 엔들리스 타워
                    UI.ShortCut<UIEndlessTower>();
                    break;

                case ItemSourceCategoryType.ForestMaze: // 미궁숲
                    UI.ShortCut<UIAdventureMazeSelect>();
                    break;

                case ItemSourceCategoryType.UseExchange: // 카프라 교환소
                    AsyncGoToExchangeNpc().WrapUIErrors();
                    break;

                case ItemSourceCategoryType.UseKafraDelivery: // 카프라 운송
                    AsyncGoToSorinNpc().WrapUIErrors();
                    break;

                case ItemSourceCategoryType.DisassembleEquipment:
                    UIInven.tabType = UIInven.TabType.Equipment;
                    UI.ShortCut<UIInven>();
                    break;

                case ItemSourceCategoryType.DisassembleCard:
                    UIInven.tabType = UIInven.TabType.Card;
                    UI.ShortCut<UIInven>();
                    break;

                case ItemSourceCategoryType.DisassembleCostume:
                    UIInven.tabType = UIInven.TabType.Costume;
                    UI.ShortCut<UIInven>();
                    break;

                case ItemSourceCategoryType.TimePatrol:
                    UI.ShortCut<UIAdventureMazeSelect>().ShowTimePatrol();
                    break;

                case ItemSourceCategoryType.GuildBattle: // 길드전
                    if (guildModel.GuildBattleSeasonType == GuildBattleSeasonType.Ready)
                    {
                        UI.Show<UIGuildBattleReady>();
                    }
                    else if (guildModel.GuildBattleSeasonType == GuildBattleSeasonType.InProgress)
                    {
                        UI.Show<UIGuildBattleEnter>();
                    }
                    break;

                case ItemSourceCategoryType.Gate: // 카드 미로 UI
                    UI.ShortCut<UIAdventureMazeSelect>().SelectId(MultiMazeWaitingRoomData.GATE_1);
                    break;

                case ItemSourceCategoryType.Nabiho: // 나비호 UI
                case ItemSourceCategoryType.UseNabiho: // 나비호 UI
                    UI.ShortCut<UINabiho>();
                    break;
            }
        }

        public ItemInfo GetItemInfo()
        {
            return view.GetItemInfo();
        }

        /// <summary>
        /// 이용 가능한 컨텐츠의 카테고리인지 체크
        /// </summary>
        public bool IsOpenedContent(ItemSourceCategoryType categoryType, bool isShowPopup = true)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return false;
            }

            BattleMode curMode = BattleManager.Instance.Mode;
            if (curMode == BattleMode.Stage || curMode == BattleMode.MultiMazeLobby || curMode == BattleMode.Lobby || curMode == BattleMode.TimePatrol)
            {
                // Do Nothing
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._46400.ToText()); // 현재 위치에서 이동이 불가능합니다.
                return false;
            }

            switch (categoryType)
            {
                case ItemSourceCategoryType.StageDrop: // 1 << 0. 스테이지 드랍
                    return questModel.IsOpenContent(ContentType.Stage, isShowPopup);

                case ItemSourceCategoryType.DungeonDrop: // 1 << 1. 던전 드랍
                    return questModel.IsOpenContent(ContentType.Dungeon, isShowPopup);

                case ItemSourceCategoryType.WorldBoss: // 1 << 2. 월드 보스
                    return questModel.IsOpenContent(ContentType.Dungeon, isShowPopup);

                case ItemSourceCategoryType.Box: // 1 << 3. 상자
                    return true;

                case ItemSourceCategoryType.Make: // 1 << 4. 제작
                    return true; // 컨텐츠 내부 잠금

                case ItemSourceCategoryType.SecretShop: // 1 << 5. 비밀 상점
                    return questModel.IsOpenContent(ContentType.SecretShop, isShowPopup);

                case ItemSourceCategoryType.ConsumableShop: // 1 << 6. 아이템 상점
                    return true;

                case ItemSourceCategoryType.BoxShop: // 1 << 7. 상자 상점
                    return true;

                case ItemSourceCategoryType.GuildShop: // 1 << 8. 길드 상점
                    return true;

                case ItemSourceCategoryType.EveryDayShop: // 1 << 9. 매일매일 상점
                    return true;

                case ItemSourceCategoryType.DailyQuest: // 1 << 10. 일일 퀘스트
                    return true;

                case ItemSourceCategoryType.GuildQuest: // 1 << 11. 길드 퀘스트
                    return true;

                case ItemSourceCategoryType.Event: // 1 << 12. 이벤트
                    return true;

                case ItemSourceCategoryType.UseMake: // 1 << 13. [사용처] 제작
                    return true; // 컨텐츠 내부 잠금

                case ItemSourceCategoryType.UseEnchantMaterial: // 1 << 14. [사용처] 강화 재료
                    return questModel.IsOpenContent(ContentType.ItemEnchant, isShowPopup);

                case ItemSourceCategoryType.UseTierChange: // 1 << 15. [사용처] 등급 변경
                    return true;

                case ItemSourceCategoryType.UseProtectEnchantBreak: // 1 << 16. [사용처] 강화 보호
                    return questModel.IsOpenContent(ContentType.ItemEnchant, isShowPopup);

                case ItemSourceCategoryType.UseDungeonSweep: // 1 << 17. [사용처] 던전 소탕
                    return questModel.IsOpenContent(ContentType.Dungeon, isShowPopup);

                case ItemSourceCategoryType.League: // 1 << 18. 대전
                    return questModel.IsOpenContent(ContentType.Pvp, isShowPopup);

                case ItemSourceCategoryType.UseLeague: // 1 << 19. [사용처] 대전
                    return questModel.IsOpenContent(ContentType.Pvp, isShowPopup);

                case ItemSourceCategoryType.DisassembleEquipment: // 1 << 20. 장비 분해
                    return true;

                case ItemSourceCategoryType.DisassembleCard: // 1 << 21. 카드 분해
                    return true;

                case ItemSourceCategoryType.Maze: // 1 << 22. [획득처] 미로
                    return questModel.IsOpenContent(ContentType.Maze, isShowPopup);

                case ItemSourceCategoryType.MVP: // 1 << 23. [획득처] MVP
                    return true;

                case ItemSourceCategoryType.AgentExplore: // 1 << 24. [획득처] 파견
                    return questModel.IsOpenContent(ContentType.Explore, isShowPopup);

                case ItemSourceCategoryType.UseEvent: // 1 << 25. [사용처] 이벤트
                    return true;

                case ItemSourceCategoryType.FreeFight: // 1 << 26. [획득처] 난전
                    return questModel.IsOpenContent(ContentType.FreeFight, isShowPopup);

                case ItemSourceCategoryType.EndlessTower: // 1 << 27. [획득처] 엔들리스 타워
                    if (!BasisOpenContetsType.EndlessTower.IsOpend())
                    {
                        if (isShowPopup)
                        {
                            string message = LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                            UI.ShowToastPopup(message);
                        }
                        return false;
                    }

                    return questModel.IsOpenContent(ContentType.Dungeon, isShowPopup) && dungeonModel.IsOpened(DungeonType.EnlessTower, isShowPopup);

                case ItemSourceCategoryType.ForestMaze: // 1 << 28. [획득처] 미궁숲
                    if (!BasisOpenContetsType.ForestMaze.IsOpend())
                    {
                        if (isShowPopup)
                        {
                            string message = LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                            UI.ShowToastPopup(message);
                        }
                        return false;
                    }

                    return questModel.IsOpenContent(ContentType.Maze, isShowPopup) && dungeonModel.IsOpened(DungeonType.ForestMaze, isShowPopup);

                case ItemSourceCategoryType.UseExchange: // 1 << 29. [사용처] 카프라 교환소
                case ItemSourceCategoryType.UseKafraDelivery: // 1 << 30. [사용처] 카프라 운송
                case ItemSourceCategoryType.Nabiho: // 1 << 35 [획득처] 나비호
                case ItemSourceCategoryType.UseNabiho: // 1 << 36 [사용처] 나비호
                    return questModel.IsOpenContent(ContentType.TradeTown, isShowPopup);

                case ItemSourceCategoryType.DisassembleCostume: // 1 << 31 [획득처] 코스튬 분해
                    return true;

                case ItemSourceCategoryType.TimePatrol: // 1 << 32 [획득처] 타임패트롤
                    return dungeonModel.IsOpenTimePatrol(isShowPopup);

                case ItemSourceCategoryType.GuildBattle: // 1 << 33 [획득처] 길드전
                    return guildModel.HasGuild(isShowPopup);

                case ItemSourceCategoryType.Gate: // 1 << 34 [획득처] 게이트
                    return dungeonModel.IsOpenGate(isShowPopup);
            }

            return true;
        }

        private async Task AsyncGoToExchangeNpc()
        {
            if (!await UI.SelectPopup(LocalizeKey._90294.ToText())) // 카프라 교환소는 거래소에 있습니다.\n거래소로 이동하시겠습니까?
                return;

            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            // 이미 거래소에 존재
            if (battleManager.Mode == BattleMode.Lobby)
            {
                UI.ShowToastPopup(LocalizeKey._90181.ToText()); // 이미 거래소에 위치하고 있습니다.
                return;
            }

            if (!questModel.IsOpenContent(ContentType.TradeTown, true))
                return;

            battleManager.StartBattle(BattleMode.Lobby, LobbyEntry.PostAction.MoveToNpcTailing);
            UIManager.Instance.ShortCut();
        }

        private async Task AsyncGoToSorinNpc()
        {
            if (!await UI.SelectPopup(LocalizeKey._90305.ToText())) // 카프라 운송은 거래소에 있습니다.\n거래소로 이동하시겠습니까?
                return;

            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            // 이미 거래소에 존재
            if (battleManager.Mode == BattleMode.Lobby)
            {
                UI.ShowToastPopup(LocalizeKey._90181.ToText()); // 이미 거래소에 위치하고 있습니다.
                return;
            }

            if (!questModel.IsOpenContent(ContentType.TradeTown, true))
                return;

            battleManager.StartBattle(BattleMode.Lobby, LobbyEntry.PostAction.MoveToNpcSorin);
            UIManager.Instance.ShortCut();
        }
    }
}