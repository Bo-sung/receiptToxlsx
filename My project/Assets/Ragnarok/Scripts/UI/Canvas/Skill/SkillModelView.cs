using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillModelView : UIView<SkillModelView.IListener>, IAutoInspectorFinder
    {
        public interface IListener
        {
            void OnInitSkillPoint();
        }

        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelJobName;
        [SerializeField] UILabelHelper labelJobLevel;
        [SerializeField] UILabelHelper labelSkillPointLabel;
        [SerializeField] UILabelHelper labelSkillPoint;
        [SerializeField] UIButtonHelper btnResetSkill;
        [SerializeField] GameObject notice;
        [SerializeField] Color normalColor;
        [SerializeField] Color highlightColor;
        [SerializeField] AnimationCurve skillPointAlphaAnimCurve;
        [SerializeField] float alphaAnimDuration;
        [SerializeField] GameObject skillPointFx;

        int jobLevel;
        int skillPoint;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnResetSkill.OnClick, OnClickedBtnResetSkill);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnResetSkill.OnClick, OnClickedBtnResetSkill);
        }

        float timer = 0;

        private void Update()
        {
            if (skillPoint > 0)
            {
                timer += Time.deltaTime;
                float prog = timer / alphaAnimDuration;
                Color c = highlightColor;
                c.a = skillPointAlphaAnimCurve.Evaluate(prog);
                labelSkillPoint.uiLabel.color = c;
            }
            else
            {
                labelSkillPoint.uiLabel.color = normalColor;
            }
        }

        void OnClickedBtnResetSkill()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnInitSkillPoint();
            }
        }

        protected override void OnLocalize()
        {
            btnResetSkill.LocalKey = LocalizeKey._39001; // 스킬 초기화
            labelSkillPointLabel.LocalKey = LocalizeKey._39023; // Skill Point :
            UpdateJobLevelText();
            UpdateSkillPointText();
        }

        /// <summary>
        /// 직업 표시
        /// </summary>
        public void ShowJob(string icon, string jobName)
        {
            jobIcon.Set(icon);
            labelJobName.Text = jobName;
        }

        /// <summary>
        /// 직업 레벨 표시
        /// </summary>
        public void ShowJobLevel(int jobLevel)
        {
            this.jobLevel = jobLevel;
            UpdateJobLevelText();
        }

        /// <summary>
        /// 스킬 포인트 표시
        /// </summary>
        public void ShowSkillPoint(int skillPoint)
        {
            this.skillPoint = skillPoint;
            UpdateSkillPointText();
            NGUITools.SetActive(skillPointFx, this.skillPoint > 0);
        }

        /// <summary>
        /// 장착 가능한 스킬 
        /// </summary>
        public void SetHasLevelUpSkill(bool hasLevelUpSkill)
        {
            btnResetSkill.IsEnabled = hasLevelUpSkill; // 보유한 스킬이 존재할 경우에만 스킬 초기화 가능
        }

        /// <summary>
        /// 알림 표시
        /// </summary>
        public void SetHasNewSkillPoint(bool hasNewSkillPoint)
        {
            NGUITools.SetActive(notice, hasNewSkillPoint);
        }

        private void UpdateJobLevelText()
        {
            labelJobLevel.Text = LocalizeKey._39003.ToText() // Lv. {LEVEL}
                .Replace(ReplaceKey.LEVEL, jobLevel);
        }

        private void UpdateSkillPointText()
        {
            labelSkillPoint.Text = skillPoint.ToString();
        }
    }
}