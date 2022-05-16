using UnityEngine;

namespace Ragnarok
{
    public class UIButtonhWithGrayScale : UIButtonHelper
    {
        [SerializeField] UIGraySprite graySprite;

        public void SetMode(UIGraySprite.SpriteMode mode)
        {
            graySprite.Mode = mode;
        }

        public override bool Find()
        {
            base.Find();

            graySprite = GetComponent<UIGraySprite>();
            return true;
        }
    }
}