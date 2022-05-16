using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public enum State
        {
            /// <summary>
            /// 전직
            /// </summary>
            JobChange,

            /// <summary>
            /// 직업 변경
            /// </summary>
            JobReplace,
        }

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButton btnEscape;
        [SerializeField] JobInfoSelectView jobInfoSelectView;
        [SerializeField] JobPreview jobPreview;
        [SerializeField] JobDetailInfoView jobDetailInfoView;

        private State state;

        JobSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new JobSelectPresenter();

            jobInfoSelectView.OnJobPreview += ShowJobPreview;
            jobInfoSelectView.OnSelectJob += UpdateSelectView;
            jobInfoSelectView.OnConfirm += OnConfirm;
            jobPreview.OnSelectJob += UpdateDetailInfoVew;
            jobDetailInfoView.OnSelectJob += UpdateDetailInfoVew;
            jobDetailInfoView.OnConfirm += OnConfirmDetailView;

            presenter.OnFinishedJobChange += OnFinishedJobChange;
            presenter.OnFinishedJobReplace += OnFinishedJobReplace;

            presenter.AddEvent();

            EventDelegate.Add(btnEscape.onClick, OnBack);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            jobInfoSelectView.OnJobPreview -= ShowJobPreview;
            jobInfoSelectView.OnSelectJob -= UpdateSelectView;
            jobInfoSelectView.OnConfirm -= OnConfirm;
            jobPreview.OnSelectJob -= UpdateDetailInfoVew;
            jobDetailInfoView.OnSelectJob -= UpdateDetailInfoVew;
            jobDetailInfoView.OnConfirm -= OnConfirmDetailView;

            presenter.OnFinishedJobChange -= OnFinishedJobChange;
            presenter.OnFinishedJobReplace -= OnFinishedJobReplace;

            EventDelegate.Remove(btnEscape.onClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
            jobPreview.Hide();
            jobDetailInfoView.Hide();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            jobDetailInfoView.SetBtnConfirmText(LocalizeKey._2015.ToText()); // 확인
        }

        protected override void OnBack()
        {
            if (jobDetailInfoView.IsShow)
            {
                ShowJobPreview();
            }
            else if (jobPreview.IsShow)
            {
                ShowJobInfoSelectView();
            }
            else
            {
                base.OnBack();
            }
        }

        public void SetData(State state, Job job)
        {
            this.state = state;
            UdpateText();
            UpdateSelectView(job);
        }

        private void UdpateText()
        {
            switch (state)
            {
                case State.JobChange:
                    labelMainTitle.LocalKey = LocalizeKey._30000; // 전 직
                    jobInfoSelectView.SetNotice(LocalizeKey._30003.ToText()  // JOB 레벨 {LEVEL}이상 가능합니다.
                        .Replace(ReplaceKey.LEVEL, presenter.GetNeedJobLevel()));
                    jobInfoSelectView.SetBtnConfirmText(LocalizeKey._30001.ToText()); // 전직하기

                    break;

                case State.JobReplace:
                    labelMainTitle.LocalKey = LocalizeKey._30004; // 직업변경
                    jobInfoSelectView.SetNotice(LocalizeKey._22003.ToText()); // 적직 변경 시 스탯포인트와 스킬포인트가 모두 반환됩니다.
                    jobInfoSelectView.SetBtnConfirmText(LocalizeKey._30005.ToText()); // 변경하기

                    break;
            }
        }

        private void UpdateSelectView(Job job)
        {
            jobInfoSelectView.Initialize(presenter.GetGender());
            Job[] arrayJob = presenter.GetJobArray(state);
            int jobLength = arrayJob.Length;

            Job preJob = job;
            Job nextJob = job;

            if (jobLength > 1)
            {
                for (int i = 0; i < jobLength; i++)
                {
                    if (job == arrayJob[i])
                    {
                        int prev = i > 0 ? i - 1 : arrayJob.Length - 1;
                        int next = i < arrayJob.Length - 1 ? i + 1 : 0;
                        preJob = arrayJob[prev];
                        nextJob = arrayJob[next];
                        break;
                    }
                }
            }
            jobInfoSelectView.SetJob(job, preJob, nextJob, presenter.GetJobDescription(job), presenter.GetSkillPreviews(job), presenter.GetProgressStatus(job));
            ShowJobInfoSelectView();
        }

        private void UpdateDetailInfoVew(Job job)
        {
            jobDetailInfoView.Initialize(presenter.GetGender());
            Job[] arrayJob = presenter.GetJobArray(job);
            int jobLength = arrayJob.Length;

            Job preJob = job;
            Job nextJob = job;

            if (jobLength > 1)
            {
                for (int i = 0; i < jobLength; i++)
                {
                    if (job == arrayJob[i])
                    {
                        int prev = i > 0 ? i - 1 : arrayJob.Length - 1;
                        int next = i < arrayJob.Length - 1 ? i + 1 : 0;
                        preJob = arrayJob[prev];
                        nextJob = arrayJob[next];
                        break;
                    }
                }
            }
            jobDetailInfoView.SetJob(job, preJob, nextJob, presenter.GetJobDescription(job), presenter.GetSkillPreviews(job), presenter.GetProgressStatus(job));
            ShowJobDetailInfoView();
        }

        private void OnConfirm(Job job)
        {
            switch (state)
            {
                case State.JobChange:
                    presenter.RequestCharacterChangeJob(job).WrapNetworkErrors();
                    break;

                case State.JobReplace:
                    presenter.RequestJobChangeTicket(job).WrapNetworkErrors();
                    break;
            }
        }

        private void OnConfirmDetailView(Job job)
        {
            ShowJobPreview();
        }

        private void ShowJobInfoSelectView()
        {
            jobInfoSelectView.Show();
            jobPreview.Hide();
            jobDetailInfoView.Hide();
        }

        private void ShowJobPreview()
        {
            jobInfoSelectView.Hide();
            jobPreview.Show();
            jobDetailInfoView.Hide();
        }

        private void ShowJobDetailInfoView()
        {
            jobInfoSelectView.Hide();
            jobPreview.Hide();
            jobDetailInfoView.Show();
        }

        void OnFinishedJobChange()
        {
            CloseUI();
        }

        void OnFinishedJobReplace()
        {
            UI.Close<UIJobReplace>();
            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIJobSelect>();
        }        
    }
}