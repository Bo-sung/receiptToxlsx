namespace Ragnarok
{
    public class MvpMonsterPacket : IPacket<Response>
    {
        public int mvpTableId;
        public long remainTime;
        public int randomKey;

        public void Initialize(Response response)
        {
            mvpTableId = response.GetInt("1");
            remainTime = response.GetLong("2");
            randomKey = response.GetInt("3");
        }
    }
}