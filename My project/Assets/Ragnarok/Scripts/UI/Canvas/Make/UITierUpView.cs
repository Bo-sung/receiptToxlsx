using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class UITierUpView : MonoBehaviour, IAutoInspectorFinder
    {
        [System.Serializable]
        public class StatBeforeAfter
        {
            [SerializeField] UILabelHelper panelLabel;
            [SerializeField] GameObject showRoot;
            [SerializeField] GameObject hideRoot;
            [SerializeField] UILabel beforeLabel;
            [SerializeField] UILabel afterLabel;

            public void SetLabel(int lid)
            {
                panelLabel.LocalKey = lid;
            }

            public void SetActive(bool value)
            {
                showRoot.SetActive(value);
                hideRoot.SetActive(!value);
            }

            public void SetData(int before, int after)
            {
                beforeLabel.text = before.ToString();
                afterLabel.text = after.ToString();
            }
        }

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIButtonHelper selectTargetButton;
        [SerializeField] UIEquipmentProfile targetEquipItem;
        [SerializeField] GameObject materialListPanelRoot;
        [SerializeField] UILabelHelper listPanelLabel;
        [SerializeField] UIHextechSlot[] hextechSlots;
        [SerializeField] UIGridHelper hextechSlotGrid;
        [SerializeField] UILabelHelper selectNotice;
        [SerializeField] UIButtonHelper requestButton;

        [SerializeField] StatBeforeAfter[] statChangePanels;

        [SerializeField] GameObject root;
        [SerializeField] GameObject root2;
        [SerializeField] GameObject contentsLock;
        [SerializeField] UILabelHelper contentsLockTitle;
        [SerializeField] UILabelHelper contentsLockDesc;
        [SerializeField] GameObject goLimitInfo;
        [SerializeField] UILabelHelper labelLimit;
        [SerializeField] GameObject tierUpFx;
        [SerializeField] float fxTime;

        private TierUpViewPresenter presenter;
        private ItemInfo curTargetItem;
        private int tierUpEquipmentItemCount;
        private bool isOwningNonEquipmentMaterials;
        private int needJobLevel;
        private List<ItemInfo> selectedMaterials;
        private bool isSendRequest;

        public void OnInit()
        {
            presenter = new TierUpViewPresenter();

            presenter.OnTierUp += OnTierUp;

            presenter.AddEvent();
        }

        public void OnClose()
        {
            presenter.OnTierUp -= OnTierUp;
            presenter.RemoveEvent();
        }

        public void OnShow()
        {
            isSendRequest = false;
            tierUpFx.SetActive(false);
            materialListPanelRoot.SetActive(true);
            selectNotice.gameObject.SetActive(true);
            selectNotice.LocalKey = LocalizeKey._28040;

            requestButton.LocalKey = LocalizeKey._28034;
            listPanelLabel.LocalKey = LocalizeKey._28041;
            titleLabel.LocalKey = LocalizeKey._28054;

            statChangePanels[0].SetLabel(LocalizeKey._4011); // ATK
            statChangePanels[1].SetLabel(LocalizeKey._4013); // MATK
            statChangePanels[2].SetLabel(LocalizeKey._4012); // DEF
            statChangePanels[3].SetLabel(LocalizeKey._4014); // MDEF

            for (int i = 0; i < statChangePanels.Length; ++i)
                statChangePanels[i].SetActive(false);

            EventDelegate.Add(selectTargetButton.OnClick, ShowSelectTargetItem);
            EventDelegate.Add(requestButton.OnClick, TrySendRequest);

            for (int i = 0; i < hextechSlots.Length; ++i)
                hextechSlots[i].Init(i, OnClickHextechSlot);

            DeselectTargetItem();
            SetContentsOpen(presenter.IsContentsOpen());
        }

        public void OnHide()
        {
            EventDelegate.Remove(selectTargetButton.OnClick, ShowSelectTargetItem);
            EventDelegate.Remove(requestButton.OnClick, TrySendRequest);
        }

        private void ShowSelectTargetItem()
        {
            DeselectTargetItem();

            UI.Show<UIItemSelect>(new UIItemSelect.Input()
            {
                filter = TierUp_Filter,
                sort = TierUp_Sort,
                targetSelectionCount = 1,
                onSelectionFinished = OnTargetItemSelected,
                preSelection = StopTargetSelection
            });
        }

        private void DeselectTargetItem()
        {
            curTargetItem = null;
            targetEquipItem.gameObject.SetActive(false);
            HideAllHexTechSlots();
            selectNotice.gameObject.SetActive(true);
            tierUpEquipmentItemCount = 0;
            isOwningNonEquipmentMaterials = false;
            selectedMaterials = null;
            for (int i = 0; i < statChangePanels.Length; ++i)
                statChangePanels[i].SetActive(false);
            UpdateRequestButtonEnable();

            needJobLevel = 0;
            NGUITools.SetActive(goLimitInfo, false);
        }

        private void OnTargetItemSelected(List<ItemInfo> selected)
        {
            curTargetItem = selected[0];
            var item = curTargetItem as EquipmentItemInfo;
            EquipmentItemInfo itemTierUpPreview = new EquipmentItemInfo(Entity.player.Inventory);
            itemTierUpPreview.SetData(ItemDataManager.Instance.Get(curTargetItem.ItemId));
            itemTierUpPreview.SetItemInfo(curTargetItem.Tier, curTargetItem.Smelt, (byte)curTargetItem.EquippedSlotType,
                item.EquippedCardNo1, item.EquippedCardNo2, item.EquippedCardNo3, item.EquippedCardNo4, false, curTargetItem.ItemTranscend + 1, curTargetItem.ItemChangedElement, curTargetItem.ElementLevel);

            selectNotice.gameObject.SetActive(false);

            targetEquipItem.gameObject.SetActive(true);
            targetEquipItem.SetData(curTargetItem);

            int tierUpCount = curTargetItem.ItemTranscend;
            int classBitType = (int)curTargetItem.ClassType;
            TierUpData tierUpData = ItemTierUpDataManager.Instance.Get(tierUpCount, classBitType);

            if (tierUpData == null)
            {
                selectNotice.gameObject.SetActive(true);
                selectNotice.LocalKey = LocalizeKey._28042;

                needJobLevel = 0;
                NGUITools.SetActive(goLimitInfo, false);
            }
            else
            {
                var prevStatus = curTargetItem.GetStatus();
                var nextStatus = itemTierUpPreview.GetStatus();

                if (prevStatus.atk > 0)
                {
                    statChangePanels[0].SetActive(true);
                    statChangePanels[0].SetData(prevStatus.atk, nextStatus.atk);
                }

                if (prevStatus.matk > 0)
                {
                    statChangePanels[1].SetActive(true);
                    statChangePanels[1].SetData(prevStatus.matk, nextStatus.matk);
                }

                if (prevStatus.def > 0)
                {
                    statChangePanels[2].SetActive(true);
                    statChangePanels[2].SetData(prevStatus.def, nextStatus.def);
                }

                if (prevStatus.mdef > 0)
                {
                    statChangePanels[3].SetActive(true);
                    statChangePanels[3].SetData(prevStatus.mdef, nextStatus.mdef);
                }

                int nextIndex = 0;

                tierUpEquipmentItemCount = tierUpData.equipment_item_count;

                if (tierUpEquipmentItemCount > 0)
                {
                    hextechSlots[nextIndex++].Show()
                        .SetIcon(curTargetItem)
                        .SetRequiredCount(0, tierUpEquipmentItemCount)
                        .ShowPlus(UIHextechSlot.PlusPos.UpperRight);
                }

                isOwningNonEquipmentMaterials = true;
                foreach (var each in tierUpData.GetMaterials())
                {
                    var itemData = ItemDataManager.Instance.Get(each.Item1);
                    var curOwningCount = Entity.player.Inventory.GetItemCount(itemData.id);
                    if (curOwningCount < each.Item2)
                        isOwningNonEquipmentMaterials = false;
                    hextechSlots[nextIndex++].Show().SetIcon(itemData).SetRequiredCount(curOwningCount, each.Item2);
                }

                int nextTier = tierUpCount + 1;
                needJobLevel = presenter.NeedJobLevel(nextTier);
                NGUITools.SetActive(goLimitInfo, needJobLevel > 0);
                labelLimit.Text = LocalizeKey._28058.ToText() // JOB Lv이 부족하여 초월이 불가능합니다.\n(JOB Lv {LEVEL} 필요)
                    .Replace(ReplaceKey.LEVEL, needJobLevel);
            }

            hextechSlotGrid.Reposition();
            UpdateRequestButtonEnable();
        }

        private void OnClickHextechSlot(int index)
        {
            if (tierUpEquipmentItemCount > 0)
            {
                if (index == 0)
                {
                    selectedMaterials = null;
                    hextechSlots[0].Show()
                           .SetIcon(curTargetItem)
                           .SetRequiredCount(0, tierUpEquipmentItemCount)
                           .ShowPlus(UIHextechSlot.PlusPos.UpperRight);

                    UI.Show<UIItemSelect>(new UIItemSelect.Input()
                    {
                        filter = TierUp_Mat_Filter,
                        sort = TierUp_Sort,
                        targetSelectionCount = tierUpEquipmentItemCount,
                        onSelectionFinished = OnSelectTierUpMat,
                        preSelection = StopCardEquippedItemSelection
                    });
                }
                else
                {
                    if (hextechSlots[index].ShowingItemData != null)
                    {
                        var info = new PartsItemInfo();
                        info.SetData(hextechSlots[index].ShowingItemData);
                        UI.Show<UIPartsInfo>(info);
                    }
                }
            }
        }

        public void ResetMaterial()
        {
            var cur = curTargetItem;
            DeselectTargetItem();
            OnTargetItemSelected(new List<ItemInfo>() { cur });
        }

        private void OnSelectTierUpMat(List<ItemInfo> selected)
        {
            selectedMaterials = selected;
            hextechSlots[0].SetRequiredCount(tierUpEquipmentItemCount, tierUpEquipmentItemCount);
            UpdateRequestButtonEnable();
        }

        private void TrySendRequest()
        {
            if (!CheckCondition())
                return;

            presenter.RequestTierUp(curTargetItem, selectedMaterials);
        }

        private void UpdateRequestButtonEnable()
        {
            requestButton.IsEnabled = CheckCondition();
        }

        private bool CheckCondition()
        {
            if (isSendRequest)
                return false;

            // 초월 필요 Job레벨
            if (needJobLevel > 0)
                return false;

            return curTargetItem != null && isOwningNonEquipmentMaterials && (tierUpEquipmentItemCount == 0 || selectedMaterials != null);
        }

        private void HideAllHexTechSlots()
        {
            for (int i = 0; i < hextechSlots.Length; ++i)
                hextechSlots[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// 초월 장비 필터
        /// </summary>
        private bool TierUp_Filter(ItemInfo itemInfo)
        {
            return itemInfo.ItemGroupType == ItemGroupType.Equipment && itemInfo.Rating >= 5 && !itemInfo.IsShadow;
        }

        /// <summary>
        /// 초월 재료 필터
        /// </summary>
        private bool TierUp_Mat_Filter(ItemInfo itemInfo)
        {
            // 초월재료 포함 - 무기
            var isBradium = curTargetItem.ClassType.HasFlag(EquipmentClassType.OneHandedSword) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.OneHandedStaff) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.Dagger) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.Bow) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.TwoHandedSword) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.TwoHandedSpear);
            if (isBradium)
            {
                if (itemInfo.ItemId == BasisItem.BeyondSword.GetID())
                    return true;
            }

            // 초월재료 포함 - 방어구
            var isOradium = curTargetItem.ClassType.HasFlag(EquipmentClassType.HeadGear) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.Garment) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.Armor) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.Accessory1) ||
                curTargetItem.ClassType.HasFlag(EquipmentClassType.Accessory2);
            if (isOradium)
            {
                if (itemInfo.ItemId == BasisItem.BeyondArmor.GetID())
                    return true;
            }

            // 초월 횟수가 0인 것만 세팅
            var tierZero = itemInfo.ItemId == curTargetItem.ItemId && itemInfo.ItemNo != curTargetItem.ItemNo && itemInfo.ItemTranscend == 0;
            return tierZero;
        }

        private int TierUp_Sort(ItemInfo a, ItemInfo b)
        {
            return a.ItemId - b.ItemId;
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

            // 초월 장비는 거래 불가 & 초월 재료는 거래 가능
            if(!curTargetItem.CanTrade && item.CanTrade)
            {
                return await UI.SelectPopup(LocalizeKey._90309.ToText()); // 초월 재료 아이템 경고 문구
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

                contentsLockTitle.Text = LocalizeKey._54310.ToText(); // 장비 초월 잠금 해제
                var scenario = ScenarioMazeDataManager.Instance.GetByContents(ContentType.TierUp);
                contentsLockDesc.Text = LocalizeKey._54306.ToText().Replace(ReplaceKey.NAME, scenario.name_id.ToText());
            }
        }

        /// <summary>
        /// 장비 초월 결과
        /// </summary>
        private void OnTierUp(bool isSuccess)
        {
            isSendRequest = false;

            if (!isSuccess)
                return;

            Timing.RunCoroutineSingleton(YieldTierUpEffect().CancelWith(gameObject), nameof(YieldTierUpEffect), SingletonBehavior.Overwrite);
        }

        /// <summary>
        ///  연출 후 갱신
        /// </summary>
        private IEnumerator<float> YieldTierUpEffect()
        {
            tierUpFx.SetActive(false);
            tierUpFx.SetActive(true);

            yield return Timing.WaitForSeconds(fxTime);

            tierUpFx.SetActive(false);
            UI.ShowToastPopup(LocalizeKey._28052.ToText());
            ResetMaterial();
        }
    }
}
