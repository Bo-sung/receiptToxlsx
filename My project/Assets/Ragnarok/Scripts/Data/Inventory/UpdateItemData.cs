namespace Ragnarok
{
    public class UpdateItemData : IPacket<Response>
    {
        public DirtyType dirtyType;
        public CharacterItemData characterItemData;

        void IInitializable<Response>.Initialize(Response response)
        {
            dirtyType = response.GetByte("1").ToEnum<DirtyType>();
            characterItemData = response.GetPacket<CharacterItemData>("2");
        }
    }
}