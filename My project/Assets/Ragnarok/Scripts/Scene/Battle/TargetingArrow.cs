using UnityEngine;

namespace Ragnarok
{
    public class TargetingArrow : PoolObject
    {
        public void SetPosition(Vector3 position, Vector3 offset)
        {
            Show();

            CachedTransform.position = position;
            CachedTransform.localPosition += offset;
        }
    }
}