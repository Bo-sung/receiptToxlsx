using Ragnarok.View.EquipmentView;

using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEquipmentInfo : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] EquipmentInfoView equipmentInfoView;
        [SerializeField] EquipmentInfoBattleOptionView equipmentInfoBattleOptionView;

        [SerializeField] UICostButtonHelper btnLevelUp;
        [SerializeField] GameObject levelUpLock;
        [SerializeField] UITextureHelper materialIcon;
        [SerializeField] UIButtonHelper btnEquip;
        [SerializeField] UIButtonHelper btnLock;
        [SerializeField] UIButtonHelper btnChange;
        [SerializeField] UIButtonHelper btnClose;

        [SerializeField] GameObject[] fxLevelUp;
        [SerializeField] UIWidget itemEnchantWidget;
        [SerializeField] UIWidget cardEquipWidget;
        [SerializeField] UIWidget cardAutoEquipWidget;
        [SerializeField] UIWidget[] cardEnchantWidgets;
        [SerializeField] GameObject fxBtnLevelUp;
        [SerializeField] TweenScale tweenLevelUp;

        public delegate void SelectCardSlotEvent(byte slotIndex, CardSlotEvent cardSlotEvent);

        EquipmentInfoPresenter presenter;

        public UIWidget ItemEnchantWidget { get { return itemEnchantWidget; } }
        public UIWidget CardEquipWidget { get { return cardEquipWidget; } }
        public UIWidget CardAutoEquipWidget { get { return cardAutoEquipWidget; } }
        public ItemInfo CurItem { get { return presenter.GetEquipment(); } }
        public UIWidget FirstCardEnchantWidget
        {
            get
            {
                var curItem = CurItem;
                EquipmentItemInfo asEquip = curItem as EquipmentItemInfo;

                if (asEquip == null)
                    return null;

                int firstCardIndex = asEquip.GetFirstEquippedCardIndex();
                if (firstCardIndex == -1)
                    return null;

                return cardEnchantWidgets[firstCardIndex];
            }
        }

        protected override void OnInit()
        {
            presenter = new EquipmentInfoPresenter();

            EventDelegate.Add(background.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnLevelUp.OnClick, presenter.OnClickedBtnLevelUp);
            EventDelegate.Add(btnEquip.OnClick, presenter.OnClickedBtnEquip);
            EventDelegate.Add(btnLock.OnClick, presenter.OnClickedBtnLock);
            EventDelegate.Add(btnChange.OnClick, OnClickedBtnChange);
            EventDelegate.Add(btnClose.OnClick, OnClickedBtnClose);

            equipmentInfoView.OnSelectSource += OnSelectSource;
            equipmentInfoBattleOptionView.OnSelect += presenter.OnSelectCardSlot;

            presenter.OnUpdateItem += UpdateItem;
            presenter.OnEquipItemLevelUp += OnEquipItemLevelUp;
            presenter.OnUpdateEquipment += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(background.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnLevelUp.OnClick, presenter.OnClickedBtnLevelUp);
            EventDelegate.Remove(btnEquip.OnClick, presenter.OnClickedBtnEquip);
            EventDelegate.Remove(btnLock.OnClick, presenter.OnClickedBtnLock);
            EventDelegate.Remove(btnChange.OnClick, OnClickedBtnChange);
            EventDelegate.Remove(btnClose.OnClick, OnClickedBtnClose);

            equipmentInfoView.OnSelectSource -= OnSelectSource;
            equipmentInfoBattleOptionView.OnSelect -= presenter.OnSelectCardSlot;

            presenter.OnUpdateItem -= UpdateItem;
            presenter.OnEquipItemLevelUp -= OnEquipItemLevelUp;
            presenter.OnUpdateEquipment -= CloseUI;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_ItemEnchant(); // 신규 컨텐츠 플래그 제거

            levelUpLock.SetActive(!Entity.player.Quest.IsOpenContent(ContentType.ItemEnchant, false));
            equipmentInfoBattleOptionView.ShowSlotView(true);
        }

        protected override void OnHide()
        {
            HideEffectLevelUp();
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._16000; // 아이템 정보
            btnLevelUp.LocalKey = LocalizeKey._16011; // 강화
            btnClose.LocalKey = LocalizeKey._4100; // 닫기
        }

        void CloseUI()
        {
            UI.Close<UIEquipmentInfo>();
        }

        public void Set(long equipmentNo, InventoryModel invenModel = null)
        {
            if (equipmentNo == 0L)
            {
                CloseUI();
                return;
            }
            presenter.SetOthersInventoryModel(invenModel);
            presenter.SetEquipmentNo(equipmentNo);
            UpdateEquipmentView();
            UpdateEquipmentBattleOptionView();
            UpdateButtonInfo();
            equipmentInfoBattleOptionView.ResetPosition();
        }

        void UpdateItem()
        {
            UpdateEquipmentView();
            UpdateEquipmentBattleOptionView();
            UpdateButtonInfo();
        }

        void UpdateEquipmentView()
        {
            ItemInfo info = presenter.GetEquipment();
            if (info == null)
                return;

            equipmentInfoView.SetData(info);
        }

        void UpdateEquipmentBattleOptionView()
        {
            ItemInfo info = presenter.GetEquipment();
            if (info == null)
                return;

            equipmentInfoBattleOptionView.SetData(info, presenter.IsEditableEquipment(), presenter.GetShadowCardOpenSlotItemIconName(), presenter.GetShadowCardOpenSlotItemName());
        }

        void UpdateButtonInfo()
        {
            bool isEditable = presenter.IsEditableEquipment();
            // 편집 불가능한 장비인 경우 버튼 숨기기
            btnEquip.SetActive(isEditable);
            btnLevelUp.SetActive(isEditable);
            btnLock.SetActive(isEditable);
            btnClose.SetActive(!isEditable);

            if (!isEditable)
                return;

            ItemInfo info = presenter.GetEquipment();
            if (info == null)
                return;

            // 초월재료용 장비인 경우
            bool isMaterial = info.ClassType.HasFlag(EquipmentClassType.Weapon) || info.ClassType.HasFlag(EquipmentClassType.AllArmor);
            if (isMaterial)
            {
                btnEquip.SetActive(false);
                btnLevelUp.SetActive(false);
                btnClose.SetActive(true);
                return;
            }

            btnEquip.Text = info.IsEquipped ? LocalizeKey._16001.ToText() : LocalizeKey._16008.ToText(); // 장착

            // 강화 최대치 일때 강화버튼 끄기
            btnLevelUp.SetActive(!info.IsMaxSmelt);

            if (info.IsMaxSmelt)
                return;

            bool canLevelUp = presenter.CanLevelUp();
            RewardData material = presenter.GetMeterialItem();

            if (material != null)
            {
                materialIcon.Set(material.IconName);
                btnLevelUp.CostText = material.Count.ToString("N0");
            }

            fxBtnLevelUp.SetActive(canLevelUp);
            tweenLevelUp.tweenFactor = 1;
            tweenLevelUp.enabled = canLevelUp;
        }

        void OnSelectSource(UIItemSource.Mode mode)
        {
            ItemInfo info = presenter.GetEquipment();
            if (info == null)
                return;

            UI.Show<UIItemSource>(new UIItemSource.Input(mode, info));
        }

        private void OnEquipItemLevelUp(bool isLock, int smelt)
        {
            HideEffectLevelUp();

            if (isLock)
            {
            }
            else
            {
                ShowEffectLevelUp(smelt);
                equipmentInfoView.PlayTween();
            }
        }

        void OnClickedBtnChange()
        {
            equipmentInfoBattleOptionView.ToggleSlotView();
        }

        void OnClickedBtnClose()
        {
            CloseUI();
        }

        public bool CanEquipmentLevelUp()
        {
            return presenter.CanEquipmentLevelUp();
        }

        public bool IsRequestEquipmentLevelUp()
        {
            return presenter.IsRequestEquipmentLevelUp();
        }

        private void HideEffectLevelUp()
        {
            foreach (var item in fxLevelUp)
            {
                item.SetActive(false);
            }
        }

        private void ShowEffectLevelUp(int smelt)
        {
            int index = Mathf.Clamp(GetEffectIndex(smelt), 0, fxLevelUp.Length - 1);
            fxLevelUp[index].SetActive(true);
        }

        private int GetEffectIndex(int smelt)
        {
            if (smelt < 10)
                return 0;

            if (smelt < 20)
                return 1;

            if (smelt < 30)
                return 2;

            if (smelt < 40)
                return 3;

            return 4;
        }
    }
}