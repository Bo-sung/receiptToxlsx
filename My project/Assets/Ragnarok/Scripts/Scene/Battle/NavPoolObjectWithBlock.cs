using UnityEngine;
using UnityEngine.AI;

namespace Ragnarok
{
    public class NavPoolObjectWithBlock : NavPoolObjectWithReady
    {
        [SerializeField] NavMeshObstacle obstacle;

        public override void HideMain()
        {
            base.HideMain();

            SetBlockState(false);
        }

        public override void ShowMain()
        {
            base.ShowMain();

            SetBlockState(true);
        }

        private void SetBlockState(bool isBlock)
        {
            obstacle.enabled = isBlock;
        }
    }
}