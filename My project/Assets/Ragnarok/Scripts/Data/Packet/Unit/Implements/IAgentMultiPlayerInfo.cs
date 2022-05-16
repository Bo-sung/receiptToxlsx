namespace Ragnarok
{
    public interface IAgentMultiPlayerInfo
    {
        AgentSlotInfoPacket[] AgentSlots { get; }
        int JobId { get; }
        int JobLevel { get; }
        int BaseLevel { get; }
    }
}