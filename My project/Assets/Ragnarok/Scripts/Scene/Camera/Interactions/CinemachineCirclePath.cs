using UnityEngine;
using Cinemachine;

namespace Ragnarok
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CinemachinePath))]
    public class CinemachineCirclePath : MonoBehaviour
    {
        [SerializeField] float radius = 4;

        CinemachinePath path;

        void Awake()
        {
            path = GetComponent<CinemachinePath>();
        }

        void Start()
        {
            path.m_Looped = true;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Awake();
            SetRadius(radius);
        }
#endif

        public void SetRadius(float radius)
        {
            this.radius = radius;

            float tangent = radius * 0.5f;

            if (path.m_Waypoints.Length != 4)
                path.m_Waypoints = new CinemachinePath.Waypoint[4];

            path.m_Waypoints[0] = new CinemachinePath.Waypoint { position = Vector3.right * radius, tangent = Vector3.forward * tangent, roll = 0 };
            path.m_Waypoints[1] = new CinemachinePath.Waypoint { position = Vector3.forward * radius, tangent = Vector3.left * tangent, roll = 0 };
            path.m_Waypoints[2] = new CinemachinePath.Waypoint { position = Vector3.left * radius, tangent = Vector3.back * tangent, roll = 0 };
            path.m_Waypoints[3] = new CinemachinePath.Waypoint { position = Vector3.back * radius, tangent = Vector3.right * tangent, roll = 0 };
        }
    }
}