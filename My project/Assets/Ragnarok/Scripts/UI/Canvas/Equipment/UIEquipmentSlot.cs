using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="EquipmentItemInfo"/>
    /// </summary>
    public class UIEquipmentSlot : UIInfo<ItemInfo>
    {
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UIButtonHelper btnShowInfo;
        System.Action<ItemInfo> onClickevent;
        private InventoryModel invenModel;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnShowInfo.OnClick, OnClickedBtnShowInfo);
        }

        public override void SetData(ItemInfo info)
        {
            base.SetData(info);
            equipmentProfile.SetData(info);
        }

        public void SetOthersInventoryModel(InventoryModel invenModel = null)
        {
            if (invenModel is null)
                invenModel = Entity.player.Inventory;

            this.invenModel = invenModel;
        }

        public void SetPowerUpIcon(bool isActive)
        {
            equipmentProfile.SetPowerUpIcon(isActive);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);
        }

        public void OnClickedEvent(System.Action<ItemInfo> onClickevent)
        {
            this.onClickevent = onClickevent;
        }

        void OnClickedBtnShowInfo()
        {
            if (onClickevent == null)
            {
                UI.Show<UIEquipmentInfo>().Set(info.ItemNo, this.invenModel);
            }
            else
            {
                onClickevent?.Invoke(info);
            }
        }
    }
}
