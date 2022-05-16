using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class UIJobSkillList : MonoBehaviour, IInspectorFinder
    {
        private enum Mode
        {
            Lock,
            Unlock,
        }

        [SerializeField] GameObject locked;
        [SerializeField] UILabelHelper labelLocked;
        [SerializeField] GameObject unlock;
        [SerializeField] UILabelHelper labelJobName;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UISkillInfoToggle[] skillToggles;

        public event UISkillInfoSelect.SelectEvent OnSelectSkill;

        void Awake()
        {
            foreach (var item in skillToggles)
            {
                item.OnSelect += OnSelect;
            }
        }

        void OnDestroy()
        {
            foreach (var item in skillToggles)
            {
                item.OnSelect -= OnSelect;
            }
        }

        void OnSelect(int skillId)
        {
            OnSelectSkill?.Invoke(skillId);
        }

        public void ShowUnlock(string jobIconName, string jobName, UISkillInfoToggle.IInfo[] skills)
        {
            SetMode(Mode.Unlock); // Unlock

            labelJobName.Text = jobName;
            jobIcon.Set(jobIconName);
            for (int i = 0; i < skillToggles.Length; i++)
            {
                skillToggles[i].Show(i < skills.Length ? skills[i] : null);
            }
        }

        public void ShowLock(int jobGrade)
        {
            SetMode(Mode.Lock); // Lock

            int jobLevel = BasisType.JOB_MAX_LEVEL.GetInt(jobGrade);
            int maxLevel = BasisType.MAX_JOB_LEVEL_STAGE_BY_SERVER.GetInt(ConnectionManager.Instance.GetSelectServerGroupId());

            if (maxLevel < jobLevel)
            {
                labelLocked.Text = LocalizeKey._39024.ToText() // {VALUE}차 전직 업데이트 예정입니다.
                    .Replace(ReplaceKey.VALUE, jobGrade)
                    .Replace(ReplaceKey.LEVEL, jobLevel);
            }
            else
            {
                labelLocked.Text = LocalizeKey._39006.ToText() // {VALUE}차 전직 후 개방됩니다
                    .Replace(ReplaceKey.VALUE, jobGrade)
                    .Replace(ReplaceKey.LEVEL, jobLevel);
            }
        }

        public void SetSelect(int selectedSkillId)
        {
            foreach (var item in skillToggles)
            {
                item.SetSelect(selectedSkillId);
            }
        }

        public void SetParent(UIPanel panel)
        {
            foreach (var item in skillToggles)
            {
                item.SetParent(panel);
            }
        }

        public void ResetParent()
        {
            foreach (var item in skillToggles)
            {
                item.ResetParent();
            }
        }

        /// <summary>
        /// 드래그 가능 여부 설정
        /// </summary>
        public void SetDraggable(bool isDraggable)
        {
            foreach (var skill in skillToggles)
            {
                skill.SetDraggable(isDraggable);
            }
        }

        private void SetMode(Mode type)
        {
            locked.SetActive(type == Mode.Lock);
            labelLocked.SetActive(type == Mode.Lock);
            unlock.SetActive(type == Mode.Unlock);
        }

        bool IInspectorFinder.Find()
        {
            skillToggles = GetComponentsInChildren<UISkillInfoToggle>();
            return true;
        }

        #region Tutorial
        public UIWidget GetWidget(int index)
        {
            return skillToggles[index].GetComponent<UIWidget>();
        }
        #endregion
    }
}