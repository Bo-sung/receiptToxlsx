using UnityEngine;

namespace Ragnarok
{
    public class TitleAttribute : PropertyAttribute
    {
        public readonly string title;

        public TitleLabelType titleLabelType = TitleLabelType.BoldLabel;
        public float preSpacing = 8f;
        public float postSpacing = 4f;

        public TitleAttribute()
        {
        }

        public TitleAttribute(string title, bool isSymbol = true)
        {
            this.title = isSymbol ? string.Concat("● ", title) : title;
        }
    }
}