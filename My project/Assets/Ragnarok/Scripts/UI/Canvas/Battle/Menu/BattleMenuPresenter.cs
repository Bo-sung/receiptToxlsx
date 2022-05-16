using System.Linq;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIBattleMenu"/>
    /// </summary>
    public class BattleMenuPresenter : ViewPresenter
    {
        private readonly int maxDuelPoint; // 듀얼 포인트 최대치

        private readonly DuelModel duelModel;
        private readonly AlarmModel alarmModel;
        private readonly QuestModel questModel;
        private readonly CharacterModel characterModel;
        private readonly BuffItemListModel buffItemListModel;
        private readonly EventBuffModel eventBuffModel;
        private readonly InventoryModel invenModel;
        private readonly EventModel eventModel;
        private readonly AgentModel agentModel;
        private readonly ShopModel shopModel;
        private readonly GuildModel guildModel;
        private readonly BattleManager battleManager;
        private readonly ConnectionManager connectionManager;
        private readonly int needNormalRouletteCoinCount;
        private readonly int needRareRouletteCoinCount;
        private bool isStartBattle;

        public event System.Action OnUpdateDuelPoint;
        public event System.Action OnUpdateAlarm;
        public event System.Action OnUpdateBuff
        {
            add { buffItemListModel.OnUpdateBuff += value; eventBuffModel.OnUpdateEventBuff += value; }
            remove { buffItemListModel.OnUpdateBuff -= value; eventBuffModel.OnUpdateEventBuff -= value; }
        }

        public event System.Action OnUpdateGuideQuest
        {
            add { questModel.OnUpdateMainQuest += value; }
            remove { questModel.OnUpdateMainQuest -= value; }
        }

        public event System.Action OnUpdateNewOpenContent
        {
            add { questModel.OnUpdateNewOpenContent += value; }
            remove { questModel.OnUpdateNewOpenContent -= value; }
        }

        public event InventoryModel.ItemUpdateEvent OnUpdateItem
        {
            add { invenModel.OnUpdateItem += value; }
            remove { invenModel.OnUpdateItem -= value; }
        }

        public event System.Action OnUpdatePackageJobLevel
        {
            add { shopModel.OnUpdatePackageJobLevel += value; }
            remove { shopModel.OnUpdatePackageJobLevel -= value; }
        }

        public event System.Action OnUpdateShopMail
        {
            add { shopModel.OnUpdateShopMail += value; }
            remove { shopModel.OnUpdateShopMail -= value; }
        }

        public event System.Action OnUpdateKafra
        {
            add { questModel.OnUpdateKafra += value; }
            remove { questModel.OnUpdateKafra -= value; }
        }

        public event System.Action OnUpdateNabiho
        {
            add { invenModel.OnUpdateNabiho += value; }
            remove { invenModel.OnUpdateNabiho += value; }
        }

        public event System.Action OnUpdateExploreState;
        private bool lastExploreFinishNoticeVal = false;

        public BattleMenuPresenter()
        {
            duelModel = Entity.player.Duel;
            alarmModel = Entity.player.AlarmModel;
            questModel = Entity.player.Quest;
            buffItemListModel = Entity.player.BuffItemList;
            eventBuffModel = Entity.player.EventBuff;
            characterModel = Entity.player.Character;
            invenModel = Entity.player.Inventory;
            eventModel = Entity.player.Event;
            agentModel = Entity.player.Agent;
            shopModel = Entity.player.ShopModel;
            guildModel = Entity.player.Guild;
            battleManager = BattleManager.Instance;
            connectionManager = ConnectionManager.Instance;

            maxDuelPoint = BasisType.DUEL_POINT_DROP_MAX.GetInt();
            needNormalRouletteCoinCount = BasisType.NORMAL_ROULETTE_PIECE.GetInt();
            needRareRouletteCoinCount = BasisType.RARE_ROULETTE_PIECE.GetInt();
        }

        public override void AddEvent()
        {
            duelModel.OnDualPointChanged += InvokeUpdateDuelPoint;
            alarmModel.OnAlarm += OnAlarm;
            agentModel.OnExploreStateChanged += OnExploreStateChanged;
            BattleManager.OnStart += OnStartBattle;
            BattleManager.OnEnterFail += OnEnterFail;
        }

        public override void RemoveEvent()
        {
            duelModel.OnDualPointChanged -= InvokeUpdateDuelPoint;
            alarmModel.OnAlarm -= OnAlarm;
            agentModel.OnExploreStateChanged -= OnExploreStateChanged;
            BattleManager.OnStart -= OnStartBattle;
            BattleManager.OnEnterFail -= OnEnterFail;
        }

        private void OnStartBattle(BattleMode mode)
        {
            isStartBattle = false;
        }

        private void OnEnterFail(BattleMode mode)
        {
            if (mode == BattleMode.Lobby)
                isStartBattle = false;
        }

        private void InvokeUpdateDuelPoint(int duelPoint)
        {
            OnUpdateDuelPoint?.Invoke();
        }

        public int GetCurDuelPoint()
        {
            return duelModel.DuelPoint;
        }

        public int GetMaxDuelPoint()
        {
            return maxDuelPoint;
        }

        public bool CanDuel()
        {
            return duelModel.CanDuel();
        }

        /// <summary>
        /// 현재 적용 중인 버프 유무
        /// </summary>
        public bool IsUseBuff()
        {
            return buffItemListModel.GetBuffItemInfos().Length > 0;
        }

        /// <summary>
        /// 컨텐츠 오픈 여부
        /// </summary>
        public bool IsOpenContent(UIBattleMenu.MenuContent content, bool isShowPopup)
        {
            switch (content)
            {
                case UIBattleMenu.MenuContent.Exit:
                    return true;

                case UIBattleMenu.MenuContent.Trade:
                    return true;

                case UIBattleMenu.MenuContent.Maze:
                    return questModel.IsOpenContent(ContentType.Maze, isShowPopup);

                case UIBattleMenu.MenuContent.Duel:
                    return true;

                case UIBattleMenu.MenuContent.Explore:
                    return questModel.IsOpenContent(ContentType.Explore, isShowPopup);

                case UIBattleMenu.MenuContent.Buff:
                    return questModel.IsOpenContent(ContentType.Buff, isShowPopup);

                case UIBattleMenu.MenuContent.Boss:
                    return questModel.IsOpenContent(ContentType.Boss, isShowPopup);

                case UIBattleMenu.MenuContent.Roulette:
                    return IsValidNormalRoulette();

                case UIBattleMenu.MenuContent.JobLevel:
                    return IsValidJobLevel();

                case UIBattleMenu.MenuContent.FirstPayment:
                    return !shopModel.HasPaymentHistory();

                case UIBattleMenu.MenuContent.Cupet:
                    return true;

                case UIBattleMenu.MenuContent.GuildAgit:
                    return true;

                case UIBattleMenu.MenuContent.CustomerReward:
                    return shopModel.HasPaymentHistory();

                case UIBattleMenu.MenuContent.Square:
                    return questModel.IsOpenContent(ContentType.TradeTown, isShowPopup);

                case UIBattleMenu.MenuContent.NpcMove:
                    return true;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattleMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 알림 표시 여부
        /// </summary>
        public bool GetHasNotice(UIBattleMenu.MenuContent content)
        {
            switch (content)
            {
                case UIBattleMenu.MenuContent.Exit:
                    return false;

                case UIBattleMenu.MenuContent.Trade:
                    return false;

                case UIBattleMenu.MenuContent.Maze:
                    return false;

                case UIBattleMenu.MenuContent.Duel:
                    return alarmModel.HasAlarm(AlarmType.Duel);

                case UIBattleMenu.MenuContent.Explore:
                    if (agentModel.FastestExploreState == null || agentModel.FastestExploreState.RemainTime > 0)
                    {
                        lastExploreFinishNoticeVal = false;
                        return false;
                    }
                    else
                    {
                        if (!lastExploreFinishNoticeVal)
                        {
                            var first = agentModel.FastestExploreState.Participants.First();
                            int count = agentModel.FastestExploreState.ParticipantCount;

                            if (count == 1)
                                UI.ShowToastPopup(LocalizeKey._2108.ToText().Replace(ReplaceKey.NAME, first.AgentData.name_id.ToText())); // {NAME}의 탐험이 완료되었습니다. 보상을 수령해주세요.
                            else
                                UI.ShowToastPopup(LocalizeKey._2109.ToText().Replace(ReplaceKey.NAME, first.AgentData.name_id.ToText()).Replace(ReplaceKey.COUNT, count)); // {NAME} 외 {COUNT}명의 탐험이 완료되었습니다. 보상을 수령해주세요.
                        }

                        lastExploreFinishNoticeVal = true;
                        return true;
                    }
                case UIBattleMenu.MenuContent.Buff:
                    return false;

                case UIBattleMenu.MenuContent.Boss:
                    return false;

                case UIBattleMenu.MenuContent.Roulette:
                    return (needNormalRouletteCoinCount <= GetNormalRouletteItemCount()
                        || needRareRouletteCoinCount <= GetRareRouletteItemCount());

                case UIBattleMenu.MenuContent.JobLevel:
                    return false;

                case UIBattleMenu.MenuContent.FirstPayment:
                    return false;

                case UIBattleMenu.MenuContent.Cupet:
                    return false;

                case UIBattleMenu.MenuContent.GuildAgit:
                    return false;

                case UIBattleMenu.MenuContent.CustomerReward:
                    return false;

                case UIBattleMenu.MenuContent.Square:
                    return IsNoticeSquare();

                case UIBattleMenu.MenuContent.NpcMove:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattleMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        /// <summary>
        /// 신규 컨텐츠 여부
        /// </summary>
        public bool GetHasNewIcon(UIBattleMenu.MenuContent content)
        {
            switch (content)
            {
                case UIBattleMenu.MenuContent.Exit:
                    return false;

                case UIBattleMenu.MenuContent.Trade:
                    return false;

                case UIBattleMenu.MenuContent.Maze:
                    return questModel.HasNewOpenContent(ContentType.Maze);

                case UIBattleMenu.MenuContent.Duel:
                    return questModel.HasNewOpenContent(ContentType.Duel);

                case UIBattleMenu.MenuContent.Explore:
                    return questModel.HasNewOpenContent(ContentType.Explore);

                case UIBattleMenu.MenuContent.Buff:
                    return questModel.HasNewOpenContent(ContentType.Buff);

                case UIBattleMenu.MenuContent.Boss:
                    return questModel.HasNewOpenContent(ContentType.Boss);

                case UIBattleMenu.MenuContent.Roulette:
                    return false;

                case UIBattleMenu.MenuContent.JobLevel:
                    return false;

                case UIBattleMenu.MenuContent.FirstPayment:
                    return false;

                case UIBattleMenu.MenuContent.Cupet:
                    return false;

                case UIBattleMenu.MenuContent.GuildAgit:
                    return false;

                case UIBattleMenu.MenuContent.CustomerReward:
                    return false;

                case UIBattleMenu.MenuContent.Square:
                    return questModel.HasNewOpenContent(ContentType.TradeTown);

                case UIBattleMenu.MenuContent.NpcMove:
                    return false;

                default:
                    throw new System.ArgumentException($"[올바르지 않은 {nameof(UIBattleMenu.MenuContent)}] {nameof(content)} = {content}");
            }
        }

        private void OnAlarm(AlarmType alarmType)
        {
            OnUpdateAlarm?.Invoke();
        }

        public void RemoveAlarm(AlarmType alarmType)
        {
            alarmModel.RemoveAlarm(alarmType);
        }

        private bool IsValidNormalRoulette()
        {
            if (eventModel.Roulette == null)
                return false;

            if (eventModel.Roulette.NormalCostItem == null)
                return false;

            return true;
        }

        public int GetNormalRouletteItemCount()
        {
            if (!IsValidNormalRoulette())
                return 0;

            return invenModel.GetItemCount(eventModel.Roulette.NormalCostItem.id);
        }

        private bool IsValidRareRoulette()
        {
            if (eventModel.Roulette == null)
                return false;

            if (!eventModel.Roulette.IsActiveRareGacha)
                return false;

            if (eventModel.Roulette.RareCostItem == null)
                return false;

            return true;
        }

        public int GetRareRouletteItemCount()
        {
            if (!IsValidRareRoulette())
                return 0;

            return invenModel.GetItemCount(eventModel.Roulette.RareCostItem.id);
        }

        private void OnExploreStateChanged(int arg1, AgentExploreState arg2)
        {
            OnUpdateExploreState?.Invoke();
        }

        private bool IsValidJobLevel()
        {
            return shopModel.JobLevelPackageShopId > 0;
        }

        /// <summary>
        /// 직업 레벨 패키지 구매 가능까지 남은시간
        /// </summary>
        /// <returns></returns>
        public float GetJobLevelPackageRemainTime()
        {
            if (!IsValidJobLevel())
                return 0;

            return shopModel.JobLevelPackageRemainTime.ToRemainTime();
        }

        public bool GetIsNeedShowJobLevelPopup()
        {
            return shopModel.GetIsNeedShowJobLevelPopup();
        }

        public void SetIsNeedShowJobLevelPopup()
        {
            shopModel.SetIsNeedShowJobLevelPopup(false);
        }

        public void ShowGuildCupet()
        {
            UI.Show<UICupet>();
        }

        public void GoToLobby()
        {
            if (isStartBattle)
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
           
            battleManager.StartBattle(BattleMode.Lobby);
            isStartBattle = true;
        }

        private bool IsNoticeSquare()
        {
            if (questModel.KafraCompleteType != KafraCompleteType.None)
                return true;

            return IsOpenNabiho() && invenModel.IsNoticeNobiho();
        }

        private bool IsOpenNabiho()
        {
            return BasisType.NABIHO_OPEN_BY_SERVER.GetInt(connectionManager.GetSelectServerGroupId()) == 0;
        }
    }
}