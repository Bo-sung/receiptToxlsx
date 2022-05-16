namespace Ragnarok
{
    public class ItemGetNoticePacket : IPacket<Response>
    {
        public int cid;
        public string name;
        public int item_id;
        public byte goods_type; // RewardType

        public void Initialize(Response response)
        {
            cid = response.GetInt("1");
            name = response.GetUtfString("2");
            item_id = response.GetInt("3");
            goods_type = response.GetByte("4");
        }
    }
}