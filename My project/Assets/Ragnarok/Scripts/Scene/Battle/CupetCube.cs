using UnityEngine;

namespace Ragnarok
{
    public class CupetCube : PoolObject
    {
        private enum State
        {
            Init,
            Play,
            Stay,
            Release,
        }

        [SerializeField] TweenPosition tween;

        [SerializeField, Rename(displayName = "목적지로 이동 시 걸리는 시간 (초)")]
        float moveDuration = 0.3f;

        [SerializeField, Rename(displayName = "큐브 높이")]
        float cubeMaxHeight = 1.5f;

        private State state;
        private float time;
        private Vector3 home;
        private Vector3 target;
        private CupetEntity cupet;

        public System.Action<Vector3, CupetEntity> OnFinish;
        public CupetEntity Cupet => cupet;

        public void Initialize(Vector3 home, Vector3 target, CupetEntity cupet)
        {
            this.home = home;
            this.target = target;
            this.cupet = cupet;

            this.time = 0f;
            this.state = State.Init;
        }

        public void Play()
        {
            this.time = 0f;
            this.state = State.Play;
        }

        private void Update()
        {
            time += Time.deltaTime;

            switch (state)
            {
                case State.Init:
                    break;

                case State.Play:
                    UpdatePlay();
                    break;

                case State.Stay:
                    break;

                case State.Release:
                    Release();
                    break;
            }
        }

        void UpdatePlay()
        {
            float normalizedTime = Mathf.Clamp01(this.time / moveDuration);
            float curHeight = this.home.y + tween.animationCurve.Evaluate(normalizedTime) * cubeMaxHeight;
            Vector3 newPos = Vector3.Lerp(this.home, this.target, normalizedTime);
            newPos.y = curHeight;
            CachedTransform.position = newPos;

            if (normalizedTime == 1f)
            {
                OnFinish?.Invoke(CachedTransform.position, cupet);
                OnFinish = null;
                this.state = State.Stay;
            }
        }
    }
}