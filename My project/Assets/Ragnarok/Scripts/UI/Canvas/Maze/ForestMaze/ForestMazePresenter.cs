using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIForestMaze"/>
    /// </summary>
    public class ForestMazePresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;
        private readonly SharingModel sharingModel;

        // <!-- Repositories --!>
        private readonly DungeonInfoDataManager dungeonInfoDataRepo;
        private readonly MultiMazeWaitingRoomDataManager multiMazeWaitingRoomDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly ForestBaseDataManager forestBaseDataRepo;
        private readonly int ticketItemId;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        private bool isStartBattle;

        public ForestMazePresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            inventoryModel = Entity.player.Inventory;
            questModel = Entity.player.Quest;
            sharingModel = Entity.player.Sharing;

            dungeonInfoDataRepo = DungeonInfoDataManager.Instance;
            multiMazeWaitingRoomDataRepo = MultiMazeWaitingRoomDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            forestBaseDataRepo = ForestBaseDataManager.Instance;
            ticketItemId = BasisItem.ForestMazeTicket.GetID(); // 입장권

            battleManager = BattleManager.Instance;
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
                UI.Close<UIForestMaze>();
        }

        /// <summary>
        /// 던전 인포 데이터 반환
        /// </summary>
        public ForestMazeView.IInput GetData()
        {
            int infoId = DungeonInfoType.ForestMaze.GetDungeonInfoId();
            return dungeonInfoDataRepo.Get(infoId);
        }

        /// <summary>
        /// 권장전투력 반환
        /// </summary>
        public int GetBattleScore()
        {
            MultiMazeWaitingRoomData data = multiMazeWaitingRoomDataRepo.Get(MultiMazeWaitingRoomData.MULTI_MAZE_LOBBY_FOREST_MAZE);
            if (data == null)
                return 0;

            return data.battle_score;
        }

        /// <summary>
        /// 무료 입장 가능 수
        /// </summary>
        public int GetFreeEntryCount()
        {
            return dungeonModel.GetFreeEntryCount(DungeonType.ForestMaze);
        }

        /// <summary>
        /// 입장권 아이콘
        /// </summary>
        public string GetTicketItemIcon()
        {
            ItemData itemData = itemDataRepo.Get(ticketItemId);
            if (itemData == null)
                return string.Empty;

            return itemData.icon_name;
        }

        /// <summary>
        /// 입장권 보유 개수
        /// </summary>
        public int GetTicketCount()
        {
            return inventoryModel.GetItemCount(ticketItemId);
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartBattle()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            ForestBaseData firstData = forestBaseDataRepo.GetFirstData();
            if (firstData == null)
                return;

            // 오픈조건
            if (!dungeonModel.IsOpend(firstData, isShowPopup: true))
                return;

            // 가방 무게 체크
            if (!UI.CheckInvenWeight())
                return;

            // 무료 입장 횟수 없음
            if (GetFreeEntryCount() == 0)
            {
                // 티켓 부족
                if (GetTicketCount() == 0)
                {
                    UI.ShowConfirmItemPopup(RewardType.Item, ticketItemId, 1, LocalizeKey._90262); // 아이템이 부족합니다.
                    return;
                }
            }

            StartForestMaze(firstData.id).WrapUIErrors();
        }

        private async Task StartForestMaze(int id)
        {
            if (CanEquipSharingCharacter())
            {
                if (!await UI.SelectShortCutPopup(LocalizeKey._48711.ToText(), LocalizeKey._48712.ToText(), ShowShareUI))
                    return;
            }

            isStartBattle = true;
            battleManager.StartBattle(BattleMode.ForestMaze, id);
        }

        /// <summary>
        /// 셰어 가능 시간이 남아있는데 셰어캐릭터가 하나도 없을 경우.
        /// </summary>
        private bool CanEquipSharingCharacter()
        {
            // 셰어기능 사용 가능 체크
            if (!questModel.IsOpenContent(ContentType.Sharing))
                return false;

            // 셰어가능 시간 보유량 체크
            if (sharingModel.GetRemainTimeForShare() <= 1000)
                return false;

            // 셰어캐릭터를 이미 장착중인지 체크
            if (sharingModel.HasSharingCharacters())
                return false;

            return true;
        }

        private void ShowShareUI()
        {
            UI.Show<UICharacterShare>();
        }
    }
}