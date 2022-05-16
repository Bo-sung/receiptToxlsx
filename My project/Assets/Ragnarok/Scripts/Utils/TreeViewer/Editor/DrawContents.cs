using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class ContentDrawer : System.IDisposable
    {
        private static readonly ObjectPool<ContentDrawer> pool = new ObjectPool<ContentDrawer>(null, null);

        public static ContentDrawer Default
        {
            get
            {
                ContentDrawer current = pool.Get();
                current.BeginContents();
                return current;
            }
        }

        void System.IDisposable.Dispose()
        {
            EndContents();
            pool.Release(this);
        }

        private void BeginContents()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        protected void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }
    }
}