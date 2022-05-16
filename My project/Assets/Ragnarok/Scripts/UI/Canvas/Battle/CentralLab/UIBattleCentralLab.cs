using Ragnarok.View;
using Ragnarok.View.Skill;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleCentralLab : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIGrid grid;
        [SerializeField] UIItemSkillInfo[] skills;
        [SerializeField] UIAniProgressBar point;
        [SerializeField] UILabelHelper labelPoint;

        public event System.Action<int> OnSelect;

        private int pointValue;

        protected override void OnInit()
        {
            foreach (var item in skills)
            {
                item.OnSelect += OnSelectItemSkill;
            }
        }

        protected override void OnClose()
        {
            foreach (var item in skills)
            {
                item.OnSelect -= OnSelectItemSkill;
            }
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            UpdatePointText();
        }

        void OnSelectItemSkill(int skillId)
        {
            OnSelect?.Invoke(skillId);
        }

        public void SetItemSkills(UIItemSkillInfo.IInput[] inputs)
        {
            int dataCount = inputs == null ? 0 : inputs.Length;
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i].SetData(i < dataCount ? inputs[i] : null);
            }

            grid.Reposition();
        }

        public void SetPoint(int cur, int max)
        {
            if (cur == 0)
            {
                point.Set(cur, max);
            }
            else
            {
                point.Tween(cur, max);
            }

            float progress = MathUtils.GetProgress(cur, max);
            pointValue = Mathf.RoundToInt(progress * 100f);
            UpdatePointText();
        }

        private void UpdatePointText()
        {
            labelPoint.Text = LocalizeKey._48318.ToText() // [84ACEB]Item[-] {VALUE}%
                .Replace(ReplaceKey.VALUE, pointValue);
        }

        public override bool Find()
        {
            base.Find();

            skills = GetComponentsInChildren<UIItemSkillInfo>();
            return true;
        }
    }
}