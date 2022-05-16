using UnityEngine;

namespace Ragnarok
{
    public class UIExplore : UICanvas
    {
        public class Input : IUIData
        {
            public StageData stageData;
        }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        private DigCollectExplorePresenter digCollectExplorePresenter;
        private TradeExplorePresenter tradeExplorePresenter;
        private ExploreStatePresenter exploreStatePresenter;

        [SerializeField] UIExploreDigCollectView digcollectExploreView;
        [SerializeField] UIExploreTradeView tradeExploreView;
        [SerializeField] UIExploreStateView exploreStateView;
        [SerializeField] UIButtonHelper closeButton;

        protected override void OnInit()
        {
            digCollectExplorePresenter = new DigCollectExplorePresenter(digcollectExploreView);
            tradeExplorePresenter = new TradeExplorePresenter(tradeExploreView);
            exploreStatePresenter = new ExploreStatePresenter(exploreStateView);
            digcollectExploreView.Init(digCollectExplorePresenter);
            tradeExploreView.Init(tradeExplorePresenter);
            exploreStateView.Init(exploreStatePresenter);

            digCollectExplorePresenter.AddEvent();
            tradeExplorePresenter.AddEvent();
            exploreStatePresenter.AddEvent();

            EventDelegate.Add(closeButton.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            digCollectExplorePresenter.RemoveEvent();
            tradeExplorePresenter.RemoveEvent();
            exploreStatePresenter.RemoveEvent();

            EventDelegate.Remove(closeButton.OnClick, CloseUI);
        }

        private void CloseUI()
        {
            UI.Close<UIExplore>();
        }

        protected override void OnLocalize()
        {
            digcollectExploreView.OnLocalize();
            tradeExploreView.OnLocalize();
            exploreStateView.OnLocalize();
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;
            StageData stageData = input.stageData;
            ExploreType exploreType = stageData.agent_explore_type.ToEnum<ExploreType>();

            var exploreState = Entity.player.Agent.GetExploreState(stageData.id);

            if (exploreState != null)
            {
                digcollectExploreView.gameObject.SetActive(false);
                tradeExploreView.gameObject.SetActive(false);
                exploreStateView.gameObject.SetActive(true);
                exploreStatePresenter.OnShow(stageData, exploreState);
            }
            else
            {
                if (exploreType == ExploreType.Dig || exploreType == ExploreType.Collect) // 발굴 & 채집
                {
                    digcollectExploreView.gameObject.SetActive(true);
                    tradeExploreView.gameObject.SetActive(false);
                    exploreStateView.gameObject.SetActive(false);
                    digCollectExplorePresenter.OnShow(stageData);
                }
                else if (exploreType == ExploreType.Trade || exploreType == ExploreType.Production) // 교역 & 생산
                {
                    digcollectExploreView.gameObject.SetActive(false);
                    tradeExploreView.gameObject.SetActive(true);
                    exploreStateView.gameObject.SetActive(false);
                    tradeExplorePresenter.OnShow(stageData);
                }
            }
        }

        protected override void OnHide()
        {
        }
    }
}