using UnityEngine;

namespace Ragnarok.SceneComposition
{
    public abstract class TutorialZone : MonoBehaviour
    {
        public Transform CachedTransform { get; private set; }

        protected virtual void Awake()
        {
            CachedTransform = transform;
        }

        protected virtual void OnDestroy()
        {
            CachedTransform = null;
        }

        public Vector3 GetPosition()
        {
            return CachedTransform.position;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.gray;
            {
                Gizmos.DrawWireSphere(transform.position, 2f);
            }
            Gizmos.color = Color.white;
        }
#endif
    }
}