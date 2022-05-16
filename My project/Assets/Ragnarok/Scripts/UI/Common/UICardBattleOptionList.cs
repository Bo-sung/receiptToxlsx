using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Ragnarok.View
{
    public class UICardBattleOptionList : UIView, IInspectorFinder
    {
        [SerializeField] Color32 colorLow, colorMiddle, colorHigh;
        [SerializeField] float rangeLow, rangeMiddle;
        [SerializeField] UIGrid grid;
        [SerializeField] CardOptionSlot[] slots;

        protected override void OnLocalize()
        {
        }

        public void SetData(IEnumerable<BattleOption> collection, IEnumerable<CardBattleOption> maxOptionCollection)
        {
            IEnumerator<BattleOption> enumerator = collection.Reverse().GetEnumerator();
            IEnumerator<CardBattleOption> maxOptionEnumerator = maxOptionCollection.Reverse().GetEnumerator();

            foreach (var slot in slots.Reverse())
            {
                bool moveNext = enumerator.MoveNext();
                maxOptionEnumerator.MoveNext();
                slot.SetActive(moveNext);

                if (moveNext)
                {
                    BattleOption option = enumerator.Current;
                    CardBattleOption cardBattleOption = maxOptionEnumerator.Current;

                    if (option.battleOptionType.IsConditionalSkill())
                    {
                        string title = option.GetTitleText();
                        string optionValue = string.Empty;
                        string maxValue = option.GetValueText();
                        float rateValue = 1f;
                        slot.Set(title, optionValue, maxValue, rateValue, GetColorByRate(rateValue));
                    }
                    else
                    {
                        string title = option.GetTitleText();
                        string optionValue = option.GetValueText();
                        string maxValue = LocalizeKey._18011.ToText().Replace(ReplaceKey.VALUE, cardBattleOption.GetMaxValueText()); // (MAX {VALUE})
                        float rateValue = cardBattleOption.GetRateValue();
                        slot.Set(title, optionValue, maxValue, rateValue, GetColorByRate(rateValue));
                    }
                }
            }

            Reposition();
        }

        Color32 GetColorByRate(float rate)
        {
            if (rate < rangeLow)
                return colorLow;

            if (rate < rangeMiddle)
                return colorMiddle;

            return colorHigh;
        }

        private void Reposition()
        {
            grid.Reposition();
        }

        bool IInspectorFinder.Find()
        {
            grid = GetComponent<UIGrid>();
            slots = GetComponentsInChildren<CardOptionSlot>();
            grid.hideInactive = true;
            return true;
        }
    }
}