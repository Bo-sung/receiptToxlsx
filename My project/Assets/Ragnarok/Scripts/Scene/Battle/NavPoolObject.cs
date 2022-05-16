using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavPoolObject : PoolObject, IAutoInspectorFinder
    {
        NavMeshAgent agent;

        protected override void Awake()
        {
            base.Awake();

            agent = GetComponent<NavMeshAgent>();
        }

        public void Warp(Vector3 pos)
        {
            // 가까운 거리면 이동 X (자기자신에 의해 경로가 방해됨)
            if (agent.isOnNavMesh && Approximately(pos))
                return;

            Vector3 position = FindClosestEdge(pos); // 씬 로드 후에 네비 위치로 변경
            agent.Warp(position);
        }

        private Vector3 FindClosestEdge(Vector3 pos)
        {
            bool isHit = NavMesh.SamplePosition(pos, out var hit, Constants.Map.GuildMaze.NAVMESH_SAMPLE_POSITION_RANGE, NavMesh.AllAreas);

            if (!isHit)
                Debug.LogError("Nav가 존재하지 않는 곳으로 이동처리", CachedGameObject);

            return isHit ? hit.position : pos;
        }

        private bool Approximately(Vector3 pos)
        {
            return (CachedTransform.position - pos).sqrMagnitude < 1f;
        }
    }
}