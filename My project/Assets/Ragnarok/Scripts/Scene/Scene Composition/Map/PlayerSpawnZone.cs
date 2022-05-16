using UnityEngine;

namespace Ragnarok.SceneComposition
{
    /// <summary>
    /// 플레이어 소환 지역
    /// </summary>
    public class PlayerSpawnZone : MonoBehaviour
    {
        public float radius = 2f;

        Vector3 center;

        void Awake()
        {
            center = transform.position;
        }

        public Vector3 GetCenter()
        {
            return center;
        }        

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            {
                Gizmos.DrawWireSphere(transform.position, radius);
            }
            Gizmos.color = Color.white;
        }
#endif
    }
}