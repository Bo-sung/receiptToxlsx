using System.Linq;

namespace Ragnarok
{
    public static class CardItemInfoExtensions
    {
        public static CardItemInfo[] Sort(this CardItemInfo[] infos, bool isAscending)
        {
            var result = from pair in infos select pair;

            if (isAscending)
            {
                result = result.OrderBy(x => x.CardLevel)
                               .ThenBy(x => x.ItemId);
            }
            else
            {
                result = result.OrderByDescending(x => x.CardLevel)
                              .ThenBy(x => x.ItemId);
            }

            return result.ToArray();
        }
    }
}