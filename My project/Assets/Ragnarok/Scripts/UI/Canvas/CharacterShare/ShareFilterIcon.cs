using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class ShareFilterIcon : UIView, IInspectorFinder
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper label;
        [SerializeField] UIButtonHelper button;

        private Job job;

        public event System.Action<Job> OnClickJobFilter;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(button.OnClick, OnClickedBtnJob);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(button.OnClick, OnClickedBtnJob);
        }

        protected override void OnLocalize()
        {
        }

        public void SetData(Job job)
        {
            this.job = job;

            icon.Set(job.GetJobIcon(), isAsync: false);
            label.Text = job.GetJobName();
        }

        private void OnClickedBtnJob()
        {
            OnClickJobFilter?.Invoke(job);
        }

        bool IInspectorFinder.Find()
        {
            button = GetComponent<UIButtonHelper>();
            return true;
        }

    }
}