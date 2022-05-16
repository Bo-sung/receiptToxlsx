namespace Ragnarok
{
    public interface ICupetModel : IInfo
    {
        bool IsInPossession { get; }

        int CupetID { get; }
        int Rank { get; }
        int Level { get; }
        string Name { get; }
        ElementType ElementType { get; }
        CupetType CupetType { get; }
        bool IsNeedEvolution { get; }
        int Exp { get; }

        /// <summary>
        /// 조각 개수
        /// </summary>
        int Count { get; }
    }
}