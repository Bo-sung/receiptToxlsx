namespace Ragnarok
{
    /// <summary>
    /// 이름은 '버프'지만
    /// 실제로는 사용중인 모든 아이템 정보이다.
    /// </summary>
    public class BuffPacket : IPacket<Response>, BuffItemListModel.IInputBuffValue
    {
        public int Cid { get; private set; }
        public int ItemId { get; private set; }
        public long CoolDown { get; private set; }
        public long Duration { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            Cid = response.GetInt("1");
            ItemId = response.GetInt("2");
            CoolDown = response.GetLong("3");
            Duration = response.GetLong("4");
        }
    }
}