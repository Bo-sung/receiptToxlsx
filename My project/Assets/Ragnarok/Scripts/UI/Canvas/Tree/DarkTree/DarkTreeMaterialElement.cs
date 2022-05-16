using UnityEngine;

namespace Ragnarok.View
{
    public class DarkTreeMaterialElement : UIElement<DarkTreeMaterialElement.IInput>
    {
        public interface IInput
        {
            event System.Action OnUpdateSelectedCount;

            ItemInfo ItemInfo { get; }
            int MaxCount { get; }
            int Point { get; }

            int GetSelectedCount();
            void PlusCount();
            void MinusCount();
            void ToggleSelect();
        }

        [SerializeField] UIButton btnSelect;
        [SerializeField] GameObject goUnselect, goSelect;
        [SerializeField] UIPartsProfile partsProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelOwned;
        [SerializeField] UILabelHelper labelOwnedValue;
        [SerializeField] UIGrid grid;
        [SerializeField] UIButtonWithIcon btnElement;
        [SerializeField] UILabelHelper labelPlusPoint;
        [SerializeField] GameObject goBaseCount;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UIPressButton btnMinus, btnPlus;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Add(btnSelect.onClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Remove(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Remove(btnSelect.onClick, OnClickedBtnSelect);
        }

        void OnClickedBtnMinus()
        {
            if (info == null)
                return;

            info.MinusCount();
        }

        void OnClickedBtnPlus()
        {
            if (info == null)
                return;

            info.PlusCount();
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            info.ToggleSelect();
        }

        protected override void OnLocalize()
        {
            labelOwned.LocalKey = LocalizeKey._20005; // 보유
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            info.OnUpdateSelectedCount += UpdateSelectedCount;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            info.OnUpdateSelectedCount -= UpdateSelectedCount;
        }

        protected override void Refresh()
        {
            partsProfile.SetData(info.ItemInfo);
            labelName.Text = info.ItemInfo.Name;
            labelOwnedValue.Text = info.MaxCount.ToString("N0");

            bool isElementStone = info.ItemInfo.IsElementStone;
            btnElement.SetActive(isElementStone);
            if (isElementStone)
            {
                ElementType elementType = info.ItemInfo.ElementType;
                btnElement.SetIconName(elementType.GetIconName());
                btnElement.Text = info.ItemInfo.GetElementLevelText();
            }

            grid.Reposition();

            labelPlusPoint.Text = info.Point.ToString("N0");
            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            int cur = info == null ? 0 : info.GetSelectedCount();
            int max = info == null ? 0 : info.ItemInfo.ItemCount;

            goUnselect.SetActive(cur == 0);
            goSelect.SetActive(cur > 0);

            goBaseCount.SetActive(cur > 0);
            btnMinus.isEnabled = cur > 0;
            btnPlus.isEnabled = cur < max;

            labelCount.Text = cur.ToString("N0");
        }
    }
}