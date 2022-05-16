namespace Ragnarok
{
    public sealed class EventChallengePacket : IPacket<Response>
    {
        public StageEventModePacket[] stageEventModes;
        public StageChallengeModePacket[] stageChallengeModes;
        public EventStagePacket eventStagePacket;

        void IInitializable<Response>.Initialize(Response response)
        {
            stageEventModes = response.GetPacketArray<StageEventModePacket>("1");
            stageChallengeModes = response.GetPacketArray<StageChallengeModePacket>("2");

            if (response.ContainsKey("3"))
            {
                eventStagePacket = response.GetPacket<EventStagePacket>("3");
            }
            else
            {
                eventStagePacket = EventStagePacket.EMPTY;
            }
        }
    }
}