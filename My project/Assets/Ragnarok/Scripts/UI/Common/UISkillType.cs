using UnityEngine;

namespace Ragnarok.View
{
    public class UISkillType : MonoBehaviour, IInspectorFinder
    {
        [System.Serializable]
        private struct SkillTypeColor
        {
            public Color background;
            public Color outline;
        }

        [SerializeField] SkillTypeColor colorActive = new SkillTypeColor { background = new Color32(236, 124, 111, 255), outline = new Color32(197, 78, 64, 255) };
        [SerializeField] SkillTypeColor colorPassive = new SkillTypeColor { background = new Color32(122, 216, 108, 255), outline = new Color32(44, 133, 31, 255) };

        [SerializeField] UIWidget background;
        [SerializeField] UILabelHelper label;

        public void Show(SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.Active:
                case SkillType.BasicActiveSkill:
                    label.LocalKey = LocalizeKey._39007; // 액티브
                    background.color = colorActive.background;
                    label.Outline = colorActive.outline;
                    break;

                case SkillType.Passive:
                case SkillType.Plagiarism:
                case SkillType.Reproduce:
                case SkillType.SummonBall:
                case SkillType.RuneMastery:
                    label.LocalKey = LocalizeKey._39008; // 패시브
                    background.color = colorPassive.background;
                    label.Outline = colorPassive.outline;
                    break;

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(skillType)}] {nameof(skillType)} = {skillType.ToString()}");
                    break;
            }
        }

        bool IInspectorFinder.Find()
        {
            background = GetComponent<UIWidget>();
            return true;
        }
    }
}