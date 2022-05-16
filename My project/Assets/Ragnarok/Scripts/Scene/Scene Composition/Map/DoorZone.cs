using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok.SceneComposition
{
    /// <summary>
    /// 문 지역
    /// </summary>
    public class DoorZone : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] NavMeshObstacle navMeshObstacle;
        [SerializeField] Transform spawn;

        Vector3 center;

        void Awake()
        {
            center = transform.position;
            navMeshObstacle.enabled = true;
        }

        public Vector3 GetCenter()
        {
            return center;
        }

        public void OpenDoor()
        {
            gameObject.SetActive(false);
        }

        public void CloseDoor()
        {
            gameObject.SetActive(true);
        }

        public Transform GetSpawn()
        {
            return spawn;
        }

        public bool IsZoneEffect()
        {
            return spawn.childCount != 0;
        }

        bool IInspectorFinder.Find()
        {
#if UNITY_EDITOR
            navMeshObstacle = transform.GetComponent<NavMeshObstacle>();
            spawn = transform.Find("Spawn");
#endif
            return true;
        }
    }
}