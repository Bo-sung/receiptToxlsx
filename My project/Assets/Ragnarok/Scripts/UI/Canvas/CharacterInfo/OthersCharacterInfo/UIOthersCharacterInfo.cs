using UnityEngine;

namespace Ragnarok
{
    public sealed class UIOthersCharacterInfo : UICanvas<OthersCharacterInfoPresenter>, OthersCharacterInfoPresenter.IView, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;
        public override int layer => Layer.UI_Popup;

        public enum TabType
        {
            Profile = 0,
            Ability = 1,
            Skill = 2,
        }

        public enum SlotViewType
        {
            Normal,
            Shadow,
        }

        // 일반, 쉐도우 장비 텍스트 아웃라인 색상
        private readonly Color32 normalColor = new Color32(0, 44, 114, 255);
        private readonly Color32 shadowColor = new Color32(80, 64, 173, 255);

        // 최상단
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;

        // 캐릭터 뷰
        [SerializeField] UIUnitViewer unitViewer;
        [SerializeField] UIEquipmentSlot[] equipmentSlots;
        [SerializeField] UILabelHelper labelAP;

        // 탭 
        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelHelper[] labelOffTabLabel;

        // 하단
        [SerializeField] UIButtonHelper btnClose;

        [SerializeField] UIButtonWithIcon btnSlotChange;
        [SerializeField] GameObject goEquipBase;
        [SerializeField] GameObject goShadowBase;

        private TabType tabType = TabType.Profile;
        private SlotViewType slotViewType = SlotViewType.Normal;

        protected override void OnInit()
        {
            presenter = new OthersCharacterInfoPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);

            EventDelegate.Add(tab.OnChange[TabType.Profile.ToIntValue()], OnClickedTabProfile);
            EventDelegate.Add(tab.OnChange[TabType.Ability.ToIntValue()], OnClickedTabAbility);
            EventDelegate.Add(tab.OnChange[TabType.Skill.ToIntValue()], OnClickedTabSkill);

            EventDelegate.Add(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Add(btnSlotChange.OnClick, OnClickedBtnSlotChange);

            GetProfileSubcanvas().Initialize(isEditable: false);
            GetAbilitySubcanvas().Initialize(isEditable: false);
        }

        protected override void OnClose()
        {
            OnHide();
            presenter.Dispose();
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);

            EventDelegate.Remove(tab.OnChange[TabType.Profile.ToIntValue()], OnClickedTabProfile);
            EventDelegate.Remove(tab.OnChange[TabType.Ability.ToIntValue()], OnClickedTabAbility);
            EventDelegate.Remove(tab.OnChange[TabType.Skill.ToIntValue()], OnClickedTabSkill);

