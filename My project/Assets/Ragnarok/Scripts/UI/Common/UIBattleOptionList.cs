using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIGrid))]
    public class UIBattleOptionList : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] protected UIGrid grid;
        [SerializeField] UILabelValue[] labels;

        public void SetData(IEnumerable<BattleOption> collection)
        {
            IEnumerator<BattleOption> enumerator = collection.Reverse().GetEnumerator();

            foreach (var label in labels.Reverse())
            {
                bool moveNext = enumerator.MoveNext();
                label.SetActive(moveNext);

                if (moveNext)
                {
                    BattleOption option = enumerator.Current;
                    label.Title = option.GetTitleText();
                    label.Value = option.GetValueText();
                }
            }

            Reposition();
        }

        public void SetCardData(IEnumerable<CardBattleOption> collection)
        {
            IEnumerator<CardBattleOption> enumerator = collection.Reverse().GetEnumerator();

            foreach (var label in labels.Reverse())
            {
                bool moveNext = enumerator.MoveNext();

                if (moveNext)
                {
                    CardBattleOption option = enumerator.Current;
                    label.Value += $" {LocalizeKey._18011.ToText().Replace(ReplaceKey.VALUE, option.GetMaxValueText())}";
                }
            }
        }

        protected virtual void Reposition()
        {
            grid.Reposition();
        }

        bool IInspectorFinder.Find()
        {
            grid = GetComponent<UIGrid>();
            labels = GetComponentsInChildren<UILabelValue>();

            grid.hideInactive = true;
            return true;
        }
    }
}