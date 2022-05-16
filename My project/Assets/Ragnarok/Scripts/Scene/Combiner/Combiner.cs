#if UNITY_EDITOR

using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Text;

namespace EditorTool
{
    public class Combiner
    {
        private class ThreadInput
        {
            public int index;
            public CombinerContext context;
        }

        private class ThreadResult
        {
            public bool result;
        }

        private struct GridPos
        {
            public int x;
            public int y;
        }

        private struct Vertex
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 uv;
            public Color color;

            public static bool operator ==(Vertex a, Vertex b)
            {
                if ((a.position - b.position).sqrMagnitude > 0.001f)
                    return false;
                if ((a.normal - b.normal).sqrMagnitude > 0.001f)
                    return false;
                if ((a.color - b.color).maxColorComponent > 0.001f)
                    return false;
                if (a.uv != b.uv)
                    return false;
                return true;
            }

            public static bool operator !=(Vertex a, Vertex b)
            {
                return !(a == b);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
        }

        private class Pair
        {
            public Vertex vertex;
            public int index;

            public Pair(Vertex vertex, int index)
            {
                this.vertex = vertex;
                this.index = index;
            }
        }

        private class CombinerContext
        {
            public List<MeshFilter> meshFilterList;

            public LinkedList<Triangle> triangleList = new LinkedList<Triangle>();
            public AABBTree aabbTree = new AABBTree(100, ThreadCount);
            public List<Texture2D> textures = new List<Texture2D>();

            public List<IHasAABB>[] threadVarPool = new List<IHasAABB>[ThreadCount];
            public Triangle[] threadTriangles = new Triangle[ThreadCount];
            public ThreadResult[] threadResult = new ThreadResult[ThreadCount]; // 일반 value 형 타입 배열로 하면 IndexOutOfRange 가 나더라. 왜 일까?
            public List<Thread> threadList = new List<Thread>();
            public bool stopThreading = false;

            public int nextTriangleToProcess;
            public readonly object nextTriangleLock = new object();

            public float raycastAngleCos;
            public float raycastAngleSin;
            public float gridSize = 36.0f;
            public bool materialColor2VertexColor = true;
            public bool genAABBTreeOnlyWithTopFace = true;

            public string assetResultFileDirectory;
            public string osResultFileDirectory;

            public CombinerContext(List<MeshFilter> meshFilterList, string newSceneFolderPath, CombinerSettings combinerSetting)
            {
                for (int i = 0; i < threadVarPool.Length; ++i)
                    threadVarPool[i] = new List<IHasAABB>();
                for (int i = 0; i < threadResult.Length; ++i)
                    threadResult[i] = new ThreadResult();

                this.meshFilterList = meshFilterList;

                assetResultFileDirectory = newSceneFolderPath + "/";
                const string Assets = "Assets";
               
                osResultFileDirectory = Application.dataPath + newSceneFolderPath.Remove(0, Assets.Length) + "/";

                raycastAngleCos = Mathf.Cos(Mathf.Deg2Rad * combinerSetting.RaycastAngle);
                raycastAngleSin = Mathf.Sin(Mathf.Deg2Rad * combinerSetting.RaycastAngle);
                gridSize = combinerSetting.GridSize;
                materialColor2VertexColor = combinerSetting.MaterialColor2VertexColor;
                genAABBTreeOnlyWithTopFace = combinerSetting.GenAABBTreeOnlyWithTopFace;
            }
        }

        private class Triangle : IHasAABB
        {
            public Vector3 min;
            public Vector3 max;
            public Vertex[] vertices;
            public Vector3[] projDirs;
            public int meshID;
            public int textureIndex;
            public bool isInTree;
            public bool ignoreCombine;
            
            public LinkedListNode<Triangle> link;

            public Triangle(int meshID, int textureIndex, Vertex[] vertices)
            {
                this.textureIndex = textureIndex;
                this.vertices = vertices.Clone() as Vertex[];

                projDirs = new Vector3[3];
                for (int i = 0; i < 3; ++i)
                {
                    Vector3 diff = vertices[(i + 1) % 3].position - vertices[i].position;
                    projDirs[i] = Vector3.Cross(diff, vertices[i].normal).normalized;
                }

                min = Vector3.positiveInfinity;
                max = Vector3.negativeInfinity;

                for (int i = 0; i < 3; ++i)
                {
                    min = Vector3.Min(min, vertices[i].position);
                    max = Vector3.Max(max, vertices[i].position);
                }

                min -= Vector3.one * AABBPadding;
                max += Vector3.one * AABBPadding;

                this.meshID = meshID;
            }

