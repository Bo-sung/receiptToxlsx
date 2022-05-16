using UnityEngine;

namespace Ragnarok
{
    public class UIAgentExploreReward : UICanvas
    {
        public class Input : IUIData
        {
            public readonly ExploreAgent[] agents;
            public readonly int stageID;
            public readonly ExploreType exploreType;
            public readonly RewardData rewardData;

            public Input(int stageID, ExploreType exploreType, ExploreAgent[] agents, RewardPacket[] rewardPackets)
            {
                this.stageID = stageID;
                this.exploreType = exploreType;
                this.agents = agents;

                var type = rewardPackets[0].rewardType;
                int count = 0;

                for (int i = 0; i < rewardPackets.Length; ++i)
                    if (type == rewardPackets[i].rewardType)
                        count += rewardPackets[i].rewardCount;

                rewardPackets[0].rewardCount = count;

                rewardData = new RewardData(rewardPackets[0].rewardType, rewardPackets[0].rewardValue, rewardPackets[0].rewardCount, rewardPackets[0].rewardOption); // 동료 파견 보상은 1개
            }
        }

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIGridHelper agentGrid;
        [SerializeField] GameObject[] agentSlotRoots;
        [SerializeField] UITextureHelper[] agentIcons;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper noticeLabel;
        [SerializeField] UIButtonHelper reExploreButton;
        [SerializeField] UIButtonHelper okButton;
        [SerializeField] UIButtonHelper closeButton;

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        private int stageID;
        private ExploreType exploreType;
        private ExploreAgent[] agents;
        private AgentModel agentModel;
        private ShopModel shopModel;
        private StageDataManager stageDataRepo;
        private IronSourceManager ironSourceManager;
        private bool isCompleteAd;

        protected override void OnInit()
        {
            agentModel = Entity.player.Agent;
            shopModel = Entity.player.ShopModel;
            stageDataRepo = StageDataManager.Instance;
            ironSourceManager = IronSourceManager.Instance;
            EventDelegate.Add(okButton.OnClick, OnClickOkButton);
            EventDelegate.Add(closeButton.OnClick, OnClickOkButton);
            EventDelegate.Add(reExploreButton.OnClick, OnClickReExploreButton);

            agentModel.OnExploreStart += OnExploreStart;
            agentModel.OnResetTradeCount += OnResetTradeCount;
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(okButton.OnClick, OnClickOkButton);
            EventDelegate.Remove(closeButton.OnClick, OnClickOkButton);
            EventDelegate.Remove(reExploreButton.OnClick, OnClickReExploreButton);

            agentModel.OnExploreStart -= OnExploreStart;
            agentModel.OnResetTradeCount -= OnResetTradeCount;
        }

        protected override void OnShow(IUIData data = null)
        {           
            var input = data as Input;
            stageID = input.stageID;
            exploreType = input.exploreType;
            agents = input.agents;

            rewardHelper.SetData(input.rewardData);

            for (int i = 0; i < agentSlotRoots.Length; ++i)
            {
                if (i < input.agents.Length)
                {
                    agentSlotRoots[i].SetActive(true);
                    agentIcons[i].Set(input.agents[i].AgentData.GetIconName(AgentIconType.RectIcon));
                }
                else
                {
                    agentSlotRoots[i].SetActive(false);
                }
            }

            noticeLabel.Text = LocalizeKey._47320.ToText().Replace(ReplaceKey.NAME, exploreType.ToExploreName()); // [9E9B9E][c]동료가[/c][-] [69B2E6][c]{NAME}[/c][-][9E9B9E][c]을 마치고 돌아왔습니다.[/c][-]
            agentGrid.Reposition();
        }

        protected override void OnLocalize()
        {
            titleLabel.Text = LocalizeKey._47333.ToText().Replace(ReplaceKey.NAME, exploreType.ToExploreName()); // 발굴 결과
            reExploreButton.LocalKey = LocalizeKey._47332; // 재파견
            okButton.LocalKey = LocalizeKey._47600; // 확인
        }

        protected override void OnHide()
        {
        }

        private void OnClickOkButton()
        {
            UI.Close<UIAgentExploreReward>();
        }

        private void OnClickReExploreButton()
        {
            if ((exploreType == ExploreType.Trade || exploreType == ExploreType.Production) 
                && agentModel.GetTradeProductionRemainCount(stageID) == 0)
            {

                if (agentModel.IsTradeProuctionAd(stageID))
                {
                    ShowRewardedVideo();
                }
                else
                {
                    OpenCostPopup();
                }
            }
            else
            {
                RequestReExplore();
            }
        }

        /// <summary>
        /// 재파견 요청 (발굴 & 채집)
        /// </summary>
        private void RequestReExplore()
        {
            agentModel.RequestAgentExploreStart(agents, stageID, exploreType).WrapNetworkErrors();
        }

