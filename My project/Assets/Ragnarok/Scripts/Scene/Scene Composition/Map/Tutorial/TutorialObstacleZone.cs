using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok.SceneComposition
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public sealed class TutorialObstacleZone : TutorialZone
    {
        NavMeshObstacle navMeshObstacle;

        protected override void Awake()
        {
            base.Awake();

            navMeshObstacle = GetComponent<NavMeshObstacle>();
        }

        /// <summary>
        /// 가로막기
        /// </summary>
        public void Close()
        {
            navMeshObstacle.enabled = true;
        }

        /// <summary>
        /// 통과
        /// </summary>
        public void Open()
        {
            navMeshObstacle.enabled = false;
        }
    }
}