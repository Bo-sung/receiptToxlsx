namespace Ragnarok
{
    public interface IAgent : AgentModel.IAgentValue
    {
        AgentData AgentData { get; }

        AgentType AgentType { get; }
        bool IsNew { get; set; }

        void UpdateData(AgentModel.IAgentValue packet);
    }
}