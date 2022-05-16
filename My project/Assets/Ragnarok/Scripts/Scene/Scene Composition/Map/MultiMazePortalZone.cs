using UnityEngine;

namespace Ragnarok
{
    public class MultiMazePortalZone : MonoBehaviour, IInspectorFinder
    {
        public const int CHRISTMAS_EVENT_INDEX = 100; // 크리스마스 이벤트
        public const int FOREST_MAZE_INDEX = 200; // 미궁숲
        public const int DARK_MAZE_EVENT_INDEX = 300; // 이벤트미궁:암흑

        public const int GATE_MAZE_1_INDEX = 1001; // 게이트 1
        public const int GATE_MAZE_2_INDEX = 1002; // 게이트 2

        public enum ZoneState { Open, Lock }

        [SerializeField] int index;
        [SerializeField] GameObject goOpen;
        [SerializeField] GameObject goLock;

        private ZoneState zoneState;
        private bool isEvent;

        public int GetIndex()
        {
            return index;
        }

        public void SetEvent()
        {
            isEvent = true;
            SetState(ZoneState.Open);
        }

        public void SetState(ZoneState zoneState)
        {
            this.zoneState = zoneState;

            NGUITools.SetActive(goOpen, zoneState == ZoneState.Open);
            NGUITools.SetActive(goLock, zoneState == ZoneState.Lock);
        }

        public bool GetIsLock()
        {
            return zoneState == ZoneState.Lock;
        }

        public bool GetIsEvent()
        {
            return isEvent;
        }

        bool IInspectorFinder.Find()
        {
#if UNITY_EDITOR
            index = transform.GetSiblingIndex() + 1;
            gameObject.name = index.ToString("00");
            gameObject.tag = Tag.PORTAL;
            CreateSphereCollider(gameObject, 3.6f);

            goOpen = GetChild(gameObject, "Open");
            goLock = GetChild(gameObject, "Lock");

            CreatePrefabChild(goOpen, "Assets/ArtResources/ArtAssets/Utility/Objects/Prefabs/UnderGround_Fx.prefab");
            CreatePrefabChild(goLock, "Assets/ArtResources/ArtAssets/Utility/Objects/Prefabs/Lock.prefab");
#endif
            return true;
        }

#if UNITY_EDITOR
        private void CreateSphereCollider(GameObject go, float radius)
        {
            SphereCollider collider = go.AddMissingComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = radius;
        }

        private GameObject GetChild(GameObject go, string childName)
        {
            Transform tf = go.transform.Find(childName);
            if (tf == null)
                return CreateChild(go, childName);

            return tf.gameObject;
        }

        private GameObject CreateChild(GameObject go, string childName)
        {
            GameObject goNew = NGUITools.AddChild(go);
            goNew.name = childName;
            return goNew;
        }

        private void CreatePrefabChild(GameObject go, string path)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            Transform tf = go.transform.Find(name);
            if (tf != null)
                return;

            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError("Empty Prefab: path = " + path);
                return;
            }

            GameObject clone = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            clone.transform.SetParent(go.transform, worldPositionStays: false);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;
        }
#endif
    }
}