namespace Ragnarok
{
    public class DuelServerPacket : IPacket<Response>, System.IComparable<DuelServerPacket>
    {
        public int duelCount; // 듀얼 조각 수

        public int id;
        public int nameId;
        public int alphabetIndex;
        public int orderNum;

        void IInitializable<Response>.Initialize(Response response)
        {
            duelCount = response.GetInt("1");
        }

        public void Initialize(int id, int nameId, int alphabetIndex, int myServerId)
        {
            this.id = id;
            this.nameId = nameId;
            this.alphabetIndex = alphabetIndex;

            orderNum = id - myServerId;
        }

        private bool IsMyServer()
        {
            return orderNum == 0;
        }

        private bool IsEmptyDuelCount()
        {
            return duelCount == 0;
        }

        private bool IsNegativeOrder()
        {
            return orderNum < 0;
        }

        int System.IComparable<DuelServerPacket>.CompareTo(DuelServerPacket other)
        {
            int result0 = other.IsMyServer().CompareTo(IsMyServer());
            int result1 = result0 == 0 ? (IsEmptyDuelCount().CompareTo(other.IsEmptyDuelCount())) : result0; // 듀얼 조각 수 존재
            int result2 = result1 == 0 ? (IsNegativeOrder().CompareTo(other.IsNegativeOrder())) : result1; // 음수 정렬 체크
            int result3 = result2 == 0 ? orderNum.CompareTo(other.orderNum) : result2; // 정렬 체크
            return result3;
        }
    }
}