namespace Ragnarok
{
    public sealed class OnBuffPassSeasonPacket : IPacket<Response>, IPassSeasonPacket
    {
        public static readonly OnBuffPassSeasonPacket EMPTY = new OnBuffPassSeasonPacket();

        private long pass_end_time; // 패스 시즌 종료까지 남은 시간
        private int seasion_no; // 시즌 회차

        public long PassEndTime => pass_end_time;
        public int SeasonNo => seasion_no;

        void IInitializable<Response>.Initialize(Response response)
        {
            pass_end_time = response.GetLong("1");
            seasion_no = response.GetInt("2");
        }
    }
}