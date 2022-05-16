using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISkipPrologue : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] UIButtonHelper btnSkip;
        [SerializeField] SkipPrologueView skipPrologueView;
        [SerializeField] PrologueTipView prologueTipView;
        [SerializeField] PrologueTutorialView prologueTutorialView;

        public event System.Action OnSkip;

        protected override void OnInit()
        {
            EventDelegate.Add(btnSkip.OnClick, OnClickedBtnSkip);
            skipPrologueView.OnSkip += OnSkipPrologue;
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnSkip.OnClick, OnClickedBtnSkip);
            skipPrologueView.OnSkip -= OnSkipPrologue;
        }

        protected override void OnShow(IUIData data = null)
        {
            skipPrologueView.Hide();
            prologueTipView.Hide();
            prologueTutorialView.Hide();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnSkip.LocalKey = LocalizeKey._922; // Skip
        }

        void OnClickedBtnSkip()
        {
            skipPrologueView.Show();
        }

        void OnSkipPrologue()
        {
            Hide();

            OnSkip?.Invoke();
        }

        public void ActiveSkipButton(bool isShow)
        {
            btnSkip.SetActive(isShow);
        }

        public void ShowTipView()
        {
            prologueTipView.Show();
        }

        public void ActiveTutorialView(bool isActive)
        {
            if (isActive) prologueTutorialView.Show();
            else prologueTutorialView.Hide();
        }

        public SkipPrologueView GetSkipPrologueView()
        {
            return skipPrologueView;
        }
    }
}