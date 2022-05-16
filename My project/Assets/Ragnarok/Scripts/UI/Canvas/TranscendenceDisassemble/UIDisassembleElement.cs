using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="DisassembleListView"/>
    /// </summary>
    public class UIDisassembleElement : UIElement<UIDisassembleElement.IInput>
    {
        public interface IInput
        {
            ItemInfo itemInfo { get; }
        }

        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UIButtonHelper btnSelect;

        public event System.Action<long> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            equipmentProfile.SetData(info.itemInfo);
        }

        void OnClickedBtnSelect()
        {
            OnSelect?.Invoke(info.itemInfo.ItemNo);
        }
    }
}