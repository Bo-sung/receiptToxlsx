using System.Collections.Generic;

namespace Ragnarok
{
    public class AgentBookPresenter : ViewPresenter
    {
        private UIAgent mainView;
        private UIAgentBook view;
        private AgentModel agentModel;
        private AgentType viewAgentType;

        public AgentBookPresenter(UIAgent mainView, UIAgentBook view)
        {
            this.mainView = mainView;
            this.view = view;
            agentModel = Entity.player.Agent;
        }

        public override void AddEvent()
        {
            agentModel.OnAgentBookEnable += OnAgentBookEnable;
        }

        public override void RemoveEvent()
        {
            agentModel.OnAgentBookEnable -= OnAgentBookEnable;
        }

        List<IAgent> agentList = new List<IAgent>();
        List<AgentBookSlotViewInfo> viewInfos = new List<AgentBookSlotViewInfo>();
        public void OnShow(AgentType viewAgentType)
        {
            this.viewAgentType = viewAgentType;

            agentList.Clear();
            foreach (var each in agentModel.GetAllAgents())
                agentList.Add(each);

            viewInfos.Clear();

            foreach (var each in agentModel.GetBookStates())
            {
                if (each.AgentType != viewAgentType)
                    continue;

                viewInfos.Add(new AgentBookSlotViewInfo()
                {
                    bookData = each.BookData,
                    bookState = each
                });
            }

            view.ShowBookDatas(viewInfos.ToArray());
        }

        /// <summary>
        /// 도감 보상 받기 요청
        /// </summary>
        /// <param name="bookID"></param>
        public void RequestReceiveBookReward(int bookID)
        {
            agentModel.RequestAgentBookEnable(bookID).WrapNetworkErrors();
        }      

        /// <summary>
        /// 도감 성공 이벤트
        /// </summary>
        void OnAgentBookEnable(int bookID)
        {
            view.UpdateSlot(bookID);
            mainView.UpdateNotice();
        }
    }
}