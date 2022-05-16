using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public sealed class CubemapSpreadEditorWindow : EditorWindow
    {
        private const string TITLE = "큐브맵 세팅";

        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<CubemapSpreadEditorWindow>(utility: true, TITLE, focus: true);
            window.minSize = window.maxSize = Vector2.one * 240f;
            window.Focus();
            window.Repaint();
            window.Show();
        }

        private GameObject selected;
        private Texture texture;
        private Vector3 size;
        private string warningMessage;
        private string message;

        private int left, right, bottom, top;

        private Texture textureDefaultPrefab;

        void OnEnable()
        {
            Selection.selectionChanged += Refresh;
            Selection.selectionChanged += Repaint;
            EditorSceneManager.sceneOpened += OnOpendScene;
            SceneView.duringSceneGui += OnSceneGUI;

            textureDefaultPrefab = EditorGUIUtility.FindTexture("Prefab Icon");

            Refresh();
        }

        void OnDisable()
        {
            Selection.selectionChanged -= Refresh;
            Selection.selectionChanged -= Repaint;
            EditorSceneManager.sceneOpened -= OnOpendScene;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnDestroy()
        {
            selected = null;
            texture = null;
        }

        void OnGUI()
        {
            const float SIZE = 24f;

            const float FOOTER = 32f;
            const float SPACING = 4f;

            if (!string.IsNullOrEmpty(message))
            {
                EditorGUILayout.HelpBox(message, MessageType.Info);
            }

            if (!string.IsNullOrEmpty(warningMessage))
            {
                EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
            }

            GUI.changed = false;

            Rect rect = new Rect(position.width * 0.5f - SIZE * 0.5f, position.height * 0.5f - SIZE * 0.5f, 24f, 24f);
            if (GUI.Button(rect, GUIContent.none))
                Reset();

            GUI.DrawTexture(rect, texture ?? textureDefaultPrefab);

            rect.x -= SIZE + SPACING + SIZE + SPACING;
            if (GUI.Button(rect, Styles.LEFT))
                IncreaseLeft();

            rect.x += SIZE + SPACING;
            left = EditorGUI.IntField(rect, left);

            // Reset
            rect.x += SIZE + SPACING;

            rect.x += SIZE + SPACING + SIZE + SPACING;
            if (GUI.Button(rect, Styles.RIGHT))
                IncreaseRight();

            rect.x -= SIZE + SPACING;
            right = EditorGUI.IntField(rect, right);

            // Reset
            rect.x -= SIZE + SPACING;

            rect.y += SIZE + SPACING + SIZE + SPACING;
            if (GUI.Button(rect, Styles.BOTTOM))
                IncreaseBottom();

            rect.y -= SIZE + SPACING;
            bottom = EditorGUI.IntField(rect, bottom);

            // Reset
            rect.y -= SIZE + SPACING;

            rect.y -= SIZE + SPACING + SIZE + SPACING;
            if (GUI.Button(rect, Styles.TOP))
                IncreaseTop();

            rect.y += SIZE + SPACING;
            top = EditorGUI.IntField(rect, top);

            if (GUI.changed)
            {
                RepaintSceneView();
            }

            GUILayout.FlexibleSpace();

            using (new GUILayout.AreaScope(new Rect(0, position.height - FOOTER, position.width, FOOTER)))
            {
                if (GUILayout.Button("Apply", GUILayout.Height(FOOTER)))
                {
                    Apply();
                }
            }
        }

        void OnOpendScene(Scene scene, OpenSceneMode mode)
        {
            Refresh();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (selected == null)
                return;

            Vector3 center = selected.transform.position;

            for (int i = 1; i <= left; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.left) * i);
                Handles.DrawWireCube(pos, size);

                for (int j = 1; j <= bottom; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.back) * j);
                    Handles.DrawWireCube(pos2, size);
                }

                for (int j = 1; j <= top; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.forward) * j);
                    Handles.DrawWireCube(pos2, size);
                }
            }

            for (int i = 1; i <= right; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.right) * i);
                Handles.DrawWireCube(pos, size);

                for (int j = 1; j <= bottom; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.back) * j);
                    Handles.DrawWireCube(pos2, size);
                }

                for (int j = 1; j <= top; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.forward) * j);
                    Handles.DrawWireCube(pos2, size);
                }
            }

            for (int i = 1; i <= bottom; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.back) * i);
                Handles.DrawWireCube(pos, size);
            }

            for (int i = 1; i <= top; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.forward) * i);
                Handles.DrawWireCube(pos, size);
            }
        }

        private void Refresh()
        {
            selected = null;
            texture = null;
            size = Vector3.zero;
            message = string.Empty;

            if (Selection.gameObjects.Length == 0)
            {
                warningMessage = "세팅할 Cubemap을 선택하세요.";
                return;
            }

            if (Selection.gameObjects.Length > 1)
            {
                warningMessage = "여러개의 Cubemap이 선택되었습니다.";
                return;
            }

            selected = Selection.activeGameObject;
            Renderer renderer = selected.GetComponent<Renderer>();
            if (renderer == null)
            {
                warningMessage = "Renderer가 없는 Cubemap입니다.";
                return;
            }

            texture = renderer.sharedMaterial.mainTexture;
            size = renderer.bounds.size;
            warningMessage = string.Empty;
            message = $"Selected: {selected.name}";
        }

        private void Reset()
        {
            left = right = bottom = top = 0;
            GUI.FocusControl(null);
            RepaintSceneView();
        }

        private void IncreaseLeft()
        {
            ++left;
            GUI.FocusControl(null);
            RepaintSceneView();
        }

        private void IncreaseRight()
        {
            ++right;
            GUI.FocusControl(null);
            RepaintSceneView();
        }

        private void IncreaseBottom()
        {
            ++bottom;
            GUI.FocusControl(null);
            RepaintSceneView();
        }

        private void IncreaseTop()
        {
            ++top;
            GUI.FocusControl(null);
            RepaintSceneView();
        }

        private void RepaintSceneView()
        {
            SceneView.RepaintAll();
        }

        private void Apply()
        {
            if (selected == null)
            {
                ShowDialog("세팅할 Cubemap을 선택하세요.");
                return;
            }

            if (left == 0 && right == 0 && bottom == 0 && top == 0)
            {
                ShowDialog("세팅값이 없습니다.");
                return;
            }

            if (!EditorUtility.DisplayDialog(TITLE, "세팅하시겠습니까?", "확인", "취소"))
                return;

            Vector3 center = selected.transform.position;
            for (int i = 1; i <= left; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.left) * i);
                InstantiateCubemap(pos);

                for (int j = 1; j <= bottom; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.back) * j);
                    InstantiateCubemap(pos2);
                }

                for (int j = 1; j <= top; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.forward) * j);
                    InstantiateCubemap(pos2);
                }
            }

            for (int i = 1; i <= right; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.right) * i);
                InstantiateCubemap(pos);

                for (int j = 1; j <= bottom; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.back) * j);
                    InstantiateCubemap(pos2);
                }

                for (int j = 1; j <= top; j++)
                {
                    Vector3 pos2 = pos + (Vector3.Scale(size, Vector3.forward) * j);
                    InstantiateCubemap(pos2);
                }
            }

            for (int i = 1; i <= bottom; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.back) * i);
                InstantiateCubemap(pos);
            }

            for (int i = 1; i <= top; i++)
            {
                Vector3 pos = center + (Vector3.Scale(size, Vector3.forward) * i);
                InstantiateCubemap(pos);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            ShowDialog("세팅 완료!");
        }

        private void InstantiateCubemap(Vector3 position)
        {
            Transform parent = selected.transform.parent;
            Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(selected);
            GameObject clone;

            if (prefab)
            {
                clone = PrefabUtility.InstantiatePrefab(prefab, selected.transform.parent) as GameObject;
                Undo.RegisterCreatedObjectUndo(clone, "Spread Prefab");
            }
            else
            {
                clone = Instantiate(selected, parent);
                Undo.RegisterCreatedObjectUndo(clone, "Spread Object");
                clone.name = clone.name.Replace("(Clone)", string.Empty); // 이름에서 Clone 제거
            }

            clone.transform.position = position;
        }

        private void ShowDialog(string message)
        {
            EditorUtility.DisplayDialog(TITLE, message, "확인");
        }

        private class Styles
        {
            public static readonly GUIContent LEFT = new GUIContent("◀");
            public static readonly GUIContent RIGHT = new GUIContent("▶");
            public static readonly GUIContent BOTTOM = new GUIContent("▼");
            public static readonly GUIContent TOP = new GUIContent("▲");
        }
    }
}