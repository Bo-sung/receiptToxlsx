using UnityEngine;

namespace Ragnarok.View
{
    public class SkipPrologueView : UIView
    {
        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UILabelHelper labDesc;
        [SerializeField] UILabelHelper labSkip;

        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnCancel;

        public event System.Action OnSkip;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnConfirm.OnClick, SkipPrologue);
            EventDelegate.Add(btnCancel.OnClick, Hide);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnConfirm.OnClick, SkipPrologue);
            EventDelegate.Remove(btnCancel.OnClick, Hide);
        }

        protected override void OnLocalize()
        {
            labTitle.LocalKey = LocalizeKey._904; // 의문의 남자
            labDesc.LocalKey = LocalizeKey._901; // <스토리 요약>\n\n위에서 깨어났지만\n어디를 향하고 있었는지 기억이 나지 않고,\n의문의 남자에게 무언가를 받게 되는데..
            labSkip.LocalKey = LocalizeKey._902; // Skip 하시겠습니까?

            btnConfirm.LocalKey = LocalizeKey._1;
            btnCancel.LocalKey = LocalizeKey._2;
        }

        private void SkipPrologue()
        {
            Hide();

            OnSkip?.Invoke();
        }
    }
}