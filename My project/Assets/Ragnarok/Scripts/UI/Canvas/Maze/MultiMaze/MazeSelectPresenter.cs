using UnityEngine;

namespace Ragnarok
{
    public class MazeSelectPresenter : ViewPresenter
    {
        private readonly UIMazeSelect view;
        private readonly BattleManager battleManager;
        private readonly DungeonModel dungeonModel;
        private readonly MultiMazeDataManager multiMazeDataRepo;
        private readonly ScenarioMazeDataManager scenarioMazeDataRepo;
        private readonly StageDataManager stageDataRepo;

        private readonly int buyTicketCatCoin; // 유료 입장 비용
        private readonly int buyTicketIncCatCoin; // 유료 입장 비용 가중치

        private MultiMazeData multiMazeData;
        private ScenarioMazeData prevScenarioMazeData;
        private ScenarioMazeData scenarioMazeData;
        private bool isStartBattle = false;

        public int MultiMazeDungeonInfoId => multiMazeData.dungeon_info_id;

        public MazeSelectPresenter(UIMazeSelect view)
        {
            this.view = view;
            dungeonModel = Entity.player.Dungeon;
            multiMazeDataRepo = MultiMazeDataManager.Instance;
            scenarioMazeDataRepo = ScenarioMazeDataManager.Instance;
            stageDataRepo = StageDataManager.Instance;

            battleManager = BattleManager.Instance;

            buyTicketCatCoin = BasisType.MULTI_MAZE_CAT_COIN_JOIN.GetInt();
            buyTicketIncCatCoin = BasisType.MULTI_MAZE_CAT_COIN_INC.GetInt();
        }

        public void OnShow(int multiMazeId)
        {
            isStartBattle = false;
            multiMazeData = multiMazeDataRepo.Get(multiMazeId);
            scenarioMazeData = scenarioMazeDataRepo.Get(multiMazeData.open_scenario_id);
            prevScenarioMazeData = multiMazeData.open_scenario_id == 1 ? null : scenarioMazeDataRepo.Get(multiMazeData.open_scenario_id - 1);

            view.ShowRemainTicketCount(GetTicketCount(), GetTicketMaxCount());
            view.SetScenarioMazeLock(!IsScenarioMazeOpen(isShowMessage: false));
            view.SetMultiMazeLock(!IsMultiMazeOpen());
            view.SetScenarioMazeFirstClearReward(new RewardData(scenarioMazeData.reward_type1, scenarioMazeData.reward_value1, scenarioMazeData.reward_count1));
            view.SetScenarioMazeFirstClearRewardReceived(IsMultiMazeOpen());
            view.SetRecommandPower(multiMazeData.battle_score);
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
            dungeonModel.OnUpdateMultiMazeTicket += OnUpdateMultiMazeTicket;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
            dungeonModel.OnUpdateMultiMazeTicket -= OnUpdateMultiMazeTicket;
        }

        private void OnUpdateMultiMazeTicket()
        {
            view.ShowRemainTicketCount(GetTicketCount(), GetTicketMaxCount());
        }

        public void ViewEventHandler(UIMazeSelect.Event eventType, object data)
        {
            if (eventType == UIMazeSelect.Event.EnterMultiMaze)
            {
                if (UIBattleMatchReady.IsMatching)
                {
                    string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                    UI.ShowToastPopup(message);
                    return;
                }

                if (!IsMultiMazeOpen())
                {
                    UI.ShowToastPopup(LocalizeKey._90171.ToText()); // 싱글모드 클리어시 오픈됩니다
                    return;
                }

                bool buyTicket = (bool)data;
                if (!buyTicket)
                {
                    if (GetTicketCount() <= 0)
                    {
                        view.ShowTicketView(GetNeedCatCoint());
                        return;
                    }
                }

                StartMultiMaze();
            }
            else if (eventType == UIMazeSelect.Event.EnterScenarioMaze)
            {
                if (UIBattleMatchReady.IsMatching)
                {
                    string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                    UI.ShowToastPopup(message);
                    return;
                }

                if (!IsScenarioMazeOpen(isShowMessage: true))
                    return;

                StartScenarioMaze();
            }
            else if (eventType == UIMazeSelect.Event.Close)
            {
                UI.Close<UIMazeSelect>();
            }
        }

        public int GetTicketCount()
        {
            return dungeonModel.GetFreeEntryCount(DungeonType.MultiMaze);
        }

        public int GetTicketMaxCount()
        {
            return dungeonModel.GetFreeEntryMaxCount(DungeonType.MultiMaze);
        }

