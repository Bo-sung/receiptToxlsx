namespace Ragnarok
{
    public class CharacterItemData : IPacket<Response>, InventoryModel.IInputItemValue
    {
        public long No { get; private set; }
        public int ItemId { get; private set; }
        public int ItemCount { get; private set; }
        public int TierPer { get; private set; }
        public int ItemLevel { get; private set; }
        public byte ItemPos { get; private set; }
        public long EquippedCardNo1 { get; private set; }
        public long EquippedCardNo2 { get; private set; }
        public long EquippedCardNo3 { get; private set; }
        public long EquippedCardNo4 { get; private set; }
        public bool IsLock { get; private set; }
        public int ItemTranscend { get; private set; }
        public int ItemChangedElement { get; private set; }
        public int ElementLevel { get; private set; }
        public byte TradeFlag { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            No = response.GetLong("1");
            ItemId = response.GetInt("3");
            ItemCount = response.GetInt("4");
            TierPer = response.GetInt("5");
            ItemLevel = response.GetByte("6");
            ItemPos = response.GetByte("7");
            EquippedCardNo1 = response.GetLong("8");
            EquippedCardNo2 = response.GetLong("9");
            EquippedCardNo3 = response.GetLong("10");
            EquippedCardNo4 = response.GetLong("11");
            IsLock = response.GetByte("12") > 0;
            ItemTranscend = response.GetInt("13");
            ItemChangedElement = response.ContainsKey("14") ? response.GetInt("14") : 0;
            ElementLevel = response.GetInt("15");
            TradeFlag = response.GetByte("16");
        }
    }
}