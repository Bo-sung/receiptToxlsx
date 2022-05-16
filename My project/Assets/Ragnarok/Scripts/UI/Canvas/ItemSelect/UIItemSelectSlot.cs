using UnityEngine;
using System;

namespace Ragnarok
{
    public class UIItemSelectSlot : MonoBehaviour
    {
        public class Input
        {
            public ItemInfo info;
            public bool isSelected;
            public Action onUpdate;
        }

        [SerializeField] GameObject selectMark;
        [SerializeField] UIButton touchButton;
        [SerializeField] UIEquipmentProfile equipmentInfoProfile;
        [SerializeField] UIPartsProfile partsProfile;
        [SerializeField] UICardProfile cardProfile;

        private Action<UIItemSelectSlot, Input> onClickSlot;
        private Input input;

        private void Start()
        {
            EventDelegate.Add(touchButton.onClick, OnSelectSlot);
        }

        public void SetData(Input input, Action<UIItemSelectSlot, Input> onClickSlot)
        {
            if (this.input != null && this.input.onUpdate == Refresh)
                this.input.onUpdate = null;

            this.input = input;
            this.onClickSlot = onClickSlot;
            input.onUpdate = Refresh;
            Refresh();
        }

        private void OnSelectSlot()
        {
            onClickSlot?.Invoke(this, input);
        }

        private void Refresh()
        {
            selectMark.SetActive(input.isSelected);

            equipmentInfoProfile.gameObject.SetActive(false);
            partsProfile.gameObject.SetActive(false);
            cardProfile.gameObject.SetActive(false);

            if (input.info.ItemGroupType == ItemGroupType.Card)
            {
                cardProfile.gameObject.SetActive(true);
                cardProfile.SetData(input.info);
            }
            else if (input.info.ItemGroupType == ItemGroupType.Equipment)
            {
                equipmentInfoProfile.gameObject.SetActive(true);
                equipmentInfoProfile.SetData(input.info);
            }
            else if (input.info.ItemGroupType == ItemGroupType.ProductParts)
            {
                partsProfile.gameObject.SetActive(true);
                partsProfile.SetData(input.info);
            }
        }
    }
}