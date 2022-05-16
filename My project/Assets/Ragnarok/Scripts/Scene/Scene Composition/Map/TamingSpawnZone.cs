using UnityEngine;

namespace Ragnarok
{
    public class TamingSpawnZone : MonoBehaviour
    {
        const float size = 1f;

        [SerializeField] int zoneId;

        Vector3 center;

        void Awake()
        {
            center = transform.position;
        }

        public Vector3 GetCenter()
        {
            return center;
        }

        public Vector3[] GetPatrolZone()
        {
            Vector3[] vectors = new Vector3[4];
            vectors[0] = center + new Vector3(size, 0, size);
            vectors[1] = center + new Vector3(size, 0, -size);
            vectors[2] = center + new Vector3(-size, 0, -size);
            vectors[3] = center + new Vector3(-size, 0, size);
            return vectors;
        }

        public int ZoneId => zoneId;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            {
                Gizmos.DrawWireSphere(transform.position, 1f);
            }
            Gizmos.color = Color.white;

            GUIStyle style = new GUIStyle
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState { textColor = Color.black },
            };
            UnityEditor.Handles.Label(transform.position, zoneId.ToString(), style);
        } 
#endif
    } 
}
