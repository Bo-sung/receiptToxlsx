using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public sealed class CubemapReplaceEditorWindow : EditorWindow
    {
        private const string TITLE = "큐브맵 변경";

        public static void ShowWindow()
        {
            EditorWindow window = GetWindow<CubemapReplaceEditorWindow>(TITLE, focus: true);
            window.Focus();
            window.Repaint();
            window.Show();
        }

        private BetterList<DirectoryInfo> directoryList;
        private BetterList<GameObject> cloneList;
        private DirectoryInfo directoryInfo;
        private DirectoryInfo assetDirectoryInfo;
        private DirectoryInfo currentDirectoryInfo;
        private string warningMessage;
        private string message;

        private Vector2 scrollPosition;
        private Texture textureFolder;
        private Texture textureDefaultPrefab;

        void OnEnable()
        {
            Selection.selectionChanged += Refresh;
            Selection.selectionChanged += Repaint;
            EditorSceneManager.sceneOpened += OnOpendScene;

            if (directoryList == null)
                directoryList = new BetterList<DirectoryInfo>();

            if (cloneList == null)
                cloneList = new BetterList<GameObject>();

            if (assetDirectoryInfo == null)
                assetDirectoryInfo = new DirectoryInfo(Application.dataPath);

            textureFolder = EditorGUIUtility.FindTexture("Folder Icon");
            textureDefaultPrefab = EditorGUIUtility.FindTexture("Prefab Icon");

            Refresh();
        }

        void OnDisable()
        {
            Selection.selectionChanged -= Refresh;
            Selection.selectionChanged -= Repaint;
            EditorSceneManager.sceneOpened -= OnOpendScene;
        }

        void OnDestroy()
        {
            directoryList.Clear();
            cloneList.Clear();
            directoryInfo = null;
            assetDirectoryInfo = null;
            currentDirectoryInfo = null;
        }

        void OnGUI()
        {
            const float HEADER = 32f;
            const float SUB_HEADER = 24f;
            const float SPACING = 4f;
            const float CELL_SIZE = 60f;
            const float LABEL_SIZE = 12f;

            // Heaer
            Rect rect = new Rect(0f, 0f, position.width, HEADER);
            using (new GUILayout.AreaScope(rect))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    EditorGUILayout.HelpBox(message, MessageType.Info);
                }

                if (!string.IsNullOrEmpty(warningMessage))
                {
                    EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
                }

                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    Refresh();
                }
            }

            // SubHeader
            using (new GUILayout.AreaScope(new Rect(0f, HEADER + SPACING, position.width, SUB_HEADER), GUIContent.none, "ProjectBrowserTopBarBg"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int i = directoryList.size - 1; i >= 0; i--)
                    {
                        bool isLast = i == 0;
                        if (GUILayout.Button(directoryList[i].Name, isLast ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.ExpandWidth(expand: false)))
                        {
                            SelectDirectory(directoryList[i]);
                        }

                        if (i > 0)
                        {
                            GUILayout.Label("|", EditorStyles.miniLabel, GUILayout.ExpandWidth(expand: false));
                        }
                    }
                }
            }

            // Draw Prefabs
            using (new GUILayout.AreaScope(new Rect(0f, HEADER + SPACING + SUB_HEADER + SPACING, position.width, position.height - HEADER - SPACING - SUB_HEADER - SPACING)))
            {
                using (var gui = new EditorGUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = gui.scrollPosition;

                    float x = SPACING;
                    float y = x;
                    float width = position.width - SPACING;
                    float spacingX = CELL_SIZE + SPACING;
                    float spacingY = spacingX + LABEL_SIZE;

                    // Draw Directories
                    if (currentDirectoryInfo != null && currentDirectoryInfo.Exists)
                    {
                        DirectoryInfo[] directories = currentDirectoryInfo.GetDirectories();
                        foreach (var item in directories)
                        {
                            Rect cell = new Rect(x, y, CELL_SIZE, CELL_SIZE);
                            if (GUI.Button(cell, GUIContent.none))
                            {
                                SelectDirectory(item);
                            }

                            GUI.DrawTexture(cell, textureFolder);

                            cell.y += CELL_SIZE;
                            cell.height = LABEL_SIZE;
                            GUI.Label(cell, item.Name, EditorStyles.miniLabel);

                            x += spacingX;
                            if (x + spacingX > width)
                            {
                                y += spacingY;
                                x = SPACING;
                            }
                        }

                        FileInfo[] files = currentDirectoryInfo.GetFiles("*.prefab");
                        foreach (var item in files)
                        {
                            Rect cell = new Rect(x, y, CELL_SIZE, CELL_SIZE);
                            if (GUI.Button(cell, GUIContent.none))
                            {
                                Apply(item);
                            }

                            GUI.DrawTexture(cell, GetTexture(item.FullName));

                            cell.y += CELL_SIZE;
                            cell.height = LABEL_SIZE;
                            GUI.Label(cell, item.Name, EditorStyles.miniLabel);

                            x += spacingX;
                            if (x + spacingX > width)
                            {
                                y += spacingY;
                                x = SPACING;
                            }
                        }

                        GUILayout.Space(y);
                    }
                }
            }
        }

        void OnOpendScene(Scene scene, OpenSceneMode mode)
        {
            Refresh();
        }

        private void Refresh()
        {
            message = string.Empty;
            directoryList.Clear();

            if (Selection.gameObjects.Length == 0)
            {
                warningMessage = "세팅할 Cubemap을 선택하세요.";
                return;
            }

            GameObject selected = Selection.activeGameObject;
            Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(selected);
            if (prefab == null)
            {
                warningMessage = "프리팹이 아닌 Cubemap입니다.";
                return;
            }

            Renderer renderer = selected.GetComponent<Renderer>();
            if (renderer == null)
            {
                warningMessage = "Renderer가 없는 Cubemap입니다.";
                return;
            }

            string path = AssetDatabase.GetAssetPath(prefab);
            directoryInfo = new FileInfo(path).Directory;
            SelectDirectory(directoryInfo);
            warningMessage = string.Empty;
            message = $"Selected: {selected.name}";
        }

        private void SelectDirectory(DirectoryInfo directory)
        {
            currentDirectoryInfo = directory;

            directoryList.Clear();
            AddDirectory(currentDirectoryInfo);
        }

        private void AddDirectory(DirectoryInfo directory)
        {
            if (directory == null)
                return;

            if (!directoryInfo.Exists)
                return;

            // 추가
            directoryList.Add(directory);

            // Assets 마지막
            if (directory.FullName.Equals(assetDirectoryInfo.FullName))
                return;

            AddDirectory(directory.Parent); // 부모 폴더
        }

        private Texture GetTexture(string path)
        {
            string guid = PathUtils.GetGUID(path, PathUtils.PathType.Absolute);
            GameObject go = PathUtils.GetObject<GameObject>(guid);
            if (go == null)
                return textureDefaultPrefab;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer == null)
                return textureDefaultPrefab;

            return renderer.sharedMaterial.mainTexture;
        }

        private void Apply(FileInfo fileInfo)
        {
            int length = Selection.gameObjects.Length;
            if (length == 0)
            {
                ShowDialog("세팅할 Cubemap을 선택하세요.");
                return;
            }

            if (fileInfo == null || !fileInfo.Exists)
            {
                ShowDialog("존재하지 않는 프리팹입니다.");
                return;
            }

            string guid = PathUtils.GetGUID(fileInfo.FullName, PathUtils.PathType.Absolute);
            GameObject go = PathUtils.GetObject<GameObject>(guid);
            if (go == null)
            {
                ShowDialog("잘못된 요청입니다.");
                return;
            }

            Object prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
            if (prefab == null)
            {
                ShowDialog("프리팹 타입이 아닙니다.");
                return;
            }

            if (!EditorUtility.DisplayDialog(TITLE, $"선택한 오브젝트({length}개)를 {fileInfo.Name} 으로 변경하시겠습니까?", "확인", "취소"))
                return;

            // Clone
            foreach (var item in Selection.transforms)
            {
                GameObject clone = PrefabUtility.InstantiatePrefab(prefab, item.parent) as GameObject;
                Undo.RegisterCreatedObjectUndo(clone, "Replace Prefab");
                clone.transform.position = item.position;
                cloneList.Add(clone);
            }

            // Remove
            foreach (var item in Selection.gameObjects)
            {
                Undo.DestroyObjectImmediate(item);
                //NGUITools.DestroyImmediate(item);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            ShowDialog("변경 완료!");

            Selection.objects = cloneList.ToArray();
            cloneList.Clear();
        }

        private void ShowDialog(string message)
        {
            EditorUtility.DisplayDialog(TITLE, message, "확인");
        }
    }
}