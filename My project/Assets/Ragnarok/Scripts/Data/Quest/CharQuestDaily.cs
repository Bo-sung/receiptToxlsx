namespace Ragnarok
{
    public class CharQuestDaily : IPacket<Response>
    {
        public int quest_id;
        public bool receive;

        void IInitializable<Response>.Initialize(Response response)
        {
            quest_id = response.GetInt("1");
            receive = response.GetBool("2");
        }
    }
}