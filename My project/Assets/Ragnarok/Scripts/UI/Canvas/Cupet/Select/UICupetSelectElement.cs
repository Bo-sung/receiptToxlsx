using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICupetSelect"/>
    /// </summary>
    public class UICupetSelectElement : UIElement<UICupetSelectElement.IInput>
    {
        public interface IInput
        {
            CupetModel CupetModel { get; }
            bool IsSelect { get; }
            int Index { get; }
            void OnSelect();
        }

        [SerializeField] UICupetProfile cupetProfile;
        [SerializeField] GameObject goSelect;
        [SerializeField] UILongPressButton btnSelect;
        [SerializeField] UILabelHelper labelIndex;

        public event System.Action<ICupetModel> OnSelectLongPress;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.onClick, OnClickedBtnSelect);
            btnSelect.OnSelectLongPress += InvokeLongPress;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSelect.onClick, OnClickedBtnSelect);
            btnSelect.OnSelectLongPress -= InvokeLongPress;
        }

        protected override void OnLocalize()
        {

        }

        protected override void AddEvent()
        {
            base.AddEvent();
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();
        }

        protected override void Refresh()
        {
            cupetProfile.SetData(info.CupetModel);
            UpdateSelect();
        }

        private void UpdateSelect()
        {
            bool isSelect = info.IsSelect;
            goSelect.SetActive(isSelect);
            if (isSelect)
                labelIndex.Text = (info.Index + 1).ToString();
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            info.OnSelect();
        }

        void InvokeLongPress()
        {
            if (info == null)
                return;

            OnSelectLongPress?.Invoke(info.CupetModel);
        }
    }
}