using UnityEngine;

namespace Ragnarok
{
    [System.Obsolete("되도록 사용 후 제거하십시오")]
    public sealed class ForceChangeSiblingIndex : MonoBehaviour
    {
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(ForceChangeSiblingIndex))]
        class ForceChangeSiblingIndexEditor : UnityEditor.Editor
        {
            ForceChangeSiblingIndex mTarget;

            private int index;

            void OnEnable()
            {
                mTarget = target as ForceChangeSiblingIndex;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                UnityEditor.EditorGUILayout.BeginHorizontal();
                {
                    index = UnityEditor.EditorGUILayout.IntField("index", index);

                    if (GUILayout.Button("Change", GUILayout.ExpandWidth(expand: false)))
                        ChangeSiblingIndex();
                }
                UnityEditor.EditorGUILayout.EndHorizontal();
            }

            void ChangeSiblingIndex()
            {
                mTarget.transform.SetSiblingIndex(index);
            }
        }
#endif
    }
}