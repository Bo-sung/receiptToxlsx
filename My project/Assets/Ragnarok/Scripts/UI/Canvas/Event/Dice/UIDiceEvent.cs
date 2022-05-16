using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDiceEvent : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] DiceMainView diceMainView;
        [SerializeField] DiceRollPlayView diceRollPlayView;
        [SerializeField] DiceEventPlayView diceEventPlayView;

        DiceEventPresenter presenter;
        UIManager uiManager;

        private RewardData[] rewards;

        protected override void OnInit()
        {
            presenter = new DiceEventPresenter();
            uiManager = UIManager.Instance;

            diceMainView.OnSelectExit += CloseUI;
            diceMainView.OnSelectRoll += presenter.RequestRoll;
            diceMainView.OnSelectCost += ShowCostPopup;
            diceMainView.OnPassByHome += RefreshCompleteInfo;
            diceMainView.OnFinishedTweenMove += ShowResult;
            diceMainView.OnSelectCompleteReceive += presenter.RequestDiceReward;
            diceRollPlayView.OnFinished += diceMainView.Play;
            diceEventPlayView.OnHide += OnFinishedEventPlay;

            presenter.OnUpdateCostItemCount += RefreshCostItemCount;
            presenter.OnUpdateDiceRollEvent += OnPlayDiceRoll;
            presenter.OnUpdateDiceCompleteRewardEvent += RefreshCompleteInfo;

            presenter.AddEvent();

            uiManager.OnUIClose += OnCloseUI;

            Initialize();
        }

        protected override void OnClose()
        {
            uiManager.OnUIClose -= OnCloseUI;

            presenter.RemoveEvent();

            diceMainView.OnSelectExit -= CloseUI;
            diceMainView.OnSelectRoll -= presenter.RequestRoll;
            diceMainView.OnSelectCost -= ShowCostPopup;
            diceMainView.OnPassByHome -= RefreshCompleteInfo;
            diceMainView.OnFinishedTweenMove -= ShowResult;
            diceMainView.OnSelectCompleteReceive -= presenter.RequestDiceReward;
            diceRollPlayView.OnFinished -= diceMainView.Play;
            diceEventPlayView.OnHide -= OnFinishedEventPlay;

            presenter.OnUpdateCostItemCount -= RefreshCostItemCount;
            presenter.OnUpdateDiceRollEvent -= OnPlayDiceRoll;
            presenter.OnUpdateDiceCompleteRewardEvent -= RefreshCompleteInfo;
        }

        protected override void OnShow(IUIData data = null)
        {
            diceRollPlayView.Hide();
            diceEventPlayView.Hide();

            RefreshTiles();
            RefreshCostItemCount();
            RefreshCompleteInfo();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnCloseUI(ICanvas canvas)
        {
            if (canvas is UISingleReward)
            {
                rewards = null; // 보상 정보 초기화
                ShowResult();
            }
        }

        void ShowCostPopup()
        {
            UI.Show<UIPartsInfo>(presenter.costItemInfo);
        }

        void OnFinishedEventPlay(DiceEventType type, int value)
        {
            switch (type)
            {
                case DiceEventType.Move:
                    diceMainView.SetPlusStep(value);
                    diceMainView.Play(); // 주사위 연출
                    break;

                case DiceEventType.ChangeBoardTile:
                    diceMainView.ShowChangeEffect(); // 보드 변경 이벤트 보여주기
                    ShowResult();
                    break;

                case DiceEventType.GainEventCoin:
                    ShowResult();
                    break;

                case DiceEventType.MoveToHome:
                    diceMainView.SetHomeStep(isForward: true);
                    diceMainView.Play(); // 주사위 연출
                    break;

                case DiceEventType.ReturnToHome:
                    diceMainView.SetHomeStep(isForward: false);
                    diceMainView.Play(); // 주사위 연출
                    break;
            }
        }

        void OnPlayDiceRoll(byte diceNum1, byte diceNum2, bool isDouble, int eventId, RewardData[] rewards)
        {
            this.rewards = rewards;

            diceRollPlayView.Play(diceNum1, diceNum2, isDouble); // 주사위 연출
            diceMainView.SetPlusStep(diceNum1 + diceNum2); // 주사위 위치 세팅
            diceEventPlayView.SetEvent(presenter.GetDiceEventData(eventId)); // 이벤트 아이디 세팅
        }

        private void Initialize()
        {
            diceMainView.Initialize(presenter.needCostCount, presenter.eventCoinMaxCount, presenter.costItemInfo.IconName, presenter.GetCompleteRewards());
            diceEventPlayView.Initialize(presenter.GetEventImageNames());
        }

        private void RefreshTiles()
        {
            UIMonopolyTile.IInput[] data = presenter.GetData();
            if (data == null)
            {
                CloseUI();
                return;
            }

            diceMainView.SetTileData(data);
            diceMainView.SetDoubleState(presenter.IsDiceDoubleState());
            diceMainView.SetPoint(presenter.GetDiceStep());
        }

        private void RefreshCostItemCount()
        {
            diceMainView.SetItemCostCount(presenter.GetCostItemCount());
        }

        /// <summary>
        /// 완주 정보 최신화
        /// </summary>
        private void RefreshCompleteInfo()
        {
            diceMainView.SetCompleteInfo(presenter.GetCompleteCount(), presenter.GetCompleteRewardStep());
        }

        /// <summary>
        /// 결과 창 보여주기
        /// </summary>
        private void ShowResult()
        {
            if (diceEventPlayView.Play())
                return;

            if (rewards != null && rewards.Length > 0)
            {
                RewardData reward = rewards[0];
                var input = new UISingleReward.Input(UISingleReward.Mode.DICE_REWARD, reward, reward.IconName);
                UI.Show<UISingleReward>(input);
                return;
            }

            RefreshTiles();
            RefreshCompleteInfo();
        }

        private void CloseUI()
        {
            UI.Close<UIDiceEvent>();
        }

        protected override void OnBack()
        {
            if (diceRollPlayView.IsShow)
                return;

            if (diceEventPlayView.IsShow)
            {
                diceEventPlayView.TryHide();
                return;
            }

            // 연출 중
            if (diceMainView.IsPlay)
                return;

            base.OnBack();
        }
    }
}