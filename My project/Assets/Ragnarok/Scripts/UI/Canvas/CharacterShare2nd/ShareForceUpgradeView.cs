using UnityEngine;

namespace Ragnarok.View
{
    public sealed class ShareForceUpgradeView : UIView
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIShareForceUpgradeElement element;
        [SerializeField] UIButtonHelper btnReset;

        private SuperWrapContent<UIShareForceUpgradeElement, UIShareForceUpgradeElement.IInput> wrapContent;

        public event System.Action OnReset;

        protected override void Awake()
        {
            base.Awake();

            wrapContent = wrapper.Initialize<UIShareForceUpgradeElement, UIShareForceUpgradeElement.IInput>(element);
            EventDelegate.Add(btnReset.OnClick, OnClickedBtnReset);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnReset.OnClick, OnClickedBtnReset);
        }

        protected override void OnLocalize()
        {
            btnReset.LocalKey = LocalizeKey._10272; // 초기화
        }

        void OnClickedBtnReset()
        {
            OnReset?.Invoke();
        }

        public void SetData(UIShareForceUpgradeElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        public void SetCanReset(bool canReset)
        {
            btnReset.IsEnabled = canReset;
        }
    }
}