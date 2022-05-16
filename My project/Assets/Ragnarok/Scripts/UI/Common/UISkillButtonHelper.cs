using UnityEngine;

namespace Ragnarok
{
    public sealed class UISkillButtonHelper : UIButtonHelper
    {
        [SerializeField] UITextureHelper icon;
        [SerializeField] UISprite progress;

        public string Icon
        {
            set
            {
                if (icon)
                {
                    icon.SetSkill(value, isAsync: false);
                }
            }
        }

        public float Progress
        {
            set
            {
                if (progress)
                    progress.fillAmount = value;
            }
        }

        public void SetMode(UIGraySprite.SpriteMode mode)
        {
            icon.Mode = mode;
        }

        public void SetColor(Color color)
        {
            icon.color = color;
        }
    }
}