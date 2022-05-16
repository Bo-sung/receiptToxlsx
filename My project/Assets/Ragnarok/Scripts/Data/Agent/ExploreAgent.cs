using System.Collections.Generic;

namespace Ragnarok
{
    public class ExploreAgent : IAgent
    {
        public static int NewAgentCount { get; private set; } = 0;

        public AgentType AgentType => AgentType.ExploreAgent;

        public int CID { get; private set; }
        public int ID { get; private set; }
        public int DuplicationCount { get; private set; }
        public AgentData AgentData { get; private set; }
        public bool IsNew
        {
            get { return isNew; }
            set
            {
                if (isNew == value)
                    return;

                isNew = value;
                if (isNew)
                    ++NewAgentCount;
                else
                    --NewAgentCount;
            }
        }

        public ExploreType ExploreTypeBit { get; private set; }
        public IEnumerable<ExploreType> GetExploreTypes()
        {
            return AgentData.GetExploreTypes();
        }

        public bool IsDoingExplore(ExploreType exploreType)
        {
            if (IsExploring == false)
                return false;

            return ProgressingExplore.Type == exploreType;
        }

        public bool IsExploring { get { return ProgressingExplore != null; } }
        public AgentExploreState ProgressingExplore { get; private set; }

        private bool isNew = false;

        ExploreType AgentModel.IAgentValue.ExploreType => ExploreType.None;
        long AgentModel.IAgentValue.ExploreRewardRemainTime => 0L;
        int AgentModel.IAgentValue.ExploreStageID => 0;
        bool AgentModel.IAgentValue.IsExploring => false;

        public ExploreAgent(AgentModel.IAgentValue packet, AgentData agentData)
        {
            InitializePacketDependantData(packet);
            AgentData = agentData;

            ExploreTypeBit = (ExploreType)(int)agentData.explore_bit_type;
        }

        public void UpdateData(AgentModel.IAgentValue packet)
        {
            InitializePacketDependantData(packet);
        }

        private void InitializePacketDependantData(AgentModel.IAgentValue packet)
        {
            CID = packet.CID;
            ID = packet.ID;
            DuplicationCount = packet.DuplicationCount;
        }

        public void SetProgressingExplore(AgentExploreState explore)
        {
            ProgressingExplore = explore;
        }

        public static void ResetNewCount()
        {
            NewAgentCount = 0;
        }
    }
}
