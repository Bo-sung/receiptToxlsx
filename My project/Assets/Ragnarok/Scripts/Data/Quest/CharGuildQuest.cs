namespace Ragnarok
{
    public class CharGuildQuest : IPacket<Response>
    {
        public short quest_type;
        public int condition_value;
        public int progress;
        public bool received;

        void IInitializable<Response>.Initialize(Response response)
        {
            quest_type = response.GetShort("1");
            condition_value = response.GetInt("2");
            progress = response.GetInt("3");
            received = response.GetBool("4");
        }
    }
}