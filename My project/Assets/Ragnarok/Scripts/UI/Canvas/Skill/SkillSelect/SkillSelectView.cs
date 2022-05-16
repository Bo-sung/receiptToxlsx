using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillSelectView : UIView<SkillSelectView.IListener>, IInspectorFinder
    {
        public interface IListener
        {
            void OnSelect(int skillId);
        }

        [SerializeField] UISkillInfoToggleSimple[] skillToggles;
        [SerializeField] UIResizeGrid grid;
        [SerializeField] UILabelHelper labelNotice;

        int noticeLocalKey;

        public string NoticeText
        {
            set { labelNotice.Text = value; }
        }

        public int NoticeLocalKey
        {
            set
            {
                noticeLocalKey = value;
                OnLocalize();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            foreach (var item in skillToggles)
            {
                item.OnSelect += OnSelect;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in skillToggles)
            {
                item.OnSelect -= OnSelect;
            }
        }

        protected override void OnLocalize()
        {
            if (noticeLocalKey > 0)
                labelNotice.LocalKey = noticeLocalKey;
        }

        void OnSelect(int skillId)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnSelect(skillId);
            }
        }

        public void Show(UISkillInfoSelect.IInfo[] skills)
        {
            for (int i = 0; i < skillToggles.Length; i++)
            {
                skillToggles[i].Show(i < skills.Length ? skills[i] : null);
            }

            grid.Reposition();

            OnSelect(skills.Length == 0 ? 0 : skills[0].SkillId); // 첫번째 스킬 선택
        }

        public void SetSelect(int selectedSkillId)
        {
            foreach (var item in skillToggles)
            {
                item.SetSelect(selectedSkillId);
            }
        }

        bool IInspectorFinder.Find()
        {
            skillToggles = GetComponentsInChildren<UISkillInfoToggleSimple>();
            return true;
        }
    }
}