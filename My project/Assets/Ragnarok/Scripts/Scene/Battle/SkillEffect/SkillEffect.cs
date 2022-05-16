using UnityEngine;

namespace Ragnarok
{
    public class SkillEffect : AutoReleasePoolObject
    {
        private float duration;
        private bool hasDestination;
        private Vector3 source, destination;
        private AnimationCurve heightCurve;
        private AnimationCurve moveCurve;
        private AnimationCurve sideDirCurve;

        private float currentTime;

        public override void OnSpawn()
        {
            base.OnSpawn();

            currentTime = 0f;
            hasDestination = false;

            if (CachedGameObject.layer != Layer.SKILLFX)
            {
                CachedGameObject.layer = Layer.SKILLFX;
                CachedTransform.SetChildLayer(Layer.SKILLFX);
            }
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            heightCurve = null;
            moveCurve = null;
            sideDirCurve = null;
        }

        public void SetDuration(int duration = 0)
        {
            this.duration = duration * 0.01f;

            // TODO: Loop 이펙트 최소 4초로 변경 (4초보다 크도록 변경)
            if (this.duration == 0f)
                this.duration = 4f;
        }

        public void SetDestination(Vector3 destination, AnimationCurve heightCurve, AnimationCurve moveCurve, AnimationCurve sideDirCurve)
        {
            hasDestination = true;

            source = CachedTransform.position;
            this.destination = destination;
            this.heightCurve = heightCurve;
            this.moveCurve = moveCurve;
            this.sideDirCurve = sideDirCurve;
        }

        public void SetScale(Vector3 scale)
        {
            CachedTransform.localScale = scale;
        }

        protected override void Update()
        {
            base.Update();

            currentTime += Time.deltaTime;

            if (hasDestination)
            {
                float normalizedTime = currentTime / duration;
                Vector3 toTarget = destination - source;
                Vector3 move = toTarget * moveCurve.Evaluate(normalizedTime);

                move.y += heightCurve.Evaluate(normalizedTime);

                float sideDir = sideDirCurve.Evaluate(normalizedTime);

                if (!Mathf.Approximately(sideDir, 0f))
                {
                    Vector3 right = Vector3.Cross(toTarget.normalized, Vector3.up);
                    move += right * sideDir;
                }

                Vector3 next = source + move;
                CachedTransform.position = next;
                CachedTransform.LookAt(destination);
            }

            // Has Duration
            if (duration > 0f)
            {
                // Off Particle
                if (currentTime > duration)
                {
                    Release();
                    return;
                }
            }
        }
    }
}