using UnityEngine;

namespace Ragnarok
{
    public class SimpleHudUnitName : HUDObject, IAutoInspectorFinder
    {
        [SerializeField] UILabel labelName;

        protected int nameId = -1;
        protected int level = -1;

        public override void OnDespawn()
        {
            base.OnDespawn();

            nameId = -1;
            level = -1;
        }

        protected override void OnLocalize()
        {
            if (nameId == -1)
                return;

            string text;
            if (level == -1)
            {
                text = nameId.ToText();
            }
            else
            {
                text = LocalizeKey._81000.ToText() // Lv.{LEVEL} {NAME}
                    .Replace(ReplaceKey.NAME, nameId.ToText())
                    .Replace(ReplaceKey.LEVEL, level);
            }

            SetName(text);
        }

        public void Initialize(int nameId, Color color)
        {
            this.nameId = nameId;
            level = -1;

            SetColor(color);
            OnLocalize();
        }

        public void SetColor(Color col)
        {
            labelName.color = col;
        }

        public void SetFontSize(int fontSize)
        {
            labelName.fontSize = fontSize;
        }

        public void SetEffect(UILabel.Effect effectStyle, Color effectColor)
        {
            labelName.effectStyle = effectStyle;
            labelName.effectColor = effectColor;
        }

        protected void SetName(string text)
        {
            labelName.text = text;
        }
    }
}