        private bool IsScenarioMazeOpen(bool isShowMessage)
        {
            int chapter = multiMazeData.chapter;
            StageData find = stageDataRepo.FindWithChapter(StageChallengeType.Normal, chapter);
            if (find == null)
            {
                Debug.LogError($"chapter에 해당하는 stageData가 존재하지 않음: {nameof(chapter)} = {chapter}");
                return false;
            }

            // 해당 챕터의 스테이지 도달
            if (find.id <= dungeonModel.FinalStageId && dungeonModel.LastClearedScenarioID >= multiMazeData.open_scenario_id - 1)
                return true;

            if (isShowMessage)
            {
                if (find.id > dungeonModel.FinalStageId)
                {
                    string message = LocalizeKey._48801.ToText() // {NAME}에 도달하면 오픈됩니다.
                        .Replace(ReplaceKey.NAME, find.name_id.ToText());
                    UI.ShowToastPopup(message);
                }
                else
                {
                    var scenarioMaze = ScenarioMazeDataManager.Instance.Get(multiMazeData.open_scenario_id - 1);
                    UI.ShowToastPopup(LocalizeKey._54311.ToText().Replace(ReplaceKey.NAME, scenarioMaze.name_id.ToText()));
                }
            }

            return false;
        }

        private bool IsMultiMazeOpen()
        {
            return dungeonModel.IsCleardScenarioMazeId(multiMazeData.open_scenario_id);
        }

        private async void StartMultiMaze()
        {
            // 티켓 & 냥다래 체크  
            if (!dungeonModel.IsFreeEntry(DungeonType.MultiMaze))
            {
                int needCoin = GetNeedCatCoint();

                if (!CoinType.CatCoin.Check(needCoin))
                    return;
            }

            if (CanEquipSharingCharacter())
            {
                if (!await UI.SelectShortCutPopup(LocalizeKey._48711.ToText(), LocalizeKey._48712.ToText(), ShowShareUI))
                    return;
            }

            MultiMazeData.MultiMazeType mazeType = multiMazeData.GetMultiMazeType();
            int id = multiMazeData.id;
            switch (mazeType)
            {
                case MultiMazeData.MultiMazeType.Normal:
                    Analytics.TrackEvent(TrackType.LabyrinthSingleEnter);
                    StartMaze(BattleMode.MultiMaze, id);
                    break;

                case MultiMazeData.MultiMazeType.Match:
                    Analytics.TrackEvent(TrackType.LabyrinthMultiEnter);
                    CloseUi();
                    UI.Show<UIBattleMatchReady>(new UIBattleMatchReady.Input { id = id });
                    break;

                default:
                    Debug.LogError($"정의되지 않은 미로입니다: {nameof(id)} = {id}");
                    break;
            }
        }

        private async void StartScenarioMaze()
        {
            if (CanEquipSharingCharacter())
            {
                if (!await UI.SelectShortCutPopup(LocalizeKey._48711.ToText(), LocalizeKey._48712.ToText(), ShowShareUI))
                    return;
            }

            dungeonModel.SetLastEnterMultiMazeId(multiMazeData.id);
            StartMaze(BattleMode.ScenarioMaze, scenarioMazeData.id);
        }

        /// <summary>
        /// 셰어 가능 시간이 남아있는데 셰어캐릭터가 하나도 없을 경우.
        /// </summary>
        private bool CanEquipSharingCharacter()
        {
            // 셰어기능 사용 가능 체크
            if (!Entity.player.Quest.IsOpenContent(ContentType.Sharing))
                return false;

            // 셰어가능 시간 보유량 체크
            if (Entity.player.Sharing.GetRemainTimeForShare() <= 1000)
                return false;

            // 셰어캐릭터를 이미 장착중인지 체크
            if (Entity.player.Sharing.HasSharingCharacters())
                return false;

            return true;
        }

        private int GetNeedCatCoint()
        {
            int entryCount = dungeonModel.GetEntryCount(DungeonType.MultiMaze); // 실제 입장 횟수
            int overCount = entryCount - dungeonModel.GetFreeEntryMaxCount(DungeonType.MultiMaze); // 추가 입장 하려는 횟수
            int needCoin = buyTicketCatCoin + (buyTicketIncCatCoin * overCount); // 필요 냥다래
            return needCoin;
        }

        private void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
                CloseUi();
        }

        private void StartMaze(BattleMode battleMode, int id)
        {
            isStartBattle = true;

            dungeonModel.SetLastEnterMultiMazeLobbyId(multiMazeData.chapter);
            battleManager.StartBattle(battleMode, id);
        }

        void ShowShareUI()
        {
            UI.Show<UICharacterShare>();
        }

        private void CloseUi()
        {
            UI.Close<UIMazeSelect>();
        }
    }
}