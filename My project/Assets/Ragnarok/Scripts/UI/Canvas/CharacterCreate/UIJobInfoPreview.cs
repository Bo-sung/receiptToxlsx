using UnityEngine;

namespace Ragnarok
{
    public sealed class UIJobInfoPreview : UICanvas<JobInfoPreviewPresenter>, JobInfoPreviewPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper btnBack;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelSTR;
        [SerializeField] UILabelHelper labelINT;
        [SerializeField] UILabelHelper labelAGI;
        [SerializeField] UILabelHelper labelLUK;
        [SerializeField] UILabelHelper labelVIT;
        [SerializeField] UILabelHelper labelDEX;
        [SerializeField] UITextureHelper preCharImage;
        [SerializeField] UITextureHelper curCharImage;
        [SerializeField] UITextureHelper nextCharImage;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelValue labelJobName;
        [SerializeField] UILabelHelper labelJobDescription;
        [SerializeField] UILabelHelper labelSkillTitle;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UISkillPreview[] skillPreviews;
        [SerializeField] UIButtonHelper btnPrevious;
        [SerializeField] UIButtonHelper btnNext;
        [SerializeField] UIHexagonSprite hexStatus;
        [SerializeField] UIButtonHelper btnHelpStat; // 스탯 (?)버튼

        protected override void OnInit()
        {
            presenter = new JobInfoPreviewPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(btnBack.OnClick, OnClickedBtnBack);
            EventDelegate.Add(btnPrevious.OnClick, presenter.SelectPreviousJob);
            EventDelegate.Add(btnNext.OnClick, presenter.SelectNextJob);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Add(btnHelpStat.OnClick, OnClickedBtnHelpStat);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnBack.OnClick, OnClickedBtnBack);
            EventDelegate.Remove(btnPrevious.OnClick, presenter.SelectPreviousJob);
            EventDelegate.Remove(btnNext.OnClick, presenter.SelectNextJob);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
            EventDelegate.Remove(btnHelpStat.OnClick, OnClickedBtnHelpStat);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._30000; // 전 직
            labelSTR.LocalKey = LocalizeKey._4004; // STR
            labelINT.LocalKey = LocalizeKey._4007; // INT
            labelAGI.LocalKey = LocalizeKey._4005; // AGI
            labelLUK.LocalKey = LocalizeKey._4009; // LUX
            labelVIT.LocalKey = LocalizeKey._4006; // VIT
            labelDEX.LocalKey = LocalizeKey._4008; // DEX
            labelSkillTitle.LocalKey = LocalizeKey._2008; // 캐릭터 스킬
            btnConfirm.LocalKey = LocalizeKey._22001; // 확인
        }

        public void Show(Job job, Gender gender, Job selectJob)
        {
            presenter.SetData(job, gender, selectJob);

            Refresh();
        }

        public void Show(Job job, Gender gender, int index)
        {
            presenter.SetData(job, gender, index);

            Refresh();
        }

        public void Refresh()
        {
            Job job = presenter.CurrentJob;
            Gender gender = presenter.Gender;
            jobIcon.Set(job.GetJobIcon());

            labelJobName.Title = job.GetJobName();
            labelJobName.Value = presenter.IsCurrentLangEnglish() ? string.Empty : job.ToString();

            labelJobDescription.Text = presenter.JobData.des_id.ToText();
            var skillPreviewInfos = presenter.GetSkillPreviews();
            for (int i = 0; i < skillPreviews.Length; i++)
            {
                skillPreviews[i].SetData(i < skillPreviewInfos.Length ? skillPreviewInfos[i] : null);
            }

            curCharImage.Set(job.GetJobSDName(gender));
            preCharImage.Set(presenter.PreviousJob.GetJobSDName(gender));
            nextCharImage.Set(presenter.NextJob.GetJobSDName(gender));
            SetHexStatus();
        }

        void SetHexStatus()
        {
            var jobData = presenter.JobData;
            float totalGuide = (jobData.guide_str + jobData.guide_agi + jobData.guide_vit + jobData.guide_int + jobData.guide_dex + jobData.guide_luk) / 2f;

            float v1 = jobData.guide_str / totalGuide;
            float v2 = jobData.guide_agi / totalGuide;
            float v3 = jobData.guide_vit / totalGuide;
            float v4 = jobData.guide_int / totalGuide;
            float v5 = jobData.guide_dex / totalGuide;
            float v6 = jobData.guide_luk / totalGuide;
            hexStatus.UpdateVertext(v1, v2, v3, v4, v5, v6);
        }

        void OnClickedBtnConfirm()
        {
            CloseUI();
        }

        void OnClickedBtnBack()
        {
            CloseUI();
        }

        void CloseUI()
        {
            UI.Close<UIJobInfoPreview>();
        }

        void JobInfoPreviewPresenter.IView.RefreshView()
        {
            Refresh();
        }

        public override bool Find()
        {
            base.Find();

            skillPreviews = GetComponentsInChildren<UISkillPreview>();
            return true;
        }

        void OnClickedBtnHelpStat()
        {
            UI.ConfirmPopupLong(LocalizeKey._2014.ToText()); // [ STR ]\n- 근접 물리 공격력에 영향을 줍니다.\n\n[ AGI ]\n- 더 빠르게 공격하고 자주 회피할 수 있게 해줍니다.\n\n[ VIT ]\n- HP와 회복력에 영향을 줍니다.\n\n[ INT ]\n- 마법 공격력에 영향을 줍니다.\n\n[ DEX ]\n-
        }
    }
}