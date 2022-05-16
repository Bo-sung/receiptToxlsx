using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardSmeltOptionView : UIView, IAutoInspectorFinder, IInspectorFinder
    {
        [SerializeField] UIGrid grid;
        [SerializeField] UISmeltCardBattleOption[] options;

        protected override void OnLocalize()
        {

        }

        public void Set(ItemInfo info)
        {
            Timing.KillCoroutines(gameObject);
            IEnumerator<BattleOption> battleOptions = info.GetEnumerator();
            IEnumerator<CardBattleOption> cardBattleOptons = info.GetCardBattleOptionCollection().GetEnumerator();

            foreach (var item in options)
            {
                bool moveNext = battleOptions.MoveNext();
                cardBattleOptons.MoveNext();

                item.SetActive(moveNext);
                item.SetFx(false);

                if (moveNext)
                {
                    if (cardBattleOptons.Current.battleOptionType.IsConditionalSkill())
                    {
                        item.Set(battleOptions.Current.GetTitleText(), battleOptions.Current.GetValueText(), string.Empty);
                    }
                    else
                    {
                        item.Set(battleOptions.Current.GetTitleText(), battleOptions.Current.GetValueText(), cardBattleOptons.Current.GetNextMinMaxText());
                    }
                }
            }
            grid.repositionNow = true;
        }

        public void SetFx(ItemInfo before, ItemInfo after)
        {
            Timing.KillCoroutines(gameObject);
            IEnumerator<CardBattleOption> beforeOption = before.GetCardBattleOptionCollection().GetEnumerator();
            IEnumerator<CardBattleOption> afterOption = after.GetCardBattleOptionCollection().GetEnumerator();

            foreach (var item in options)
            {
                bool moveNext = beforeOption.MoveNext();
                afterOption.MoveNext();

                item.SetActive(moveNext);

                if (moveNext && afterOption.Current.serverValue > 0)
                {
                    item.SetFx(true);
                    Timing.RunCoroutine(item.SetValueFx(beforeOption.Current, afterOption.Current), gameObject);
                }
            }
            grid.repositionNow = true;
        }

        bool IInspectorFinder.Find()
        {
            options = transform.GetComponentsInChildren<UISmeltCardBattleOption>();
            return true;
        }
    }
}