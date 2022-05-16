namespace Ragnarok
{
    public class ExploreStatePresenter : ViewPresenter
    {
        private readonly AgentModel agentModel;
        private readonly GoodsModel goodsModel;

        private UIExploreStateView view;
        private AgentExploreState exploreState;

        public ExploreStatePresenter(UIExploreStateView view)
        {
            this.view = view;

            agentModel = Entity.player.Agent;
            goodsModel = Entity.player.Goods;
        }

        public override void AddEvent()
        {
            agentModel.OnAgentExploreReward += OnAgentExploreReward;
            agentModel.OnAgentExploreCancel += OnAgentExploreCancel;
        }

        public override void RemoveEvent()
        {
            agentModel.OnAgentExploreReward -= OnAgentExploreReward;
            agentModel.OnAgentExploreCancel -= OnAgentExploreCancel;
        }

        public void OnShow(StageData stageData, AgentExploreState exploreState)
        {
            this.exploreState = exploreState;
            view.SetData(stageData, exploreState);
        }

        public void ViewEventHandler(UIExploreStateView.EventType eventType)
        {
            if (eventType == UIExploreStateView.EventType.OnClickComplete)
            {
                if (exploreState.RemainTime == 0)
                {
                    UI.ShowToastPopup(LocalizeKey._47413.ToText());
                    UI.Close<UIExplore>();
                    return;
                }

                if (goodsModel.CatCoin < GetCatcoinFastClear(exploreState.RemainTime))
                {
                    UI.ShowToastPopup(LocalizeKey._47824.ToText()); // 냥다래가 부족합니다.
                    return;
                }

                RequestComplete();
            }
            else if (eventType == UIExploreStateView.EventType.OnClickRetire)
            {
                if (exploreState.RemainTime == 0)
                {
                    UI.ShowToastPopup(LocalizeKey._47413.ToText()); // 이미 파견이 완료되었습니다. 보상을 수령해주세요.
                    UI.Close<UIExplore>();
                    return;
                }

                RequestRetire();
            }
        }

        public int GetCatcoinFastClear(RemainTime remainTime)
        {
            return agentModel.GetCatcoinFastClear(remainTime);
        }

        /// <summary>
        /// 파견 즉시 완료
        /// </summary>
        private void RequestComplete()
        {
            agentModel.RequestAgentExploreReward(exploreState, true).WrapNetworkErrors();
        }

        /// <summary>
        /// 파견 즉시 완료 성공
        /// </summary>
        /// <param name="exploreState"></param>
        void OnAgentExploreReward(AgentExploreState exploreState)
        {
            UI.Close<UIExplore>();
        }

        /// <summary>
        /// 파견 포기
        /// </summary>
        private async void RequestRetire()
        {
            if (!await UI.SelectPopup(LocalizeKey._90178.ToText())) // 정말로 파견을 포기하시겠습니까?
                return;

            agentModel.RequestAgentExploreCancel(exploreState).WrapNetworkErrors();
        }

        /// <summary>
        /// 파견 포기 성공
        /// </summary>
        void OnAgentExploreCancel()
        {
            UI.Close<UIExplore>();
        }
    }
}