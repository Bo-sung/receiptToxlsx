namespace Ragnarok
{
    public class DuelArenaInfoPacket : IPacket<Response>
    {
        public int Cid { get; private set; }
        public int ArenaPoint { get; private set; }
        public byte RewardStep { get; private set; }
        public int WinCount { get; private set; }
        public int LoseCount { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            Cid = response.GetInt("1");
            ArenaPoint = response.GetInt("2");
            RewardStep = response.GetByte("3");
            WinCount = response.GetInt("4");
            LoseCount = response.GetInt("5");
        }
    }
}
