using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok.SceneComposition
{
    public class MapMenuItems
    {
        private const string CTRL = "%";
        private const string ALT = "&";
        private const string SHIFT = "#";

        private const string CUBEMAP_MANAGER_TOOL_MENU = "라그나로크/씬 관리 도구/큐브맵 관리 툴";

        [MenuItem("라그나로크/씬 관리 도구/선택한 씬 Nav Mesh 빌드")]
        private static void BuildNavMesh()
        {
            SceneEditorTools.BuildNavMesh();
        }

        [MenuItem(CUBEMAP_MANAGER_TOOL_MENU)]
        private static void ToggleCubemapManagerTools()
        {
            CubemapManagerTool.IsActive = !CubemapManagerTool.IsActive;
        }

        [MenuItem(CUBEMAP_MANAGER_TOOL_MENU, validate = true)]
        private static bool ToggleCubemapManagerToolValidate()
        {
            Menu.SetChecked(CUBEMAP_MANAGER_TOOL_MENU, CubemapManagerTool.IsActive);
            return true;
        }

        //[MenuItem("라그나로크/씬 관리 도구/기본 큐브 세팅 " + ALT + SHIFT + "1")]
        private static void InitCube()
        {
            System.Array.ForEach(Selection.gameObjects, InitCube);
        }

        //[MenuItem("라그나로크/씬 관리 도구/큐브 프리팹으로 변경 " + ALT + SHIFT + "2")]
        private static void ReplaceCubePrefab()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null || !go.name.Equals("Cube"))
            {
                Debug.LogError("Cube만 선택 가능");
                return;
            }

            // Child
            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                ReplaceCubePrefab(transform.GetChild(i));
            }
        }

        //[MenuItem("라그나로크/씬 관리 도구/오브젝트 프리팹으로 변경 " + ALT + SHIFT + "3")]
        private static void ReplaceObjectPrefab()
        {
            GameObject go = Selection.activeGameObject;
            if (go == null || !go.name.Equals("Object"))
            {
                Debug.LogError("Cube만 선택 가능");
                return;
            }

            // Child
            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                ReplaceObjectPrefab(transform.GetChild(i));
            }
        }

        //[MenuItem("라그나로크/씬 관리 도구/몬스터 프리팹 초기화 " + ALT + SHIFT + "3")]
        private static void InitMonsterPrefab()
        {
            //foreach (GameObject go in Selection.gameObjects)
            //{
            //    if (go.GetComponent<Animation>() == null)
            //        continue;

            //    Transform tf = go.transform;
            //    tf.position = Vector3.zero;
            //    tf.rotation = Quaternion.identity;
            //    tf.localScale = Vector3.one;

            //    go.tag = Tag.CUPET;

            //    SphereCollider collider = go.GetComponent<SphereCollider>();
            //    if (collider)
            //    {
            //        UnitSettings settings = go.AddMissingComponent<UnitSettings>();

            //        int radius = Mathf.RoundToInt(collider.radius * 100);
            //        SerializedObject so = new SerializedObject(settings);
            //        so.FindProperty("colliderSize").intValue = radius;
            //        so.FindProperty("isOverGround").boolValue = Mathf.Approximately(collider.center.y, radius);
            //        so.ApplyModifiedProperties();

            //        Object.DestroyImmediate(collider, true);
            //    }
            //}
        }

        //[MenuItem("라그나로크/뭐야뭐야")]
        private static void AddUnitCollider()
        {
            //foreach (GameObject go in Selection.gameObjects)
            //{
            //    bool isPrefab = PrefabUtility.GetCorrespondingObjectFromSource(go) == null;

            //    if (isPrefab)
            //    {
            //        string assetPath = AssetDatabase.GetAssetPath(go);
            //        GameObject prefab = PrefabUtility.LoadPrefabContents(assetPath);

            //        UnitSettings settings = prefab.GetComponent<UnitSettings>();

            //        if (settings == null)
            //        {
            //            SphereCollider collider = prefab.AddMissingComponent<SphereCollider>();
            //            collider.radius = 1.4f;
            //            collider.center = new Vector3(0f, 1.4f, 0f);

            //            prefab.tag = Tag.CHARACTER;
            //        }
            //        else
            //        {
            //            SphereCollider collider = prefab.AddMissingComponent<SphereCollider>();
            //            collider.radius = settings.GetColliderRadius();
            //            collider.center = settings.GetColliderCenter();
            //            Object.DestroyImmediate(settings, true);
            //        }

            //        PrefabUtility.SaveAsPrefabAsset(prefab, assetPath);
            //        PrefabUtility.UnloadPrefabContents(prefab);
            //    }
            //    else
            //    {
            //        UnitSettings settings = go.GetComponent<UnitSettings>();

            //        if (settings == null)
            //        {
            //            Debug.LogError("Settings 값이 존재하지 않습니다: name = " + go.name);
            //            continue;
            //        }

            //        SphereCollider collider = go.AddMissingComponent<SphereCollider>();
            //        collider.radius = settings.GetColliderRadius();
            //        collider.center = settings.GetColliderCenter();

            //        Object.DestroyImmediate(collider, true);

            //        NGUITools.SetDirty(go);
            //    }
            //}
        }

        private static void InitCube(GameObject go)
        {
            Transform transform = go.transform;

            // Transform 초기화
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            transform.localScale = Vector3.one;

            // 컬라이더 초기화
            BoxCollider collider = go.GetComponent<BoxCollider>();
            if (collider == null)
                collider = go.AddComponent<BoxCollider>();

            collider.center = Vector3.forward * 1.8f;
            collider.size = Vector3.one * 3.6f;

            // 테그 및 레이어
            go.tag = Tag.CUBE;
            go.layer = Layer.CUBE;

            // 네비게이션
            bool isWalkable = IsWalkable(go);

            GameObjectUtility.SetNavMeshArea(go, GameObjectUtility.GetNavMeshAreaFromName(isWalkable ? "Walkable" : "Not Walkable"));
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
        }

        private static void ReplaceCubePrefab(Transform transform)
        {
            GameObject go = transform.gameObject;

            Object obj = PrefabUtility.GetCorrespondingObjectFromSource(go);

            // 프리팹이 아니면 방법이 없음
            if (obj == null)
            {
                go.name = string.Concat("#", go.name);
                return;
            }

            GameObject original = obj as GameObject;

            MeshFilter oriMeshFilter = original.GetComponent<MeshFilter>();
            MeshFilter curMeshFilter = go.GetComponent<MeshFilter>();

            // Mesh 가 같음
            if (oriMeshFilter.sharedMesh.Equals(curMeshFilter.sharedMesh))
            {
                if (PrefabUtility.RevertPrefabInstance(go))
                {
                    // 이름 초기화
                    go.name = original.name;

                    // 위치 부동소수점 제거
                    Vector3 pos = transform.localPosition;
                    float x = GetPos(pos.x, 3.6f); // 3.6 의 배수
                    float y = GetPos(pos.y, 1.2f); // 1.2 의 배수
                    float z = GetPos(pos.z, 3.6f); // 3.6 의 배수
                    transform.localPosition = new Vector3(x, y, z);
                }
                else
                {
                    go.name = string.Concat("###", go.name);
                }
            }
            else
            {
                go.name = string.Concat("##", go.name);
            }
        }

        private static void ReplaceObjectPrefab(Transform transform)
        {
            GameObject go = transform.gameObject;

            Object obj = PrefabUtility.GetCorrespondingObjectFromSource(go);

            // 프리팹이 아니면 방법이 없음
            if (obj == null)
            {
                go.name = string.Concat("#", go.name);
                return;
            }

            GameObject original = obj as GameObject;

            MeshFilter oriMeshFilter = original.GetComponent<MeshFilter>();
            MeshFilter curMeshFilter = go.GetComponent<MeshFilter>();

            // Mesh 가 같음
            if (oriMeshFilter.sharedMesh.Equals(curMeshFilter.sharedMesh))
            {
                Vector3 scale = transform.localScale; // 기존 스케일 저장
                if (PrefabUtility.RevertPrefabInstance(go))
                {
                    // 이름 초기화
                    go.name = original.name;
                    transform.localScale = scale;
                }
                else
                {
                    go.name = string.Concat("###", go.name);
                }
            }
            else
            {
                go.name = string.Concat("##", go.name);
            }
        }

        private static bool IsWalkable(GameObject go)
        {
            string name = go.name;

            string[] walkableNames = { "Prt_ground001", "Prt_ground002", "Prt_ground003", };
            foreach (var item in walkableNames)
            {
                if (name.Equals(item))
                    return true;
            }

            return false;
        }

        private static float GetPos(float value, float checkSize)
        {
            float a = value / checkSize;
            int b = Mathf.RoundToInt(a);

            if (!Mathf.Approximately(a, b))
                Debug.LogWarning($"value가 {checkSize}의 배수가 아닙니다: value = {value}");

            return b * checkSize;
        }
    }
}