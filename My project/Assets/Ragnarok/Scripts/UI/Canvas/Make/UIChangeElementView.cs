using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class UIChangeElementView : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIEquipmentProfile targetEquipItem;
        [SerializeField] GameObject materialListPanelRoot;
        [SerializeField] UILabelHelper listPanelLabel;
        [SerializeField] UIHextechSlot materialStoneSlot;
        [SerializeField] UIGridHelper materialStoneSlotGrid;
        [SerializeField] UIButtonHelper selectTargetButton;
        [SerializeField] UIButtonHelper requestButton;
        [SerializeField] UILabelHelper[] beforeAfterLabels;
        [SerializeField] UISprite beforeElement;
        [SerializeField] UILabelHelper beforeElementLevel;
        [SerializeField] UISprite afterElement;
        [SerializeField] UILabelHelper afterElementLevel;

        [SerializeField] GameObject root;
        [SerializeField] GameObject root2;
        [SerializeField] GameObject contentsLock;
        [SerializeField] UILabelHelper contentsLockTitle;
        [SerializeField] UILabelHelper contentsLockDesc;
        [SerializeField] GameObject changeElementFx;
        [SerializeField] float fxTime;

        private ChangeElementViewPresenter presenter;
        private ItemInfo selectedTargetItem;
        private ItemInfo selectedStone;
        private bool isSendRequest;

        public void OnInit()
        {
            selectedTargetItem = null;
            selectedStone = null;
            presenter = new ChangeElementViewPresenter();

            presenter.OnChangeElement += OnChangeElement;

            presenter.AddEvent();
        }

        public void OnClose()
        {
            presenter.OnChangeElement -= OnChangeElement;
            presenter.RemoveEvent();
        }

        public void OnShow()
        {
            isSendRequest = false;
            changeElementFx.SetActive(false);
            materialListPanelRoot.SetActive(true);
            selectedTargetItem = null;
            selectedStone = null;
            UpdateRequestButtonAndElementIcon();

            materialStoneSlot.Init(0, OnClickMaterialSlot);
            materialStoneSlot.Show().ShowPlus(UIHextechSlot.PlusPos.Center);
            materialStoneSlotGrid.Reposition();

            requestButton.LocalKey = LocalizeKey._28033;
            listPanelLabel.LocalKey = LocalizeKey._28037;
            titleLabel.LocalKey = LocalizeKey._28054;
            beforeAfterLabels[0].LocalKey = LocalizeKey._28056;
            beforeAfterLabels[1].LocalKey = LocalizeKey._28057;

            EventDelegate.Add(selectTargetButton.OnClick, ShowSelectTargetItem);
            EventDelegate.Add(requestButton.OnClick, TrySendRequest);

            SetContentsOpen(presenter.IsContentsOpen());
        }

        public void OnHide()
        {
            EventDelegate.Remove(selectTargetButton.OnClick, ShowSelectTargetItem);
            EventDelegate.Remove(requestButton.OnClick, TrySendRequest);
        }

        private void ShowSelectTargetItem()
        {
            selectedTargetItem = null;
            targetEquipItem.gameObject.SetActive(false);
            UpdateRequestButtonAndElementIcon();

            UI.Show<UIItemSelect>(new UIItemSelect.Input()
            {
                filter = GiveElement_Filter,
                sort = GiveElement_Sort,
                targetSelectionCount = 1,
                onSelectionFinished = OnTargetItemSelected,
                preSelection = StopTargetSelection
            });
        }

        private void OnTargetItemSelected(List<ItemInfo> selected)
        {
            selectedTargetItem = selected[0];
            var item = selectedTargetItem as EquipmentItemInfo;
            targetEquipItem.gameObject.SetActive(true);
            targetEquipItem.SetData(selectedTargetItem);
            UpdateRequestButtonAndElementIcon();
        }

        private void OnClickMaterialSlot(int slotID)
        {
            selectedStone = null;
            materialStoneSlot.Show().ShowPlus(UIHextechSlot.PlusPos.Center);
            UpdateRequestButtonAndElementIcon();

            UI.Show<UIItemSelect>(new UIItemSelect.Input()
            {
                filter = GiveElement_Mat_Filter,
                sort = GiveElement_Sort,
                targetSelectionCount = 1,
                onSelectionFinished = OnSelectElementStone,
                preSelection = StopCardEquippedItemSelection
            });
        }

        private void OnSelectElementStone(List<ItemInfo> selected)
        {
            selectedStone = selected[0];
            materialStoneSlot.Show().SetIcon(selectedStone);
            UpdateRequestButtonAndElementIcon();
        }

        public void ResetView()
        {
            selectedStone = null;
            materialStoneSlot.Show().ShowPlus(UIHextechSlot.PlusPos.Center);
            UpdateRequestButtonAndElementIcon();
        }

        private void TrySendRequest()
        {
            if (!CheckCondition())
                return;

            presenter.RequestGiveElemental(selectedTargetItem, selectedStone);
        }

        private bool GiveElement_Filter(ItemInfo itemInfo)
        {
            return itemInfo.ItemGroupType == ItemGroupType.Equipment
                && (itemInfo.SlotType == ItemEquipmentSlotType.Weapon || itemInfo.SlotType == ItemEquipmentSlotType.Armor);
        }

        private bool GiveElement_Mat_Filter(ItemInfo itemInfo)
        {
            return itemInfo.IsElementStone;
        }

        private int GiveElement_Sort(ItemInfo a, ItemInfo b)
        {
            return a.ItemId - b.ItemId;
        }

        private void UpdateRequestButtonAndElementIcon()
        {
            requestButton.IsEnabled = CheckCondition();

            bool hasBeforeElement = selectedTargetItem != null;
            beforeElement.gameObject.SetActive(hasBeforeElement);
            if (hasBeforeElement)
            {
                beforeElement.spriteName = selectedTargetItem.ElementType.GetIconName();
                beforeElementLevel.Text = selectedTargetItem.GetElementLevelText();
            }
            else
            {
                beforeElementLevel.Text = "-";
            }

            bool hasAfterElement = selectedStone != null;
            afterElement.gameObject.SetActive(hasAfterElement);
            if (hasAfterElement)
            {
                afterElement.spriteName = selectedStone.ElementType.GetIconName();
                afterElementLevel.Text = selectedStone.GetElementLevelText();
            }
            else
            {
                afterElementLevel.Text = "-";
            }
        }

        private bool CheckCondition()
        {
            if (selectedTargetItem == null || selectedStone == null || isSendRequest)
                return false;

            // 무기 속성과 속성석 속성이 동일
            // 무기 속성레벨과 속성석 속성레벨이 동일
            if (selectedTargetItem.ElementType == selectedStone.ElementType && selectedTargetItem.ElementLevel == selectedStone.ElementLevel)
                return false;

            return true;
        }

        private async Task<bool> StopTargetSelection(ItemInfo item)
        {
            if (item.IsLock)
            {
                UI.ShowToastPopup(LocalizeKey._28038.ToText());
                return false;
            }

            return true;
        }

        private async Task<bool> StopCardEquippedItemSelection(ItemInfo item)
        {
            if (item.IsEquipped)
            {
                UI.ShowToastPopup(LocalizeKey._28039.ToText());
                return false;
            }

            if (item.IsLock)
            {
                UI.ShowToastPopup(LocalizeKey._28038.ToText());
                return false;
            }

            for (int i = 0; i < Constants.Size.MAX_EQUIPPED_CARD_COUNT; ++i)
            {
                ItemInfo card = item.GetCardItem(i);
                if (card != null)
                {
                    UI.ShowToastPopup(LocalizeKey._90196.ToText()); // 카드가 장착된 장비는 등록할 수 없습니다.
                    return false;
                }
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

                contentsLockTitle.Text = LocalizeKey._54309.ToText(); // 속성 부여 잠금 해제
                var scenario = ScenarioMazeDataManager.Instance.GetByContents(ContentType.ChangeElement);
                contentsLockDesc.Text = LocalizeKey._54306.ToText().Replace(ReplaceKey.NAME, scenario.name_id.ToText());
            }
        }

        private void OnChangeElement(bool isSuccess)
        {
            isSendRequest = false;

            if (!isSuccess)
                return;

            Timing.RunCoroutineSingleton(YieldChangeElementEffect().CancelWith(gameObject), nameof(YieldChangeElementEffect), SingletonBehavior.Overwrite);
        }

        /// <summary>
        ///  연출 후 갱신
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldChangeElementEffect()
        {
            changeElementFx.SetActive(false);
            changeElementFx.SetActive(true);

            yield return Timing.WaitForSeconds(fxTime);

            changeElementFx.SetActive(false);
            UI.ShowToastPopup(LocalizeKey._28051.ToText());
            ResetView();
        }
    }
}