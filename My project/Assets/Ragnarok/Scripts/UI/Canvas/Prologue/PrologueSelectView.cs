using UnityEngine;

namespace Ragnarok.View
{
    public class PrologueSelectView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper buttonSelect1;
        [SerializeField] UIButtonHelper buttonSelect2;

        public event System.Action OnHidePopup;

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._917; // 희망의 영웅?
            buttonSelect1.LocalKey = LocalizeKey._918; // 잘못 보신 게 아닐까요.
            buttonSelect2.LocalKey = LocalizeKey._919; // 제가 영웅이라고요?
        }

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(buttonSelect1.OnClick, OnClickButton);
            EventDelegate.Add(buttonSelect2.OnClick, OnClickButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(buttonSelect1.OnClick, OnClickButton);
            EventDelegate.Remove(buttonSelect2.OnClick, OnClickButton);
        }

        private void OnClickButton()
        {
            OnHidePopup?.Invoke();
            Hide();
        }
    }
}