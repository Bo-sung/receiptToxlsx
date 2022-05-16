using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class EquipmentViewSlot : UIView
    {
        [SerializeField] ItemEquipmentSlotType slotType;
        [SerializeField] UIEquipmentSlot equipment;
        [SerializeField] UIButtonHelper btnEmptySlot;
        [SerializeField] UIButtonHelper btnQuickChange;

        public event Action<ItemEquipmentSlotType> OnSelect;

        protected override void OnLocalize()
        {            
        }

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnEmptySlot.OnClick, OnClickdedBtnEmptySlot);
            EventDelegate.Add(btnQuickChange.OnClick, OnClickedBtQuickChange);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnEmptySlot.OnClick, OnClickdedBtnEmptySlot);
            EventDelegate.Remove(btnQuickChange.OnClick, OnClickedBtQuickChange);
        }

        private void OnClickdedBtnEmptySlot()
        {
            OnSelect?.Invoke(slotType);
        }

        private void OnClickedBtQuickChange()
        {
            OnSelect?.Invoke(slotType);
        }

        public ItemEquipmentSlotType GetSlotType()
        {
            return slotType;
        }

        public void Set(ItemInfo item, bool canEquip, bool hasStronger)
        {
            equipment.SetData(item);
            btnEmptySlot.SetNotice(canEquip);
            btnQuickChange.SetActive(item != null);
            SetStrongerEquipmentNotice(hasStronger);
        }

        public void SetStrongerEquipmentNotice(bool hasStronger)
        {
            btnQuickChange.SetNotice(hasStronger);
        }
    }
}