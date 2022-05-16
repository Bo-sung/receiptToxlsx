using System;
using UnityEngine;

namespace Ragnarok.View
{
    public class CostumeTitleView : UIView
    {
        [SerializeField] UILabelHelper labelEmpty;
        [SerializeField] GameObject goProfile;
        [SerializeField] UITextureHelper iconTitle;
        [SerializeField] UILabelHelper labelCostumeTitle;
        [SerializeField] UIButtonHelper btnSelect;
        [SerializeField] GameObject goNotice;

        public event Action<ItemEquipmentSlotType> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.OnClick, OnClickdedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSelect.OnClick, OnClickdedBtnSelect);
        }

        protected override void OnLocalize()
        {
            labelEmpty.LocalKey = LocalizeKey._4057; // 칭호 미장착
        }

        private void OnClickdedBtnSelect()
        {
            OnSelect?.Invoke(ItemEquipmentSlotType.CostumeTitle);
        }

        public void Set(string iconName, int titleId)
        {
            iconTitle.Set(iconName);
            labelCostumeTitle.LocalKey = titleId;
        }

        public void SetNotice(bool isNotice)
        {
            goNotice.SetActive(isNotice);
        }

        public void SetEmpty(bool isEmpty)
        {
            labelEmpty.SetActive(isEmpty);
            goProfile.SetActive(!isEmpty);
        }
    }
}