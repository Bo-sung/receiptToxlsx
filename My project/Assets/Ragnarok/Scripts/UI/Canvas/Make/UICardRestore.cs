using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class UICardRestore : MonoBehaviour
    {
        [System.Serializable]
        private class OptionBar
        {
            [SerializeField] UIProgressBar gauge;
            [SerializeField] UILabelHelper value;
            [SerializeField] UILabelHelper maxValue;

            public void SetData(string valueText, string maxValueText, float rateValue)
            {
                gauge.value = rateValue;
                value.Text = valueText;
                maxValue.Text = maxValueText;
            }
        }

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIButtonHelper selectTargetButton;
        [SerializeField] UICardProfile targetCardItem;

        [SerializeField] GameObject[] beforeAfterPanelRoots;
        [SerializeField] UILabelHelper[] beforeAfterPanelLabels;
        [SerializeField] UILabelHelper[] levelLabels;
        [SerializeField] UILabelHelper[] qualityLabels;
        
        [SerializeField] UILabelHelper listPanelLabel;
        [SerializeField] UICostButtonHelper requestButton;

        [SerializeField] UILabelHelper selectCardNotice;

        [SerializeField] UIGrid optionGrid;
        [SerializeField] GameObject[] optionRoots;
        [SerializeField] UILabelHelper[] optionNameLabels;
        [SerializeField] OptionBar[] beforeOptionBars;
        [SerializeField] OptionBar[] afterOptionBars;
        [SerializeField] UILabelHelper[] optionInc;
        [SerializeField] UILabelHelper[] optionDec;

        [SerializeField] GameObject root;
        [SerializeField] GameObject root2;
        [SerializeField] GameObject contentsLock;
        [SerializeField] UILabelHelper contentsLockTitle;
        [SerializeField] UILabelHelper contentsLockDesc;

        private CardRestorePresenter presenter;
        private CardItemInfo selectedTargetItem;
        private CardItemInfo restoredSelectedTargetItem;
        private int requiredCatCoin;

        public void OnInit()
        {
            presenter = new CardRestorePresenter(this);
            presenter.AddEvent();
        }

        public void OnClose()
        {
            presenter.RemoveEvent();
        }

        public void OnShow()
        {
            selectCardNotice.LocalKey = LocalizeKey._28043;
            requestButton.LocalKey = LocalizeKey._28035;
            listPanelLabel.LocalKey = LocalizeKey._28044;
            titleLabel.LocalKey = LocalizeKey._28055;
            beforeAfterPanelLabels[0].LocalKey = LocalizeKey._22207;
            beforeAfterPanelLabels[1].LocalKey = LocalizeKey._22208;
            ResetSelection();

            EventDelegate.Add(selectTargetButton.OnClick, ShowSelectTargetItem);
            EventDelegate.Add(requestButton.OnClick, RequestRestore);

            SetContentsOpen(presenter.IsContentsOpen());
        }

        public void OnHide()
        {
            EventDelegate.Remove(selectTargetButton.OnClick, ShowSelectTargetItem);
            EventDelegate.Remove(requestButton.OnClick, RequestRestore);
        }

        public void ResetSelection()
        {
            selectedTargetItem = null;
            targetCardItem.gameObject.SetActive(false);

            beforeAfterPanelRoots[0].SetActive(false);
            beforeAfterPanelRoots[1].SetActive(false);

            for (int i = 0; i < optionRoots.Length; ++i)
                optionRoots[i].SetActive(false);

            requestButton.CostText = "0";
            requiredCatCoin = 0;
            selectCardNotice.SetActive(true);

            UpdateRequestButton();
        }

        private void ShowSelectTargetItem()
        {
            ResetSelection();

            UI.Show<UIItemSelect>(new UIItemSelect.Input()
            {
                filter = Element_Filter,
                sort = Element_Sort,
                targetSelectionCount = 1,
                onSelectionFinished = OnTargetItemSelected,
                preSelection = StopTargetSelection
            });
        }

        private void OnTargetItemSelected(List<ItemInfo> selected)
        {
            SelectCard(selected[0] as CardItemInfo);
        }

        public void SelectCard(CardItemInfo cardItem)
        {
            selectedTargetItem = cardItem;

            var restored = selectedTargetItem.GetRestoredCard();
            restoredSelectedTargetItem = restored;

            targetCardItem.gameObject.SetActive(true);
            targetCardItem.SetData(selectedTargetItem);

            beforeAfterPanelRoots[0].SetActive(true);
            beforeAfterPanelRoots[1].SetActive(true);

            levelLabels[0].Text = $"Lv.{selectedTargetItem.CardLevel}";
            qualityLabels[0].Text = $"[{MathUtils.ToInt(selectedTargetItem.OptionRate * 100)}%]";
            levelLabels[1].Text = $"Lv.{restored.CardLevel}";
            qualityLabels[1].Text = $"[{MathUtils.ToInt(restored.OptionRate * 100)}%]";

            SetData(selectedTargetItem, selectedTargetItem, restored, restored);
            requiredCatCoin = BasisType.CARD_RESTORING_COST.GetInt(selectedTargetItem.Rating);
            requestButton.CostText = requiredCatCoin.ToString("n0");

            selectCardNotice.SetActive(false);

            UpdateRequestButton();
        }

        private void UpdateRequestButton()
        {
            requestButton.IsEnabled = CheckCondition();
        }

        private bool CheckCondition()
        {
            return selectedTargetItem != null && Entity.player.Goods.CatCoin >= requiredCatCoin && selectedTargetItem.CardLevel != restoredSelectedTargetItem.CardLevel;
        }

        private void RequestRestore()
        {
            presenter.RequestRestore(selectedTargetItem);
        }

        private void SetData(IEnumerable<BattleOption> beforeBO, IEnumerable<CardBattleOption> beforeCBO, IEnumerable<BattleOption> afterBO, IEnumerable<CardBattleOption> afterCBO)
        {
            IEnumerator<BattleOption> beforeOption = beforeBO.GetEnumerator();
            IEnumerator<CardBattleOption> beforeMaxOption = beforeCBO.GetEnumerator();

            IEnumerator<BattleOption> afterOption = afterBO.GetEnumerator();
            IEnumerator<CardBattleOption> afterMaxOption = afterCBO.GetEnumerator();

            for (int i = 0; i < beforeOptionBars.Length; ++i)
            {
                var eachBefore = beforeOptionBars[i];
                var eachAfter = afterOptionBars[i];

                bool moveNext = beforeOption.MoveNext();
                afterOption.MoveNext();

                beforeMaxOption.MoveNext();
                afterMaxOption.MoveNext();

                optionRoots[i].SetActive(moveNext);

                if (moveNext)
                {
                    BattleOption beforeValue = beforeOption.Current;
                    CardBattleOption beforeMaxValue = beforeMaxOption.Current;
                    BattleOption afterValue = afterOption.Current;
                    CardBattleOption afterMaxValue = afterMaxOption.Current;

                    optionNameLabels[i].Text = beforeValue.GetTitleText();

                    if (beforeValue.battleOptionType.IsConditionalSkill())
                    {
                        string value = beforeValue.GetValueText();
                        eachBefore.SetData(value, string.Empty, 1f);
                        eachAfter.SetData(value, string.Empty, 1f);
                        optionInc[i].SetActive(false);
                        optionDec[i].SetActive(false);
                    }
                    else
                    {
                        string value = beforeValue.GetValueText();
                        string maxValue = LocalizeKey._18011.ToText().Replace(ReplaceKey.VALUE, beforeMaxValue.GetMaxValueText()); // (MAX {VALUE})
                        float rateValue = beforeMaxValue.GetRateValue();
                        float beforeRate = rateValue;
                        eachBefore.SetData(value, maxValue, rateValue);

                        value = afterValue.GetValueText();
                        maxValue = LocalizeKey._18011.ToText().Replace(ReplaceKey.VALUE, afterMaxValue.GetMaxValueText()); // (MAX {VALUE})
                        rateValue = afterMaxValue.GetRateValue();
                        float afterRate = rateValue;

                        eachAfter.SetData(value, maxValue, rateValue);

                        if (Mathf.Abs(beforeRate - afterRate) > 0.0001)
                        {
                            if (beforeRate < afterRate)
                            {
                                optionInc[i].SetActive(true);
                                optionDec[i].SetActive(false);
                                optionInc[i].Text = $"+{((afterRate - beforeRate) * 100f).ToString("f2")}%";
                            }
                            else
                            {
                                optionInc[i].SetActive(false);
                                optionDec[i].SetActive(true);
                                optionDec[i].Text = $"-{((beforeRate - afterRate) * 100f).ToString("f2")}%";
                            }
                        }
                        else
                        {
                            optionInc[i].SetActive(false);
                            optionDec[i].SetActive(false);
                        }
                    }
                }
            }

            optionGrid.Reposition();
        }

        private bool Element_Filter(ItemInfo itemInfo)
        {
            return itemInfo.ItemGroupType == ItemGroupType.Card;
        }

        private int Element_Sort(ItemInfo a, ItemInfo b)
        {
            return a.ItemId - b.ItemId;
        }

        private async Task<bool> StopTargetSelection(ItemInfo item)
        {
            if (item.IsLock)
            {
                UI.ShowToastPopup(LocalizeKey._28045.ToText());
                return false;
            }

            return true;
        }

        public void SetContentsOpen(bool value)
        {
            if (value)
            {
                root.SetActive(true);
                root2.SetActive(true);
                contentsLock.SetActive(false);
            }
            else
            {
                root.SetActive(false);
                root2.SetActive(false);
                contentsLock.SetActive(true);

                contentsLockTitle.Text = LocalizeKey._54308.ToText(); // 카드 복원 잠금 해제
                var scenario = ScenarioMazeDataManager.Instance.GetByContents(ContentType.ManageCard);
                contentsLockDesc.Text = LocalizeKey._54306.ToText().Replace(ReplaceKey.NAME, scenario.name_id.ToText());
            }
        }
    }
}
