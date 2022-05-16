using UnityEngine;

namespace Ragnarok
{
    public class FollowTarget : PoolObject, IInspectorFinder
    {
        private const string ANIMATION_IDLE = "Idle";
        private const string ANIMATION_RUN = "Run";
        private const float DISTANCE_TARGET = 3f;
        private const float MOVE_SPEED = 1.5f;
        private Transform target;

        [SerializeField] Transform effectParent;
        [SerializeField] Animation animation;

        void Update()
        {
            MoveToTarget();
        }

        private void MoveToTarget()
        {
            if (target == null)
            {
                Play(ANIMATION_IDLE);
                return;
            }

            float dist = (target.position - CachedTransform.position).sqrMagnitude;

            if (dist > DISTANCE_TARGET)
            {
                CachedTransform.position = Vector3.Lerp(CachedTransform.position, target.position, Time.deltaTime * MOVE_SPEED);
                CachedTransform.LookAt(target);
                Play(ANIMATION_RUN);
            }
            else
            {
                Play(ANIMATION_IDLE);
            }
        }

        void Play(string name)
        {
            if (animation == null)
                return;

            if (!animation.IsPlaying(name))
            {
                animation.CrossFade(name, 0.05f);
            }
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public Transform GetEffectParent()
        {
            return effectParent;
        }

        bool IInspectorFinder.Find()
        {
            animation = transform.GetComponentInChildren<Animation>();
            return true;
        }
    }
}