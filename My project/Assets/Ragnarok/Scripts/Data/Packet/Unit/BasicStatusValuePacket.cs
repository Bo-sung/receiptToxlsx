namespace Ragnarok
{
    public class BasicStatusValuePacket : IPacket<Response>
    {
        public int str;
        public int agi;
        public int vit;
        public int @int;
        public int dex;
        public int luk;

        void IInitializable<Response>.Initialize(Response response)
        {
            str = response.GetInt("1");
            agi = response.GetInt("2");
            vit = response.GetInt("3");
            @int = response.GetInt("4");
            dex = response.GetInt("5");
            luk = response.GetInt("6");
        }
    }
}