            public AABB GetAABB()
            {
                return new AABB(min, max);
            }

            public bool OverlapsRay(ref Vector3 pos, ref Vector3 dir)
            {
                Vector3 collPos = Vector3.zero;

                Vector3 v = vertices[0].position;
                Vector3 n = vertices[0].normal;
                float pProj = Vector3.Dot(n, v - pos);
                float dProj = Vector3.Dot(n, dir);

                if (Mathf.Abs(dProj) <= 0.000001f)
                    return false;

                if (pProj * dProj <= 0.0f)
                    return false;

                collPos = pos + dir * (pProj / dProj);

                for (int i = 0; i < 3; ++i)
                {
                    float val = Vector3.Dot(projDirs[i], collPos - vertices[i].position);
                    if (val > 0)
                        return false;
                }

                return true;
            }
        }

        private const int ThreadCount = 20;
        private const float AABBPadding = 0.01f;
        private const string DialogTitle = "Combiner";
        private const string ShaderNameToCombine = "Ragnarok/Diffuse";
        private const string MeshNameToCombine = "cube000";
        private const string NewShaderName = "Ragnarok/Diffuse";

        private static int meshFilterCount;
        private static int meshFilterMaxCount;
        private static int trianglesLength;
        private static int trianglesMaxLength;
        private static int trianglesSumLength;

        [MenuItem("라그나로크/Ignore Combine 선택")]
        private static void SelectIgnoreCombine()
        {
            var selected = Selection.instanceIDs;

            List<GameObject> targetToFind;
            int[] newSelected;

            if (selected == null || selected.Length == 0)
            {
                targetToFind = new List<GameObject>(GameObject.FindGameObjectsWithTag("IgnoreCombine"));
            }
            else
            {
                targetToFind = new List<GameObject>();

                for (int i = 0; i < selected.Length; ++i)
                {
                    GameObject gameObj = EditorUtility.InstanceIDToObject(selected[i]) as GameObject;
                    if (gameObj.tag == "IgnoreCombine")
                        targetToFind.Add(gameObj);
                }
            }

            newSelected = new int[targetToFind.Count];
            for (int i = 0; i < targetToFind.Count; ++i)
                newSelected[i] = targetToFind[i].GetInstanceID();

            Selection.instanceIDs = newSelected;
        }

        [MenuItem("라그나로크/현재 씬 콤바인")]
        private static void StartCombine()
        {
            string[] settingGUIDs = AssetDatabase.FindAssets("t:CombinerSettings", new string[0] { });

            if (settingGUIDs == null || settingGUIDs.Length == 0)
            {
                EditorUtility.DisplayDialog(DialogTitle, "콤바이너 세팅을 불러올 수 없습니다. 콤바이너 세팅 어셋을 만들어주세요.", "예");
                return;
            }

            var settingGUID = settingGUIDs[0];
            var setting = AssetDatabase.LoadAssetAtPath<CombinerSettings>(AssetDatabase.GUIDToAssetPath(settingGUID));

            Scene activeScene = EditorSceneManager.GetActiveScene();
            string scenePath = activeScene.path;

            if (scenePath == "")
            {
                EditorUtility.DisplayDialog(DialogTitle, "현재 씬은 어셋으로 저장되지 않은 씬입니다. 콤바인을 진행할 수 없습니다.", "예");
                return;
            }

            string[] split = scenePath.Split('/');
            string sceneName = Path.GetFileNameWithoutExtension(split[split.Length - 1]);

            if (sceneName.StartsWith("Combined"))
            {
                EditorUtility.DisplayDialog(DialogTitle, "현재 씬은 콤바인된 씬입니다. 콤바인을 진행할 수 없습니다. 원본 씬을 열고 다시 수행해주세요.", "예");
                return;
            }

            if (activeScene.isDirty)
            {
                if (!EditorUtility.DisplayDialog(DialogTitle, "현재 씬에 변경사항이 있습니다. 저장하고 진행하시겠습니까?", "예", "아니오"))
                    return;

                EditorSceneManager.SaveScene(activeScene);
            }

            string sceneFolderPath = Path.GetDirectoryName(scenePath);
            string newSceneFolderName = string.Format("Combined_{0}", sceneName);
            string newSceneName = string.Format("Combined_{0}.unity", sceneName);
            string newSceneFolderPath = sceneFolderPath + "/" + newSceneFolderName;

            split[split.Length - 1] = string.Format("{0}/{1}", newSceneFolderName, newSceneName);
            string newScenePath = string.Join("/", split);
            
            if (!AssetDatabase.IsValidFolder(newSceneFolderPath))
            {
                AssetDatabase.CreateFolder(sceneFolderPath, newSceneFolderName);
            }
            else
            {
                if (!EditorUtility.DisplayDialog(DialogTitle, "콤바인 결과 폴더가 이미 있습니다. 폴더 안의 모든 어셋이 제거되고, 콤바인을 수행합니다. 진행하시겠습니까?", "예", "아니오"))
                    return;

                string[] assets = AssetDatabase.FindAssets("", new string[] { newSceneFolderPath });

                for (int i = 0; i < assets.Length; ++i)
                    AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(assets[i]));
            }

