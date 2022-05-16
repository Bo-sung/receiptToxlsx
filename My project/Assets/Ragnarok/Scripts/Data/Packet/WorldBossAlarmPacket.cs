namespace Ragnarok
{
    public class WorldBossAlarmPacket : IPacket<Response>
    {
        public int worldBossId;
        public long remainTime;

        void IInitializable<Response>.Initialize(Response response)
        {
            worldBossId = response.GetInt("1");
            remainTime = response.GetLong("2");
        }
    }
}
