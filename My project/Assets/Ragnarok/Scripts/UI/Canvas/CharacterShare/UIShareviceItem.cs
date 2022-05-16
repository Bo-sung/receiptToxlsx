using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIShareviceItem : UIElement<ShareviceItem>
    {
        /// <summary>
        /// 버튼입력 가속도
        /// </summary>
       private const float INPUT_ACCELERATION = 0.03f;

        [SerializeField] UIPressButton btnSelect;
        [SerializeField] GameObject select;
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UILabelHelper labelSelectCount;

        public event System.Action OnChangeSelect;

        private int ticks = 0;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.onClick, OnClickedBtnSelect);
            btnSelect.OnDepressed += OnDepressedBtnSelect;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnSelect.onClick, OnClickedBtnSelect);
            btnSelect.OnDepressed -= OnDepressedBtnSelect;
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            labelCount.Text = info.itemCount.ToString();
            labelSelectCount.Text = info.SelectedCount.ToString();
            icon.SetItem(info.itemIcon);

            select.SetActive(info.SelectedCount > 0);
        }

        /// <summary>
        /// 버튼 입력
        /// </summary>
        void OnClickedBtnSelect()
        {
            if (!info.CanSelect())
                return;

            int value = (int)(1 + INPUT_ACCELERATION * ticks * ticks);
            info.PlusSelectCountBeforeOverload(value);

            Refresh();
            OnChangeSelect?.Invoke();
            ticks++;
        }

        /// <summary>
        /// 버튼 입력 해제
        /// </summary>
        void OnDepressedBtnSelect()
        {
            ticks = 0;
        }
    }

}