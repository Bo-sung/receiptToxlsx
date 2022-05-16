using MEC;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISimpleQuest"/>
    /// </summary>
    public sealed class SimpleQuestPresenter : ViewPresenter
    {
        private readonly QuestModel questModel;
        private readonly DungeonModel dungeonModel;
        private readonly BattleManager battleManager;
        private readonly SoundManager soundManager;
        private readonly SimpleQuest simpleQuest;

        public event System.Action OnRefreshSimpleQuest;

        public event System.Action<bool> OnReqeustReward
        {
            add { questModel.OnReqeustReward += value; }
            remove { questModel.OnReqeustReward -= value; }
        }

        public SimpleQuestPresenter()
        {
            questModel = Entity.player.Quest;
            dungeonModel = Entity.player.Dungeon;
            battleManager = BattleManager.Instance;
            soundManager = SoundManager.Instance;
            simpleQuest = new SimpleQuest();
        }

        public override void AddEvent()
        {
            questModel.OnUpdateMainQuest += OnGuideQuest;
            questModel.OnUpdateKafra += OnUpdateKafra;
            BattleManager.OnReady += OnBattleReady;
            BattleManager.OnChangeBattleMode += OnChangeBattleMode;
        }

        public override void RemoveEvent()
        {
            questModel.OnUpdateMainQuest -= OnGuideQuest;
            questModel.OnUpdateKafra -= OnUpdateKafra;
            BattleManager.OnReady -= OnBattleReady;
            BattleManager.OnChangeBattleMode -= OnChangeBattleMode;
        }

        public void Initialize()
        {
            UpdateSimpleQuest();
        }

        void OnGuideQuest()
        {
            RefreshSimpleQuest();
        }

        void OnUpdateKafra()
        {
            RefreshSimpleQuest();
        }

        void OnBattleReady()
        {
            RefreshSimpleQuest();
        }

        void OnChangeBattleMode(BattleMode mode)
        {
            RefreshSimpleQuest();
        }

        public UISimpleQuest.ISimpleQuest GetSimpleQuest()
        {
            return simpleQuest;
        }

        public void OnClickedBtnQuest_UI()
        {
            if (simpleQuest.IsComplete())
            {
                RequestQuestReward();
                return;
            }

            if (simpleQuest.CurrentMode == UISimpleQuest.Mode.Kafra)
            {
                UI.ShowToastPopup(LocalizeKey._19525.ToText()); // 화살표를 따라 "소티"에게 이동하세요.
                return;
            }

            if (simpleQuest.IsInvalidData())
                return;

            UIQuest.view = UIQuest.ViewType.Main;
            UI.Show<UIQuest>();
        }

        /// <summary>
        /// 퀘스트 클릭 이벤트
        /// </summary>
        public void OnClickedBtnQuest()
        {
            if (simpleQuest.IsComplete())
            {
                RequestQuestReward();
                return;
            }

            if (simpleQuest.CurrentMode == UISimpleQuest.Mode.Kafra)
            {
                GoToNpcSortie();
                return;
            }

            if (simpleQuest.IsInvalidData())
                return;

            // 바로가기 타입이 없을 경우에는 메인 Quest UI를 띄운다.
            ShortCutType shortCutType = simpleQuest.GetShortcutType();
            if (shortCutType == ShortCutType.None)
            {
                UIQuest.view = UIQuest.ViewType.Main;
                UI.Show<UIQuest>();
                return;
            }

            if (shortCutType == ShortCutType.Stage)
            {
                if (UIBattleMatchReady.IsMatching)
                {
                    string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                    UI.ShowToastPopup(message);
                    return;
                }

                int stageId = simpleQuest.GetShortcutValue();
                if (!dungeonModel.IsStageOpend(stageId))
                {
                    stageId = dungeonModel.FinalStageId + 1;
                }

                // 이미 해당 스테이지 진행중이면 쇼트컷 X
                if (IsPlayingStage(stageId))
                    return;

                dungeonModel.StartBattleStageMode(StageMode.Normal, stageId);
                return;
            }

            simpleQuest.GoShortcut();
        }

        /// <summary>
        /// 보상 받기
        /// </summary>
        private void RequestQuestReward()
        {
            if (simpleQuest.CurrentMode == UISimpleQuest.Mode.Kafra)
            {
                RequestKafraDeliveryReward();
                return;
            }

            QuestInfo info = questModel.GetMaintQuest();
            if (info.IsInvalidData)
                info = questModel.GetDailyTotalClearQuest();

            questModel.RequestQuestRewardAsync(info).WrapNetworkErrors();
        }

        /// <summary>
        /// 스테이지 진행중 여부
        /// </summary>
        private bool IsPlayingStage(int stageId)
        {
            return dungeonModel.LastEnterStageId == stageId;
        }

        private void RefreshSimpleQuest()
        {
            bool isPreComplete = simpleQuest.IsComplete();
            UISimpleQuest.Mode preMode = simpleQuest.CurrentMode;

            UpdateSimpleQuest();

            // 이번에 퀘스트가 클리어 된 경우 SFX 발생
            if (!isPreComplete && simpleQuest.IsComplete())
            {
                soundManager.PlayUISfx(Sfx.UI.ChangeCard);
            }

            OnRefreshSimpleQuest?.Invoke();
        }

        private void UpdateSimpleQuest()
        {
            simpleQuest.SetIsLobby(battleManager.Mode == BattleMode.Lobby);
            simpleQuest.SetKafraQuest(questModel.CurKafraType, questModel.KafraCompleteType);
            simpleQuest.SetMainQuest(questModel.GetMaintQuest());
        }

        private void GoToNpcSortie()
        {
            if (battleManager.GetCurrentEntry() is LobbyEntry lobbyEntry)
            {
                UI.ShowToastPopup(LocalizeKey._19525.ToText()); // 화살표를 따라 "소티"에게 이동하세요.
                lobbyEntry.GoToNpc(NpcType.Sortie);
            }
        }

        private void RequestKafraDeliveryReward()
        {
            if (battleManager.GetCurrentEntry() is LobbyEntry lobbyEntry)
            {
                lobbyEntry.RequestKafraDeliveryReward();
            }
        }

        private class SimpleQuest : UISimpleQuest.ISimpleQuest
        {
            private QuestInfo curQuest;
            private bool isLobby;
            private KafraType kafraQuestType;
            private KafraCompleteType kafraCompleteType;

            public UISimpleQuest.Mode CurrentMode => (isLobby && kafraQuestType != KafraType.Exchange) ? UISimpleQuest.Mode.Kafra : UISimpleQuest.Mode.Main;

            public void SetIsLobby(bool isLobby)
            {
                this.isLobby = isLobby;
            }

            public void SetKafraQuest(KafraType questType, KafraCompleteType completeType)
            {
                kafraQuestType = questType;
                kafraCompleteType = completeType;
            }

            public void SetMainQuest(QuestInfo mainQuest)
            {
                curQuest = mainQuest;
            }

            public string GetTitle()
            {
                if (CurrentMode == UISimpleQuest.Mode.Kafra)
                {
                    switch (kafraQuestType)
                    {
                        case KafraType.RoPoint:
                            return LocalizeKey._19511.ToText(); // 귀금속 전달

                        default:
                        case KafraType.Zeny:
                            return LocalizeKey._19512.ToText(); // 긴급! 도움 요청
                    }
                }

                if (IsInvalidData())
                    return LocalizeKey._19508.ToText(); // 모든 가이드 퀘스트를 완료하셨습니다.

                return StringBuilderPool.Get()
                    .Append("(").Append(curQuest.GetMainQuestGroup()).Append(") ").Append(curQuest.Name)
                    .Release();
            }

            public string GetDesc()
            {
                if (CurrentMode == UISimpleQuest.Mode.Kafra)
                {
                    int currentValue = kafraCompleteType == KafraCompleteType.StandByReward ? 1 : 0;
                    int maxValue = 1;

                    switch (kafraQuestType)
                    {
                        case KafraType.RoPoint:
                            return StringBuilderPool.Get()
                                .Append(LocalizeKey._19513.ToText()).Append(" (").Append(currentValue).Append("/").Append(maxValue).Append(")") // 소티에게 귀금속 전달
                                .Release();

                        default:
                        case KafraType.Zeny:
                            return StringBuilderPool.Get()
                                .Append(LocalizeKey._19514.ToText()).Append(" (").Append(currentValue).Append("/").Append(maxValue).Append(")") // 소린의 부탁 들어주기
                                .Release();
                    }
                }

                if (IsInvalidData())
                    return LocalizeKey._19508.ToText(); // 모든 가이드 퀘스트를 완료하셨습니다.

                return StringBuilderPool.Get()
                    .Append(curQuest.ConditionText).Append(" (").Append(curQuest.CurrentValue).Append("/").Append(curQuest.MaxValue).Append(")")
                    .Release();
            }

            public bool IsFirst()
            {
                if (CurrentMode == UISimpleQuest.Mode.Kafra)
                    return false;

                if (IsInvalidData())
                    return false;

                return curQuest.Group == 1;
            }

            public bool IsComplete()
            {
                if (CurrentMode == UISimpleQuest.Mode.Kafra)
                    return kafraCompleteType == KafraCompleteType.StandByReward;

                if (IsInvalidData())
                    return false;

                return curQuest.CompleteType == QuestInfo.QuestCompleteType.StandByReward;
            }

            public bool IsInvalidData()
            {
                return curQuest == null || curQuest.IsInvalidData;
            }

            public ShortCutType GetShortcutType()
            {
                return curQuest.ShortCutType;
            }

            public int GetShortcutValue()
            {
                return curQuest.ShortCutValue;
            }

            public void GoShortcut()
            {
                curQuest.GoShortCut();
            }
        }
    }
}