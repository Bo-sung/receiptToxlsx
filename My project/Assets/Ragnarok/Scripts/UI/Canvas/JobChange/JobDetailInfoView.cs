using UnityEngine;

namespace Ragnarok.View
{
    public class JobDetailInfoView : UIView
    {
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
        [SerializeField] UISkillPreview[] skillPreviews;
        [SerializeField] UIButtonHelper btnPrevious;
        [SerializeField] UIButtonHelper btnNext;
        [SerializeField] UIHexagonSprite hexStatus;
        [SerializeField] UIButtonHelper btnHelpStat; // 스탯 (?)버튼
        [SerializeField] UIButtonHelper btnConfirm;

        private Gender gender;
        protected Job job, preJob, nextJob;

        public event System.Action<Job> OnSelectJob;
        public event System.Action<Job> OnConfirm;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnPrevious.OnClick, OnClickedBtnPrevious);
            EventDelegate.Add(btnNext.OnClick, OnClickedBtnNext);
            EventDelegate.Add(btnHelpStat.OnClick, OnClickedBtnHelpStat);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnPrevious.OnClick, OnClickedBtnPrevious);
            EventDelegate.Remove(btnNext.OnClick, OnClickedBtnNext);
            EventDelegate.Remove(btnHelpStat.OnClick, OnClickedBtnHelpStat);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnLocalize()
        {
            labelSTR.LocalKey = LocalizeKey._4004; // STR
            labelINT.LocalKey = LocalizeKey._4007; // INT
            labelAGI.LocalKey = LocalizeKey._4005; // AGI
            labelLUK.LocalKey = LocalizeKey._4009; // LUX
            labelVIT.LocalKey = LocalizeKey._4006; // VIT
            labelDEX.LocalKey = LocalizeKey._4008; // DEX
            labelSkillTitle.LocalKey = LocalizeKey._2008; // 캐릭터 스킬
        }

        public void Initialize(Gender gender)
        {
            this.gender = gender;
        }

        public void SetJob(Job job, Job preJob, Job nextJob, string jobDescription, UISkillPreview.IInput[] skills, float[] progressStats)
        {
            this.job = job;
            this.preJob = preJob;
            this.nextJob = nextJob;

            jobIcon.SetJobIcon(job.GetJobIcon());

            string jobName = job.GetJobName();
            string jonNameEnglish = job.GetJobName(LanguageType.ENGLISH);
            labelJobName.Title = job.GetJobName();
            labelJobName.Value = jobName.Equals(jonNameEnglish) ? string.Empty : jonNameEnglish;

            labelJobDescription.Text = jobDescription;
            int skillLength = skills == null ? 0 : skills.Length;
            for (int i = 0; i < skillPreviews.Length; i++)
            {
                skillPreviews[i].SetData(i < skillLength ? skills[i] : null);
            }
            curCharImage.SetJobSD(job.GetJobSDName(gender));
            preCharImage.SetJobSD(preJob.GetJobSDName(gender));
            nextCharImage.SetJobSD(nextJob.GetJobSDName(gender));
            hexStatus.UpdateVertext(progressStats[0], progressStats[1], progressStats[2], progressStats[3], progressStats[4], progressStats[5]);
        }

        public void SetBtnConfirmText(string text)
        {
            btnConfirm.Text = text;
        }

        void OnClickedBtnPrevious()
        {
            OnSelectJob?.Invoke(preJob);
        }

        void OnClickedBtnNext()
        {
            OnSelectJob?.Invoke(nextJob);
        }

        void OnClickedBtnHelpStat()
        {
            UI.ConfirmPopupLong(LocalizeKey._2014.ToText()); // [ STR ]\n- 근접 물리 공격력에 영향을 줍니다.\n\n[ AGI ]\n- 더 빠르게 공격하고 자주 회피할 수 있게 해줍니다.\n\n[ VIT ]\n- HP와 회복력에 영향을 줍니다.\n\n[ INT ]\n- 마법 공격력에 영향을 줍니다.\n\n[ DEX ]\n-
        }

        void OnClickedBtnConfirm()
        {
            OnConfirm?.Invoke(job);
        }
    }
}