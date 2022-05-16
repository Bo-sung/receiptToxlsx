using UnityEngine;
using Ragnarok.Text;

namespace Ragnarok
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UILabel), typeof(BoxCollider2D))]
    public class OpenTooltipOnClick : MonoBehaviour
    {
        private static readonly char[] separator = { ':' };

        private const string CONTENT_SKILL = "skill";
        private const string CONTENT_CUPET = "cupet";

        UILabel label;

        public event System.Action OnShowTooltip;

        void Awake()
        {
            label = GetComponent<UILabel>();

            if (!label.autoResizeBoxCollider)
            {
                label.autoResizeBoxCollider = true;
                label.ResizeCollider();
            }
        }

        void OnClick()
        {
            string url = label.GetUrlAtPosition(UICamera.lastWorldPosition);
            if (string.IsNullOrEmpty(url))
                return;

            string[] results = url.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
            if (results.Length < 2)
                return;

            TextLinkType type = results[0].ToLinkType();
            switch (type)
            {
                case TextLinkType.Item:
                    break;

                case TextLinkType.Skill:
                    UI.Show<UISkillTooltip>(new UISkillTooltip.Input(int.Parse(results[1]), skillLevel: 1));
                    break;

                case TextLinkType.Monster:
                    break;

                case TextLinkType.Content:
                    string contentName = results[1];
                    switch (contentName)
                    {
                        case CONTENT_SKILL:
                            UI.Show<UISkill>();
                            break;

                        case CONTENT_CUPET:
                            UI.Show<UICupet>();
                            break;

                        default:
                            Debug.LogError($"[정의되지 않은 {nameof(TextLinkType.Content)}] {nameof(contentName)} = {contentName}");
                            break;
                    }
                    break;

                case TextLinkType.CrowdControl:
                    var cCType = (CrowdControlType)System.Enum.Parse(typeof(CrowdControlType), results[1]);
                    UI.Show<UISelectPropertyPopup>().ShowCCView(cCType);
                    break;

                default:
                    Debug.LogError($"[정의되지 않은 {nameof(TextLinkType)}] {nameof(type)} = {type}");
                    break;
            }

            OnShowTooltip?.Invoke();
        }
    }
}