            AssetDatabase.CopyAsset(scenePath, newScenePath);
            Scene newScene = EditorSceneManager.OpenScene(newScenePath, OpenSceneMode.Single);

            AssetDatabase.Refresh();

            string realScenePath = Application.dataPath;
            realScenePath = realScenePath.Replace("Assets", newScenePath);
            string newSceneYaml = null;

            using (StreamReader reader = File.OpenText(realScenePath))
            {
                string sceneYaml = reader.ReadToEnd();
                int navMeshIndex = sceneYaml.IndexOf("m_NavMeshData");

                if (navMeshIndex != -1)
                {
                    int guidIndex = sceneYaml.IndexOf("guid", navMeshIndex);
                    int readerIndex = guidIndex;
                    StringBuilder sb = new StringBuilder();
                    bool startRead = false;

                    while (true)
                    {
                        ++readerIndex;

                        if (sceneYaml[readerIndex] == ',')
                            break;

                        if (sceneYaml[readerIndex] == ' ')
                            continue;

                        if (sceneYaml[readerIndex] == ':')
                        {
                            startRead = true;
                            continue;
                        }

                        if (startRead)
                            sb.Append(sceneYaml[readerIndex]);
                    }

                    string originNavMeshGUID = sb.ToString();
                    string navMeshPath = AssetDatabase.GUIDToAssetPath(originNavMeshGUID);
                    string navMeshName = Path.GetFileName(navMeshPath);
                    string newNavMeshPath = newSceneFolderPath + "/" + navMeshName;
                    AssetDatabase.CopyAsset(navMeshPath, newNavMeshPath);
                    AssetDatabase.Refresh();
                    string newNavMeshGUID = AssetDatabase.AssetPathToGUID(newNavMeshPath);

                    newSceneYaml = sceneYaml.Replace(originNavMeshGUID, newNavMeshGUID);
                }
            }

            if (newSceneYaml != null)
                File.WriteAllText(realScenePath, newSceneYaml);

            AssetDatabase.Refresh();

            GameObject[] rootGameObjects = newScene.GetRootGameObjects();

            for (int i = 0; i < rootGameObjects.Length; ++i)
                DisconnectPrefab(rootGameObjects[i]);

            List<MeshFilter> meshFilterList = new List<MeshFilter>();

            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                var meshFilters = rootGameObjects[i].GetComponentsInChildren<MeshFilter>();
                for (int j = 0; j < meshFilters.Length; ++j)
                {
                    var eachFilter = meshFilters[j];

                    MeshRenderer meshRenderer = eachFilter.GetComponent<MeshRenderer>();
                    if (meshRenderer == null)
                        continue;

                    if (ShaderNameToCombine.Length > 0 && meshRenderer.sharedMaterial.shader.name != ShaderNameToCombine)
                        continue;

                    if (MeshNameToCombine.Length > 0 && eachFilter.sharedMesh.name != MeshNameToCombine)
                        continue;

                    meshFilterList.Add(eachFilter);
                }
            }

            CombinerContext context = new CombinerContext(meshFilterList, newSceneFolderPath, setting);

            if (!EditorUtility.DisplayDialog(DialogTitle, $"메시 필터 리스트의 갯수가 '{context.meshFilterList.Count}'개 입니다. 진행 하시겠습니까?", "예", "아니오"))
                return;

            try
            {
                if (!GenTree(context))
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                    return;
                }
            
