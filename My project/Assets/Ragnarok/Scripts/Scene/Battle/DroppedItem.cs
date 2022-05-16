using UnityEngine;

namespace Ragnarok
{
    public class DroppedItem : PoolObject
    {
        protected enum State
        {
            None,
            Jump = 1,
            MoveToUI,
            Release,
        }

        [SerializeField, Rename(displayName = "튀어오르는 높이")]
        float jumpHeight = 0.14f;

        [SerializeField, Rename(displayName = "UI로 이동 시 걸리는 시간 (초)")]
        float moveDuration = 0.3f;

        [SerializeField, Rename(displayName = "UI카메라 사이의 거리")]
        protected float cameraDistance = 10f;

        [SerializeField, Rename(displayName = "중력 가속도")]
        float gravity = 1;

        [SerializeField, Rename(displayName = "오브젝트 크기")]
        float scale = 0.2f;

        [SerializeField, Rename(displayName = "트레일 크기")]
        float trailScale = 0.5f;

        [SerializeField, Rename(displayName = "위치 오프셋")]
        protected Vector3 offset;

        private State state;
        protected float time;
        private Vector3 home;

        protected float homeY;
        protected Vector3 velocity; // 최종 이동 속도값
        protected Vector3 baseScale;

        public override void OnSpawn()
        {
            base.OnSpawn();

            time = 0f;

            baseScale = CachedTransform.localScale;
            homeY = CachedTransform.position.y;
            velocity = Vector3.up * GetVelocity(jumpHeight + GetPlusHeight());

            ChangeState(State.Jump);
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            ResetItem();
        }

        void LateUpdate()
        {
            time += Time.deltaTime;

            switch (state)
            {
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

        public void SetJumpHeight(float height)
        {
            jumpHeight = height;
        }

        public void SetGravity(float gravity)
        {
            this.gravity = gravity;
        }

        protected void ResetItem()
        {
            time = 0f;
            homeY = CachedTransform.position.y;
            velocity = Vector3.up * GetVelocity(jumpHeight + GetPlusHeight());
            ChangeState(State.None);
        }

        /// <summary>
        /// 튀어오르기
        /// </summary>
		protected virtual void Jump()
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
        /// 점프 종료 여부
        /// </summary>
        protected virtual bool CheckFinishJump()
        {
            return CachedTransform.position.y < homeY;
        }

        /// <summary>
        /// 추가 높이
        /// </summary>
        protected virtual float GetPlusHeight()
        {
            return 0f;
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
                item.widthMultiplier = Mathf.Lerp(1f, trailScale, normalizedTime);
            }

            if (normalizedTime == 1f)
                ChangeState(State.Release);
        }

        /// <summary>
        /// 특정 높이에 해당하는 순간 속력을 가져옵니다
        /// </summary>
        private float GetVelocity(float height)
        {
            return Mathf.Sqrt(height * 2 * gravity);
        }

        /// <summary>
        /// 도착점
        /// </summary>
        protected virtual Vector3 GetDestination()
        {
            UIMainTop ui = UI.GetUI<UIMainTop>();
            if (ui == null)
                return Vector3.zero;

            UIWidget targetWidget = UI.GetUI<UIMainTop>().GetWidget(UIMainTop.MenuContent.Zeny);
            if (targetWidget == null)
                return Vector3.zero;

            Vector3 dest = UI.CurrentCamera.WorldToScreenPoint(targetWidget.cachedTransform.position);
            dest.z = cameraDistance;
            return mainCamera.ScreenToWorldPoint(dest) + offset;
        }

        /// <summary>
        /// 랜덤 값 반환
        /// </summary>
        private float GetRandValue(float value)
        {
            return Random.Range(-value, value);
        }

        protected void ChangeState(State state)
        {
            this.state = state;
            time = 0f;
        }
    }
}