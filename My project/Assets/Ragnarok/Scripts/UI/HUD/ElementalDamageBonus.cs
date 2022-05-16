using UnityEngine;

namespace Ragnarok
{
    public class ElementalDamageBonus : HUDObject
    {
        [SerializeField] UILabelHelper labelValue;
        [SerializeField] UIGraySprite iconElement;
        [SerializeField] UIPlayTween playTween;

        private static readonly float REVERSE_255 = 1f / 255f;
        private static readonly Color[] COLOR_NEGATIVE = new Color[] { MakeColor(0xBC, 0xBC, 0xBC), MakeColor(0x6D, 0x6D, 0x6D) };
        private static readonly Color[] COLOR_NEUTRAL = new Color[] { MakeColor(0xD4, 0xD4, 0xD4), MakeColor(0x6D, 0x6D, 0x6D) };
        private static readonly Color[] COLOR_FIRE = new Color[] { MakeColor(0xDE, 0x97, 0x93), MakeColor(0x97, 0x3A, 0x27) };
        private static readonly Color[] COLOR_WATER = new Color[] { MakeColor(0x9E, 0xDB, 0xF3), MakeColor(0x32, 0x64, 0x9D) };
        private static readonly Color[] COLOR_WIND = new Color[] { MakeColor(0xD0, 0xF3, 0x98), MakeColor(0x57, 0x83, 0x28) };
        private static readonly Color[] COLOR_EARTH = new Color[] { MakeColor(0xE0, 0xB2, 0x91), MakeColor(0x98, 0x64, 0x29) };
        private static readonly Color[] COLOR_POISON = new Color[] { MakeColor(0xD9, 0xA7, 0xDF), MakeColor(0x5D, 0x56, 0xA8) };
        private static readonly Color[] COLOR_HOLY = new Color[] { MakeColor(0xFF, 0xFD, 0xD3), MakeColor(0xB8, 0xA5, 0x28) };
        private static readonly Color[] COLOR_SHADOW = new Color[] { MakeColor(0x9A, 0xA0, 0xDA), MakeColor(0x36, 0x36, 0x90) };
        private static readonly Color[] COLOR_GHOST = new Color[] { MakeColor(0xFD, 0xD8, 0xFC), MakeColor(0xCC, 0x58, 0xB3) };
        private static readonly Color[] COLOR_UNDEAD = new Color[] { MakeColor(0xCC, 0xDF, 0xFE), MakeColor(0x40, 0x6A, 0xA4) };

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(playTween.onFinished, Release);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(playTween.onFinished, Release);
        }

        public void Initialize(ElementType elementType, float elementRate)
        {
            bool isNegative = elementRate < 0f;
            Color[] gradientColor = GetGradientColor(elementType, isNegative);
            labelValue.uiLabel.gradientTop = gradientColor[0];
            labelValue.uiLabel.gradientBottom = gradientColor[1];
            labelValue.Text = elementRate.ToString("+0%;-0%"); // 0.25123 -> +25%

            iconElement.spriteName = elementType.GetIconName();
            iconElement.Mode = isNegative ? UIGraySprite.SpriteMode.Grayscale : UIGraySprite.SpriteMode.None;

            playTween.Play(true);
        }

        private Color[] GetGradientColor(ElementType elementType, bool isNegative)
        {
            if (isNegative)
                return COLOR_NEGATIVE;

            switch (elementType)
            {
                case ElementType.None:
                case ElementType.Neutral:
                    return COLOR_NEUTRAL;
                case ElementType.Fire:
                    return COLOR_FIRE;
                case ElementType.Water:
                    return COLOR_WATER;
                case ElementType.Wind:
                    return COLOR_WIND;
                case ElementType.Earth:
                    return COLOR_EARTH;
                case ElementType.Poison:
                    return COLOR_POISON;
                case ElementType.Holy:
                    return COLOR_HOLY;
                case ElementType.Shadow:
                    return COLOR_SHADOW;
                case ElementType.Ghost:
                    return COLOR_GHOST;
                case ElementType.Undead:
                    return COLOR_UNDEAD;

                default:
#if UNITY_EDITOR
                    Debug.LogError($"[올바르지 않은 {nameof(ElementType)}] {nameof(elementType)} = {elementType}");
#endif
                    return COLOR_NEUTRAL;
            }
        }

        private static Color MakeColor(int r, int g, int b)
        {
            return new Color(r * REVERSE_255, g * REVERSE_255, b * REVERSE_255);
        }
    }
}