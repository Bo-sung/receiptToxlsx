using UnityEngine;

namespace Ragnarok.View
{
    public class UIItemSelectElement : UIElement<UIItemSelectElement.IInput>, IInspectorFinder
    {
        public interface IInput
        {
            ItemInfo Info { get; }
            bool IsSelect { get; }

            void ToggleSelect();
        }

        [SerializeField] UIButton button;
        [SerializeField] UIEquipmentProfile equipmentProfile;
        [SerializeField] UICostumeProfile costumeProfile;
        [SerializeField] UICardProfile cardProfile;
        [SerializeField] GameObject goSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(button.onClick, OnClickdButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Add(button.onClick, OnClickdButton);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            if (info == null)
                return;

            ItemGroupType type = info.Info.ItemGroupType;
            equipmentProfile.SetActive(type == ItemGroupType.Equipment);
            costumeProfile.SetActive(type == ItemGroupType.Costume);
            cardProfile.SetActive(type == ItemGroupType.Card);

            switch (type)
            {
                case ItemGroupType.Equipment:
                    equipmentProfile.SetData(info.Info);
                    break;

                case ItemGroupType.Costume:
                    costumeProfile.Set(info.Info);
                    break;

                case ItemGroupType.Card:
                    cardProfile.SetData(info.Info);
                    break;
            }

            NGUITools.SetActive(goSelect, info.IsSelect);
        }

        void OnClickdButton()
        {
            if (info == null)
                return;

            info.ToggleSelect();
            Refresh();
        }

        bool IInspectorFinder.Find()
        {
            button = GetComponent<UIButton>();
            return true;
        }
    }
}