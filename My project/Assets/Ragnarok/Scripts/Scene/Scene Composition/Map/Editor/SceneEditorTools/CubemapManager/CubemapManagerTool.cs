using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public static class CubemapManagerTool
    {
        private const float PADDING = 10f;
        private const float SPACING = 4f;
        private const float WIDTH = 100f;
        private const float HEIGHT = 20f;

        private static bool isActive;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            isActive = IsActive;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public static bool IsActive
        {
            get => EditorPrefs.GetBool(nameof(CubemapManagerTool), defaultValue: false);
            set
            {
                EditorPrefs.SetBool(nameof(CubemapManagerTool), value);

                isActive = value;
                SceneView.RepaintAll();
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isActive)
                return;

            Handles.BeginGUI();
            {
                DrawViewSelector(sceneView);
                DrawCubeTools(sceneView);
            }
            Handles.EndGUI();
        }

        private static void DrawViewSelector(SceneView sceneView)
        {
            Rect rect = new Rect(PADDING, PADDING, WIDTH, HEIGHT);
            if (GUI.Button(rect, "Fix View"))
                FixView(sceneView);

            rect.y += SPACING + HEIGHT;
            if (GUI.Button(rect, "Quarter View"))
                QuarterView(sceneView);
        }

        private static void DrawCubeTools(SceneView sceneView)
        {
            Rect rect = new Rect(sceneView.position.width - PADDING - WIDTH, sceneView.position.height - PADDING - HEIGHT, WIDTH, HEIGHT);
            rect.y -= HEIGHT;
            if (GUI.Button(rect, "Select Parent"))
                CubemapParentSelectEditorWindow.ShowWindow();

            rect.y -= SPACING + HEIGHT;
            if (GUI.Button(rect, "Replace"))
                CubemapReplaceEditorWindow.ShowWindow();

            rect.y -= SPACING + HEIGHT;
            if (GUI.Button(rect, "Spread"))
                CubemapSpreadEditorWindow.ShowWindow();
        }

        private static void FixView(SceneView sceneView)
        {
            Undo.RecordObject(sceneView, "Change SceneView - Top");
            //sceneView.LookAt(sceneView.pivot, Quaternion.Euler(90f, 90f, 180f), sceneView.size, ortho: true);
            sceneView.LookAt(sceneView.pivot, Quaternion.Euler(90f, -180f, 180f), sceneView.size, ortho: true);
        }

        private static void QuarterView(SceneView sceneView)
        {
            Undo.RecordObject(sceneView, "Change SceneView - Quarter");
            sceneView.LookAt(sceneView.pivot, Quaternion.Euler(35f, -135f, 0f), sceneView.size, ortho: false);
        }
    }
}