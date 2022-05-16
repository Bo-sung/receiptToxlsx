using UnityEngine;

namespace Ragnarok
{
    public sealed class UIWarning : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override bool IsVisible => base.IsVisible && animator.gameObject.activeSelf;

        [SerializeField] Animator animator;
        [SerializeField] UILabel labelDescription;

        private int localKey = LocalizeKey._90105; // MVP 출현!

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            animator.gameObject.SetActive(false);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            UpdateText();
        }

        public void Initialize(int localKey)
        {
            this.localKey = localKey;
            UpdateText();
        }

        public void PlayWarning()
        {
            animator.gameObject.SetActive(true);
            animator.Play("Ui_Warning");
        }

        private void Update()
        {
            if (!animator.gameObject.activeSelf || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                return;

            animator.gameObject.SetActive(false);
        }

        private void UpdateText()
        {
            labelDescription.text = localKey.ToText();
        }
    }
}