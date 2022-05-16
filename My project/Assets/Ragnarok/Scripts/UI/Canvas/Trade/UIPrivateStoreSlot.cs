using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class UIPrivateStoreSlot : UIInfo<PrivateStorePresenter, PrivateStoreItemData>
    {
        [SerializeField] UICardProfile cardProfile;
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UIPartsProfile partsProfile;

        [SerializeField] UIButtonHelper btnIcon; // 등록됨: 아이템 정보 보기
        [SerializeField] UIButtonHelper btnAdd; // 아이템 추가
        [SerializeField] UICostButtonHelper btnPrice; // 등록됨&판매자: 회수하기 / 등록됨&구매자 : 구매하기

        [SerializeField] GameObject goNoneBase;
        [SerializeField] GameObject goActive; // 아이템 칸 자체의 액티브

        protected override void Awake()
        {
            base.Awake();

            if (btnIcon != null)
                EventDelegate.Add(btnIcon.OnClick, OnClickBtnIcon);

            if (btnPrice != null)
                EventDelegate.Add(btnPrice.OnClick, OnClickBtnPrice);

            if (btnAdd != null)
                EventDelegate.Add(btnAdd.OnClick, OnClickBtnAdd);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btnIcon != null)
                EventDelegate.Remove(btnIcon.OnClick, OnClickBtnIcon);

            if (btnPrice != null)
                EventDelegate.Remove(btnPrice.OnClick, OnClickBtnPrice);

            if (btnAdd != null)
                EventDelegate.Remove(btnAdd.OnClick, OnClickBtnAdd);
        }

        protected override void Refresh()
        {
            // 빈칸인 경우
            if (info == null)
            {
                NGUITools.SetActive(goActive, presenter.IsMyStore);

                if (!presenter.IsMyStore) // 구매자의 입장에서 빈 아이템은 빈칸으로 표기.
                    return;

                NGUITools.SetActive(goNoneBase, true);

                NGUITools.SetActive(cardProfile.gameObject, false);
                NGUITools.SetActive(equipmentProfile.gameObject, false);
                NGUITools.SetActive(partsProfile.gameObject, false);

                cardProfile.SetData(null);
                equipmentProfile.SetData(null);
                partsProfile.SetData(null);

                if (btnPrice != null)
                {
                    btnPrice.CostView(false);
                    btnPrice.SetCostCount(0);
                    btnPrice.TitleLocalKey = LocalizeKey._45007; // 등록하기
                }

                return;
            }

            NGUITools.SetActive(goActive, true);
            NGUITools.SetActive(goNoneBase, false);

            ItemGroupType type = info.ItemInfo.ItemGroupType;
            NGUITools.SetActive(cardProfile.gameObject, type == ItemGroupType.Card);
            NGUITools.SetActive(equipmentProfile.gameObject, type == ItemGroupType.Equipment);
            NGUITools.SetActive(partsProfile.gameObject, type == ItemGroupType.ProductParts);

            if (btnPrice != null)
            {
                btnPrice.CostView(true);
                btnPrice.SetCostCount(info.item_price);
                btnPrice.TitleText = string.Empty;
            }

            switch (type)
            {
                case ItemGroupType.Card:
                    cardProfile.SetData(info.ItemInfo as CardItemInfo);
                    break;
                case ItemGroupType.Equipment:
                    equipmentProfile.SetData(info.ItemInfo as EquipmentItemInfo);
                    break;
                case ItemGroupType.ProductParts:
                    partsProfile.SetData(info.ItemInfo as PartsItemInfo);
                    break;
            }
        }

        void OnClickBtnAdd()
        {
            if (info != null)
                return;

            if (!presenter.IsMyStore)
                return;

            AddItem().WrapNetworkErrors();
        }

        void OnClickBtnIcon()
        {
            if (info == null)
            {
                OnClickBtnAdd();
                return;
            }

            ShowItemInfo(info.ItemInfo);
        }

        void OnClickBtnPrice()
        {
            if (info == null)
            {
                OnClickBtnAdd();
                return;
            }

            if (presenter.IsMyStore)
            {
                RemoveItem(info).WrapNetworkErrors();
                return;
            }

            ShowPurchase(info);
        }

        private async Task AddItem()
        {
            /// TODO: 최대 등록 개수 초과 팝업
            /// TODO: 임시 코드
            bool isOverAllowCount = false;
            if (isOverAllowCount)
            {
                bool isYes = await UI.SelectPopup(LocalizeKey._45005.ToText()); // 현재 최대 등록 개수는 {VALUE}개 입니다.\n상점에서 추가 등록권을 구매 하시겠습니까?
                if (!isYes)
                    return;

                /// TODO: 추가등록권을 구매하기 위해 상점 팝업 띄우기.
                return;
            }

            UITradeStoreInvenSelectData param = new UITradeStoreInvenSelectData();
            param.exceptPrivateExist = true;
            UI.Show<UITradeStoreInvenSelect>(param);
        }

        private void ShowItemInfo(ItemInfo itemInfo)
        {
            switch (itemInfo.ItemGroupType)
            {
                case ItemGroupType.Card:
                    UI.Show<UICardInfoShop>().Show(itemInfo);
                    break;

                case ItemGroupType.ProductParts:
                    UI.Show<UIPartsInfo>(itemInfo);
                    break;

                case ItemGroupType.ConsumableItem:
                    UI.Show<UIConsumableInfo>(itemInfo);
                    break;

                case ItemGroupType.Equipment:
                    UI.Show<UIEquipmentInfoSimple>(itemInfo);
                    break;
            }
        }

        private async Task RemoveItem(PrivateStoreItemData info)
        {
            // 회수 팝업 띄우기
            bool isYes = await UI.SelectPopup(string.Empty, LocalizeKey._45215.ToText()); // 아이템을 회수 하시겠습니까?
            if (!isYes)
                return;

            if (!info.isVirtualRegister) // 가상이 아니라 서버에 등록된 아이템이면 회수 프로토콜을 보낸다.
            {
                if (!UI.CheckInvenWeight())
                {
                    UI.ConfirmPopup(LocalizeKey._5.ToText(), LocalizeKey._45216.ToText()); // 가방 무게가 부족하여 아이템 회수를 할 수 없습니다.\n가방 정리 후 다시 시도 해주세요.
                    return;
                }
                // 판매중이면 회수프로토콜을 보내야한다. 
                bool res = await presenter.RestoreItemToInvenWhenSelling(info);
                if (!res)
                    return;

                // 성공했으면 개인상점 인벤에서 제거
                presenter.RemoveFromRegisteredList(info);
            }
            else
            {
                // 회수하기
                // 인벤에 추가 -> 개인상점인벤에서 제거
                presenter.RestoreItemToInven(info);
            }

            presenter.Refresh();
        }

        private void ShowPurchase(PrivateStoreItemData info)
        {
            int spendRoPoint = info.itemData.point_value;

            // 장비 초월의 경우 구매시 필요 ROPoint 추가
            // 서버에서 itemTranscend값을 카드에서 사용중
            if (info.itemData.ItemGroupType == ItemGroupType.Equipment && info.itemTranscend > 0)
            {
                spendRoPoint += BasisType.TRANSCEND_ITEM_EXTRA_ROPOINT.GetInt(info.itemTranscend);
            }

            UI.Show<UIPrivateStoreProductSetting>().Show(itemInfo: info.ItemInfo,
                price: info.item_price,
                isMyStore: presenter.IsMyStore,
                lowestPriceLimit: info.itemData.price,
                maxPriceLimit: info.itemData.price_max_limit,
                spendRoPoint,
                CID: presenter.CID,
                info.Index);
        }
    }
}