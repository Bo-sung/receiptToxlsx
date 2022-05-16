namespace Ragnarok
{
    public class GuildCupetPacket : IPacket<Response>, CupetListModel.IInputValue
    {
        private int guild_id;
        private int cupet_id;
        private int count;
        private int rank;
        public int exp;
        private int level;

        public int Cid => 0;
        public int CupetId => cupet_id;
        public int CupetRank => rank;
        public int CupetLevel => level;
        public int CupetExp => exp;
        public int Count => count;

        void IInitializable<Response>.Initialize(Response t)
        {
            guild_id = t.GetInt("1");
            cupet_id = t.GetInt("2");
            count = t.GetInt("3");
            rank = t.GetInt("4");
            exp = t.GetInt("8");
        }

        public void SetCupetLevel(int cupetLevel)
        {
            level = cupetLevel;
        }
    }
}