using System.Collections.Generic;

namespace Ragnarok
{
    public class AgentBookState : AgentModel.IAgentBookValue
    {
        public bool IsRewarded { get; private set; }
        public AgentBookData BookData { get; private set; }
        public int RequireAgentCount => agents.Length;

        private readonly AgentData[] agents;
        private readonly bool[] owningStates;

        public AgentType AgentType { get { return agents[0].agent_type.ToEnum<AgentType>(); } }

        int AgentModel.IAgentBookValue.Id
        {
            get
            {
                if (BookData == null || !IsRewarded)
                    return 0;

                return BookData.id;
            }
        }

        public IEnumerable<(AgentData, bool)> GetAgentStates()
        {
            for (int i = 0; i < agents.Length; ++i)
                yield return (agents[i], owningStates[i]);
            yield break;
        }

        public AgentBookState(AgentBookData bookData, AgentData[] agents)
        {
            BookData = bookData;
            this.agents = agents;
            owningStates = new bool[agents.Length];
        }

        public void SetEnabled()
        {
            IsRewarded = true;
        }

        public bool IsRequireAgent(int agentID)
        {
            for (int i = 0; i < agents.Length; ++i)
                if (agents[i].id == agentID)
                    return true;
            return false;
        }

        public void AddOwningAgent(int agentID)
        {
            for (int i = 0; i < agents.Length; ++i)
                if (agents[i].id == agentID)
                    owningStates[i] = true;
        }

        public bool CanComplete()
        {
            if (IsRewarded)
                return false;

            for (int i = 0; i < owningStates.Length; ++i)
                if (!owningStates[i])
                    return false;

            return true;
        }
    }
}
