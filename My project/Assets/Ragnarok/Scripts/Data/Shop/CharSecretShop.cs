namespace Ragnarok
{
    /// <summary>
    /// 비밀상점 목록 및 구매 정보
    /// </summary>
    public sealed class CharSecretShop : IPacket<Response>
    {
        public int gacha_data_id;
        public short buy_count;

        void IInitializable<Response>.Initialize(Response response)
        {
            gacha_data_id = response.GetInt("1");
            buy_count = response.GetShort("2");
        }
    }
}
