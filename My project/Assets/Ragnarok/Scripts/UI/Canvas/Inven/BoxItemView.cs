using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BoxItemView : ConsumableItemView
    {
        public override void SortItem()
        {
            arrayInfo = presenter.GetBoxItemInfos();
            System.Array.Sort(arrayInfo, SortByCustom);
        }

        protected override void UpdateView()
        {            
            var array = presenter.GetBoxItemInfos();
            // 아이템 추가 또는 삭제 체크
            var intersect = arrayInfo.Intersect(array);

            if (array.Length != intersect.Count())
            {
                arrayInfo = intersect.Union(array).ToArray();
            }
            else
            {
                arrayInfo = intersect.ToArray();
            }
            // 소모품 정보            
            wrapper.Resize(arrayInfo.Length);
        }
    }
}