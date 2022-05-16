using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class CostumeViewSlot : UIView
    {
        [SerializeField] ItemEquipmentSlotType slotType;
        [SerializeField] UICostumeInfoSlot costume;
        [SerializeField] UIButtonHelper btnEmptySlot;
        [SerializeField] UIButtonHelper btnQuickChange;
        [SerializeField] GameObject Invisible;

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

        public void Set(UICostumeInfoSlot.Info info, bool canEquip)
        {
            costume.Set(info);
            btnQuickChange.SetActive(info.item != null);
            btnEmptySlot.SetNotice(canEquip);
        }

        /// <summary>
        /// 장착 무기와 장착 무기코스튬 타입이 다를때 미사용 표시
        /// </summary>
        /// <param name="isInvisible"></param>
        public void SetInvisible(bool isInvisible)
        {
            if (Invisible)
                Invisible.SetActive(isInvisible);
        }
    }
}