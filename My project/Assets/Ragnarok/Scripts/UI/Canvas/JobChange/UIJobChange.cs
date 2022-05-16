using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobChange : UICanvas, TurotialJobChange.ISelectJobImpl
    {
        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIGrid grid;
        [SerializeField] JobInfoSlot[] jobInfoSlots;
        [SerializeField] UIButtonHelper btnEscape;
        [SerializeField] UIButtonHelper btnClose;

        JobChangePresenter presenter;

        protected override void OnInit()
        {
            presenter = new JobChangePresenter();

            foreach (var item in jobInfoSlots)
            {
                item.Initialize(presenter.Gender);
                item.OnSelect += OnSelectJob;
            }

            presenter.OnChangedJob += OnChangedJob;

            presenter.AddEvent();

            EventDelegate.Add(btnEscape.OnClick, Escape);
            EventDelegate.Add(btnClose.OnClick, Escape);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            foreach (var item in jobInfoSlots)
            {
                item.OnSelect -= OnSelectJob;
            }

            EventDelegate.Remove(btnEscape.OnClick, Escape);
            EventDelegate.Remove(btnClose.OnClick, Escape);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContentJobChange(); // 신규 컨텐츠 플래그 제거
            SetJob(presenter.GetNextJobs()); // 전직 세팅
            StartTutorial(); // ShortCut을 통해서 JobChange 를 들어올 수 있다.
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._22000; // 전직할 직업을 선택해 주세요
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
            UI.Show<UIJobSelect>().SetData(UIJobSelect.State.JobChange, job);
        }

        void Escape()
        {
            if (Tutorial.isInProgress)
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                return;
            }

            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIJobChange>();
        }

        private void StartTutorial()
        {
            Tutorial.Run(TutorialType.JobChange);
        }

        private void OnChangedJob(bool isInit)
        {
            isFinishedJobChange = true;
            CloseUI();
        }

        #region Tutorial

        bool isFinishedJobChange;

        bool TurotialJobChange.ISelectJobImpl.IsFinishedJobChange()
        {
            if (isFinishedJobChange)
            {
                isFinishedJobChange = false;
                return true;
            }

            return false;
        }

        #endregion
    }
}