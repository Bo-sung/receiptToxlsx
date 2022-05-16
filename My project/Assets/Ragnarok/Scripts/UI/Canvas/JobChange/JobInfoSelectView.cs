using UnityEngine;

namespace Ragnarok.View
{
    public class JobInfoSelectView : JobDetailInfoView
    {
        [SerializeField] UIButtonHelper btnJobPreview;
        [SerializeField] UILabelHelper labelNotice;

        public event System.Action OnJobPreview;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnJobPreview.OnClick, OnClickedBtnJobPreview);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnJobPreview.OnClick, OnClickedBtnJobPreview);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();
        }

        public void SetNotice(string notice)
        {
            if (string.IsNullOrEmpty(notice))
            {
                labelNotice.SetActive(false);
                return;
            }

            labelNotice.SetActive(true);
            labelNotice.Text = notice;
        }

        private void OnClickedBtnJobPreview()
        {
            OnJobPreview?.Invoke();
        }
    }
}