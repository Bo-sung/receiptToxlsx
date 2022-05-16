namespace Ragnarok
{
    public sealed class DarkTreePacket : IPacket<Response>
    {
        /// <summary>
        /// a. 0일때: 수확 준비 상태
        /// b. 0보다 클때: 어둠의 나무 수확 가능까지 남은 시간
        /// c. -1일때: 수확 가능 상태
        /// </summary>
        public long time;
        public int rewardId;
        public int point;

        void IInitializable<Response>.Initialize(Response response)
        {
            time = response.GetLong("1");
            rewardId = response.GetInt("2");
            point = response.GetInt("3");
        }
    }
}