using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class DefineSymbols
    {
        private enum Type
        {
            [Tooltip("Chapter7 던전 데이터로 전투 시작 가능\n[라그나로크/전투/테스트 던전 시작] 참조")]
            TEST_HIDDEN_CHAPTER,
        }

        private readonly GUIContent[] contents;
        private Vector2 scroll;

        public DefineSymbols()
        {
            BetterList<GUIContent> list = new BetterList<GUIContent>();
            foreach (Type type in System.Enum.GetValues(typeof(Type)))
            {
                string text = type.ToString();
                TooltipAttribute attribute = type.GetAttribute<TooltipAttribute>();
                string tooltip = attribute == null ? string.Empty : attribute.tooltip;
                list.Add(EditorGUIUtility.TrTextContent(text, tooltip));
            }

            contents = list.ToArray();
        }

        public void Draw()
        {
            using (new EditorGUILayout.ScrollViewScope(scroll))
            {
                foreach (var item in contents)
                {
                    EditorGUILayout.Toggle(item, true);
                }
            }

            if (GUILayout.Button(nameof(Apply)))
                Apply();
        }

        private void Apply()
        {

        }
    }
}