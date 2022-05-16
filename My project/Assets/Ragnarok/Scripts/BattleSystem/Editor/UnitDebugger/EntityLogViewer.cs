using System.Text;
using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public sealed class EntityLogViewer
    {
        Vector2 scrollPosition;

        private EntityLog log;

        public void OnGUI(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            {
                using (var gui = new GUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = gui.scrollPosition;

                    using (new GUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(log == null))
                        {
                            if (GUILayout.Button("클립보드로 복사", GUILayout.ExpandWidth(false)))
                                CopyToClipboard(log);
                        }
                    }

                    if (log == null)
                        return;

                    for (int i = 0; i < log.Count; i++)
                    {
                        GUILayout.Label(log[i]);
                    }
                }
            }
        }

        public void SetLog(EntityLog log)
        {
            this.log = log;
        }

        /// <summary>
        /// 클립보드에 복사
        /// </summary>
        private void CopyToClipboard(EntityLog log)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < log.Count; i++)
            {
                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append(log[i]);
            }

            // Copy To Clipboard
            GUIUtility.systemCopyBuffer = sb.ToString();
        }
    }
}