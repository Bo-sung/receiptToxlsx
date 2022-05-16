using UnityEngine;

namespace Ragnarok
{
    public class UISizeType : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelSize;
        [SerializeField] UISprite background;
        [SerializeField] Color32[] outline = { new Color32(67, 117, 198, 255), new Color32(212, 87, 71, 255) };
        [SerializeField] string[] spriteName = { "Ui_Common_BG_Label_03", "Ui_Common_BG_Label_04" };

        public void Set(UnitSizeType type)
        {
            labelSize.Text = type.ToSizeName();
            switch (type)
            {
                case UnitSizeType.Small:
                case UnitSizeType.Medium:
                    labelSize.Outline = outline[0];
                    background.spriteName = spriteName[0];
                    break;
                case UnitSizeType.Large:
                    labelSize.Outline = outline[1];
                    background.spriteName = spriteName[1];
                    break;
            }
        }
    }
}