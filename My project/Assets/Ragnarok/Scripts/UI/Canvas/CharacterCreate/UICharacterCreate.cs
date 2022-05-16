using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICharacterCreate : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single | UIType.Reactivation;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] SynopsisView synopsisView;
        [SerializeField] NoviceCreateView noviceCreateView;
        [SerializeField] JobPreview jobPreview;
        [SerializeField] JobDetailInfoView jobDetailInfoView;

        CharacterCreatePresenter presenter;

        protected override void OnInit()
        {
            presenter = new CharacterCreatePresenter();

            synopsisView.OnNoviceView += ShowNoviceView;
            noviceCreateView.OnJobPreview += ShowJobPreview;
            noviceCreateView.OnRandom += OnRandom;
            noviceCreateView.OnMale += OnMale;
            noviceCreateView.OnFemale += OnFemale;
            noviceCreateView.OnCreate += presenter.CreateCharacter;
            jobPreview.OnSelectJob += OnSelectJob;
            jobDetailInfoView.OnSelectJob += OnSelectJob;
            jobDetailInfoView.OnConfirm += OnConfirm;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            synopsisView.OnNoviceView -= ShowNoviceView;
            noviceCreateView.OnJobPreview -= ShowJobPreview;
            noviceCreateView.OnRandom -= OnRandom;
            noviceCreateView.OnMale -= OnMale;
            noviceCreateView.OnFemale -= OnFemale;
            noviceCreateView.OnCreate -= presenter.CreateCharacter;
            jobPreview.OnSelectJob -= OnSelectJob;
            jobDetailInfoView.OnSelectJob -= OnSelectJob;
            jobDetailInfoView.OnConfirm -= OnConfirm;
        }

        protected override void OnShow(IUIData data = null)
        {
            UpdateNoviceView();

            noviceCreateView.Hide();
            jobPreview.Hide();
            jobDetailInfoView.Hide();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._2007; // 캐릭터 생성
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
                ShowNoviceView();
            }
            else if (noviceCreateView.IsShow)
            {
                ShowSynopsisView();
            }
            else
            {
                base.OnBack();
            }
        }

        private void UpdateNoviceView()
        {
            noviceCreateView.SetJob(Job.Novice);
            noviceCreateView.SetGender(presenter.Gender);
            noviceCreateView.SetCharacterName(presenter.CharacterName);
            noviceCreateView.SetJobDescription(presenter.GetJobDescription(Job.Novice));
            noviceCreateView.SetSkill(presenter.GetSkillPreviews(Job.Novice));
            noviceCreateView.SetStats(presenter.GetProgressStatus(Job.Novice));
        }

        private void OnRandom()
        {
            presenter.SetGender((presenter.Gender == Gender.Female) ? Gender.Male : Gender.Female);
            presenter.SetRandomCharacterName();
            noviceCreateView.SetGender(presenter.Gender);
            noviceCreateView.SetCharacterName(presenter.CharacterName);
        }

        private void OnMale()
        {
            presenter.SetGender(Gender.Male);
            noviceCreateView.SetGender(presenter.Gender);
        }

        private void OnFemale()
        {
            presenter.SetGender(Gender.Female);
            noviceCreateView.SetGender(presenter.Gender);
        }

        private void OnSelectJob(Job job)
        {
            jobDetailInfoView.Initialize(presenter.Gender);
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
            ShowJobPreview();
        }

        private void ShowSynopsisView()
        {
            synopsisView.Show();
            noviceCreateView.Hide();
            jobPreview.Hide();
            jobDetailInfoView.Hide();
        }

        private void ShowNoviceView()
        {
            synopsisView.Hide();
            noviceCreateView.Show();
            jobPreview.Hide();
            jobDetailInfoView.Hide();
        }

        private void ShowJobPreview()
        {
            synopsisView.Hide();
            noviceCreateView.Hide();
            jobPreview.Show();
            jobDetailInfoView.Hide();
        }

        private void ShowJobDetailInfoView()
        {
            synopsisView.Hide();
            noviceCreateView.Hide();
            jobPreview.Hide();
            jobDetailInfoView.Show();
        }
    }
}