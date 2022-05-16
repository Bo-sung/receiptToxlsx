using UnityEngine;

namespace Ragnarok
{
    public sealed class JobInfoSlot : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIButtonHelper btnJobChange;
        [SerializeField] UITextureHelper jobIllust;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelJobName;

        GameObject myGameObject;

        private Gender gender;
        private Job job;

        public event System.Action<Job> OnSelect;

        void Awake()
        {
            myGameObject = gameObject;
            EventDelegate.Add(btnJobChange.OnClick, OnClickedBtnJobChange);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(btnJobChange.OnClick, OnClickedBtnJobChange);
            myGameObject = null;
        }

        void OnClickedBtnJobChange()
        {
            OnSelect?.Invoke(job);
        }

        public void Initialize(Gender gender)
        {
            this.gender = gender;
        }

        public void SetJob(Job job)
        {
            this.job = job;
            Refrash();
        }

        private void Refrash()
        {
            if (job == default)
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            jobIllust.SetJobIllust(job.GetJobIllust(gender));
            jobIcon.SetJobIcon(job.GetJobIcon());
            labelJobName.Text = job.GetJobName();
        }

        private void SetActive(bool isActive)
        {
            myGameObject.SetActive(isActive);
        }
    }
}