namespace Ragnarok
{
    public sealed class StageEventModePacket : IPacket<Response>
    {
        public int stageId;
        public int level;

        void IInitializable<Response>.Initialize(Response response)
        {
            stageId = response.GetInt("1");
            level = response.GetInt("2");
        }
    }
}