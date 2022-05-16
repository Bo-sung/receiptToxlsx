using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIToggle))]
    public class UICloneSelector : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper profile;
        [SerializeField] UITextureHelper jobIcon;

        public event System.Action<Job, Gender> OnSelect;

        GameObject myGameObject;
        UIToggle toggle;

        private Job job;
        private Gender gender;

        void Awake()
        {
            myGameObject = gameObject;
            toggle = GetComponent<UIToggle>();

            EventDelegate.Add(toggle.onChange, OnChangedToggle);
        }

        void OnDestroy()
        {
            EventDelegate.Remove(toggle.onChange, OnChangedToggle);
        }

        void OnChangedToggle()
        {
            if (!UIToggle.current.value)
                return;

            OnSelect?.Invoke(job, gender);
        }

        public void SetData(Job job, Gender gender)
        {
            this.job = job;
            this.gender = gender;
            Refresh();
        }

        public void Select()
        {
            toggle.Set(true);
        }

        public void Unselect()
        {
            toggle.Set(false);
        }

        private void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);
            profile.Set(job.GetJobProfile(gender));
            jobIcon.Set(job.GetJobIcon());
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        private bool IsInvalid()
        {
            return job == default || gender == default;
        }
    }
}