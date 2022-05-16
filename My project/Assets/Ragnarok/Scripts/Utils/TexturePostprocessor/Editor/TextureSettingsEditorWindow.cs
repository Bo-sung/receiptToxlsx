using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TextureSettingsEditorWindow : EditorWindow, IHasCustomMenu
    {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            // Texture 타입이 아님
            string assetPath = PathUtils.GetPath(guid, PathUtils.PathType.Relative);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null)
                return;

            Preset preset = TextureSettingsCollection.FindPreset(assetPath);

            // Draw Background
            if (preset != null)
            {
                bool isDataEquals = preset.DataEquals(textureImporter);
                Color savedColor = GUI.color;
                GUI.color = isDataEquals ? new Color(0f, 8f, 0f, 0.4f) : new Color(8f, 0f, 0f, 0.4f);
                GUI.Box(selectionRect, GUIContent.none);
                GUI.color = savedColor;
            }

            // Settings Button
            GUIContent content = EditorGUIUtility.IconContent("d_Settings");
            Texture texture = content.image;
            Rect rect = selectionRect;
            rect.width = texture.width;
            rect.height = texture.height;
            rect.x = selectionRect.xMax - texture.width;
            rect.y = selectionRect.yMax - texture.height;

            // Draw Settings Button
            if (GUI.Button(rect, GUIContent.none))
            {
                GenericMenu menu = new GenericMenu();
                if (preset == null)
                {
                    menu.AddItem(Style.ADD, false, TextureSettingsCollection.Add, guid); // 세팅 추가
                }
                else
                {
                    menu.AddItem(Style.APPLY_FILE, false, TextureSettingsCollection.Apply, guid); // 세팅 적용
                    menu.AddItem(Style.APPLY_FOLDER, false, TextureSettingsCollection.ApplyFolder, guid); // 세팅 적용 (해당 폴더전체)
                    menu.AddItem(Style.SELECT_PRESET, false, TextureSettingsCollection.SelectPreset, guid); // 프리셋 확인
                    menu.AddSeparator(string.Empty);
                    menu.AddItem(Style.UPDATE, false, TextureSettingsCollection.Add, guid); // 세팅 업데이트
                    menu.AddItem(Style.REMOVE, false, TextureSettingsCollection.Remove, guid); // 세팅 제거
                }

                menu.AddSeparator(string.Empty);
                menu.AddItem(Style.SELECT_COLLECTION_DATA, false, TextureSettingsCollection.SelectCollectionData); // 관리자 데이터 위치 확인
                menu.AddSeparator(string.Empty);
                menu.AddItem(Style.OPEN_BROWSER, false, ShowWindow); // 세팅 관리자
                menu.ShowAsContext();
            }
            GUI.DrawTexture(rect, texture);
        }

        private static void ShowWindow()
        {
            EditorWindow window = GetWindow<TextureSettingsEditorWindow>();
            window.titleContent = Style.TITLE;
        }

        HorizontalSplitter splitter;
        TextureSettingsTreeView treeView;
        PresetViewer presetViewer;

        private bool isInvalid;

        void OnEnable()
        {
            Initialize();
            Reload();
        }

        void OnGUI()
        {
            if (isInvalid)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.VerticalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Style.CREATE, GUILayout.Width(150f)))
                        {
                            TextureSettingsCollection.Create();
                            Reload();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.FlexibleSpace();
                }
            }
            else
            {
                splitter.OnGUI(position);
                treeView.OnGUI(splitter[0]);
                presetViewer.OnGUI(splitter[1]);
            }
        }

        private void Initialize()
        {
            if (splitter == null)
            {
                splitter = new HorizontalSplitter(Repaint);
            }

            if (treeView == null)
            {
                treeView = new TextureSettingsTreeView { onSingleClicked = OnSelect, onContextClicked = OnContextClicked, onDoubleClicked = OnDoubleClicked };
            }

            if (presetViewer == null)
            {
                presetViewer = new PresetViewer();
            }
        }

        /// <summary>
        /// 선택
        /// </summary>
        void OnSelect(TextureSettingsCollection.Settings settings)
        {
            presetViewer.SetData(settings.preset);
        }

        /// <summary>
        /// 오른쪽 클릭
        /// </summary>
        void OnContextClicked(TextureSettingsCollection.Settings settings)
        {
            string guid = settings.guid;

            GenericMenu menu = new GenericMenu();
            menu.AddItem(Style.APPLY_FILE, false, TextureSettingsCollection.Apply, guid); // 세팅 적용
            menu.AddItem(Style.APPLY_FOLDER, false, TextureSettingsCollection.ApplyFolder, guid); // 세팅 적용 (해당 폴더전체)
            menu.AddItem(Style.SELECT_PRESET, false, TextureSettingsCollection.SelectPreset, guid); // 프리셋 확인
            menu.AddSeparator(string.Empty);
            menu.AddItem(Style.REMOVE, false, TextureSettingsCollection.Remove, guid); // 세팅 제거
            menu.AddSeparator(string.Empty);
            menu.AddItem(Style.SELECT_COLLECTION_DATA, false, TextureSettingsCollection.SelectCollectionData); // 관리자 데이터 위치 확인
            menu.AddSeparator(string.Empty);
            menu.AddItem(Style.OPEN_BROWSER, false, ShowWindow); // 세팅 관리자
            menu.ShowAsContext();
        }

        /// <summary>
        /// 더블 클릭
        /// </summary>
        void OnDoubleClicked(TextureSettingsCollection.Settings settings)
        {
            Object obj = PathUtils.GetObject(settings.guid, typeof(Object));
            PingObject(obj);
        }

        /// <summary>
        /// 다시로드
        /// </summary>
        private void Reload()
        {
            Debug.Log("Reload");

            TextureSettingsCollection collection = TextureSettingsCollection.Get();
            isInvalid = collection == null;

            if (isInvalid)
                return;

            treeView.SetData(collection.settingsList);
            presetViewer.SetData(null);
        }

        private void PingObject(Object obj)
        {
            if (obj == null)
                return;

            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(Style.RELOAD, false, Reload);
            menu.AddItem(Style.SELECT_COLLECTION_DATA, false, TextureSettingsCollection.SelectCollectionData); // 관리자 데이터 위치 학인
            menu.AddItem(Style.ALL_APPLY, false, TextureSettingsCollection.AllApply); // 모든 세팅 적용
        }

        private class Style
        {
            public static readonly GUIContent TITLE = new GUIContent(TextureSettingsCollection.TITLE);
            public static readonly GUIContent CREATE = new GUIContent("세팅 관리자 생성");

            public static readonly GUIContent ADD = new GUIContent("세팅 추가");
            public static readonly GUIContent UPDATE = new GUIContent("세팅 업데이트");

            public static readonly GUIContent APPLY_FILE = new GUIContent("프리셋 적용");
            public static readonly GUIContent APPLY_FOLDER = new GUIContent("프리셋 적용 (해당 폴더전체)");
            public static readonly GUIContent SELECT_PRESET = new GUIContent("프리셋 위치 확인");
            public static readonly GUIContent REMOVE = new GUIContent("세팅 제거");

            public static readonly GUIContent OPEN_BROWSER = new GUIContent("세팅 관리자 브라우저 열기");
            public static readonly GUIContent SELECT_COLLECTION_DATA = new GUIContent("관리자 데이터 위치 확인");
            public static readonly GUIContent ALL_APPLY = new GUIContent("모든 세팅 적용");

            public static readonly GUIContent RELOAD = new GUIContent("새로고침");
        }
    }
}