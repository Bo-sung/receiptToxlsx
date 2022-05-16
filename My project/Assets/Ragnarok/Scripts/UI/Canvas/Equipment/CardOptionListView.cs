using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardOptionListView : UIView, IInspectorFinder
    {
        [SerializeField] UILabelValue[] labels;

        protected override void OnLocalize()
        {

        }

        public void SetData(ItemInfo info)
        {
            IEnumerator<BattleOption> enumerator = info.GetEnumerator();
            IEnumerator<CardBattleOption> enumeratorCard = info.GetCardBattleOptionCollection().GetEnumerator();

            foreach (var label in labels)
            {
                bool moveNext = enumerator.MoveNext();
                enumeratorCard.MoveNext();
                label.SetActive(moveNext);

                if (moveNext)
                {
                    BattleOption option = enumerator.Current;
                    CardBattleOption cardOption = enumeratorCard.Current;

                    label.Title = option.GetTitleText();
                    if (option.battleOptionType.IsConditionalSkill())
                    {
                        label.Value = option.GetValueText();
                    }
                    else
                    {
                        label.Value = $"{option.GetValueText()} {LocalizeKey._18011.ToText().Replace(ReplaceKey.VALUE, cardOption.GetMaxValueText())}";
                    }
                }
            }
        }

        bool IInspectorFinder.Find()
        {
            labels = GetComponentsInChildren<UILabelValue>();
            return true;
        }
    }
}