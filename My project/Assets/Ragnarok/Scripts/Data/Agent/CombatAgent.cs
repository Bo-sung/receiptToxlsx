namespace Ragnarok
{
    public class CombatAgent : IAgent
    {
        public static int NewAgentCount { get; private set; } = 0;

        public AgentType AgentType => AgentType.CombatAgent;

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

        public bool IsUsingAgent { get { return UsingSlot != -1; } }
        public long UsingSlot { get; private set; } = -1;

        private bool isNew = false;

        ExploreType AgentModel.IAgentValue.ExploreType => ExploreType.None;
        long AgentModel.IAgentValue.ExploreRewardRemainTime => 0L;
        int AgentModel.IAgentValue.ExploreStageID => 0;
        bool AgentModel.IAgentValue.IsExploring => false;

        public CombatAgent(AgentModel.IAgentValue packet, AgentData agentData)
        {
            CID = packet.CID;
            ID = packet.ID;
            DuplicationCount = packet.DuplicationCount;
            AgentData = agentData;
        }

        public void UpdateData(AgentModel.IAgentValue packet)
        {
            DuplicationCount = packet.DuplicationCount;
        }

        public void SetUsingSlot(long slotNumber)
        {
            UsingSlot = slotNumber;
        }

        public static void ResetNewCount()
        {
            NewAgentCount = 0;
        }
    }
}