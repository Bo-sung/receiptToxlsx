namespace Ragnarok
{
    public sealed class ShopPacket : IPacket<Response>
    {
        /// <summary>
        /// 비밀상점 목록 및 구매 정보
        /// </summary>
        public CharSecretShop[] charSecretShops;

        /// <summary>
        /// 비밀상점 무료 변경까지 남은시간
        /// </summary>
        public long nextSecretShopTime;

        void IInitializable<Response>.Initialize(Response response)
        {
            if (response.ContainsKey("1"))
                charSecretShops = response.GetPacketArray<CharSecretShop>("1");

            if (response.ContainsKey("2"))
                nextSecretShopTime = response.GetLong("2");
        }
    }
}