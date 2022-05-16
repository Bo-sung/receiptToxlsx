namespace Ragnarok
{
    public sealed class StageChallengeModePacket : IPacket<Response>
    {
        public int stageId;
        public int level;
        public int clearCount;

        void IInitializable<Response>.Initialize(Response response)
        {
            stageId = response.GetInt("1");
            level = response.GetInt("2");
            clearCount = response.GetInt("3");
    }
    }
}