        /// <summary>
        /// 재파견 성공 (발굴 & 채집)
        /// </summary>
        void OnExploreStart(ExploreType exploreType)
        {
            StageData stageData = stageDataRepo.Get(stageID);

            if (stageData.agent_explore_type.ToEnum<ExploreType>() != exploreType)
                return;

            string a = LocalizeKey._47322.ToText(); // 으로
            string b = LocalizeKey._47323.ToText(); // 로
            string c = LocalizeKey._47324.ToText(); // 이
            string d = LocalizeKey._47325.ToText(); // 가
            string e = LocalizeKey._47326.ToText(); // 과
            string f = LocalizeKey._47327.ToText(); // 와

            string stageName = stageData.name_id.ToText();
            KLUtil.Divide(stageName[stageName.Length - 1], out char _, out char _, out char jong1);

            string agentName = agents[0].AgentData.name_id.ToText();
            KLUtil.Divide(agentName[agentName.Length - 1], out char _, out char _, out char jong2);

            if (agents.Length == 1)
            {
                UI.ShowToastPopup(LocalizeKey._47328.ToText() // {NAME1}{POSTPOSITION1} {NAME2}{POSTPOSITION2} {NAME3}을 하러 떠납니다.
                    .Replace(ReplaceKey.NAME, 1, stageName)
                    .Replace(ReplaceKey.POSTPOSITION, 1, jong1 == ' ' ? b : a)
                    .Replace(ReplaceKey.NAME, 2, agentName)
                    .Replace(ReplaceKey.POSTPOSITION, 2, jong2 == ' ' ? d : c)
                    .Replace(ReplaceKey.NAME, 3, exploreType.ToExploreName()));
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._47329.ToText() // {NAME1}{POSTPOSITION1} {NAME2}{POSTPOSITION2} 동료 {COUNT}명이 {NAME3}을 하러 떠납니다.
                    .Replace(ReplaceKey.NAME, 1, stageName)
                    .Replace(ReplaceKey.POSTPOSITION, 1, jong1 == ' ' ? b : a)
                    .Replace(ReplaceKey.NAME, 2, agentName)
                    .Replace(ReplaceKey.POSTPOSITION, 2, jong2 == ' ' ? f : e)
                    .Replace(ReplaceKey.COUNT, agents.Length - 1)
                    .Replace(ReplaceKey.NAME, 3, exploreType.ToExploreName()));
            }

            UI.Close<UIAgentExploreReward>();
        }

        /// <summary>
        /// 광고 보고 파견 횟수 초기화
        /// </summary>
        private void ShowRewardedVideo()
        {
            bool isFree = shopModel.IsSkipTradeProductionAd();
            // 광고를 시청하시면 무료 1회 재파견이 가능합니다.\n광고를 시청하시겠습니까?\n(횟수는 매일 자정(GMT+8)에 초기화 됩니다.)
            ironSourceManager.ShowRewardedVideo(IronSourceManager.PlacementNameType.None, isFree, isBeginner: false, OnCompleteRewardVideo, descriptionId: 90276);
        }

        private void OnCompleteRewardVideo()
        {
            if (agentModel.GetTradeProductionRemainCount(stageID) != 0)
            {
                // 광고 보는 도중에 하루 초기화가 된 상태
                UI.ShowToastPopup(LocalizeKey._90281.ToText()); // 자정(GMT+8)이 지나, 파견 횟수가 초기화되었습니다.
                return;
            }
            isCompleteAd = true;
            agentModel.RequestAgentResetTradeCount(stageID).WrapNetworkErrors();
        }

        private async void OpenCostPopup()
        {
            var result = await UI.CostPopup(CoinType.CatCoin, BasisType.AGENT_TRADE_RESET_PRICE.GetInt(),
                LocalizeKey._47330.ToText(), // 초기화
                LocalizeKey._90278.ToText()); // [9E9B9E][c]해당 구역의 일일 파견 가능 횟수를[/c][-] [69B2E6][c]초기화[/c][-] [9E9B9E][c]하시겠습니까?[/c][-]\n[A9A9A9][c](횟수는 매일 자정(GMT+8)에 초기화 됩니다.)[/c][-]

            if (result)
            {
                agentModel.RequestAgentResetTradeCount(stageID).WrapNetworkErrors();
            }
        }

        /// <summary>
        /// 교역 횟수 초기화 성공
        /// </summary>
        void OnResetTradeCount()
        {
            if (isCompleteAd && shopModel.IsSkipTradeProductionAd())
            {
                UI.ShowToastPopup(LocalizeKey._90277.ToText()); // 결제 마일리지가 충족되어 광고 시청 없이 초기화 되었습니다.
            }
            else
            {
                UI.ShowToastPopup(LocalizeKey._90279.ToText()); // 파견 횟수가 초기화되었습니다.
            }
            isCompleteAd = false;
            UI.Close<UIAgentExploreReward>();
        }
    }
}