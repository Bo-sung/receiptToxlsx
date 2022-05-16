using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobReplace : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIGrid grid;
        [SerializeField] JobInfoSlot[] jobInfoSlots;
        [SerializeField] UIButtonHelper btnEscape;
        [SerializeField] UIButtonHelper btnClose;
        [SerializeField] EnvelopContent topResizer;
        [SerializeField] EnvelopContent bottomResizer;
        [SerializeField] UILabelHelper labelNotice;

        JobReplacePresenter presenter;

        protected override void OnInit()
        {
            presenter = new JobReplacePresenter();

            foreach (var item in jobInfoSlots)
            {
                item.Initialize(presenter.Gender);
                item.OnSelect += OnSelectJob;
            }

            presenter.AddEvent();

            EventDelegate.Add(btnEscape.OnClick, OnBack);
            EventDelegate.Add(btnClose.OnClick, OnBack);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            foreach (var item in jobInfoSlots)
            {
                item.OnSelect -= OnSelectJob;
            }

            EventDelegate.Remove(btnEscape.OnClick, OnBack);
            EventDelegate.Remove(btnClose.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
            SetJob(presenter.GetReplaceJobs()); // 변경 가능 직업 세팅
            topResizer.Execute();
            bottomResizer.Execute();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._22002; // 변경할 직업을 선택해주세요.
            labelNotice.LocalKey = LocalizeKey._22003; // 적직 변경 시 스탯포인트와 스킬포인트가 모두 반환됩니다.
        }

        void SetJob(Job[] jobs)
        {
            int jobLength = jobs == null ? 0 : jobs.Length;

            for (int i = 0; i < jobInfoSlots.Length; i++)
            {
                jobInfoSlots[i].SetJob(i < jobLength ? jobs[i] : default);
            }

            grid.Reposition();
        }

        void OnSelectJob(Job job)
        {
            UI.Show<UIJobSelect>().SetData(UIJobSelect.State.JobReplace, job);
        }
    }
}