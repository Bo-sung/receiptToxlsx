using UnityEngine;

namespace Ragnarok
{
    public class UIBookReward : UICanvas
    {
        public class Input : IUIData
        {
            public BookData rewardData;
        }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UILabelHelper beforeLevelLabel;
        [SerializeField] UILabelHelper afterLevelLabel;
        [SerializeField] UILabelHelper optionLabel;
        [SerializeField] UIButtonHelper okButton;

        protected override void OnInit()
        {
            EventDelegate.Add(okButton.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(okButton.OnClick, CloseUI);
        }

        protected override void OnLocalize()
        {
            titleLabel.LocalKey = LocalizeKey._40228; // 도감 레벨업
            okButton.LocalKey = LocalizeKey._1; // 확인
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;

            beforeLevelLabel.Text = $"Lv.{input.rewardData.Level - 1}";
            afterLevelLabel.Text = $"Lv.{input.rewardData.Level}";
            optionLabel.Text = $"[+] {input.rewardData.GetOption().GetDescription()}";
        }

        protected override void OnHide()
        {
        }

        private void CloseUI()
        {
            UI.Close<UIBookReward>();
        }
    }
}