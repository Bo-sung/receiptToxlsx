using UnityEngine;

namespace Ragnarok
{
    public class MiddleBossTargetingArrow : PoolObject
    {
        private const float SHOW_DISTANCE = 3.6f;
        private const float SHOW_SQR_MAGNITUDE = SHOW_DISTANCE * SHOW_DISTANCE;

        public void SetPosition(Vector3 start, Vector3 end)
        {
            // 타겟과의 거리가 작을 경우에는 Hiide
            Vector3 direction = end - start;
            direction.y = 0f;

            float dist = direction.sqrMagnitude;
            if (dist < SHOW_SQR_MAGNITUDE)
            {
                Hide();
            }
            else
            {
                Show();

                Quaternion rotation = Quaternion.LookRotation(direction);
                CachedTransform.rotation = rotation.normalized;
            }
        }
    }
}