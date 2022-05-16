namespace Ragnarok
{
    /// <summary>
    /// 이름은 '버프'지만
    /// 실제로는 사용중인 모든 아이템 정보이다.
    /// </summary>
    public class UpdateBuffPacket : IPacket<Response>
    {
        public DirtyType dirtyType;
        public BuffPacket buffPacket;

        void IInitializable<Response>.Initialize(Response response)
        {
            dirtyType = response.GetByte("1").ToEnum<DirtyType>();
            buffPacket = response.GetPacket<BuffPacket>("2");
        }
    }
}