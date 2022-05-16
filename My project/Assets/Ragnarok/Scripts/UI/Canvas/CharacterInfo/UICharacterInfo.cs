using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICharacterInfo : UICanvas<CharacterInfoPresenter>, CharacterInfoPresenter.IView, IInspectorFinder
    {
        public delegate void ActiveEvent(bool isActive);

        public static bool IsActive { get; private set; }
        public static event ActiveEvent OnActive;

        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single | UIType.Reactivation;

        public enum TabType
        {
            Ability = 0,
            Ribirth = 1,
            Profile = 2,
        }

        public enum ItemTabType
        {
            Equipment = 0,
            Shadow = 1,
            Costume = 2,
        }

        public static TabType tabType = TabType.Ability;

        // 타이틀
        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelHelper[] labelTabOffLabels;
        [SerializeField] UIButtonHelper btnExit;

        [SerializeField] UILabelHelper labPowerValue;
        [SerializeField] UILabelHelper labUpValue;
        [SerializeField] UIPlayTween playUpValue;

        [SerializeField] UIButtonWithToggle btnEquipmentView;
        [SerializeField] UIButtonWithToggle btnShadowView;
        [SerializeField] UIButtonWithToggle btnCostumeView;
        [SerializeField] GameObject equipmentView;
        [SerializeField] GameObject shadowView;
        [SerializeField] GameObject costumeView;

        [SerializeField] EquipmentViewSlot[] equipmentSlots;
        [SerializeField] CostumeViewSlot[] costumeSlots;

        [SerializeField] CostumeTitleView costumeTitleView;

        CameraController cameraController;

        protected override void OnInit()
        {
            presenter = new CharacterInfoPresenter(this);
            cameraController = CameraController.Instance;

            presenter.AddEvent();
            EventDelegate.Add(tab.OnChange[TabType.Ability.ToIntValue()], OnClickedTabAbility);
            EventDelegate.Add(tab.OnChange[TabType.Ribirth.ToIntValue()], OnClickEdTabRebirth);
            EventDelegate.Add(tab.OnChange[TabType.Profile.ToIntValue()], OnClickEdTabProfile);
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);

            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].OnSelect += OnSelectEquipment;
            }
            for (int i = 0; i < costumeSlots.Length; i++)
            {
                costumeSlots[i].OnSelect += OnSelectCostume;
            }
            costumeTitleView.OnSelect += OnSelectCostume;

            EventDelegate.Add(btnEquipmentView.OnClick, OnClickedBtnEquipmentView);
            EventDelegate.Add(btnShadowView.OnClick, OnClickedBtnShadowView);
            EventDelegate.Add(btnCostumeView.OnClick, OnClickedBtnCostumeView);

            GetProfileSubcanvas().Initialize(isEditable: true);
            GetAbilitySubcanvas().Initialize(isEditable: true);
        }

        protected override void OnClose()
        {
            OnHide();

            presenter.RemoveEvent();
            EventDelegate.Remove(tab.OnChange[TabType.Ability.ToIntValue()], OnClickedTabAbility);
            EventDelegate.Remove(tab.OnChange[TabType.Ribirth.ToIntValue()], OnClickEdTabRebirth);
            EventDelegate.Remove(tab.OnChange[TabType.Profile.ToIntValue()], OnClickEdTabProfile);
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);

            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].OnSelect -= OnSelectEquipment;
            }
            for (int i = 0; i < costumeSlots.Length; i++)
            {
                costumeSlots[i].OnSelect -= OnSelectCostume;
            }

            EventDelegate.Remove(btnEquipmentView.OnClick, OnClickedBtnEquipmentView);
            EventDelegate.Remove(btnShadowView.OnClick, OnClickedBtnShadowView);
            EventDelegate.Remove(btnCostumeView.OnClick, OnClickedBtnCostumeView);
        }

        protected override void OnShow(IUIData data = null)
        {
            cameraController.SetClearshot(CameraController.Clearshot.Inven);
            UI.HideHUD();
            UI.RemoveMask(Layer.UI_ExceptForCharZoom);

            GetAbilitySubcanvas().SetPlayer(presenter.GetPlayer());
            GetProfileSubcanvas().SetPlayer(presenter.GetPlayer());

            tab[(int)tabType].Value = true;

            presenter.OnUpdateAP();

            SetEquippedItemView(ItemTabType.Equipment);
            ShowEquippedItemSlot();

            IsActive = true;
            OnActive?.Invoke(IsActive);

            subCanvas[tabType.ToIntValue()].Show();
        }

        protected override void OnHide()
        {
            cameraController.SetClearshot(CameraController.Clearshot.None);
            UI.ShowHUD();
            UI.AddMask(Layer.UI_ExceptForCharZoom);

            IsActive = false;
            OnActive?.Invoke(IsActive);
            ContentAbility ability = subCanvas[TabType.Ability.ToIntValue()] as ContentAbility;
            ability.HideAbilityPopup();
        }

        protected override void OnBack()
        {
            if (GetAbilitySubcanvas().IsActiveAbilityPopup())
            {
                GetAbilitySubcanvas().HideAbilityPopup();
                return;
            }

            if (GetProfileSubcanvas().IsActiveNameChangeView())
            {
                GetProfileSubcanvas().HideNameChangeView();
                return;
            }

            base.OnBack();
        }

        protected override void OnLocalize()
        {
            // 타이틀
            labTitle.LocalKey = LocalizeKey._4000; // 영웅
            labelTabOffLabels[0].LocalKey = tab[0].LocalKey = LocalizeKey._4001; // 능력치
            labelTabOffLabels[1].LocalKey = tab[1].LocalKey = LocalizeKey._4003; // 전승
            labelTabOffLabels[2].LocalKey = tab[2].LocalKey = LocalizeKey._31003; // 프로필
            btnEquipmentView.LocalKey = LocalizeKey._4055; // 장비
            btnShadowView.LocalKey = LocalizeKey._4058; // 쉐도우
            btnCostumeView.LocalKey = LocalizeKey._4056; // 코스튬
        }

        #region 탭버튼 이벤트

        void HideAllContent()
        {
            foreach (var item in subCanvas)
            {
                item.Hide();
            }
        }

        // 능력치 탭버튼
        void OnClickedTabAbility()
        {
            if (!UIToggle.current.value) return;
            HideAllContent();
            tabType = TabType.Ability;
            subCanvas[tabType.ToIntValue()].Show();
        }

        // 전승 탭버튼
        void OnClickEdTabRebirth()
        {
            if (!UIToggle.current.value) return;
            HideAllContent();
            tabType = TabType.Ribirth;
            subCanvas[tabType.ToIntValue()].Show();
        }

        void OnClickEdTabProfile()
        {
            if (!UIToggle.current.value) return;
            HideAllContent();
            tabType = TabType.Profile;
            subCanvas[tabType.ToIntValue()].Show();
        }

        // 닫기 버튼
        void OnClickedBtnExit()
        {
            UI.Close<UICharacterInfo>();
        }

        /// <summary>
        /// 장비 가방 오픈
        /// </summary>
        /// <param name="slotType"></param>
        private void OnSelectEquipment(ItemEquipmentSlotType slotType)
        {
            UI.Show<UIEquipmentInven>(new UIEquipmentInvenData(slotType));
        }

        /// <summary>
        /// 코스튬 가방 오픈
        /// </summary>
        /// <param name="obj"></param>
        private void OnSelectCostume(ItemEquipmentSlotType slotType)
        {
            UI.Show<UICostumeInven>().Set(slotType);
        }

        #endregion

        public ContentAbility GetAbilitySubcanvas()
        {
            return subCanvas[(int)TabType.Ability] as ContentAbility;
        }

        public ContentProfile GetProfileSubcanvas()
        {
            return subCanvas[(int)TabType.Profile] as ContentProfile;
        }

        public void ShowEquippedItemSlot()
        {
            if (!gameObject.activeInHierarchy)
                return;

            // 장비 슬롯 정보 갱신
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                ItemEquipmentSlotType slotType = equipmentSlots[i].GetSlotType();
                ItemInfo item = presenter.GetEquippedItem(slotType);
                bool canEquip = presenter.CanEquip(slotType);
                bool hasStronger = presenter.HasStrongerEquipment(slotType);
                equipmentSlots[i].Set(item, canEquip, hasStronger);
            }

            // 코스튬 슬롯 정보 갱신
            for (int i = 0; i < costumeSlots.Length; i++)
            {
                ItemEquipmentSlotType slotType = costumeSlots[i].GetSlotType();
                ItemInfo item = presenter.GetEquippedItem(slotType);
                bool canEquip = presenter.EquipableCostume(slotType);
                costumeSlots[i].Set(new UICostumeInfoSlot.Info(item, presenter), canEquip);

                // 장착 무기와 장착 무기코스튬 타입이 다를때 미사용 표시
                if (slotType == ItemEquipmentSlotType.CostumeWeapon)
                {
                    if (item == null)
                    {
                        costumeSlots[i].SetInvisible(false);
                    }
                    else
                    {
                        ItemInfo weapon = presenter.GetEquippedItem(ItemEquipmentSlotType.Weapon);
                        EquipmentClassType weaponType = weapon == null ? EquipmentClassType.OneHandedSword : weapon.ClassType;

                        EquipmentClassType costumeWeaponType = item.CostumeType.ToWeaponType();
                        bool isInvisible = !costumeWeaponType.Equals(weaponType);
                        costumeSlots[i].SetInvisible(isInvisible);
                    }
                }
                else if (slotType == ItemEquipmentSlotType.CostumeBody)
                {
                    if (item == null)
                    {
                        costumeSlots[i].SetInvisible(false);
                    }
                    else
                    {
                        Gender gender = presenter.GetPlayer().Character.Gender;
                        costumeSlots[i].SetInvisible(!item.CostumeBodyType.IsInvisible(gender));
                    }
                }
                else
                {
                    costumeSlots[i].SetInvisible(false);
                }
            }

            // 칭호
            {
                ItemInfo item = presenter.GetEquippedItem(ItemEquipmentSlotType.CostumeTitle);
                bool canEquip = presenter.EquipableCostume(ItemEquipmentSlotType.CostumeTitle);
                bool isEmpty = item == null;
                costumeTitleView.SetEmpty(isEmpty);
                costumeTitleView.SetNotice(canEquip);
                if (!isEmpty)
                {
                    costumeTitleView.Set(item.IconName, item.CostumeTitleID);
                }
            }
        }

        UILabelHelper CharacterInfoPresenter.IView.GetAPLabel()
        {
            return labPowerValue;
        }

        UIPlayTween CharacterInfoPresenter.IView.GetUpValuePlay()
        {
            return playUpValue;
        }

        UILabelHelper CharacterInfoPresenter.IView.GetUpValueLabel()
        {
            return labUpValue;
        }

        void CharacterInfoPresenter.IView.SetStrongerEquipmentNotice()
        {
            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                ItemEquipmentSlotType slotType = equipmentSlots[i].GetSlotType();
                bool hasStronger = presenter.HasStrongerEquipment(slotType);
                equipmentSlots[i].SetStrongerEquipmentNotice(hasStronger);
            }
        }

        private void SetEquippedItemView(ItemTabType itemTabType)
        {
            switch (itemTabType)
            {
                case ItemTabType.Equipment:
                    btnEquipmentView.SetToggle(true);
                    equipmentView.SetActive(true);
                    btnShadowView.SetToggle(false);
                    shadowView.SetActive(false);
                    btnCostumeView.SetToggle(false);
                    costumeView.SetActive(false);
                    break;

                case ItemTabType.Shadow:
                    btnEquipmentView.SetToggle(false);
                    equipmentView.SetActive(false);
                    btnShadowView.SetToggle(true);
                    shadowView.SetActive(true);
                    btnCostumeView.SetToggle(false);
                    costumeView.SetActive(false);
                    break;
                case ItemTabType.Costume:
                    btnEquipmentView.SetToggle(false);
                    equipmentView.SetActive(false);
                    btnShadowView.SetToggle(false);
                    shadowView.SetActive(false);
                    btnCostumeView.SetToggle(true);
                    costumeView.SetActive(true);
                    break;
            }

            btnEquipmentView.SetNotice(presenter.HasStrongerEquipment());
            btnShadowView.SetNotice(presenter.HasStrongerShadowEquipment());
            btnCostumeView.SetNotice(presenter.EquipableCostume());
        }

        void OnClickedBtnEquipmentView()
        {
            SetEquippedItemView(ItemTabType.Equipment);
        }

        void OnClickedBtnShadowView()
        {
            if (!BasisOpenContetsType.Shadow.IsOpend())
            {
                string message = LocalizeKey._90045.ToText(); // 업데이트 예정입니다.
                UI.ShowToastPopup(message);
                return;
            }

            SetEquippedItemView(ItemTabType.Shadow);
        }

        void OnClickedBtnCostumeView()
        {
            SetEquippedItemView(ItemTabType.Costume);
        }

        bool IInspectorFinder.Find()
        {
            equipmentSlots = GetComponentsInChildren<EquipmentViewSlot>();
            costumeSlots = GetComponentsInChildren<CostumeViewSlot>();
            return true;
        }
    }
}