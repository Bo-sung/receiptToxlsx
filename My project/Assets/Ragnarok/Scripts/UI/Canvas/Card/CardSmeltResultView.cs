using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardSmeltResultView : UIView, IInspectorFinder
    {
        private const int CENTER_MIN = 5;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UITextureHelper mainIcon;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] UILabelHelper labelCardName;
        [SerializeField] UIGridHelper gridRate;
        [SerializeField] UILabelHelper labelTitleCardLevel;
        [SerializeField] UILabelHelper labelBeforeLevel;
        [SerializeField] UILabelHelper labelAfterLevel;
        [SerializeField] UISmeltCardBattleOption[] options;
        [SerializeField] UILabelHelper labelTitleUseGoods;
        [SerializeField] UIScrollView scrollView;
        [SerializeField] UIGrid grid;
        [SerializeField] UICardSmeltResultMaterialSlot[] slot;
        [SerializeField] UILabelHelper labelUseZeny;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnExit.OnClick, Hide);
            EventDelegate.Add(btnConfirm.OnClick, Hide);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnExit.OnClick, Hide);
            EventDelegate.Remove(btnConfirm.OnClick, Hide);
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._18515; // 카드 제련 결과
            labelTitleCardLevel.LocalKey = LocalizeKey._18516; // 카드 제련도
            labelTitleUseGoods.LocalKey = LocalizeKey._18517; // 소비 재화
            btnConfirm.LocalKey = LocalizeKey._18518; // 확인
        }

        public void Set(ItemInfo before, ItemInfo after)
        {
            mainIcon.Set(after.IconName, isAsync: false); // 애니메이션이 Active를 제어하기 때문에 isAsync 를 false 처리
            labelLevel.Text = LocalizeKey._18005.ToText(). // Lv. {LEVEL}
                    Replace("{LEVEL}", after.GetCardLevelView());
            labelCardName.Text = after.Name;
            gridRate.SetValue(after.Rating);

            labelBeforeLevel.Text = before.GetCardLevelView();
            labelAfterLevel.Text = after.GetCardLevelView();

            IEnumerator<BattleOption> beforeOptions = before.GetEnumerator();
            IEnumerator<BattleOption> afterOptions = after.GetEnumerator();

            foreach (var item in options)
            {
                bool moveNext = beforeOptions.MoveNext();
                afterOptions.MoveNext();

                item.SetActive(moveNext);

                if (moveNext)
                {
                    if (beforeOptions.Current.battleOptionType.IsConditionalSkill())
                    {
                        item.Set(beforeOptions.Current.GetTitleText(), beforeOptions.Current.GetValueText(), string.Empty);
                    }
                    else
                    {
                        item.Set(beforeOptions.Current.GetTitleText(), beforeOptions.Current.GetValueText(), afterOptions.Current.GetValueText());
                    }
                }
            }
        }

        public void SetUseGoods(UICardSmeltResultMaterialSlot.Info[] infos, int useZeny)
        {
            for (int i = 0; i < slot.Length; i++)
            {
                if (i < infos.Length)
                {
                    slot[i].Set(infos[i]);
                    slot[i].Show();
                }
                else
                {
                    slot[i].Hide();
                }
            }
            if (infos.Length <= CENTER_MIN)
            {
                grid.pivot = UIWidget.Pivot.Center;
                scrollView.contentPivot = UIWidget.Pivot.Center;
            }
            else
            {
                grid.pivot = UIWidget.Pivot.Left;
                scrollView.contentPivot = UIWidget.Pivot.Left;
            }
            grid.Reposition();
            scrollView.ResetPosition();

            labelUseZeny.Text = useZeny.ToString("N0");
        }

        bool IInspectorFinder.Find()
        {
            options = GetComponentsInChildren<UISmeltCardBattleOption>();
            slot = GetComponentsInChildren<UICardSmeltResultMaterialSlot>();
            return true;
        }
    }
}