using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardInfoSimpleOptionView : UIView, IInspectorFinder
    {
        [SerializeField] UIGrid grid;
        [SerializeField] UILabelValue[] labels;

        protected override void OnLocalize()
        {

        }

        public void Set(IEnumerable<CardBattleOption> collection)
        {
            IEnumerator<CardBattleOption> enumerator = collection.GetEnumerator();

            foreach (var label in labels)
            {
                bool moveNext = enumerator.MoveNext();
                label.SetActive(moveNext);

                if (moveNext)
                {
                    CardBattleOption option = enumerator.Current;
                    label.Title = option.GetTitleText();
                    label.Value = option.GetTotalMinMaxText();
                }
            }

            grid.repositionNow = true;
        }

        bool IInspectorFinder.Find()
        {
            grid = transform.GetComponent<UIGrid>();
            labels = transform.GetComponentsInChildren<UILabelValue>();

            return true;
        }
    }
}