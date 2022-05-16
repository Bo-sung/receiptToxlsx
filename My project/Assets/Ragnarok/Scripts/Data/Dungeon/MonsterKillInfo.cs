namespace Ragnarok
{
    public class MonsterKillInfo : IPacket<Response>
    {
        public int monsterID;
        public int count;

        void IInitializable<Response>.Initialize(Response response)
        {
            monsterID = response.GetInt("1");
            count = response.GetInt("2");
        }
    }
}