using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok.View
{
    public class BasicStatusListView : UIView, IInspectorFinder
    {
        [SerializeField] UIGrid grid; // 장비 추가 옵션 정렬
        [SerializeField] UILabelValue[] labels; // 장비 추가 옵션

        protected override void OnLocalize()
        {
        }

        public void SetData(IEnumerable<BasicStatusOptionValue> collection)
        {
            bool isValid = false;

            if (collection != null)
            {
                IEnumerator<BasicStatusOptionValue> enumerator = collection.Reverse().GetEnumerator();
                foreach (var label in labels.Reverse())
                {
                    bool moveNext = enumerator.MoveNext();
                    label.SetActive(moveNext);

                    if (moveNext)
                    {
                        isValid = true;
                        BasicStatusOptionValue option = enumerator.Current;
                        label.Title = option.titleKey.ToText();
                        label.Value = option.value;
                    }
                }
            }

            SetActive(isValid);
            grid.Reposition();
        }

        bool IInspectorFinder.Find()
        {
            grid = GetComponent<UIGrid>();
            labels = GetComponentsInChildren<UILabelValue>();
            return true;
        }
    }
}