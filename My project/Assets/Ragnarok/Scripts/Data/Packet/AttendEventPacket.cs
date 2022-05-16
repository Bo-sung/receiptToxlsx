namespace Ragnarok
{
    public sealed class AttendEventPacket : IPacket<Response>
    {
        public static readonly AttendEventPacket EMPTY = new AttendEventPacket();

        public int day_count; // 출석 일수
        public int reward_step; // 보상 수령한 회차

        void IInitializable<Response>.Initialize(Response response)
        {
            day_count = response.GetInt("1");
            reward_step = response.GetInt("2");
        }
    }
}