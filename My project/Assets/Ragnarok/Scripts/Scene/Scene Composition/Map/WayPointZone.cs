using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ragnarok
{
    public class WayPointZone : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] int id;
        [SerializeField] WayPointZone[] neighborWayPoints;
        [SerializeField] bool isSpawnable; // 스폰 가능 여부

        private const float COLLIDER_SIZE = 1f;
        private const float COLLIDER_SIZE_EXTRA_Y = 1.5f;

        private Transform cachedTransform;
        public Transform CachedTransform
        {
            get
            {
                if (cachedTransform == null)
                    cachedTransform = transform;
                return cachedTransform;
            }
        }

        public int Id => id;
        public WayPointZone[] NeighborWayPoints => neighborWayPoints;
        public bool IsSpawnable => isSpawnable;

        /// <summary>
        /// 이동할 수 있는 웨이포인트 추가
        /// </summary>
        public void AddNeighbor(WayPointZone newNode)
        {
            var savedList = new List<WayPointZone>(NeighborWayPoints);
            savedList.Add(newNode);
            neighborWayPoints = savedList.ToArray();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // 원
            Gizmos.color = IsSpawnable ? Color.cyan : Color.blue;
            {
                Gizmos.DrawWireSphere(transform.position, 0.25f);
            }
            Gizmos.color = Color.white;

            // ID 라벨
            GUIStyle style = new GUIStyle
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState { textColor = (neighborWayPoints == null || neighborWayPoints.Length == 0) ? Color.red : Color.blue },
            };
            UnityEditor.Handles.Label(transform.position, id.ToString(), style);

            // 이웃 표시선
            bool isSelected = UnityEditor.Selection.objects.Contains(gameObject);
            Vector3 me = CachedTransform.position;
            foreach (var neighbor in neighborWayPoints.OrEmptyIfNull())
            {
                Vector3 neighborPos = neighbor.CachedTransform.position;
                DrawArrow.ForGizmo(me, neighborPos - me, isSelected ? Color.red : Color.blue, 1f, 45f);

                if (!isSelected)
                {
                    bool isNeighborSelected = UnityEditor.Selection.objects.Contains(neighbor.gameObject);
                    if (isNeighborSelected)
                    {
                        DrawArrow.ForGizmo(neighborPos, me - neighborPos, Color.red, .75f, 45f);
                    }
                }
            }
        }
#endif

        bool IInspectorFinder.Find()
        {
            // ID 세팅
            if (id == 0)
            {
                id = int.Parse(new Regex(@"(\d+)$").Match(name).Value);
            }

            // 누락된 이웃 세팅
            foreach (var neighbor in neighborWayPoints.OrEmptyIfNull())
            {
                if (neighbor.NeighborWayPoints.Contains(this))
                    continue;

                Debug.Log($"Add WayPoint {neighbor.Id} -> {Id}");
                neighbor.AddNeighbor(this);
            }

            // Collider 추가
            BoxCollider collider = gameObject.AddMissingComponent<BoxCollider>();
            collider.size = Vector3.one * COLLIDER_SIZE + Vector3.up * COLLIDER_SIZE_EXTRA_Y;
            collider.isTrigger = true;

            // Tag, Layer 세팅
            this.tag = Tag.WAYPOINT;
            gameObject.layer = Layer.WAYPOINT;

            return true;
        }
    }

    public static class DrawArrow
    {
        public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction.normalized * 2.34f, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction.normalized * 2.34f, left * arrowHeadLength);
            Gizmos.DrawRay(pos + direction.normalized * 2.34f + right, left - right);
        }

        public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
    }
}
