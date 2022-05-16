namespace Ragnarok
{
    /// <summary>
    /// 합체 코스튬 정보
    /// </summary>
    public sealed class TimeSuitPacket : IPacket<Response>
    {
        public int type; // 합체 코스튬 타입
        public int level; // 합체 코스튬 레벨

        void IInitializable<Response>.Initialize(Response response)
        {
            type = response.GetInt("1");
            level = response.GetInt("2");
        }
    }
}