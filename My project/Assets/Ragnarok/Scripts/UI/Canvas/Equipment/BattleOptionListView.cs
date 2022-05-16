using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok.View
{
    public class BattleOptionListView : UIView
    {
        [SerializeField] UIGrid grid; // 장비 추가 옵션 정렬
        [SerializeField] UILabelValue[] labels; // 장비 추가 옵션

        protected override void OnLocalize()
        {
        }

        public void SetData(IEnumerable<BattleOption> collection)
        {
            IEnumerator<BattleOption> enumerator = collection.Reverse().GetEnumerator();

            bool isValid = false;
            foreach (var label in labels.Reverse())
            {
                bool moveNext = enumerator.MoveNext();
                label.SetActive(moveNext);

                if (moveNext)
                {
                    isValid = true;
                    BattleOption option = enumerator.Current;
                    label.Title = option.GetTitleText();
                    label.Value = option.GetValueText();
                }
            }

            SetActive(isValid);
            grid.Reposition();
        }
    }
}