namespace Ragnarok
{
    public class MazeCupetStatPacket : IPacket<Response>
    {
        public int cupet_id;
        public int hp;

        public void Initialize(Response response)
        {
            cupet_id = response.GetInt("1");
            hp = response.GetInt("2");
        }
    }
}