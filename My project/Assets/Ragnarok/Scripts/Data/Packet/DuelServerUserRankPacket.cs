namespace Ragnarok
{
    public class DuelServerUserRankPacket : IPacket<Response>
    {
        public int cid;
        public string name;
        public byte job;
        public byte gender;
        public short jobLevel;
        public int battleScore;
        public int winCount;

        void IInitializable<Response>.Initialize(Response response)
        {
            cid = response.GetInt("1");
            name = response.GetUtfString("2");
            job = response.GetByte("3");
            gender = response.GetByte("4");
            jobLevel = response.GetShort("5");
            battleScore = response.GetInt("6");
            winCount = response.GetInt("7");
        }
    }
}