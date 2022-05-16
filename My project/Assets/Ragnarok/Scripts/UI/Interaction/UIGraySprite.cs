using UnityEngine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    public class UIGraySprite : UISprite
    {
        public enum SpriteMode
        {
            /// <summary>
            /// 일반
            /// </summary>
            None,

            /// <summary>
            /// 회색조
            /// </summary>
            Grayscale,
        }

        [HideInInspector, SerializeField]
        SpriteMode mode;

        public SpriteMode Mode
        {
            get { return mode; }
            set
            {
                if (mode == value)
                    return;

                mode = value;

                if (panel != null)
                {
                    panel.RemoveWidget(this);
                    panel = null;
                }

                CreatePanel();
            }
        }

        public override Shader shader
        {
            get
            {
                if (mode == SpriteMode.None)
                    return base.shader;

                return Shader.Find("Unlit/GrayScale");
            }
            set { base.shader = value; }
        }
    }
}