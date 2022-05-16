namespace Ragnarok
{
    public class CentralLabCharacterPacket : IPacket<Response>
    {
        public short str;
        public short agi;
        public short vit;
        public short @int;
        public short dex;
        public short luk;
        public int weaponId;

        void IInitializable<Response>.Initialize(Response response)
        {
            str = response.GetShort("1");
            agi = response.GetShort("2");
            vit = response.GetShort("3");
            @int = response.GetShort("4");
            dex = response.GetShort("5");
            luk = response.GetShort("6");
            weaponId = response.GetInt("7");
        }
    }
}