using UnityEngine;

namespace Ragnarok
{
    public class DropCube : PoolObject
    {
        private enum State
        {
            Init,
            Delay,
            Jump,
            MoveToUI,
            Release,
        }

        [SerializeField, Rename(displayName = "대기시간 (초)")]
        float DelayDuration = 0.3f;

        [SerializeField, Rename(displayName = "UI로 이동 시 걸리는 시간 (초)")]
        float moveDuration = 0.3f;

        // 거리가 크면 바닥으로 들어감
        [SerializeField, Rename(displayName = "UI카메라 사이의 거리")]
        protected float cameraDistance = 10f;

        [SerializeField, Rename(displayName = "위치 오프셋")]
        Vector3 offset;

        [SerializeField, Rename(displayName = "튀어오르는 높이")]
        float jumpHeight = 0.14f;

        [SerializeField, Rename(displayName = "중력 가속도")]
        float gravity = 1;

        [SerializeField, Rename(displayName = "오브젝트 크기")]
        float scale = 0.2f;

        private State state;
        private float time;
        private Vector3 home;
        private UIWidget target;
        private float homeY;
        private Vector3 velocity; // 최종 이동 속도값
        private Vector3 baseScale;

        public void Initialize(UIWidget target)
        {
            this.target = target;
            //home = CachedTransform.position; // 현재 위치            
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            ChangeState(State.Init);
        }

        private void ChangeState(State state)
        {
            this.state = state;
            time = 0f;
        }

        public void Play()
        {
            baseScale = CachedTransform.localScale;
            homeY = CachedTransform.position.y;
            velocity = Vector3.up * GetVelocity(jumpHeight);
            ChangeState(State.Jump);
        }

        void LateUpdate()
        {
            time += Time.deltaTime;

            switch (state)
            {
                case State.Delay:
                    Delay();
                    break;

                case State.Jump:
                    Jump();
                    break;

                case State.MoveToUI:
                    MoveToUI();
                    break;

                case State.Release:
                    Release();
                    break;
            }
        }

        private void Delay()
        {
            float normalizedTime = Mathf.Clamp01(time / DelayDuration);
            if (normalizedTime == 1f)
            {
                ChangeState(State.Jump);
            }
        }

        private void Jump()
        {
            velocity.y -= gravity * Time.deltaTime; // Apply Gravity
            CachedTransform.position += velocity;

            if (CheckFinishJump())
            {
                home = CachedTransform.position;
                ChangeState(State.MoveToUI);
            }
        }

        /// <summary>
        /// 특정 높이에 해당하는 순간 속력을 가져옵니다
        /// </summary>
        private float GetVelocity(float height)
        {
            return Mathf.Sqrt(height * 2 * gravity);
        }

        /// <summary>
        /// 점프 종료 여부
        /// </summary>
        protected virtual bool CheckFinishJump()
        {
            return CachedTransform.position.y < homeY;
        }

        /// <summary>
        /// UI를 향하여 이동
        /// </summary>
		private void MoveToUI()
        {
            float normalizedTime = Mathf.Clamp01(time / moveDuration);
            CachedTransform.position = Vector3.Lerp(home, GetDestination(), normalizedTime);
            CachedTransform.localScale = Vector3.Lerp(baseScale, Vector3.one * scale, normalizedTime);
            foreach (var item in trailParticles)
            {
                item.widthMultiplier = Mathf.Lerp(1f, scale, normalizedTime);
            }

            if (normalizedTime == 1f)
                ChangeState(State.Release);
        }

        /// <summary>
        /// 도착점
        /// </summary>
        protected virtual Vector3 GetDestination()
        {
            if (target == null)
                return Vector3.zero;

            Vector3 dest = UI.CurrentCamera.WorldToScreenPoint(target.cachedTransform.position);
            dest.z = cameraDistance;

            return mainCamera.ScreenToWorldPoint(dest) + offset;
        }
    }
}
