using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIGrid))]
    public class UIBattleOptionPreviewList : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] protected UIGrid grid;
        [SerializeField] UILabelPreviewValue[] labels;

        public void Show(IEnumerable<BattleOption> values, int skillBlowCount = 0)
        {
            IEnumerator<BattleOption> enumerator = values.GetEnumerator();
            foreach (var label in labels)
            {
                bool moveNext = enumerator.MoveNext();
                label.SetActive(moveNext);

                if (moveNext)
                {
                    BattleOption option = enumerator.Current;
                    label.Title = option.GetTitleText(); // Title 세팅
                    label.Show(option.GetValueText(skillBlowCount));
                }
            }

            Reposition();
        }

        public void Show(IEnumerable<BattleOption> beforeValues, IEnumerable<BattleOption> afterValues, int beforeSkillBlowCount = 0, int afterSkillBlowCount = 0)
        {
            IEnumerator<BattleOption> beforeEnumerator = beforeValues.GetEnumerator();
            IEnumerator<BattleOption> afterEnumerator = afterValues.GetEnumerator();

            foreach (var label in labels)
            {
                bool beforeMoveNext = beforeEnumerator.MoveNext();
                bool afterMoveNext = afterEnumerator.MoveNext();

                bool isShow = beforeMoveNext || afterMoveNext;
                label.SetActive(isShow);

                if (isShow)
                {
                    BattleOption beforeOption = beforeEnumerator.Current;
                    BattleOption afterOption = afterEnumerator.Current;

                    if (beforeOption.battleOptionType != afterOption.battleOptionType)
                    {
                        Debug.LogWarning($"비교하는 옵션이 일치하지 않음: {beforeOption} = {beforeOption.battleOptionType}, {afterOption} = {afterOption.battleOptionType}");
                    }

                    label.Title = afterOption.GetTitleText(); // Title 세팅

                    // Value 값이 같을 경우
                    if (beforeOption.value1 == afterOption.value1 && beforeOption.value2 == afterOption.value2)
                    {
                        label.Show(afterOption.GetValueText(afterSkillBlowCount));
                    }
                    else
                    {
                        label.Show(beforeOption.GetValueText(beforeSkillBlowCount), afterOption.GetValueText(afterSkillBlowCount));
                    }
                }
            }

            Reposition();
        }

        protected virtual void Reposition()
        {
            grid.Reposition();
        }

        bool IInspectorFinder.Find()
        {
            grid = GetComponent<UIGrid>();
            labels = GetComponentsInChildren<UILabelPreviewValue>();

            grid.hideInactive = true;
            return true;
        }
    }
}