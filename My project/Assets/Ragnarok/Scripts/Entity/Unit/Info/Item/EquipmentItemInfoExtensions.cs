using System.Linq;

namespace Ragnarok
{
    public static class EquipmentItemInfoExtensions
    {
        public static EquipmentItemInfo[] Sort(this EquipmentItemInfo[] infos, bool isAscending, ItemEquipmentSlotType type = ItemEquipmentSlotType.None, int rank = 0)
        {
            var result = from pair in infos select pair;

            // 종류 필터
            if (type != ItemEquipmentSlotType.None)
            {
                result = result.Where(x => x.SlotType == type);
            }

            // 등급 필터
            if (rank != 0)
            {
                result = result.Where(x => x.Rating == rank);
            }

            // 정렬(오름차순)
            if (isAscending)
            {
                result = result.OrderBy(x => !x.IsEquipped)
                               .ThenBy(x => x.Rating)
                               .ThenBy(x => x.Tier)
                               .ThenBy(x => x.Smelt)
                               .ThenBy(x => x.ItemId);
            }
            else // 내림차순
            {
                result = result.OrderBy(x => !x.IsEquipped)
                               .ThenByDescending(x => x.Rating)
                               .ThenByDescending(x => x.Tier)
                               .ThenByDescending(x => x.Smelt)
                               .ThenBy(x => x.ItemId);
            }

            return result.ToArray();
        }
    }
}