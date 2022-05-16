namespace Ragnarok
{
    public sealed class NabihoPacket : IPacket<Response>
    {
        public int NabihoId;
        public RemainTime RemainTime;
        public byte AdCount;
        public RemainTime AdRemainTime;

        void IInitializable<Response>.Initialize(Response response)
        {
            NabihoId = response.GetInt("1");
            RemainTime = response.GetLong("2");
            AdCount = response.GetByte("3");
            AdRemainTime = response.GetLong("4");
        }

        public void ResetAdCount()
        {
            AdCount = 0;
        }
    }
}