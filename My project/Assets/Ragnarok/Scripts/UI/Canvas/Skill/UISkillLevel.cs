using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class UISkillLevel : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] GameObject goBaseLevel;
        [SerializeField] UILabelHelper labelLevel;
        [SerializeField] GameObject goBaseMax;
        [SerializeField] UILabelHelper labelMax;

        UISkillLevelInfoSelect.IInfo info;

        void Awake()
        {
            UI.AddEventLocalize(OnLocalize);
        }

        void OnDestroy()
        {
            UI.RemoveEventLocalize(OnLocalize);
        }

        void OnLocalize()
        {
            UpdateLevelText();
            labelMax.LocalKey = LocalizeKey._48317; // MAX
        }

        public void SetData(UISkillLevelInfoSelect.IInfo info)
        {
            this.info = info;
            Refresh();
        }

        private void Refresh()
        {
            if (info == null)
                return;

            NGUITools.SetActive(goBaseLevel, !info.IsMaxLevel);
            NGUITools.SetActive(goBaseMax, info.IsMaxLevel);
            UpdateLevelText();
        }

        private void UpdateLevelText()
        {
            if (info == null)
                return;

            labelLevel.Text = LocalizeKey._48316.ToText() // Lv.{LEVEL}
                .Replace(ReplaceKey.LEVEL, info.SkillLevel);
        }
    }
}