using UnityEngine;

namespace Ragnarok
{
    public sealed class UIEquipmentStatusInfo : UIInfo<ItemInfo>, IAutoInspectorFinder
    {
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelWeight;
        [SerializeField] UISprite iconClassBit;
        [SerializeField] UIEquipmentBattleOptionList effectList;
        [SerializeField] UIGrid weightGrid;
        [SerializeField] UIToolTipHelper classTypeToolTip;
        [SerializeField] UIButtonWithIcon btnElement;

        public event System.Action<byte> OnSelectEquip;
        public event System.Action<byte> OnSelectUnEquip;

        private ElementType elementType;

        protected override void Awake()
        {
            base.Awake();
            effectList.OnSelectEquip += OnEquip;
            effectList.OnSelectUnEquip += OnUnEquip;

            if (btnElement)
            {
                EventDelegate.Add(btnElement.OnClick, OnClickedBtnElement);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            effectList.OnSelectEquip -= OnEquip;
            effectList.OnSelectUnEquip -= OnUnEquip;

            if (btnElement)
            {
                EventDelegate.Remove(btnElement.OnClick, OnClickedBtnElement);
            }
        }

        protected override void Refresh()
        {
            if (info is null)
            {
                Debug.LogError($"장비 아이템 정보가 주어지지 않음!"); // TODO: 디버그 로그 지울 것
                return;
            }

            equipmentProfile.SetActiveCardView(info.IsCardEnchanted);
            equipmentProfile.SetData(info);
#if UNITY_EDITOR
            if (info.Smelt > 0)
            {
                labelName.Text = $"{info.Name} +{info.Smelt}({info.ItemId})";
            }
            else
            {
                labelName.Text = $"{info.Name}({info.ItemId})";
            }
#else
            if (info.Smelt > 0)
            {
                labelName.Text = $"{info.Name} +{info.Smelt}";
            }
            else
            {
                labelName.Text = info.Name;
            }
#endif

            ItemEquipmentSlotType slotType = info.SlotType;
            bool isShowElementIcon = slotType == ItemEquipmentSlotType.Weapon || slotType == ItemEquipmentSlotType.Armor;

            if (btnElement)
            {
                btnElement.SetActive(isShowElementIcon);
                if (isShowElementIcon)
                {
                    elementType = info.ElementType;
                    btnElement.SetIconName(elementType.GetIconName());
                    btnElement.Text = info.GetElementLevelText();
                }
                else
                {
                    elementType = ElementType.None;
                }
            }

            labelDescription.Text = info.Description;
            labelWeight.Text = info.TotalWeightText;
            iconClassBit.spriteName = info.ClassType.GetIconName(info.ItemDetailType);

            weightGrid.Reposition();

            effectList.SetData(info);

            classTypeToolTip.SetToolTipLocalizeKey(info.ClassType.ToLocalizeKey());
        }

        void OnEquip(byte index)
        {
            OnSelectEquip?.Invoke(index);
        }

        void OnUnEquip(byte index)
        {
            OnSelectUnEquip?.Invoke(index);
        }

        void OnClickedBtnElement()
        {
            if (elementType == ElementType.None)
                return;

            UI.Show<UISelectPropertyPopup>().ShowElementView(elementType);
        }
    }
}