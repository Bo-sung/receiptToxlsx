using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public sealed class CubemapParentSelectEditorWindow : EditorWindow
    {
        private const string TITLE = "부모 선택 변경";

        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<CubemapParentSelectEditorWindow>(TITLE, focus: true);
            window.Focus();
            window.Repaint();
            window.Show();
        }

        ParentSelectTreeView treeView;
        Vector2 scrollPosition;

        void OnEnable()
        {
            Selection.selectionChanged += Repaint;
            EditorSceneManager.sceneOpened += OnOpendScene;

            if (treeView == null)
            {
                treeView = new ParentSelectTreeView(OnSelect);
            }
        }

        void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
            EditorSceneManager.sceneOpened -= OnOpendScene;
        }

        void OnDestroy()
        {
            treeView = null;
        }

        void OnGUI()
        {
            const float FOOTER = 32f;
            const float SPACING = 4f;

            using (var gui = new GUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = gui.scrollPosition;
                treeView.OnGUI(new Rect(0f, 0f, position.width, position.height - FOOTER - SPACING));
            }

            using (new GUILayout.AreaScope(new Rect(0f, position.height - FOOTER, position.width, FOOTER)))
            {
                if (GUILayout.Button("Reload", GUILayout.Height(FOOTER)))
                {
                    Reload();
                }
            }
        }

        void OnOpendScene(Scene scene, OpenSceneMode mode)
        {
            Reload();
        }

        private void Reload()
        {
            treeView.Refresh();
        }

        void OnSelect(Transform tf)
        {
            if (tf == null)
            {
                ShowDialog("잘못된 요청입니다.");
                return;
            }

            if (Selection.gameObjects.Length == 0)
            {
                ShowDialog("선택한 Cubemap이 없습니다.");
                return;
            }

            if (!EditorUtility.DisplayDialog(TITLE, $"{tf.name}를 부모로 선택하시겠습니까?", "확인", "취소"))
                return;

            foreach (var item in Selection.gameObjects)
            {
                // 프리팹이 아닌 경우에는 옮기지 않음
                if (PrefabUtility.GetPrefabInstanceStatus(item) != PrefabInstanceStatus.Connected)
                    continue;

                Undo.SetTransformParent(item.transform, tf, "Modify Parent Transform");
                //item.transform.SetParent(tf, worldPositionStays: true);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            ShowDialog("이동 완료!");

            EditorGUIUtility.PingObject(tf);
        }

        private void ShowDialog(string message)
        {
            EditorUtility.DisplayDialog(TITLE, message, "확인");
        }

        private sealed class ParentSelectTreeView : TreeView
        {
            private readonly BetterList<Element> itemList;
            private readonly System.Action<Transform> onDoubleClicked;

            public ParentSelectTreeView(System.Action<Transform> onDoubleClicked) : base(new TreeViewState())
            {
                itemList = new BetterList<Element>();
                this.onDoubleClicked = onDoubleClicked;

                Refresh();
            }

            protected override TreeViewItem BuildRoot()
            {
                Element root = new Element(null);
                SetupParentsAndChildrenFromDepths(root, itemList.ToArray());
                return root;
            }

            protected override void DoubleClickedItem(int id)
            {
                TreeViewItem find = FindItem(id, rootItem);
                if (find == null)
                    return;

                if (find is Element item)
                {
                    onDoubleClicked?.Invoke(item.tf);
                }
            }

            protected override bool CanMultiSelect(TreeViewItem item)
            {
                return false;
            }

            public void Refresh()
            {
                itemList.Clear();

                int sceneCount = SceneManager.sceneCount;
                for (int i = 0; i < sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    GameObject[] roots = scene.GetRootGameObjects();
                    foreach (var item in roots)
                    {
                        AddChild(item.transform);
                    }
                }

                SetSelection(System.Array.Empty<int>()); // 선택 초기화

                Reload();
            }

            private void AddChild(Transform tf)
            {
                if (tf == null)
                    return;

                // 프리팹인 경우
                if (PrefabUtility.GetPrefabInstanceStatus(tf) == PrefabInstanceStatus.Connected)
                    return;

                itemList.Add(new Element(tf));

                for (int index = 0; index < tf.childCount; index++)
                {
                    AddChild(tf.GetChild(index));
                }
            }

            private sealed class Element : TreeViewItem
            {
                public readonly Transform tf;

                public Element(Transform input)
                {
                    tf = input;

                    if (tf == null)
                    {
                        id = 0;
                        depth = -1;
                        displayName = "Root";
                    }
                    else
                    {
                        id = tf.GetInstanceID();
                        depth = GetDepth(tf);
                        displayName = tf.name;
                    }
                }

                private int GetDepth(Transform tf)
                {
                    int depth = 0;

                    while (tf.parent)
                    {
                        ++depth;
                        tf = tf.parent;
                    }

                    return depth;
                }
            }
        }
    }
}