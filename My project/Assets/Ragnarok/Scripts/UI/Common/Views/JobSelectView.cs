using UnityEngine;

namespace Ragnarok.View
{
    public class JobSelectView : UIView<JobSelectView.IListener>, IAutoInspectorFinder
    {
        public interface IListener
        {
            /// <summary>
            /// 직업 선택 시 호출
            /// </summary>
            void OnSelect(Job job);
        }

        public interface IInfo
        {
            Job Job { get; }
            string Name { get; }
        }

        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelJobName;
        [SerializeField] UIPopupListAdvanced popupList;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(popupList.onChange, OnChangedPopupList);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(popupList.onChange, OnChangedPopupList);
        }

        protected override void OnLocalize()
        {
            popupList.CategoryLocalKey = LocalizeKey._39102; // 직업
        }

        void OnChangedPopupList()
        {
            // Set Icon
            Job job = (Job)UIPopupList.current.data;
            jobIcon.Set(job.GetJobIcon());

            // Set JobName
            labelJobName.Text = UIPopupList.current.value;

            // Event
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnSelect(job);
            }
        }

        public void AddItems(IInfo[] items)
        {
            string selectName = null;

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(selectName))
                    selectName = item.Name;

                popupList.AddItem(item.Name, item.Job);
            }

            popupList.Set(selectName, notify: true);
        }

        public void ClearItem()
        {
            popupList.Clear();
        }
    }
}