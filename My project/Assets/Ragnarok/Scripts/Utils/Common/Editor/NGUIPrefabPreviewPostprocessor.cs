#undef AUTO_CREATE_ELEMENT_PREFAB_PREVIEW

using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    /**
     * Element 프리팹이 추가 또는 수정되었을 때
     * "Assets/NGUI/Editor/Preview/" 폴더에 썸네일을 자동 만들어 줍니다.
     */
    class NGUIPrefabPreviewPostprocessor : AssetPostprocessor
    {
        private const string ELEMENT_PREFAB_PATH = "Assets/ArtResources/UI/Canvas/Element/";
        private const string NGUI_PREFAB_TOOL_PREVIEW_PATH = "Assets/NGUI/Editor/Preview/{0}.png";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorUtility.ClearProgressBar();
        }

        public static async Task RefreshPreviewTexture()
        {
            DirectoryInfo info = new DirectoryInfo(ELEMENT_PREFAB_PATH);

            if (!info.Exists)
            {
                Debug.LogError($"[NGUIPrefabPreviewPostprocessor] Path 에러 path = {ELEMENT_PREFAB_PATH}");
                return;
            }

            FileInfo[] files = info.GetFiles("*.prefab");
            foreach (var item in files)
            {
                await CheckPath(PathUtils.ConvertToRelativePath(item.FullName));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

#if AUTO_CREATE_ELEMENT_PREFAB_PREVIEW
        private static async Task OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var item in importedAssets)
            {
                await CheckPath(item);
            }

            foreach (var item in deletedAssets)
            {
                await CheckPath(item);
            }

            foreach (var item in movedAssets)
            {
                await CheckPath(item);
            }

            foreach (var item in movedFromAssetPaths)
            {
                await CheckPath(item);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif

        /// <summary>
        /// 파일 체크
        /// </summary>
        private static async Task CheckPath(string path)
        {
            // Element 폴더 안의 내용이 아님
            if (!path.StartsWith(ELEMENT_PREFAB_PATH))
                return;

            FileInfo fileInfo = new FileInfo(path);

            // prefab 확장자가 아님
            if (!fileInfo.Extension.Equals(".prefab"))
                return;

            string fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            string previewPath = string.Format(NGUI_PREFAB_TOOL_PREVIEW_PATH, fileName);

            // 기존 미리보기 제거
            FileUtil.DeleteFileOrDirectory(previewPath);

            // 미리보기 생성
            if (fileInfo.Exists)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                await CreatePreviewTexture(prefab, previewPath);
            }
        }

        /// <summary>
        /// 미리보기 이미지 생성
        /// </summary>
        private static async Task CreatePreviewTexture(GameObject obj, string previewPath)
        {
            if (obj == null || obj.GetComponentInChildren<UIRect>() == null)
                return;

            int cellSize = 80; // 80 or 50
            int dim = (cellSize - 4) * 2;

            // Let's create a basic scene
            GameObject root = EditorUtility.CreateGameObjectWithHideFlags("Preview Root", HideFlags.HideAndDontSave);
            GameObject camGO = EditorUtility.CreateGameObjectWithHideFlags("Preview Camera", HideFlags.HideAndDontSave);

            // Position it far away so that it doesn't interfere with existing objects
            root.transform.position = new Vector3(0f, 0f, 10000f);
            root.layer = obj.layer;

            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;

            cam.renderingPath = RenderingPath.Forward;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);

            // Finally instantiate the prefab as a child of the root
            GameObject child = NGUITools.AddChild(root, obj);

            if (obj.GetComponent<UIPanel>() == null)
                root.AddComponent<UIPanel>();

            // If there is a UIRect present (widgets or panels) then it's an NGUI object
            Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(child.transform);
            Vector3 size = bounds.extents;
            //float objSize = size.magnitude;

            cam.transform.position = bounds.center;
            cam.cullingMask = (1 << root.layer);

            cam.nearClipPlane = -100f;
            cam.farClipPlane = 100f;
            cam.orthographicSize = Mathf.RoundToInt(Mathf.Max(size.x, size.y));

            NGUITools.ImmediatelyCreateDrawCalls(root);

            // 렌더링 전 1초 대기
            EditorUtility.DisplayProgressBar("프리팹 미리보기 저장중", $"{previewPath}... (1/1)", 1f);
            await Task.Delay(System.TimeSpan.FromSeconds(1f));
            EditorUtility.ClearProgressBar();

            // Save Texture
            cam.RenderToTexture(dim, dim);
            cam.SaveRenderTextureAsPNG(previewPath);

            // Clean up everything
            Object.DestroyImmediate(camGO);
            Object.DestroyImmediate(root);
        }
    }
}