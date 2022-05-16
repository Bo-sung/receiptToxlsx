using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok.View
{
    public class CentralLabCharacterSelectView : UIView, IInspectorFinder
    {
        [SerializeField] UITextureHelper selectedProfile;
        [SerializeField] UILabelHelper labelSelectedJobName;
        [SerializeField] UITextureHelper selectedJobIcon;
        [SerializeField] UILabelHelper labelJobDescription;
        [SerializeField] UILabelHelper labelBasicSkill;
        [SerializeField] UIGrid skillGrid;
        [SerializeField] UISkillInfoSelect[] skillInfos;
        [SerializeField] UICloneSelector[] clonSelectors;

        public event System.Action<Job> OnSelect;

        private int jobNameId;
        private int jobDescriptionId;

        protected override void Awake()
        {
            base.Awake();

            foreach (var item in skillInfos)
            {
                item.OnSelect += OnSelectSkill;
            }

            foreach (var item in clonSelectors)
            {
                item.OnSelect += OnSelectClone;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in skillInfos)
            {
                item.OnSelect -= OnSelectSkill;
            }

            foreach (var item in clonSelectors)
            {
                item.OnSelect -= OnSelectClone;
            }
        }

        protected override void OnLocalize()
        {
            labelBasicSkill.LocalKey = LocalizeKey._48300; // 기본 스킬
            UpdateSelectJobText();
        }

        void OnSelectSkill(int skillId)
        {
            if (skillId == 0)
                return;

            UI.Show<UISkillTooltip>(new UISkillTooltip.Input(skillId, 1));
        }

        void OnSelectClone(Job job, Gender gender)
        {
            if (job == default)
                return;

            selectedProfile.Set(job.GetJobIllust(gender));
            selectedJobIcon.Set(job.GetJobIcon());

            OnSelect?.Invoke(job);
        }

        /// <summary>
        /// 클론 세팅
        /// </summary>
        public void SetClone(Job[] arrJob, Gender gender)
        {
            int dataCount = arrJob == null ? 0 : arrJob.Length;
            for (int i = 0; i < clonSelectors.Length; i++)
            {
                clonSelectors[i].SetData(i < dataCount ? arrJob[i] : default, gender);
            }
        }

        /// <summary>
        /// 기본 스킬 세팅
        /// </summary>
        public void SetSelectData(int jobNameId, int jobDescriptionId, UISkillInfoSelect.IInfo[] infos)
        {
            this.jobNameId = jobNameId;
            this.jobDescriptionId = jobDescriptionId;

            int dataCount = infos == null ? 0 : infos.Length;
            for (int i = 0; i < skillInfos.Length; i++)
            {
                skillInfos[i].Show(i < dataCount ? infos[i] : null);
            }

            skillGrid.Reposition();
            UpdateSelectJobText();
        }

        /// <summary>
        /// 첫번째 클론 선택
        /// </summary>
        public void SelectFirstClone()
        {
            if (clonSelectors.Length > 0)
                clonSelectors[0].Select();
        }

        /// <summary>
        /// 토글 초기화
        /// </summary>
        public void ResetToggle()
        {
            foreach (var item in clonSelectors)
            {
                item.Unselect();
            }
        }

        private void UpdateSelectJobText()
        {
            labelSelectedJobName.LocalKey = jobNameId;
            labelJobDescription.LocalKey = jobDescriptionId;
        }

        bool IInspectorFinder.Find()
        {
            skillInfos = GetComponentsInChildren<UISkillInfoSelect>();
            clonSelectors = GetComponentsInChildren<UICloneSelector>();
            return true;
        }
    }
}