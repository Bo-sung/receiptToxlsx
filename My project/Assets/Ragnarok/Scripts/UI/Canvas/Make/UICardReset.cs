using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class UICardReset : MonoBehaviour
    {
        [System.Serializable]
        private class OptionBar
        {
            [SerializeField] GameObject root;
            [SerializeField] UILabelHelper name;
            [SerializeField] UILabelHelper value;

            public void SetData(string name, string minMaxText)
            {
                this.name.Text = name;
                value.Text = minMaxText;
            }

            public void SetActive(bool value)
            {
                root.SetActive(value);
            }
        }

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIButtonHelper selectTargetButton;
        [SerializeField] UICardProfile targetCardItem;

        [SerializeField] UILabelHelper listPanelLabel;
        [SerializeField] UICostButtonHelper requestButton;
        [SerializeField] UILabelHelper selectCardNotice;

        [SerializeField] UIGrid optionGrid;
        [SerializeField] OptionBar[] options;

        [SerializeField] UILabelHelper noticeLabel;

        [SerializeField] GameObject root;
        [SerializeField] GameObject root2;
        [SerializeField] GameObject contentsLock;
        [SerializeField] UILabelHelper contentsLockTitle;
        [SerializeField] UILabelHelper contentsLockDesc;

        private CardResetPresenter presenter;
        private int requiredCatCoin;
        private CardItemInfo selectedTargetItem;

        public void OnInit()
        {
            presenter = new CardResetPresenter(this);
            presenter.AddEvent();
        }

        public void OnClose()
        {
            presenter.RemoveEvent();
        }

        public void OnShow()
        {
            listPanelLabel.LocalKey = LocalizeKey._28046;
            selectCardNotice.LocalKey = LocalizeKey._28047;
            noticeLabel.LocalKey = LocalizeKey._28048;
            requestButton.LocalKey = LocalizeKey._28036;
            titleLabel.LocalKey = LocalizeKey._28055;
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

        private void RequestRestore()
        {
            presenter.RequestReset(selectedTargetItem);
        }

        private void OnTargetItemSelected(List<ItemInfo> selected)
        {
            selectedTargetItem = selected[0] as CardItemInfo;
            var restored = selectedTargetItem.GetRestoredCard();

            targetCardItem.gameObject.SetActive(true);
            targetCardItem.SetData(selectedTargetItem);

            UpdateOptionList();
            requiredCatCoin = BasisType.CARD_RESET_COST.GetInt(selectedTargetItem.Rating);
            requestButton.CostText = requiredCatCoin.ToString("n0");

            selectCardNotice.SetActive(false);

            UpdateRequestButton();
        }

        private void UpdateOptionList()
        {
            var ret = new CardItemInfo();
            ret.SetData(ItemDataManager.Instance.Get(selectedTargetItem.ItemId));
            ret.SetItemInfo(1, 0, 0, 0, 0, 0, 0, false, 0, 0, 0);

            var optionCollection = ret.GetCardBattleOptionCollection();

            foreach (var each in options)
                each.SetActive(false);

            int index = 0;
            foreach (var each in optionCollection)
            {
                var eachOptionSlot = options[index++];
                eachOptionSlot.SetActive(true);

                string titleText = each.GetTitleText();

                if(each.battleOptionType.IsConditionalSkill())
                {
                    eachOptionSlot.SetData(titleText, each.GetTotalMinMaxText());
                }
                else
                {
                    eachOptionSlot.SetData(titleText, $"({each.GetTotalMinMaxText()})");
                }
            }

            optionGrid.Reposition();
        }

        public void ResetSelection()
        {
            selectedTargetItem = null;
            selectCardNotice.SetActive(true);
            targetCardItem.gameObject.SetActive(false);
            
            for (int i = 0; i < options.Length; ++i)
                options[i].SetActive(false);

            requestButton.CostText = "0";

            UpdateRequestButton();
        }

        private void UpdateRequestButton()
        {
            requestButton.IsEnabled = CheckCondition();
        }

        private bool CheckCondition()
        {
            return selectedTargetItem != null && Entity.player.Goods.CatCoin >= requiredCatCoin;
        }

        private bool Element_Filter(ItemInfo itemInfo)
        {
            return itemInfo.ItemType == ItemType.Card;
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

                contentsLockTitle.Text = LocalizeKey._54307.ToText(); // 카드 재구성 잠금 해제
                var scenario = ScenarioMazeDataManager.Instance.GetByContents(ContentType.ManageCard);
                contentsLockDesc.Text = LocalizeKey._54306.ToText().Replace(ReplaceKey.NAME, scenario.name_id.ToText());
            }
        }
    }
}