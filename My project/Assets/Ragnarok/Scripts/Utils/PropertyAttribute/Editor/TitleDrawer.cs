using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitleDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            TitleAttribute myAttribute = attribute as TitleAttribute;
            position.y += myAttribute.preSpacing;
            position = EditorGUI.IndentedRect(position);

            if (string.IsNullOrEmpty(myAttribute.title))
                return;

            switch (myAttribute.titleLabelType)
            {
                case TitleLabelType.Label:
                    GUI.Label(position, myAttribute.title, EditorStyles.label);
                    break;

                case TitleLabelType.BoldLabel:
                    GUI.Label(position, myAttribute.title, EditorStyles.boldLabel);
                    break;

                case TitleLabelType.MiniLabel:
                    GUI.Label(position, myAttribute.title, EditorStyles.miniLabel);
                    break;

                case TitleLabelType.MiniBoldLabel:
                    GUI.Label(position, myAttribute.title, EditorStyles.miniBoldLabel);
                    break;

                case TitleLabelType.LargeLabel:
                    GUI.Label(position, myAttribute.title, EditorStyles.largeLabel);
                    break;
            }
        }

        public override float GetHeight()
        {
            TitleAttribute myAttribute = attribute as TitleAttribute;

            if (string.IsNullOrEmpty(myAttribute.title))
                return myAttribute.preSpacing;

            return myAttribute.preSpacing + EditorGUIUtility.singleLineHeight + myAttribute.postSpacing;
        }
    }
}