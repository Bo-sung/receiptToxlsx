namespace Ragnarok
{
    public class CharFieldQuest : IPacket<Response>
    {
        public int cid;
        public int quest_id;
        public short quest_type;
        public int condition_value;
        public int progress;
        public bool receive;
        public byte field_level;

        void IInitializable<Response>.Initialize(Response response)
        {
            cid = response.GetInt("1");
            quest_id = response.GetInt("2");
            quest_type = response.GetShort("3");
            condition_value = response.GetInt("4");
            progress = response.GetInt("5");
            receive = response.GetBool("6");
            field_level = response.GetByte("7");
        }
    }
}