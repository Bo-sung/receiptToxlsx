using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIGuildAttackBuffResultList : UIView, IInspectorFinder
    {
        [SerializeField] UIGuildAttackBuffResult[] buffResults;

        protected override void OnLocalize()
        {
        }

        public void SetBuff(IEnumerable<BattleOption> beforeValues, IEnumerable<BattleOption> afterValues)
        {
            IEnumerator<BattleOption> beforeEnumerator = beforeValues.GetEnumerator();
            IEnumerator<BattleOption> afterEnumerator = afterValues.GetEnumerator();

            foreach (var buff in buffResults)
            {
                bool beforeMoveNext = beforeEnumerator.MoveNext();
                bool afterMoveNext = afterEnumerator.MoveNext();

                bool isShow = beforeMoveNext || afterMoveNext;
                buff.SetActive(isShow);

                if (isShow)
                {
                    BattleOption beforeOption = beforeEnumerator.Current;
                    BattleOption afterOption = afterEnumerator.Current;

                    bool isNew = !beforeMoveNext && afterMoveNext;
                    bool isRemove = beforeMoveNext && !afterMoveNext;

                    if (isNew)
                    {
                        buff.SetTitle(afterOption.GetTitleText());
                        buff.SetValue(beforeOption.GetValueText());
                        buff.SetAddValue(string.Empty);
                    }
                    else if (isRemove)
                    {
                        buff.SetTitle(beforeOption.GetTitleText());
                        buff.SetValue(beforeOption.GetValueText());
                        buff.SetAddValue(string.Empty);
                    }
                    else
                    {
                        buff.SetTitle(beforeOption.GetTitleText());
                        buff.SetValue(beforeOption.GetValueText());
                        // 증가 || 다운
                        int value1 = afterOption.value1 - beforeOption.value1;
                        int value2 = afterOption.value2 - beforeOption.value2;
                        BattleOption addValue = new BattleOption(beforeOption.battleOptionType, value1, value2);
                        string text = addValue.GetValueText();
                        buff.SetAddValue(text);
                    }
                    buff.SetActiveNew(isNew);
                    buff.SetAlpha(isRemove ? 0.3f : 1f);
                }
            }
        }

        bool IInspectorFinder.Find()
        {
            buffResults = GetComponentsInChildren<UIGuildAttackBuffResult>();
            return true;
        }
    }
}