using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
#if TEST_HIDDEN_CHAPTER
    public sealed class UnitBattleOptionViewer
    {
        Vector2 scrollPosition;
        UnitEntity entity;
        EditorUnitStatus status;

        public System.Action OnRestart;

        public void OnGUI(Rect rect)
        {
            using (new GUILayout.AreaScope(rect))
            {
                using (var gui = new GUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = gui.scrollPosition;

                    using (new GUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(entity == null))
                        {
                            if (GUILayout.Button(nameof(Restart), GUILayout.ExpandWidth(false)))
                                Restart();

                            if (GUILayout.Button(nameof(Export), GUILayout.ExpandWidth(false)))
                                Export();

                            if (GUILayout.Button(nameof(Import), GUILayout.ExpandWidth(false)))
                                Import();

                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button(nameof(Cancel), GUILayout.ExpandWidth(false)))
                                Cancel();

                            if (GUILayout.Button(nameof(Apply), GUILayout.ExpandWidth(false)))
                                Apply();
                        }
                    }

                    if (entity == null)
                        return;

                    status.Draw();
                }
            }
        }

        public void Repaint()
        {
            if (entity == null)
                return;

            status = entity.battleStatusInfo;
        }

        public void Set(UnitEntity entity)
        {
            this.entity = entity;
            Repaint();
        }

        private void Restart()
        {
            if (!EditorUtility.DisplayDialog(nameof(Restart), "진심?", "ㅇㅇ", "ㄴㄴ"))
                return;

            OnRestart?.Invoke();
        }

        private void Export()
        {
            if (entity == null)
                return;

            string path = EditorUtility.SaveFilePanel("스탯 내보내기", GetDesktopFolder(), entity.GetName(), ".txt");
            if (string.IsNullOrEmpty(path))
                return;

            System.IO.File.WriteAllText(path, status.ToString());
        }

        private void Import()
        {
            if (entity == null)
                return;

            string path = EditorUtility.OpenFilePanel("스탯 가져오기", GetDesktopFolder(), ".txt");
            if (string.IsNullOrEmpty(path))
                return;

            status = System.IO.File.ReadAllText(path);
        }

        private void Cancel()
        {
            Repaint();
        }

        private void Apply()
        {
            if (entity == null)
                return;

            entity.battleStatusInfo.Initialize(status);
        }

        private string GetDesktopFolder()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        }
    }
#endif
}