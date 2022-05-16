using UnityEngine;

namespace Ragnarok
{
    public class UnitSoul : AutoReleasePoolObject
    {
        [SerializeField] float duration = 0.8f;
        [SerializeField] string node = "HitPos";
        [SerializeField] AnimationCurve moveCurve;

        private Transform target;
        private Vector3 source, destination;
        private float currentTime;

        public void Initialize(Transform target)
        {
            this.target = GetNode(target, node);

            CachedTransform.position = source;
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            source = CachedTransform.position;
            currentTime = 0f;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            target = null;
        }

        protected override void Update()
        {
            currentTime += Time.deltaTime;

            if (currentTime > duration)
            {
                Release();
                return;
            }

            if (target)
                destination = target.position;

            float normalizedTime = currentTime / duration;
            Vector3 toTarget = destination - source;
            Vector3 move = toTarget * moveCurve.Evaluate(normalizedTime);
            Vector3 next = source + move;
            CachedTransform.position = next;
            CachedTransform.LookAt(destination);
        }

        /// <summary>
        /// Find Recursive
        /// </summary>
        private Transform GetNode(Transform tf, string name)
        {
            if (tf == null || string.IsNullOrEmpty(name))
                return null;

            if (tf.name.Equals(name))
                return tf;

            // 재귀함수를 통하여 모든 Transform 의 name 을 찾음
            for (int i = 0; i < tf.childCount; ++i)
            {
                Transform child = GetNode(tf.GetChild(i), name);

                if (child)
                    return child;
            }

            return null;
        }
    }
}