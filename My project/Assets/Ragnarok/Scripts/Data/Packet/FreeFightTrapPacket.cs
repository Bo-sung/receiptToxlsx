namespace Ragnarok
{
    public class FreeFightTrapPacket : IPacket<Response>, IBattleTrapInput
    {
        public int Id { get; private set; }
        public byte State { get; private set; }
        public short IndexX { get; private set; }
        public short IndexZ { get; private set; }

        public FreeFightTrapPacket()
        {
        }

        public FreeFightTrapPacket(int id, short indexX, short indexZ, byte state)
        {
            Id = id;
            IndexX = indexX;
            IndexZ = indexZ;
            State = state;
        }

        void IInitializable<Response>.Initialize(Response response)
        {
            State = response.GetByte("1");
            Id = response.GetInt("2");
            IndexX = response.GetShort("3");
            IndexZ = response.GetShort("4");
        }
    }
}