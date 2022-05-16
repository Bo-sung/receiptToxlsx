using UnityEngine;

namespace Ragnarok
{
    public sealed class UIInputPopup : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;
        public override int layer => Layer.UI_Popup;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIInput input;
        [SerializeField] UILabelHelper labelInputDefault;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] UIButtonHelper btnSubmit;

        private System.Action<string> onSubmit;
        private int maxNum;

        protected override void OnInit()
        {
            EventDelegate.Add(btnExit.OnClick, CloseUIWithSubmit);
            EventDelegate.Add(btnClose.OnClick, CloseUIWithSubmit);
            EventDelegate.Add(btnSubmit.OnClick, OnClickedBtnSubmit);
            EventDelegate.Add(input.onChange, RefreshInput);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, CloseUIWithSubmit);
            EventDelegate.Remove(btnClose.OnClick, CloseUIWithSubmit);
            EventDelegate.Remove(btnSubmit.OnClick, OnClickedBtnSubmit);
            EventDelegate.Remove(input.onChange, RefreshInput);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void Show(string title, string description, string inputDefault = default, UIInput.KeyboardType keyboardType = UIInput.KeyboardType.Default, System.Action<string> onSubmit = null)
        {
            labelTitle.Text = title;
            labelDesc.Text = description;
            labelInputDefault.Text = inputDefault;
            input.keyboardType = keyboardType;
            input.validation = keyboardType == UIInput.KeyboardType.NumberPad ? UIInput.Validation.Integer : UIInput.Validation.None;
            this.onSubmit = onSubmit;

            maxNum = 0;

            Show();
        }

        public void Show(string title, string description, string inputDefault = default, int maxNum = 0, System.Action<string> onSubmit = null)
        {
            labelTitle.Text = title;
            labelDesc.Text = description;
            labelInputDefault.Text = inputDefault;
            input.keyboardType = UIInput.KeyboardType.NumberPad;
            input.validation = UIInput.Validation.Integer;
            this.maxNum = maxNum;
            this.onSubmit = onSubmit;

            Show();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnSubmit.LocalKey = LocalizeKey._1; // 확인
            btnClose.LocalKey = LocalizeKey._2; // 취소
        }

        void OnClickedBtnSubmit()
        {
            onSubmit?.Invoke(input.value);
            CloseUI();
        }

        void CloseUIWithSubmit()
        {
            onSubmit?.Invoke(string.Empty);
            CloseUI();
        }

        void CloseUI()
        {
            UI.Close<UIInputPopup>();
        }

        void RefreshInput()
        {
            if (input.keyboardType == UIInput.KeyboardType.NumberPad && maxNum > 0)
            {
                if (int.TryParse(input.value, out int num))
                {
                    // 최대치를 넘었을 경우
                    if (num > maxNum)
                        input.value = maxNum.ToString();
                }
            }
        }

        protected override void OnBack()
        {
            onSubmit?.Invoke(string.Empty);

            base.OnBack();
        }
    }
}