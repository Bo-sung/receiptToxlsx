namespace Ragnarok
{
    public class CharacterStatData : IPacket<Response>
    {
        public short? str;
        public short? agi;
        public short? vit;
        public short? inte;
        public short? dex;
        public short? lux;

        void IInitializable<Response>.Initialize(Response response)
        {
            if (response.ContainsKey("1"))
                str = response.GetShort("1");

            if (response.ContainsKey("2"))
                agi = response.GetShort("2");

            if (response.ContainsKey("3"))
                vit = response.GetShort("3");

            if (response.ContainsKey("4"))
                inte = response.GetShort("4");

            if (response.ContainsKey("5"))
                dex = response.GetShort("5");

            if (response.ContainsKey("6"))
                lux = response.GetShort("6");
        }
    }
}