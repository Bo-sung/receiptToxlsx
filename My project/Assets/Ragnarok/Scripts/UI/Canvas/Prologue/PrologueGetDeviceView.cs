using UnityEngine;

namespace Ragnarok.View
{
    public class PrologueGetDeviceView : UIView
    {
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelDescription;

        [SerializeField] UIButton btnNext;
        [SerializeField] float animationTime = 2f;
        RelativeRemainTime remainTime;

        public event System.Action OnHidePopup;

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._920; // 신규 획득
            labelDescription.Text = StringBuilderPool.Get()
                .Append("S/N: ").Append(System.Guid.NewGuid().ToString("N").Substring(0, 8))
                .AppendLine()
                .AppendLine()
                .Append(LocalizeKey._615.ToText()) // 1세대 쉐어 바이스가 활성화 되었습니다.
                .Release();
        }

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnNext.onClick, OnClickButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnNext.onClick, OnClickButton);
        }

        public override void Show()
        {
            base.Show();

            remainTime = animationTime;
        }

        private void OnClickButton()
        {
            if (remainTime.GetRemainTime() > 0) return;

            OnHidePopup?.Invoke();
            Hide();
        }
    }
}