                if (!CullUnvisibleTriangles(context))
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                    return;
                }
            
                if (!GenerateAssets(context))
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                    return;
                }
            
                for (int i = 0; i < meshFilterList.Count; ++i)
                {
                    if (meshFilterList[i] != null)
                    {
                        if (meshFilterList[i].transform.childCount > 0)
                        {
                            MeshRenderer meshRenderer = meshFilterList[i].GetComponent<MeshRenderer>();
                            if (meshRenderer != null)
                                Object.DestroyImmediate(meshRenderer);
                            Object.DestroyImmediate(meshFilterList[i]);
                        }
                        else
                        {
                            Object.DestroyImmediate(meshFilterList[i].gameObject);
                        }
                    }
                }
            
                EditorSceneManager.SaveScene(newScene);
            }
            catch(System.Exception e)
            {
                Debug.LogError($"filter/filterMax = {meshFilterCount}/{meshFilterMaxCount}, triangle/triangleMax = {trianglesLength}/{trianglesMaxLength}, triangleSum = {trianglesSumLength}");
                Debug.LogError(e);
            }
            
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private static void DisconnectPrefab(GameObject gameObject)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
                PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            int childCount = gameObject.transform.childCount;
            for (int i = 0; i < childCount; ++i)
                DisconnectPrefab(gameObject.transform.GetChild(i).gameObject);
        }

        private static bool GenTree(CombinerContext context)
        {
            var meshFilterList = context.meshFilterList;
            meshFilterMaxCount = meshFilterList.Count;
            trianglesSumLength = 0;

            Vertex[] vertices = new Vertex[3];

            for (int i = 0; i < meshFilterList.Count; ++i)
            {
                meshFilterCount = i;
                GameObject gameObj = meshFilterList[i].gameObject;
                var mat = meshFilterList[i].GetComponent<MeshRenderer>().sharedMaterial;

                Transform trans = gameObj.transform;
                bool ignoreCombine = false;

                while (trans != null)
                {
                    if (trans.tag == "IgnoreCombine")
                    {
                        ignoreCombine = true;
                        break;
                    }

                    trans = trans.parent;
                }

                var textureIndex = 0;
                if (mat.mainTexture != null)
                {
                    textureIndex = context.textures.FindIndex(v => v == mat.mainTexture);
                    if (textureIndex == -1)
                    {
                        textureIndex = context.textures.Count;
                        context.textures.Add(mat.mainTexture as Texture2D);
                    }
                }

                Color mainColor = mat.color;

                Mesh mesh = meshFilterList[i].sharedMesh;
                int[] triangles = mesh.GetTriangles(0);
                trianglesMaxLength = triangles.Length;
                trianglesSumLength += trianglesMaxLength;
                Matrix4x4 localToWorld = meshFilterList[i].transform.localToWorldMatrix;

                Vector3[] positions = mesh.vertices;
                Vector2[] uv = mesh.uv;

                for (int j = 0; j < triangles.Length; j += 3)
                {
                    trianglesLength = j;
                    vertices[0].position = localToWorld.MultiplyPoint3x4(positions[triangles[j]]);
                    vertices[1].position = localToWorld.MultiplyPoint3x4(positions[triangles[j + 1]]);
                    vertices[2].position = localToWorld.MultiplyPoint3x4(positions[triangles[j + 2]]);

                    Vector3 a = vertices[1].position - vertices[0].position;
                    Vector3 b = vertices[2].position - vertices[0].position;
                    Vector3 normal = Vector3.Cross(a, b).normalized;

                    vertices[0].uv = uv[triangles[j]];
                    vertices[1].uv = uv[triangles[j + 1]];
                    vertices[2].uv = uv[triangles[j + 2]];

                    vertices[0].normal = vertices[1].normal = vertices[2].normal = normal;
                    vertices[0].color = vertices[1].color = vertices[2].color = mainColor;

                    Triangle triangle = new Triangle(i, textureIndex, vertices);
                    triangle.link = context.triangleList.AddLast(triangle);
                    triangle.ignoreCombine = ignoreCombine;

                    if (context.genAABBTreeOnlyWithTopFace == false || (vertices[0].normal - Vector3.up).sqrMagnitude < 0.000001f)
                    {
                        triangle.isInTree = true;
                        context.aabbTree.InsertObject(triangle, i);
                    }
                    else
                    {
                        triangle.isInTree = false;
                    }
                }

                if (EditorUtility.DisplayCancelableProgressBar(DialogTitle, string.Format("Generating AABB Tree ({0}/{1})", i + 1, meshFilterList.Count), (i + 1) / (float)meshFilterList.Count))
                    return false;
            }

            return true;
        }

        private static void CheckVisibility(object input)
        {
            ThreadInput threadInput = input as ThreadInput;
            CombinerContext context = threadInput.context;

            int index = threadInput.index;

            while (!context.stopThreading)
            {
                int triangleToProcess;

                lock (context.nextTriangleLock)
                {
                    triangleToProcess = context.nextTriangleToProcess++;
                }

                if (triangleToProcess >= context.triangleList.Count)
                    return;

                Triangle triangle = context.threadTriangles[triangleToProcess];
                List<IHasAABB> result = context.threadVarPool[index];

                bool visibility = false;
                Vertex[] vertices = triangle.vertices;

                if (triangle.ignoreCombine)
                {
                    visibility = true;
                }
                else
                {
                    if ((vertices[0].normal - Vector3.down).sqrMagnitude > 0.00001f)
                    {
                        Vector3 castDir;

                        if ((vertices[0].normal - Vector3.up).sqrMagnitude < 0.00001f)
                            castDir = Vector3.up;
                        else
                            castDir = vertices[0].normal * context.raycastAngleCos + Vector3.up * context.raycastAngleSin;

                        int collCount = 0;

                        for (int j = 0; j < 3; ++j)
                        {
                            Vector3 vertexToCheck = vertices[j].position;
                            Vector3 otherEdgeCenter = (vertices[(j + 1) % 3].position + vertices[(j + 2) % 3].position) * 0.5f;

                            Vector3 checkPos = vertexToCheck + (otherEdgeCenter - vertexToCheck).normalized * 0.1f;
                            checkPos = checkPos + triangle.vertices[j].normal * 0.0001f;

                            context.aabbTree.QueryRay(checkPos, castDir, result, triangle.meshID, index);

                            if (result.Count == 0)
                                break;

                            for (int k = 0; k < result.Count; ++k)
                            {
                                var collided = result[k] as Triangle;

                                if (collided.OverlapsRay(ref checkPos, ref castDir))
                                {
                                    ++collCount;
                                    break;
                                }
                            }
                        }

                        visibility = collCount != 3;
                    }
                    else
                    {
                        visibility = false;
                    }
                }

                context.threadResult[triangleToProcess].result = visibility;
                Thread.Sleep(0);
            }
        }

        private static bool CullUnvisibleTriangles(CombinerContext context)
        {
            List<IHasAABB> result = new List<IHasAABB>();

            int totalCount = context.triangleList.Count;

            var node = context.triangleList.Last;

            List<Triangle> listToCheck = new List<Triangle>(ThreadCount);
            
            context.threadTriangles = new Triangle[context.triangleList.Count];
            int nextTriangleIndex = 0;
            foreach (var eachTriangle in context.triangleList)
                context.threadTriangles[nextTriangleIndex++] = eachTriangle;

            context.threadResult = new ThreadResult[context.triangleList.Count];
            for (int i = 0; i < context.threadResult.Length; ++i)
                context.threadResult[i] = new ThreadResult();
            context.nextTriangleToProcess = 0;

            for (int i = 0; i < ThreadCount && i < totalCount; ++i)
            {
                Thread checkThread = new Thread(new ParameterizedThreadStart(CheckVisibility));
                checkThread.Start(new ThreadInput()
                {
                    index = i,
                    context = context
                });
                context.threadList.Add(checkThread);
            }

            while (true)
            {
                bool finish = true;

                for (int i = 0; i < context.threadList.Count; ++i)
                {
                    if (context.threadList[i].IsAlive)
                    {
                        finish = false;
                    }
                }

                if (finish)
                    break;

                Thread.Sleep(100);
                int curProcessed = 0;

                lock (context.nextTriangleLock)
                {
                    curProcessed = context.nextTriangleToProcess;
                }

                if (EditorUtility.DisplayCancelableProgressBar(DialogTitle, string.Format("Culling with visibility ({0}/{1})", curProcessed, totalCount), curProcessed / (float)totalCount))
                {
                    context.stopThreading = true;

                    bool wait = true;
                    while (wait)
                    {
                        wait = false;
                        for (int i = 0; i < context.threadList.Count; ++i)
                            if (context.threadList[i].IsAlive)
                                wait = true;
                    }
                    
                    return false;
                }
            }

            for (int i = 0; i < totalCount; ++i)
                if (!context.threadResult[i].result)
                    context.triangleList.Remove(context.threadTriangles[i].link);
            
            return true;
        }

        /// <summary>
        /// 텍스쳐 크기 변경
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        private static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            //Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            //Color[] rpixels = result.GetPixels(0);
            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            Color[] rpixels = result.GetPixels();
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
                rpixels[px] = new Color(rpixels[px].r, rpixels[px].g, rpixels[px].b);
            }
            //result.SetPixels(rpixels, 0);
            result.SetPixels(rpixels);
            result.Apply();
            return result;
        }

        private static bool GenerateAssets(CombinerContext context)
        {
            int count = context.textures.Count;
            int widthCount = 0;
            int width = 0;
            int height = 0;

            int margin = 8; // 경계 여백
            int marginHeight = margin * 2;
            int marginBoth = margin * 2;
            int marginHeightBoth = marginBoth * 2;

            if (context.textures.Count > 0)
            {
                width = context.textures[0].width + marginBoth;
                height = context.textures[0].height + marginHeightBoth;

                float sqrt = Mathf.Sqrt(width * height * count);
                widthCount = Mathf.CeilToInt(sqrt / (float)width);
                if(widthCount % 2 == 1) // 홀수일경우에는 추가로 체크해줘야함.
                {
                    int heightCount = Mathf.FloorToInt(sqrt / height);
                    if (widthCount * heightCount < count)
                        widthCount += 1;
                }
                //widthCount = Mathf.CeilToInt(Mathf.Sqrt(width * height * count) / (float)width);
                
                Texture2D texture = new Texture2D(width * widthCount, width * widthCount, TextureFormat.RGB24, false);

                for (int i = 0; i < count; ++i)
                {
                    var each = context.textures[i];
                    var data = each.GetRawTextureData();

                    // 마진 셋팅
                    Texture2D texMargin = new Texture2D(width - marginBoth, height - marginHeightBoth, each.format, false);
                    texMargin.LoadRawTextureData(data);
                    Texture2D texMarginResize = ScaleTexture(texMargin, width, height);
                    texture.SetPixels(width * (i % widthCount), height * (i / widthCount), width, height, texMarginResize.GetPixels(0, 0, texMarginResize.width, texMarginResize.height));
                    Object.DestroyImmediate(texMargin);
                    Object.DestroyImmediate(texMarginResize);

                    // 원본 이미지 셋팅
                    Texture2D tex = new Texture2D(width - marginBoth, height - marginHeightBoth, each.format, false);
                    tex.LoadRawTextureData(data);
                    texture.SetPixels(width * (i % widthCount) + margin, height * (i / widthCount) + marginHeight, width - marginBoth, height - marginHeightBoth, tex.GetPixels(0, 0, tex.width, tex.height));
                    Object.DestroyImmediate(tex);
                }

                texture.Apply();

                byte[] bytes = texture.EncodeToPNG();
                Object.DestroyImmediate(texture);
                var textureFilePath = context.osResultFileDirectory + "CombinedTexture.png";

                if (File.Exists(textureFilePath))
                    File.Delete(textureFilePath);

                File.WriteAllBytes(textureFilePath, bytes);
                AssetDatabase.ImportAsset(context.assetResultFileDirectory + "CombinedTexture.png");
            }

            Texture2D combinedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(context.assetResultFileDirectory + "CombinedTexture.png");

            Shader newShader = Shader.Find(NewShaderName);
            Material combinedMaterial = new Material(newShader);
            AssetDatabase.CreateAsset(combinedMaterial, context.assetResultFileDirectory + "CombinedMat.mat");
            combinedMaterial = AssetDatabase.LoadAssetAtPath<Material>(context.assetResultFileDirectory + "CombinedMat.mat");
            combinedMaterial.mainTexture = combinedTexture;
            combinedMaterial.color = Color.white;

            Dictionary<GridPos, List<Triangle>> gridMap = new Dictionary<GridPos, List<Triangle>>();

            foreach (var eachTriangle in context.triangleList)
            {
                float minValX = float.MaxValue;
                float minValZ = float.MaxValue;

                if (context.gridSize > 0f)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        minValX = Mathf.Min(minValX, eachTriangle.vertices[i].position.x / context.gridSize);
                        minValZ = Mathf.Min(minValZ, eachTriangle.vertices[i].position.z / context.gridSize);
                    }
                }

                GridPos gridPos;
                gridPos.x = Mathf.FloorToInt(minValX);
                gridPos.y = Mathf.FloorToInt(minValZ);

                List<Triangle> list;
                if (!gridMap.TryGetValue(gridPos, out list))
                {
                    list = new List<Triangle>();
                    gridMap.Add(gridPos, list);
                }

                list.Add(eachTriangle);
            }

            GameObject result = new GameObject("Combine Result");
            result.isStatic = true;
            result.transform.localPosition = Vector3.zero;
            result.transform.localRotation = Quaternion.identity;
            result.transform.localScale = Vector3.one;

            int meshCount = 0;

            foreach (var eachTriangleList in gridMap.Values)
            {
                GameObject part = new GameObject("Part " + meshCount);
                part.isStatic = true;
                MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = part.AddComponent<MeshFilter>();
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                meshRenderer.receiveShadows = true;
                meshRenderer.sharedMaterial = combinedMaterial;

                part.transform.parent = result.transform;
                part.transform.localPosition = Vector3.zero;
                part.transform.localRotation = Quaternion.identity;
                part.transform.localScale = Vector3.one;

                Mesh mesh = new Mesh();
                List<Pair> vertexPairList = new List<Pair>();
                List<int> triangles = new List<int>();
                int nextIndex = 0;

                foreach (var eachTriangle in eachTriangleList)
                {
                    for (int i = 0; i < 3; ++i)
                    {
                        Vertex vertex = eachTriangle.vertices[i];
                        if (count > 0)
                        {
                            float originalUVx = vertex.uv.x;
                            float originalUVy = vertex.uv.y;
                            vertex.uv.x = (originalUVx / widthCount) + (1.0f / widthCount) * (eachTriangle.textureIndex % widthCount);
                            vertex.uv.y = (1.0f / widthCount * 2) * originalUVy
                                + (1.0f / widthCount * 2) * (eachTriangle.textureIndex / widthCount);

                            if (originalUVx < 0.1f)
                                vertex.uv.x += 1f / widthCount * (float)margin / width;
                            else if (originalUVx > 0.9f)
                                vertex.uv.x -= 1f / widthCount * (float)margin / width;

                            if (originalUVy < 0.1f)
                                vertex.uv.y += 1f / widthCount * 2 * (float)marginHeight / height;
                            else if (originalUVy > 0.9f)
                                vertex.uv.y -= 1f / widthCount * 2 * (float)marginHeight / height;
                        }

                        int index = vertexPairList.FindIndex(v => v.vertex == vertex);
                        if (index == -1)
                        {
                            vertexPairList.Add(new Pair(vertex, nextIndex));
                            triangles.Add(nextIndex);
                            ++nextIndex;
                        }
                        else
                        {
                            triangles.Add(index);
                        }
                    }
                }

                Vector3[] positions = new Vector3[vertexPairList.Count];
                Vector3[] normals = new Vector3[vertexPairList.Count];
                Vector2[] uv = new Vector2[vertexPairList.Count];
                Color[] color = new Color[vertexPairList.Count];

                for (int i = 0; i < vertexPairList.Count; ++i)
                {
                    positions[i] = vertexPairList[i].vertex.position;
                    normals[i] = vertexPairList[i].vertex.normal;
                    uv[i] = vertexPairList[i].vertex.uv;
                    color[i] = vertexPairList[i].vertex.color;
                }

                mesh.vertices = positions;
                mesh.normals = normals;
                mesh.uv = uv;
                if (context.materialColor2VertexColor)
                    mesh.colors = color;
                mesh.triangles = triangles.ToArray();

                UnwrapParam.SetDefaults(out UnwrapParam unwrapParam);
                Unwrapping.GenerateSecondaryUVSet(mesh, unwrapParam);

                string meshPath = context.assetResultFileDirectory + string.Format("CombinedMesh {0}.mesh", meshCount);
                AssetDatabase.CreateAsset(mesh, meshPath);

                meshFilter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);

                if (EditorUtility.DisplayCancelableProgressBar(DialogTitle, string.Format("Generating Assets (CombinedMesh {0}.mesh)", meshCount), 1.0f))
                    return false;

                ++meshCount;
            }

            return true;
        }
    }
}
#endif