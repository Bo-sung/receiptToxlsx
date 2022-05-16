namespace Ragnarok
{
    public class CharNormalProgress : IPacket<Response>
    {
        public int quest_id;
        public bool receive;
        public int progress;

        void IInitializable<Response>.Initialize(Response response)
        {
            quest_id = response.GetInt("1");
            receive = response.GetBool("2");
            progress = response.GetInt("3");
        }
    }
}