namespace Ragnarok
{
    public sealed class OverStatusPacket : IPacket<Response>
    {
        public int str, agi, vit, @int, dex, luk;

        public void Initialize(Response response)
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