            EventDelegate.Remove(btnClose.OnClick, OnClickedBtnClose);
            EventDelegate.Remove(btnSlotChange.OnClick, OnClickedBtnSlotChange);
        }

        protected override void OnHide()
        {
            // 능력치 상세 팝업 Hide
            ContentAbility ability = GetAbilitySubcanvas();
            ability.HideAbilityPopup();
        }

        protected override void OnShow(IUIData data = null)
        {

        }

        public void Show(BattleCharacterPacket packet)
        {
            presenter.SetPlayer(packet); // 캐릭터 정보 입력

            // 미리보기 모델 출력
            unitViewer.Show(presenter.GetDummyUIPlayer());

            // 장비 슬롯 (투구 갑옷 무기 외투 악세1 악세2)
            foreach (var slot in equipmentSlots)
            {
                slot.SetOthersInventoryModel(presenter.GetInventoryModel());
            }

            equipmentSlots[0].SetData(presenter.GetEquipment(ItemEquipmentSlotType.HeadGear));
            equipmentSlots[1].SetData(presenter.GetEquipment(ItemEquipmentSlotType.Armor));
            equipmentSlots[2].SetData(presenter.GetEquipment(ItemEquipmentSlotType.Weapon));
            equipmentSlots[3].SetData(presenter.GetEquipment(ItemEquipmentSlotType.Garment));
            equipmentSlots[4].SetData(presenter.GetEquipment(ItemEquipmentSlotType.Accessory1));
            equipmentSlots[5].SetData(presenter.GetEquipment(ItemEquipmentSlotType.Accessory2));

            equipmentSlots[6].SetData(presenter.GetEquipment(ItemEquipmentSlotType.ShadowHeadGear));
            equipmentSlots[7].SetData(presenter.GetEquipment(ItemEquipmentSlotType.ShadowArmor));
            equipmentSlots[8].SetData(presenter.GetEquipment(ItemEquipmentSlotType.ShadowWeapon));
            equipmentSlots[9].SetData(presenter.GetEquipment(ItemEquipmentSlotType.ShadowGarment));
            equipmentSlots[10].SetData(presenter.GetEquipment(ItemEquipmentSlotType.ShadowAccessory1));
            equipmentSlots[11].SetData(presenter.GetEquipment(ItemEquipmentSlotType.ShadowAccessory2));

            for (int i = 0; i < equipmentSlots.Length; i++)
            {
                equipmentSlots[i].OnClickedEvent(ShowSimpleEquipmentInfo);
            }

            // 전투력
            var sb = StringBuilderPool.Get();
            sb.Append(LocalizeKey._4047.ToText()); // 전투력 :
            sb.Append(" ");
            sb.Append(presenter.GetBattleScore());
            labelAP.Text = sb.Release();

            // 프로필
            GetProfileSubcanvas().SetPlayer(presenter.GetPlayer());

            // 능력치
            GetAbilitySubcanvas().SetPlayer(presenter.GetPlayer());

            // 스킬
            GetSkillSubcanvas().SetPlayer(presenter.GetPlayer());

            tab[(int)tabType].Value = true;
            Refresh();
            UpdateBtnSlotChange();
        }

        private void ShowSimpleEquipmentInfo(ItemInfo itemInfo)
        {
            if (itemInfo == null)
                return;

            UI.Show<UIEquipmentInfoSimple>(itemInfo);
        }

        protected override void OnLocalize()
        {
            // 탭
            labelOffTabLabel[0].LocalKey = tab[0].LocalKey = LocalizeKey._31003; // 프로필
            labelOffTabLabel[1].LocalKey = tab[1].LocalKey = LocalizeKey._4001; // 능력치
            labelOffTabLabel[2].LocalKey = tab[2].LocalKey = LocalizeKey._4002; // 스킬

            // 최하단
            btnClose.LocalKey = LocalizeKey._4100; // 닫기
        }


        private void Refresh()
        {
            // 최상단
            labelTitle.Text = presenter.GetNickname();

            // SubCanvas Refresh (OnUpdateEvent가 없기 때문에 직접 Refresh 필요)
            RefreshSubCanvas(tabType);
            return;

            void RefreshSubCanvas(TabType tabType)
            {
                switch (tabType)
                {
                    case TabType.Profile: GetProfileSubcanvas().Refresh(); break;
                    case TabType.Ability: GetAbilitySubcanvas().Refresh(); break;
                    case TabType.Skill: GetSkillSubcanvas().Refresh(); break;
                }
            }
        }

        // 프로필 탭버튼
        void OnClickedTabProfile()
        {
            if (!UIToggle.current.value) return;
            HideAllContent();
            tabType = TabType.Profile;
            subCanvas[tabType.ToIntValue()].Show();
        }

        // 능력치 탭버튼
        void OnClickedTabAbility()
        {
            if (!UIToggle.current.value) return;
            HideAllContent();
            tabType = TabType.Ability;
            subCanvas[tabType.ToIntValue()].Show();
        }

        // 스킬 탭버튼
        void OnClickedTabSkill()
        {
            if (!UIToggle.current.value) return;
            HideAllContent();
            tabType = TabType.Skill;
            subCanvas[tabType.ToIntValue()].Show();
        }

        private void HideAllContent()
        {
            foreach (var item in subCanvas)
            {
                item.Hide();
            }
        }

        // 무기 버튼
        void OnClickedEquipmentWeapon()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.Weapon);
        }

        // 갑옷 버튼
        void OnClickedEquipmentArmor()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.Armor);
        }

        // 투구 버튼
        void OnClickedEquipmentHeadGear()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.HeadGear);
        }

        // 망토 버튼
        void OnClickedEquipmentGarment()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.Garment);
        }

        // 장신구1 버튼
        void OnClickedEquipmentAccessory1()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.Accessory1);
        }

        // 장신구2 버튼
        void OnClickedEquipmentAccessory2()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.Accessory2);
        }

        // 무기 버튼
        void OnClickedEquipmentShadowWeapon()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.ShadowWeapon);
        }

        // 갑옷 버튼
        void OnClickedEquipmentShadowArmor()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.ShadowArmor);
        }

        // 투구 버튼
        void OnClickedEquipmentShadowHeadGear()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.ShadowHeadGear);
        }

        // 망토 버튼
        void OnClickedEquipmentShadowGarment()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.ShadowGarment);
        }

        // 장신구1 버튼
        void OnClickedEquipmentShadowAccessory1()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.ShadowAccessory1);
        }

        // 장신구2 버튼
        void OnClickedEquipmentShadowAccessory2()
        {
            presenter.ShowEquipmentInfo(ItemEquipmentSlotType.ShadowAccessory2);
        }

        void OnClickedBtnExit()
        {
            CloseUI();
        }

        void OnClickedBtnClose()
        {
            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIOthersCharacterInfo>();
        }

        /// <summary>
        /// 장비 슬롯 타입 변경 (일반 <-> 쉐도우)
        /// </summary>
        void OnClickedBtnSlotChange()
        {
            switch (slotViewType)
            {
                case SlotViewType.Normal:
                    slotViewType = SlotViewType.Shadow;
                    break;
                case SlotViewType.Shadow:
                    slotViewType = SlotViewType.Normal;
                    break;
            }
            UpdateBtnSlotChange();
        }

        void UpdateBtnSlotChange()
        {
            switch (slotViewType)
            {
                case SlotViewType.Normal:
                    btnSlotChange.SetIconName(Constants.CommonAtlas.UI_COMMON_ICON_EQUIP_NORMAL);
                    btnSlotChange.SetOutlineColor(normalColor);
                    btnSlotChange.LocalKey = LocalizeKey._4105; // 일반
                    goEquipBase.SetActive(true);
                    goShadowBase.SetActive(false);
                    break;
                case SlotViewType.Shadow:
                    btnSlotChange.SetIconName(Constants.CommonAtlas.UI_COMMON_ICON_EQUIP_SHADOW);
                    btnSlotChange.SetOutlineColor(shadowColor);
                    btnSlotChange.LocalKey = LocalizeKey._4106; // 쉐도우
                    goEquipBase.SetActive(false);
                    goShadowBase.SetActive(true);
                    break;
            }
        }

        private ContentProfile GetProfileSubcanvas() =>
            subCanvas[(int)TabType.Profile] as ContentProfile;

        private ContentAbility GetAbilitySubcanvas() =>
            subCanvas[(int)TabType.Ability] as ContentAbility;

        private ContentSkill GetSkillSubcanvas() =>
            subCanvas[(int)TabType.Skill] as ContentSkill;

        public override bool Find()
        {
            equipmentSlots = GetComponentsInChildren<UIEquipmentSlot>();
            return true;
        }
    }
}