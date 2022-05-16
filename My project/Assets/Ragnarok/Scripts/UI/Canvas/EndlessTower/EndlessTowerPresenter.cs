using Ragnarok.View;
using System.Linq;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIEndlessTower"/>
    /// </summary>
    public class EndlessTowerPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly DungeonModel dungeonModel;
        private readonly GoodsModel goodsModel;
        private readonly AgentModel agentModel;
        private readonly QuestModel questModel;
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly EndlessTowerDataManager endlessTowerDataRepo;
        private readonly ItemDataManager itemDataRepo;
        private readonly BetterList<EndlessTowerSkipFloor> skipFloorList; // 스킵 정보
        private readonly int ticketItemId;
        private readonly int skipItemId;
        private readonly int maxFloor;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action<long> OnUpdateZeny
        {
            add { goodsModel.OnUpdateZeny += value; }
            remove { goodsModel.OnUpdateZeny -= value; }
        }

        public event System.Action<long> OnUpdateCatCoin
        {
            add { goodsModel.OnUpdateCatCoin += value; }
            remove { goodsModel.OnUpdateCatCoin -= value; }
        }

        public event System.Action OnUpdateEquippedAgent
        {
            add { agentModel.OnUpdateAgentEquipmentState += value; }
            remove { agentModel.OnUpdateAgentEquipmentState -= value; }
        }

        public event System.Action OnUpdateNewAgent
        {
            add { agentModel.OnGetNewAgent += value; }
            remove { agentModel.OnGetNewAgent -= value; }
        }

        public event System.Action OnUpdateEndlessTowerFreeTicket
        {
            add { dungeonModel.OnUpdateEndlessTowerFreeTicket += value; }
            remove { dungeonModel.OnUpdateEndlessTowerFreeTicket -= value; }
        }

        public event System.Action OnUpdateTicketCount;

        private bool isStartBattle;

        public EndlessTowerPresenter()
        {
            dungeonModel = Entity.player.Dungeon;
            goodsModel = Entity.player.Goods;
            agentModel = Entity.player.Agent;
            questModel = Entity.player.Quest;
            inventoryModel = Entity.player.Inventory;

            endlessTowerDataRepo = EndlessTowerDataManager.Instance;
            itemDataRepo = ItemDataManager.Instance;
            skipFloorList = new BetterList<EndlessTowerSkipFloor>();
            ticketItemId = BasisItem.EndlessTowerTicket.GetID(); // 입장권
            skipItemId = BasisItem.EndlessTowerSkipItem.GetID(); // 어둠의 재
            maxFloor = BasisType.ENDLESS_TOWER_MAX_FLOOR.GetInt(); // 최대 층

            battleManager = BattleManager.Instance;

            // 층 별 스킵 재료 세팅
            skipFloorList.Add(new EndlessTowerSkipFloor(1, null)); // 기본 1층 추가
            var list = BasisType.ENDLESS_TOWER_SKIP_INFO.GetKeyList();
            for (int i = 0; i < list.Count; i++)
            {
                int skipItemCount = list[i]; // 필요 아이템 개수
                int floor = BasisType.ENDLESS_TOWER_SKIP_INFO.GetInt(skipItemCount);

                // 최대 층보다 높을 경우 제외
                if (floor > maxFloor)
                    continue;

                skipFloorList.Add(new EndlessTowerSkipFloor(floor, new RewardData(RewardType.Item, skipItemId, skipItemCount)));
            }
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStartBattle;
            inventoryModel.AddItemEvent(ticketItemId, OnUpdateTicketItem);
        }

        public override void RemoveEvent()
        {
            inventoryModel.RemoveItemEvent(ticketItemId, OnUpdateTicketItem);
            BattleManager.OnStart -= OnStartBattle;
        }

        void OnStartBattle(BattleMode mode)
        {
            if (isStartBattle)
                UI.Close<UIEndlessTower>();
        }

        void OnUpdateTicketItem()
        {
            OnUpdateTicketCount?.Invoke();
        }

        /// <summary>
        /// 전투동료 목록 반환
        /// </summary>
        public CombatAgent[] GetCombatAgents()
        {
            return agentModel.GetEquipedCombatAgents().ToArray();
        }

        /// <summary>
        /// 전투동료 슬롯 개수 반환
        /// </summary>
        public int GetCombatAgentSlotCount()
        {
            return agentModel.CombatAgentSlotCount;
        }

        /// <summary>
        /// 전투동료 장착 가능 여부
        /// </summary>
        public bool CanEquipAgent()
        {
            return agentModel.CanEquipAgent();
        }

        /// <summary>
        /// 전투동료 컨텐츠 오픈 여부
        /// </summary>
        public bool IsOpenAgent()
        {
            return questModel.IsOpenContent(ContentType.CombatAgent);
        }

        /// <summary>
        /// 층 데이터 반환
        /// </summary>
        public UIEndlessTowerElement.IInput[] GetFloorData()
        {
            BetterList<EndlessTowerData> dataList = new BetterList<EndlessTowerData>();
            EndlessTowerData[] arrData = endlessTowerDataRepo.GetArrayData();
            for (int i = 0; i < arrData.Length; i++)
            {
                // 최대 층보다 높을 경우 제외
                if (arrData[i].GetFloor() > maxFloor)
                    continue;

                dataList.Add(arrData[i]);
            }

            return dataList.ToArray();
        }

        /// <summary>
        /// 최대 클리어한 층 반환
        /// </summary>
        public int GetClearedFloor()
        {
            return dungeonModel.EndlessTowerClearedFloor;
        }

        /// <summary>
        /// 스킵 층 정보
        /// </summary>
        public EndlessTowerFloorSelectView.IInput[] GetSkipFloorData()
        {
            return skipFloorList.ToArray();
        }

        /// <summary>
        /// 무료 입장 가능 수
        /// </summary>
        public int GetFreeEntryCount()
        {
            return dungeonModel.GetFreeEntryCount(DungeonType.EnlessTower);
        }

        /// <summary>
        /// 무료 입장 최대 가능 수
        /// </summary>
        public int GetMaxFreeEntryCount()
        {
            return dungeonModel.GetFreeEntryMaxCount(DungeonType.EnlessTower);
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
        /// 최대 층 반환
        /// </summary>
        public int GetMaxFloor()
        {
            return maxFloor;
        }

        /// <summary>
        /// 엔들리스 타워 무료티켓 충전까지 남은시간
        /// </summary>
        public RemainTime GetFreeEntryCoolTime()
        {
            return dungeonModel.EndlessTowerFreeTicketCoolTime;
        }

        /// <summary>
        /// 스킵 아이템 개수
        /// </summary>
        private int GetSkipItemCount()
        {
            return inventoryModel.GetItemCount(skipItemId);
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartBattle(int skipItemCount)
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            if (battleManager.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                return;
            }

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

            System.Action actionStartBattle = () =>
            {
                if (GetSkipItemCount() < skipItemCount)
                {
                    UI.ShowConfirmItemPopup(RewardType.Item, skipItemId, skipItemCount, LocalizeKey._90262); // 아이템이 부족합니다.
                    return;
                }

                StartEnelessTower(skipItemCount).WrapUIErrors();
            };

            if (skipItemCount > 0)
            {
                int floor = BasisType.ENDLESS_TOWER_SKIP_INFO.GetInt(skipItemCount);
                if (floor > GetClearedFloor())
                {
                    UI.ShowToastPopup(LocalizeKey._39507.ToText()); // 클리어하지 않은 층은 도전할 수 없습니다.
                    return;
                }

                string message = LocalizeKey._39508.ToText() // 추가 입장 재료가 필요합니다.\n[c][62AEE4]{INDEX}[-][/c]층에 바로 도전하시겠습니까?
                    .Replace(ReplaceKey.INDEX, floor);
                string arrowText = LocalizeKey._1.ToText(); // 확인
                UI.Show<UISelectMaterialPopup>().Set(new RewardData(RewardType.Item, skipItemId, 1), GetSkipItemCount(), skipItemCount, message, arrowText, true, actionStartBattle);
                return;
            }

            actionStartBattle();
        }

        private async Task StartEnelessTower(int skipItemCount)
        {
            // 동료 장착이 가능한 경우
            if (CanEquipAgent())
            {
                // 장착 가능한 PVP 동료가 있습니다.\n\n현재 상태로 진행하시겠습니까?
                // [동료 장착 바로가기]
                if (!await UI.SelectShortCutPopup(LocalizeKey._90173.ToText(), LocalizeKey._90174.ToText(), ShowCombatAgentUI))
                    return;
            }

            isStartBattle = true;
            battleManager.StartBattle(BattleMode.EndlessTower, skipItemCount);
        }

        private void ShowCombatAgentUI()
        {
            UI.Show<UIAgent>(new UIAgent.Input() { viewAgentType = AgentType.CombatAgent });
        }

        private class EndlessTowerSkipFloor : EndlessTowerFloorSelectView.IInput
        {
            public readonly int floor;
            public readonly RewardData skipItem;

            public EndlessTowerSkipFloor(int floor, RewardData skipItem)
            {
                this.floor = floor;
                this.skipItem = skipItem;
            }

            public int GetFloor()
            {
                return floor;
            }

            public RewardData GetSkipItem()
            {
                return skipItem;
            }
        }
    }
}