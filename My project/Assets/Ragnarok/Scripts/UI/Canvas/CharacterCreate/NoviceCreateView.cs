using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UICharacterCreate"/>
    /// </summary>
    public class NoviceCreateView : UIView, IInspectorFinder
    {
        [SerializeField] UILabelHelper labelSTR;
        [SerializeField] UILabelHelper labelINT;
        [SerializeField] UILabelHelper labelAGI;
        [SerializeField] UILabelHelper labelLUK;
        [SerializeField] UILabelHelper labelVIT;
        [SerializeField] UILabelHelper labelDEX;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelValue labelJobName;
        [SerializeField] UILabelHelper labelJobDescription;
        [SerializeField] UILabelHelper labelSkillTitle;
        [SerializeField] UIInput inputName;
        [SerializeField] UIButtonHelper btnRandom;
        [SerializeField] UIButtonHelper btnMale;
        [SerializeField] UIButtonHelper btnFemale;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnCreate;
        [SerializeField] UISkillPreview[] skillPreviews;
        [SerializeField] UIHexagonSprite hexStatus;
        [SerializeField] UIButtonHelper btnJobPreview;
        [SerializeField] GameObject imageMale, imageFemale;
        [SerializeField] UILabelHelper labelMaleDesc, labelFemaleDesc;
        [SerializeField] UIButtonHelper btnHelpStat; // 스탯 (?)버튼

        public event System.Action<string> OnCreate;
        public event System.Action OnRandom;
        public event System.Action OnMale;
        public event System.Action OnFemale;
        public event System.Action OnJobPreview;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(inputName.onChange, RefreshButtonChangeName);
            EventDelegate.Add(btnRandom.OnClick, OnClickedBtnRandom);
            EventDelegate.Add(btnMale.OnClick, OnClickedBtnMale);
            EventDelegate.Add(btnFemale.OnClick, OnClickedBtnFemale);
            EventDelegate.Add(btnCreate.OnClick, OnClickedBtnCreate);
            EventDelegate.Add(btnJobPreview.OnClick, OnClickedBtnJobPreview);
            EventDelegate.Add(btnHelpStat.OnClick, OnClickedBtnHelpStat);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(inputName.onChange, RefreshButtonChangeName);
            EventDelegate.Remove(btnRandom.OnClick, OnClickedBtnRandom);
            EventDelegate.Remove(btnMale.OnClick, OnClickedBtnMale);
            EventDelegate.Remove(btnFemale.OnClick, OnClickedBtnFemale);
            EventDelegate.Remove(btnCreate.OnClick, OnClickedBtnCreate);
            EventDelegate.Remove(btnJobPreview.OnClick, OnClickedBtnJobPreview);
            EventDelegate.Remove(btnHelpStat.OnClick, OnClickedBtnHelpStat);
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
            labelNotice.LocalKey = LocalizeKey._2009; // 최소 2글자, 최대 8글자이내
            btnCreate.LocalKey = LocalizeKey._2007; // 캐릭터 생성
            btnJobPreview.LocalKey = LocalizeKey._2010; // 전직\n미리보기    
            labelMaleDesc.LocalKey = LocalizeKey._2012; // [4C4A4D]Pick Me![-] [97B5FF]Plz.[-]
            labelFemaleDesc.LocalKey = LocalizeKey._2013; // [97B5FF]함께 모험하자[-] [FF6767]♥[-]
        }

        public void SetJob(Job job)
        {
            jobIcon.SetJobIcon(job.GetJobIcon());
            string jobName = job.GetJobName();
            string jonNameEnglish = job.GetJobName(LanguageType.ENGLISH);
            labelJobName.Title = job.GetJobName();
            labelJobName.Value = jobName.Equals(jonNameEnglish) ? string.Empty : jonNameEnglish;
        }

        public void SetGender(Gender gender)
        {
            imageMale.SetActive(gender == Gender.Male);
            imageFemale.SetActive(gender != Gender.Male);
        }

        public void SetCharacterName(string characterName)
        {
            inputName.value = characterName;
        }

        public void SetJobDescription(string jobDescription)
        {
            labelJobDescription.Text = jobDescription;
        }

        public void SetSkill(UISkillPreview.IInput[] skills)
        {
            int skillLength = skills == null ? 0 : skills.Length;
            for (int i = 0; i < skillPreviews.Length; i++)
            {
                skillPreviews[i].SetData(i < skillLength ? skills[i] : null);
            }
        }

        public void SetStats(float[] progressStats)
        {
            hexStatus.UpdateVertext(progressStats[0], progressStats[1], progressStats[2], progressStats[3], progressStats[4], progressStats[5]);
        }

        bool IsChangeAbleName()
        {
            string charName = inputName.value;

            // 2글자 미만 또는 8글자 초과
            int nameLength = charName.Length;
            if (nameLength < 2 || nameLength > 8)
                return false;

            return true;
        }

        void RefreshButtonChangeName()
        {
            btnCreate.IsEnabled = IsChangeAbleName();
        }

        void OnClickedBtnCreate()
        {
            string errorMessage = FilterUtils.CheckCharacterName(inputName.value);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                UI.ConfirmPopup(errorMessage);
                return;
            }

            OnCreate?.Invoke(inputName.value);
        }

        void OnClickedBtnRandom()
        {
            OnRandom?.Invoke();
        }
        void OnClickedBtnMale()
        {
            OnMale?.Invoke();
        }

        void OnClickedBtnFemale()
        {
            OnFemale?.Invoke();
        }

        void OnClickedBtnJobPreview()
        {
            OnJobPreview?.Invoke();
        }

        void OnClickedBtnHelpStat()
        {
            UI.ConfirmPopupLong(LocalizeKey._2014.ToText()); // [ STR ]\n- 근접 물리 공격력에 영향을 줍니다.\n\n[ AGI ]\n- 더 빠르게 공격하고 자주 회피할 수 있게 해줍니다.\n\n[ VIT ]\n- HP와 회복력에 영향을 줍니다.\n\n[ INT ]\n- 마법 공격력에 영향을 줍니다.\n\n[ DEX ]\n-
        }

        bool IInspectorFinder.Find()
        {
            skillPreviews = GetComponentsInChildren<UISkillPreview>();
            return true;
        }
    }
}