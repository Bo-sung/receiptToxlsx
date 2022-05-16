namespace Ragnarok
{
    public class CharAgentPacket : IPacket<Response>, AgentModel.IAgentValue
    {
        public int CID { get; private set; }
        public int DuplicationCount { get; private set; }
        public int ID { get; private set; }
        public ExploreType ExploreType { get; private set; }
        public long ExploreRewardRemainTime { get; private set; }
        public int ExploreStageID { get; private set; }
        public bool IsExploring { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            CID = response.GetInt("1");
            DuplicationCount = response.GetInt("2");
            ID = response.GetInt("3");
            ExploreType = response.GetInt("4").ToEnum<ExploreType>();
            ExploreRewardRemainTime = response.GetLong("5");
            ExploreStageID = response.GetInt("6");
            IsExploring = response.GetInt("7") > 0;
        }
    }

    public class CharAgentBookPacket : AgentModel.IAgentBookValue
    {
        public int Id { get; private set; }

        public CharAgentBookPacket(int id)
        {
            Id = id;
        }
    }

    public class UpdateCharAgentPacket : IPacket<Response>
    {
        public DirtyType DirtyType { get; private set; }
        public CharAgentPacket Packet { get; private set; }

        void IInitializable<Response>.Initialize(Response response)
        {
            DirtyType = response.GetByte("1").ToEnum<DirtyType>();
            Packet = response.GetPacket<CharAgentPacket>("2");
        }
    }
}