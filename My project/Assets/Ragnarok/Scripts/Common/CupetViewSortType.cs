using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// 큐펫 뷰 정렬 카테고리
    /// </summary>
    public enum CupetViewSortType
    {
        /// <summary>
        /// 미보유
        /// </summary>
        Unowned = 0,

        /// <summary>
        /// 랭크 낮은 순
        /// </summary>
        RankAscending = 1,

        /// <summary>
        /// 랭크 높은 순
        /// </summary>
        RankDescending = 2,

        /// <summary>
        /// 전체
        /// </summary>
        All = 3
    }

    public static class CupetViewSortTypeExtensions
    {
        public delegate int SortFunc(CupetEntity A, CupetEntity B);
        public delegate IComparer<CupetEntity> asd(CupetEntity a, CupetEntity b);

        public static string ToText(this CupetViewSortType type)
        {
            switch (type)
            {
                case CupetViewSortType.Unowned:
                    return LocalizeKey._5026.ToText(); // 미보유
                case CupetViewSortType.RankAscending:
                    return LocalizeKey._5027.ToText(); // 랭크 낮은 순
                case CupetViewSortType.RankDescending:
                    return LocalizeKey._5028.ToText(); // 랭크 높은 순
                case CupetViewSortType.All:
                    return LocalizeKey._5029.ToText(); // 전체
            }
            return string.Empty;
        }

        /// <summary>
        /// 타입에 맞는 SortComparer 반환
        /// </summary>
        public static System.Comparison<CupetEntity> GetSortFunc(this CupetViewSortType type)
        {
            switch (type)
            {
                case CupetViewSortType.Unowned:
                    return SortFunc_Unowned;
                case CupetViewSortType.RankAscending:
                    return SortFunc_Rank_Asc; // 랭크 낮은 순
                case CupetViewSortType.RankDescending:
                    return SortFunc_Rank_Dsc; // 랭크 높은 순
                case CupetViewSortType.All:
                    return SortFunc_All; // 전체
            }
            return SortFunc_All;
        }

        /// <summary>
        /// 미보유 정렬
        /// </summary>
        static int SortFunc_Unowned(CupetEntity a, CupetEntity b)
        {
            // 미보유, 보유 정렬
            if (!a.Cupet.IsInPossession && b.Cupet.IsInPossession)
                return -1;
            if (a.Cupet.IsInPossession && !b.Cupet.IsInPossession)
                return 1;

            // 아이디 오름차 순 정렬
            if (a.Cupet.CupetID < b.Cupet.CupetID)
                return -1;
            if (a.Cupet.CupetID > b.Cupet.CupetID)
                return 1;
            return 0;
        }

        /// <summary>
        /// 랭크 낮은 순 정렬
        /// </summary>
        static int SortFunc_Rank_Asc(CupetEntity a, CupetEntity b)
        {
            // 보유, 미보유 정렬
            if (!a.Cupet.IsInPossession && b.Cupet.IsInPossession)
                return 1;
            if (a.Cupet.IsInPossession && !b.Cupet.IsInPossession)
                return -1;

            // 랭크 오름차 순 정렬
            if (a.Cupet.Rank < b.Cupet.Rank)
                return -1;
            if (a.Cupet.Rank > b.Cupet.Rank)
                return 1;

            // 아이디 오름차 순 정렬
            if (a.Cupet.CupetID < b.Cupet.CupetID)
                return -1;
            if (a.Cupet.CupetID > b.Cupet.CupetID)
                return 1;
            return 0;
        }

        /// <summary>
        /// 랭크 높은 순 정렬
        /// </summary>
        static int SortFunc_Rank_Dsc(CupetEntity a, CupetEntity b)
        {
            // 보유, 미보유 정렬
            if (!a.Cupet.IsInPossession && b.Cupet.IsInPossession)
                return 1;
            if (a.Cupet.IsInPossession && !b.Cupet.IsInPossession)
                return -1;

            // 랭크 내림차 순 정렬
            if (a.Cupet.Rank > b.Cupet.Rank)
                return -1;
            if (a.Cupet.Rank < b.Cupet.Rank)
                return 1;

            // 아이디 오름차 순 정렬
            if (a.Cupet.CupetID < b.Cupet.CupetID)
                return -1;
            if (a.Cupet.CupetID > b.Cupet.CupetID)
                return 1;
            return 0;
        }

        /// <summary>
        /// 전체 정렬 (아이디 오름차 순)
        /// </summary>
        static int SortFunc_All(CupetEntity a, CupetEntity b)
        {
            // 아이디 오름차 순 정렬
            if (a.Cupet.CupetID < b.Cupet.CupetID)
                return -1;
            if (a.Cupet.CupetID > b.Cupet.CupetID)
                return 1;
            return 0;
        }
    }
}
