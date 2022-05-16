using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class UIMenuItems
    {
        private const string CTRL = "%";
        private const string ALT = "&";
        private const string SHIFT = "#";

        private const int LABEL_DEPTH = 1000;

        [MenuItem("라그나로크/UI/기본 버튼 생성 " + ALT + SHIFT + "1")]
        private static void AddButtonHelper()
        {
            GameObject go = NGUIEditorTools.SelectedRoot(true);

            if (go == null)
            {
                Debug.Log("You must select a game object first.");
                return;
            }

            // Sprite 생성
            UISprite sprite = go.AddChild<UISprite>();
            sprite.name = "Btn";
            sprite.atlas = Get<NGUIAtlas>("Assets/ArtResources/UI/Atlas/UICommon/Atlas_Ui_Common.asset");
            sprite.spriteName = "Ui_Common_Btn_01";
            sprite.type = UIBasicSprite.Type.Sliced;
            sprite.applyGradient = false;
            sprite.width = 180;
            sprite.height = 70;
            sprite.depth = CalculateNextDepth(go); // Depth

            GameObject goBase = sprite.gameObject;
            Transform tfBase = goBase.transform;

            // Label 생성
            UILabel label = goBase.AddChild<UILabel>();
            label.name = "Label";
            label.ambigiousFont = Get<Font>("Assets/ArtResources/UI/Font/yg-jalnan.ttf");
            label.text = "Button";
            label.pivot = UIWidget.Pivot.Center;
            label.fontSize = 22;
            label.effectStyle = UILabel.Effect.Outline8;
            label.effectColor = new Color32(50, 88, 142, 205);
            label.overflowMethod = UILabel.Overflow.ShrinkContent;
            label.applyGradient = false;
            label.leftAnchor.Set(tfBase, 0f, 10f);
            label.rightAnchor.Set(tfBase, 1f, -10f);
            label.bottomAnchor.Set(tfBase, 0f, 10f);
            label.topAnchor.Set(tfBase, 1f, -10f);
            label.ResetAnchors();
            label.depth = 100; // Depth
            label.gameObject.AddComponent<UILabelHelper>();

            // 버튼 생성
            NGUITools.AddWidgetCollider(goBase); // 컬라이더 추가
            UIButton button = goBase.AddComponent<UIButton>();
            button.tweenTarget = goBase;
            button.hover = Color.white;
            button.pressed = Color.white;
            button.disabledColor = Color.gray;
            UIButtonScale buttonScale = goBase.AddComponent<UIButtonScale>();
            buttonScale.tweenTarget = tfBase;
            buttonScale.hover = Vector3.one;
            buttonScale.pressed = Vector3.one * 1.1f;
            UIButtonHelper buttonHelper = goBase.AddComponent<UIButtonHelper>();
            InspectorFinderTools.FindFields(buttonHelper); // 변수 자동 찾기

            Selection.activeGameObject = goBase;
        }

        [MenuItem("라그나로크/UI/기본 탭 생성 " + ALT + SHIFT + "2")]
        private static void AddTabHelper()
        {
            GameObject go = NGUIEditorTools.SelectedRoot(true);
            if (go == null)
            {
                Debug.Log("You must select a game object first.");
                return;
            }

            UITabHelper tabHelper = go.AddChild<UITabHelper>();
            tabHelper.name = "Tab";
            UIGrid grid = tabHelper.gameObject.AddComponent<UIGrid>();
            grid.pivot = UIWidget.Pivot.Center;

            GameObject goBase = tabHelper.gameObject;
            int depth = CalculateNextDepth(go);
            for (int i = 0; i < 3; i++)
            {

                UIToggleHelper toggleHelper = goBase.AddChild<UIToggleHelper>();
                toggleHelper.name = string.Format($"Toggle{i}");
                UISprite sprite = toggleHelper.gameObject.AddComponent<UISprite>();
                sprite.atlas = Get<NGUIAtlas>("Assets/ArtResources/UI/Atlas/UICommon/Atlas_Ui_Common.asset");
                sprite.spriteName = "Ui_Common_Btn_03";
                sprite.type = UIBasicSprite.Type.Sliced;
                sprite.applyGradient = false;
                sprite.width = 144;
                sprite.height = 70;
                sprite.depth = depth; // Depth

                GameObject goToggle = toggleHelper.gameObject;

                NGUITools.AddWidgetCollider(goToggle); // 컬라이더 추가

                UISprite highlight = goToggle.AddChild<UISprite>();
                highlight.name = "Highlight";
                highlight.atlas = Get<NGUIAtlas>("Assets/ArtResources/UI/Atlas/UICommon/Atlas_Ui_Common.asset");
                highlight.spriteName = "Ui_Common_Btn_01";
                highlight.type = UIBasicSprite.Type.Sliced;
                highlight.applyGradient = false;
                highlight.width = 144;
                highlight.height = 70;
                highlight.depth = depth + 1; // Depth

                UILabel label = goToggle.AddChild<UILabel>();
                label.ambigiousFont = Get<Font>("Assets/ArtResources/UI/Font/yg-jalnan.ttf");
                label.text = "Tab";
                label.pivot = UIWidget.Pivot.Center;
                label.fontSize = 32;
                label.effectStyle = UILabel.Effect.Outline8;
                label.effectColor = Color.black;
                label.overflowMethod = UILabel.Overflow.ShrinkContent;
                label.applyGradient = false;
                label.leftAnchor.Set(goToggle.transform, 0f, 10f);
                label.rightAnchor.Set(goToggle.transform, 1f, -10f);
                label.bottomAnchor.Set(goToggle.transform, 0f, 10f);
                label.topAnchor.Set(goToggle.transform, 1f, -10f);
                label.depth = 100;
                label.ResetAnchors();
                label.gameObject.AddComponent<UILabelHelper>();

                InspectorFinderTools.FindFields(toggleHelper);
            }
            InspectorFinderTools.FindFields(tabHelper);
            grid.Reposition();
        }

        [MenuItem("라그나로크/UI/무한 스크롤뷰 생성 " + ALT + SHIFT + "3")]
        private static void AddSuperScrollView()
        {
            GameObject go = NGUIEditorTools.SelectedRoot(true);

            if (go == null)
            {
                Debug.Log("You must select a game object first.");
                return;
            }

            // Widget 생성(스크롤 범위 설정)
            UIWidget widget = go.AddChild<UIWidget>();
            widget.name = "SuperScrollView";
            widget.depth = NGUITools.CalculateNextDepth(go); // Depth

            GameObject goBase = widget.gameObject;
            Transform tfBase = goBase.transform;

            int depth = UIPanel.nextUnusedDepth;

            // Scroll View 생성
            UIScrollView scrollView = goBase.AddChild<UIScrollView>();
            scrollView.name = "Scroll View";

            GameObject scrollBase = scrollView.gameObject;

            // Super Scroll List Wrapper 생성
            SuperScrollListWrapper wrapper = scrollBase.AddChild<SuperScrollListWrapper>();
            wrapper.name = "Wrapper";

            NGUITools.AddWidgetCollider(goBase); // 컬라이더 추가
            UIDragScrollView dragScrollView = goBase.AddComponent<UIDragScrollView>();
            dragScrollView.scrollView = scrollView;

            UIPanel panel = scrollBase.GetComponent<UIPanel>();
            panel.depth = depth;
            panel.leftAnchor.Set(tfBase, 0f, 0f);
            panel.rightAnchor.Set(tfBase, 1f, 0f);
            panel.bottomAnchor.Set(tfBase, 0f, 0f);
            panel.topAnchor.Set(tfBase, 1f, 0f);
            panel.ResetAnchors();

            Selection.activeGameObject = goBase;
        }

        [MenuItem("라그나로크/UI/선택 오브젝트 Active 변경 " + ALT + "a")]
        private static void ActivateDeactivate()
        {
            // NGUISelectionTools 의 ActivateDeactivate 참조

            if (Selection.activeTransform == null)
                return;



            GameObject[] gos = Selection.gameObjects;
            bool val = !NGUITools.GetActive(Selection.activeGameObject);
            foreach (GameObject go in gos)
            {
                Undo.RegisterFullObjectHierarchyUndo(go, "선택 오브젝트 Active 변경");
                NGUITools.SetActive(go, val);
            }
        }

        [MenuItem("라그나로크/UI/아틀라스 업그레이드 " + ALT + SHIFT + "5")]
        private static void ReplaceAtlas()
        {
            GameObject go = NGUIEditorTools.SelectedRoot(true);

            var sprites = go.transform.GetComponentsInChildren<UISprite>(true);
            foreach (var item in sprites)
            {
                if (item.atlas.replacement != null)
                {
                    item.atlas = item.atlas.replacement;
                }
            }
        }

        [MenuItem("라그나로크/UI/미리보기 새로고침")]
        private static void RefreshPreviewTexture()
        {
            NGUIPrefabPreviewPostprocessor.RefreshPreviewTexture().WrapNetworkErrors();
        }

        [MenuItem("라그나로크/UI/Depth 자동 조절 " + ALT + SHIFT + "k")]
        private static void AutoAdjestWidgetDepth()
        {
            bool isRecursively = false;

            var objs = Selection.gameObjects;

            foreach (var obj in objs)
            {
                Undo.RegisterFullObjectHierarchyUndo(obj, "Depth 자동 조절");

                int parentDepth = 0;

                if (obj.transform.parent)
                {
                    UIWidget parentWidget = obj.transform.parent.GetComponentInParent<UIWidget>();

                    if (parentWidget)
                        parentDepth = parentWidget.depth;
                }

                if (isRecursively)
                {
                    SetDepthRecursively(obj.transform, parentDepth);
                }
                else
                {
                    UIWidget[] arrWidget = obj.GetComponentsInChildren<UIWidget>(includeInactive: true);

                    foreach (var item in arrWidget)
                    {
                        FixedDepth fixedDepth = item.GetComponent<FixedDepth>();
                        if (fixedDepth)
                        {
                            item.depth = fixedDepth.depth;
                            continue;
                        }

                        if (item is UILabel)
                        {
                            item.depth = LABEL_DEPTH;
                        }
                        else
                        {
                            item.depth = ++parentDepth;
                        }
                    }
                }
            }
        }

        [MenuItem("라그나로크/UI/전체 UI 토글 Group ID 검색")]
        private static void ListToggleGroupIDForWholePrefabs()
        {
            List<GameObject> prefabs = new List<GameObject>();

            string[] files = Directory.GetFiles("Assets/ArtResources/UI/Canvas/Prefabs/", "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].EndsWith(".prefab"))
                    prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(files[i]));
            }

            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();

            HashSet<int> groupIDSet = new HashSet<int>();

            for (int i = 0; i < prefabs.Count; ++i)
            {
                string prefabName = prefabs[i].name;
                var toggle = prefabs[i].GetComponent<UIToggle>();
                groupIDSet.Clear();

                if (toggle != null && !groupIDSet.Contains(toggle.group))
                {
                    list.Add(new KeyValuePair<int, string>(toggle.group, prefabName));
                    groupIDSet.Add(toggle.group);
                }

                var toggles = prefabs[i].GetComponentsInChildren<UIToggle>(true);

                for (int j = 0; j < toggles.Length; ++j)
                {
                    if (!groupIDSet.Contains(toggles[j].group))
                    {
                        list.Add(new KeyValuePair<int, string>(toggles[j].group, prefabName));
                        groupIDSet.Add(toggles[j].group);
                    }
                }
            }

            list.Sort((a, b) =>
            {
                if (a.Key != b.Key)
                    return a.Key - b.Key;
                else
                    return string.Compare(a.Value, b.Value);
            });

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
                sb.AppendFormat($"{list[i].Key} : {list[i].Value}\n");

            Debug.Log(sb.ToString());
        }

        [MenuItem("라그나로크/UI/UI 프리팹 로컬 아틀라스 사용 검색")]
        private static void ListLocalAtlasForWholePrefabs()
        {
            List<GameObject> prefabs = new List<GameObject>();

            string[] files = Directory.GetFiles("Assets/ArtResources/UI/Canvas/Prefabs/", "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; ++i)
            {
                if (files[i].EndsWith(".prefab"))
                    prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(files[i]));
            }

            StringBuilder sb = new StringBuilder();

            INGUIAtlas atlas = Get<NGUIAtlas>("Assets/ArtResources/Local/UILocal/Atlas/Atlas_Ui_Local.asset");

            for (int i = 0; i < prefabs.Count; ++i)
            {
                var sprites = prefabs[i].GetComponentsInChildren<UISprite>(true);

                for (int j = 0; j < sprites.Length; ++j)
                {
                    if (sprites[j].atlas == atlas)
                    {
                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.Append(NGUITools.GetHierarchy(prefabs[i].gameObject));
                        sb.Append(":\t");
                        sb.Append(NGUITools.GetHierarchy(sprites[j].gameObject));
                    }
                }
            }

            Debug.Log(sb.ToString());
        }

        [MenuItem("라그나로크/UI/전체 UI 패널 Depth 검색")]
        private static void ListPanelDepthForWholePrefabs()
        {
            List<GameObject> prefabs = new List<GameObject>();
            Buffer<string> pathBuffer = new Buffer<string>();
            pathBuffer.AddRange(Directory.GetFiles("Assets/ArtResources/Local/Prefabs/", "*.prefab", SearchOption.AllDirectories));
            pathBuffer.AddRange(Directory.GetFiles("Assets/ArtResources/UI/Canvas/Prefabs/", "*.prefab", SearchOption.AllDirectories));

            for (int i = 0; i < pathBuffer.size; ++i)
            {
                prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(pathBuffer[i]));
            }

            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();

            HashSet<int> depthSet = new HashSet<int>();

            for (int i = 0; i < prefabs.Count; ++i)
            {
                string prefabName = prefabs[i].name;
                var panel = prefabs[i].GetComponent<UIPanel>();
                depthSet.Clear();

                if (panel != null && !depthSet.Contains(panel.depth))
                {
                    list.Add(new KeyValuePair<int, string>(panel.depth, prefabName));
                    depthSet.Add(panel.depth);
                }

                var panels = prefabs[i].GetComponentsInChildren<UIPanel>(true);

                for (int j = 0; j < panels.Length; ++j)
                {
                    if (!depthSet.Contains(panels[j].depth))
                    {
                        list.Add(new KeyValuePair<int, string>(panels[j].depth, prefabName));
                        depthSet.Add(panels[j].depth);
                    }
                }
            }

            list.Sort((a, b) =>
            {
                if (a.Key != b.Key)
                    return a.Key - b.Key;
                else
                    return string.Compare(a.Value, b.Value);
            });

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; ++i)
                sb.AppendFormat($"{list[i].Key} : {list[i].Value}\n");

            Debug.Log(sb.ToString());
        }

        [MenuItem("라그나로크/UI/전체 UI TempLocalization 검색")]
        private static void ListTempLocalizationForWholePrefabs()
        {
            List<GameObject> prefabs = new List<GameObject>();
            Buffer<string> pathBuffer = new Buffer<string>();
            pathBuffer.AddRange(Directory.GetFiles("Assets/ArtResources/Local/Prefabs/", "*.prefab", SearchOption.AllDirectories));
            pathBuffer.AddRange(Directory.GetFiles("Assets/ArtResources/UI/Canvas/Prefabs/", "*.prefab", SearchOption.AllDirectories));

            for (int i = 0; i < pathBuffer.size; ++i)
            {
                prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(pathBuffer[i]));
            }

            List<string> list = new List<string>();
            for (int i = 0; i < prefabs.Count; ++i)
            {
                UITempLocalization[] arrFind = prefabs[i].GetComponentsInChildren<UITempLocalization>(includeInactive: true);
                if (arrFind == null || arrFind.Length == 0)
                    continue;

                foreach (var item in arrFind)
                {
                    list.Add(NGUITools.GetHierarchy(item.gameObject));
                }
            }

            if (list.Count == 0)
            {
                Debug.Log("그런거 음슴");
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < list.Count; ++i)
                {
                    if (i > 0)
                        sb.AppendLine();

                    sb.Append(list[i]);
                }

                Debug.Log(sb.ToString());
            }
        }

        [MenuItem("라그나로크/UI/커먼아틀라스 미사용 스프라이트 검색")]
        private static void ListCommonAtlasForWholePrefabs()
        {
            List<GameObject> prefabs = new List<GameObject>();
            Buffer<string> pathBuffer = new Buffer<string>();
            pathBuffer.AddRange(Directory.GetFiles("Assets/ArtResources/Local/Prefabs/", "*.prefab", SearchOption.AllDirectories));
            pathBuffer.AddRange(Directory.GetFiles("Assets/ArtResources/UI/Canvas/Prefabs/", "*.prefab", SearchOption.AllDirectories));

            for (int i = 0; i < pathBuffer.size; ++i)
            {
                prefabs.Add(AssetDatabase.LoadAssetAtPath<GameObject>(pathBuffer[i]));
            }

            StringBuilder sb = new StringBuilder();

            INGUIAtlas atlas = Get<NGUIAtlas>("Assets/ArtResources/UI/Atlas/UICommon/Atlas/Atlas_Ui_Common.asset");

            var spriteList = atlas.spriteList;

            foreach (var item in spriteList)
            {
                int count = 0;
                for (int i = 0; i < prefabs.Count; ++i)
                {
                    var sprites = prefabs[i].GetComponentsInChildren<UISprite>(true);
                    for (int j = 0; j < sprites.Length; ++j)
                    {
                        if (item.name.Equals(sprites[j].spriteName))
                            count++;
                    }

                    var buttons = prefabs[i].GetComponentsInChildren<UIButton>(true);
                    for (int j = 0; j < buttons.Length; ++j)
                    {
                        if (item.name.Equals(buttons[j].normalSprite))
                            count++;

                        if (item.name.Equals(buttons[j].disabledSprite))
                            count++;
                    }
                }

                if (count == 0)
                {
                    sb.AppendLine(item.name);
                }
            }

            List<string> list = new List<string>();
            for (int i = 0; i < prefabs.Count; ++i)
            {
                UITempLocalization[] arrFind = prefabs[i].GetComponentsInChildren<UITempLocalization>(includeInactive: true);
                if (arrFind == null || arrFind.Length == 0)
                    continue;

                foreach (var item in arrFind)
                {
                    list.Add(NGUITools.GetHierarchy(item.gameObject));
                }
            }

            Debug.Log(sb.ToString());
        }

        [MenuItem("CONTEXT/UIGrid/자식이름 자동숫자화")]
        private static void AutoNumberingChild_Grid(MenuCommand command)
        {
            UIGrid grid = command.context as UIGrid;
            Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "자식이름 자동숫자화");

            foreach (Transform tf in grid.transform)
            {
                tf.name = (tf.GetSiblingIndex() + 1).ToString("00");
            }
        }

        [MenuItem("CONTEXT/UIResizeGrid/Execute")]
        private static void Reposition_ResizeGrid(MenuCommand command)
        {
            UIResizeGrid grid = command.context as UIResizeGrid;
            Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Reposition");
            grid.Reposition();
        }

        [MenuItem("CONTEXT/UICircleGrid/자식이름 자동숫자화")]
        private static void AutoNumberingChild_CircleGrid(MenuCommand command)
        {
            UICircleGrid grid = command.context as UICircleGrid;
            Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "자식이름 자동숫자화");

            foreach (Transform tf in grid.transform)
            {
                tf.name = (tf.GetSiblingIndex() + 1).ToString("00");
            }
        }

        [MenuItem("CONTEXT/UISprite/To Gray Sprite", priority = 1)]
        private static void ToGraySprite(MenuCommand command)
        {
            if (command.context is UIGraySprite)
            {
                Debug.LogError("이미 분위기 GraySprite");
                return;
            }

            UISprite sprite = command.context as UISprite;
            UIGraySprite graySprite = sprite.cachedGameObject.AddComponent<UIGraySprite>();

            graySprite.atlas = sprite.atlas;
            graySprite.spriteName = sprite.spriteName;
            graySprite.type = sprite.type;
            graySprite.fillDirection = sprite.fillDirection;
            graySprite.fillAmount = sprite.fillAmount;
            graySprite.invert = sprite.invert;
            graySprite.flip = sprite.flip;
            graySprite.applyGradient = sprite.applyGradient;
            graySprite.gradientTop = sprite.gradientTop;
            graySprite.gradientBottom = sprite.gradientBottom;
            graySprite.color = sprite.color;

            graySprite.pivot = sprite.pivot;
            graySprite.width = sprite.width;
            graySprite.height = sprite.height;
            graySprite.depth = sprite.depth;
            graySprite.autoResizeBoxCollider = sprite.autoResizeBoxCollider;

            graySprite.updateAnchors = sprite.updateAnchors;
            graySprite.leftAnchor = sprite.leftAnchor;
            graySprite.rightAnchor = sprite.rightAnchor;
            graySprite.bottomAnchor = sprite.bottomAnchor;
            graySprite.topAnchor = sprite.topAnchor;

            NGUITools.DestroyImmediate(sprite);
        }

        [MenuItem("CONTEXT/UIGraySprite/To Sprite", priority = 1)]
        private static void ToSprite(MenuCommand command)
        {
            UIGraySprite graySprite = command.context as UIGraySprite;
            UISprite sprite = graySprite.cachedGameObject.AddComponent<UISprite>();

            sprite.atlas = graySprite.atlas;
            sprite.spriteName = graySprite.spriteName;
            sprite.type = graySprite.type;
            sprite.fillDirection = graySprite.fillDirection;
            sprite.fillAmount = graySprite.fillAmount;
            sprite.invert = graySprite.invert;
            sprite.flip = graySprite.flip;
            sprite.applyGradient = graySprite.applyGradient;
            sprite.gradientTop = graySprite.gradientTop;
            sprite.gradientBottom = graySprite.gradientBottom;
            sprite.color = graySprite.color;

            sprite.pivot = graySprite.pivot;
            sprite.width = graySprite.width;
            sprite.height = graySprite.height;
            sprite.depth = graySprite.depth;
            sprite.autoResizeBoxCollider = graySprite.autoResizeBoxCollider;

            sprite.updateAnchors = graySprite.updateAnchors;
            sprite.leftAnchor = graySprite.leftAnchor;
            sprite.rightAnchor = graySprite.rightAnchor;
            sprite.bottomAnchor = graySprite.bottomAnchor;
            sprite.topAnchor = graySprite.topAnchor;

            NGUITools.DestroyImmediate(graySprite);
        }

        [MenuItem("CONTEXT/UIWidget/수치 자동 계산")]
        private static void CalculateWidget(MenuCommand command)
        {
            UIWidget widget = command.context as UIWidget;
            Undo.RegisterFullObjectHierarchyUndo(widget.cachedGameObject, "수치 자동 계산");

#if UNITY_2018_3_OR_NEWER
            GameObject temp = ObjectFactory.CreateGameObject("Temp");
#else
            GameObject temp = new GameObject("Temp");
#endif
            Transform tempParent = temp.transform;
            tempParent.localScale = widget.cachedTransform.localScale;
            for (int i = widget.cachedTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = widget.cachedTransform.GetChild(i);
                child.SetParent(tempParent);
            }

            Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(tempParent);
            Vector3Int center = Vector3Int.RoundToInt(bounds.center);
            Vector3Int size = Vector3Int.RoundToInt(bounds.size);
            widget.cachedTransform.localPosition = center;
            widget.width = size.x;
            widget.height = size.y;
            for (int i = tempParent.childCount - 1; i >= 0; i--)
            {
                Transform child = tempParent.GetChild(i);
                child.SetParent(widget.cachedTransform);
            }
            NGUITools.Destroy(temp);
        }

        [MenuItem("CONTEXT/UIButtonColor/Color 초기화")]
        private static void SetDefaultColor(MenuCommand command)
        {
            UIButtonColor buttonColor = command.context as UIButtonColor;
            Undo.RegisterFullObjectHierarchyUndo(buttonColor.gameObject, "Color 초기화");

            if (buttonColor.tweenTarget)
            {
                UIWidget widget = buttonColor.tweenTarget.GetComponent<UIWidget>();

                if (widget)
                {
                    Color color = widget.color;
                    buttonColor.hover = color;
                    buttonColor.pressed = color;
                }
            }
        }

        private static T Get<T>(string path)
            where T : Object
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Object obj = AssetDatabase.LoadMainAssetAtPath(path);

            if (obj == null)
                return null;

            if (obj is T)
                return obj as T;

            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                if (obj.GetType().Equals(typeof(GameObject)))
                {
                    GameObject go = obj as GameObject;
                    return go.GetComponent(typeof(T)) as T;
                }
            }

            return null;
        }

        private static int CalculateNextDepth(GameObject go)
        {
            if (go)
            {
                int depth = -1;
                UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>();
                for (int i = 0, imax = widgets.Length; i < imax; ++i)
                {
                    if (widgets[i] as UILabel)
                        continue;
                    depth = Mathf.Max(depth, widgets[i].depth);
                }
                return depth + 1;
            }
            return 0;
        }

        /// <summary>
        /// 재귀적으로 Depth 조절
        /// </summary>
        private static void SetDepthRecursively(Transform transform, int depth)
        {
            UIWidget widget = transform.GetComponent<UIWidget>();

            if (widget != null)
            {
                if (widget is UILabel)
                {
                    widget.depth = LABEL_DEPTH;
                }
                else
                {
                    widget.depth = ++depth;
                }
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                SetDepthRecursively(transform.GetChild(i), depth);
            }
        }

        [MenuItem("CONTEXT/UITexture/To UITextureHelper")]
        private static void FromTextureToTextureHelper(MenuCommand command)
        {
            if (command.context is UITextureHelper)
            {
                Debug.LogError("이미 분위기 UITextureHelper");
                return;
            }

            UITexture texture = command.context as UITexture;
            UITextureHelper textureHelper = texture.cachedGameObject.AddComponent<UITextureHelper>();

            Undo.RegisterFullObjectHierarchyUndo(texture.gameObject, "To UITextureHelper");

            SerializedObject soOld = new SerializedObject(texture);
            SerializedObject so = new SerializedObject(textureHelper);
            so.FindProperty("mApplyGradient").boolValue = soOld.FindProperty("mApplyGradient").boolValue;
            so.FindProperty("mGradientTop").colorValue = soOld.FindProperty("mGradientTop").colorValue;
            so.FindProperty("mGradientBottom").colorValue = soOld.FindProperty("mGradientBottom").colorValue;
            so.ApplyModifiedProperties();

            textureHelper.type = texture.type;
            textureHelper.fillDirection = texture.fillDirection;
            textureHelper.fillAmount = texture.fillAmount;
            textureHelper.invert = texture.invert;
            textureHelper.flip = texture.flip;
            textureHelper.color = texture.color;

            textureHelper.pivot = texture.pivot;
            textureHelper.width = texture.width;
            textureHelper.height = texture.height;
            textureHelper.depth = texture.depth;
            textureHelper.autoResizeBoxCollider = texture.autoResizeBoxCollider;

            textureHelper.updateAnchors = texture.updateAnchors;
            textureHelper.leftAnchor = texture.leftAnchor;
            textureHelper.rightAnchor = texture.rightAnchor;
            textureHelper.bottomAnchor = texture.bottomAnchor;
            textureHelper.topAnchor = texture.topAnchor;

            textureHelper.uvRect = texture.uvRect;

            NGUITools.DestroyImmediate(texture);
        }

        [MenuItem("CONTEXT/UISprite/To UITextureHelper", priority = 1000)]
        private static void FromSpriteToTextureHelper(MenuCommand command)
        {
            UISprite sprite = command.context as UISprite;
            UITextureHelper textureHelper = sprite.gameObject.AddComponent<UITextureHelper>();

            Undo.RegisterFullObjectHierarchyUndo(sprite.gameObject, "To UITextureHelper");

            SerializedObject so = new SerializedObject(textureHelper);
            so.FindProperty("mApplyGradient").boolValue = sprite.applyGradient;
            so.FindProperty("mGradientTop").colorValue = sprite.gradientTop;
            so.FindProperty("mGradientBottom").colorValue = sprite.gradientBottom;
            so.ApplyModifiedProperties();

            textureHelper.type = sprite.type;
            textureHelper.fillDirection = sprite.fillDirection;
            textureHelper.fillAmount = sprite.fillAmount;
            textureHelper.invert = sprite.invert;
            textureHelper.flip = sprite.flip;
            textureHelper.color = sprite.color;

            textureHelper.pivot = sprite.pivot;
            textureHelper.width = sprite.width;
            textureHelper.height = sprite.height;
            textureHelper.depth = sprite.depth;
            textureHelper.autoResizeBoxCollider = sprite.autoResizeBoxCollider;

            textureHelper.updateAnchors = sprite.updateAnchors;
            textureHelper.leftAnchor = sprite.leftAnchor;
            textureHelper.rightAnchor = sprite.rightAnchor;
            textureHelper.bottomAnchor = sprite.bottomAnchor;
            textureHelper.topAnchor = sprite.topAnchor;

            UIGraySprite.SpriteMode mode = UIGraySprite.SpriteMode.None;
            if (sprite is UIGraySprite graySprite)
                mode = graySprite.Mode;

            textureHelper.Mode = mode;

            NGUITools.DestroyImmediate(sprite);
        }

        [MenuItem("CONTEXT/UISprite/To Round Sprite", priority = 2)]
        private static void FromSpriteToRoundSprite(MenuCommand command)
        {
            if (command.context is UIRoundSprite)
            {
                Debug.LogError("이미 분위기 UIRoundSprite");
                return;
            }

            UISprite sprite = command.context as UISprite;
            UIRoundSprite roundSprite = sprite.cachedGameObject.AddComponent<UIRoundSprite>();

            roundSprite.atlas = sprite.atlas;
            roundSprite.spriteName = sprite.spriteName;
            roundSprite.type = sprite.type;
            roundSprite.fillDirection = sprite.fillDirection;
            roundSprite.fillAmount = sprite.fillAmount;
            roundSprite.invert = sprite.invert;
            roundSprite.flip = sprite.flip;
            roundSprite.applyGradient = sprite.applyGradient;
            roundSprite.gradientTop = sprite.gradientTop;
            roundSprite.gradientBottom = sprite.gradientBottom;
            roundSprite.color = sprite.color;

            roundSprite.pivot = sprite.pivot;
            roundSprite.width = sprite.width;
            roundSprite.height = sprite.height;
            roundSprite.depth = sprite.depth;
            roundSprite.autoResizeBoxCollider = sprite.autoResizeBoxCollider;

            roundSprite.updateAnchors = sprite.updateAnchors;
            roundSprite.leftAnchor = sprite.leftAnchor;
            roundSprite.rightAnchor = sprite.rightAnchor;
            roundSprite.bottomAnchor = sprite.bottomAnchor;
            roundSprite.topAnchor = sprite.topAnchor;

            NGUITools.DestroyImmediate(sprite);
        }

        [MenuItem("CONTEXT/UIRoundSprite/To Sprite", priority = 2)]
        private static void ToSpriteFromRoundSprite(MenuCommand command)
        {
            UIRoundSprite roundSprite = command.context as UIRoundSprite;
            UISprite sprite = roundSprite.cachedGameObject.AddComponent<UISprite>();

            sprite.atlas = roundSprite.atlas;
            sprite.spriteName = roundSprite.spriteName;
            sprite.type = roundSprite.type;
            sprite.fillDirection = roundSprite.fillDirection;
            sprite.fillAmount = roundSprite.fillAmount;
            sprite.invert = roundSprite.invert;
            sprite.flip = roundSprite.flip;
            sprite.applyGradient = roundSprite.applyGradient;
            sprite.gradientTop = roundSprite.gradientTop;
            sprite.gradientBottom = roundSprite.gradientBottom;
            sprite.color = roundSprite.color;

            sprite.pivot = roundSprite.pivot;
            sprite.width = roundSprite.width;
            sprite.height = roundSprite.height;
            sprite.depth = roundSprite.depth;
            sprite.autoResizeBoxCollider = roundSprite.autoResizeBoxCollider;

            sprite.updateAnchors = roundSprite.updateAnchors;
            sprite.leftAnchor = roundSprite.leftAnchor;
            sprite.rightAnchor = roundSprite.rightAnchor;
            sprite.bottomAnchor = roundSprite.bottomAnchor;
            sprite.topAnchor = roundSprite.topAnchor;

            NGUITools.DestroyImmediate(roundSprite);
        }
    }
}