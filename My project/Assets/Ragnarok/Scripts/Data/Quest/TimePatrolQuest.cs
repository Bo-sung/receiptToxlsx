namespace Ragnarok
{
    public class TimePatrolQuest : IPacket<Response>
    {
        public int quest_id;
        public int progress;
        public bool receive;

        void IInitializable<Response>.Initialize(Response response)
        {
            quest_id = response.GetInt("1");
            progress = response.GetInt("2");
            receive = response.GetBool("3");
        }
    }
}