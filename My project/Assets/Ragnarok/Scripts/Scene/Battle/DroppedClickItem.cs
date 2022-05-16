using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(Collider))]
    public sealed class DroppedClickItem : PoolObject, IInspectorFinder
    {
        private enum State
        {
            Init = 1,
            Drop,
            StayOnGround,
            MoveToUI,
            Release,
        }

        [SerializeField, Rename(displayName = "튀어오르는 힘")]
        float upVectorForce = 10f;

        [SerializeField, Rename(displayName = "퍼지는 힘")]
        float sideVectorForce = 1.5f;

        [SerializeField, Rename(displayName = "UI로 이동 시 걸리는 시간 (초)")]
        float moveDuration = 0.3f;

        static bool isInitOnce = false;
        static UIWidget targetWidget;

        private State state;
        private float time;
        private Vector3 home;
        private Vector3 jumpingPowerVector;
        [SerializeField] private Rigidbody cachedRigidbody;
        [SerializeField] private BoxCollider cachedCollider;
        [SerializeField] private TweenScale cachedTweenScale;

        public override void OnCreate(IPooledDespawner despawner, string poolID)
        {
            base.OnCreate(despawner, poolID);

            if (!isInitOnce)
            {
                targetWidget = UI.GetUI<UIMainTop>().GetWidget(UIMainTop.MenuContent.Zeny);
                isInitOnce = true;
            }
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            if (cachedRigidbody != null)
                cachedRigidbody.useGravity = true;

            home = CachedTransform.position;
            home.y = UnitMovement.POSITION_Y;

            jumpingPowerVector = new Vector3(Random.Range(-sideVectorForce, sideVectorForce), upVectorForce, Random.Range(-sideVectorForce, sideVectorForce));

            ChangeState(State.Init);
        }

        /// <summary>
        ///	외부에서 동전이 튀어오르는 힘을 조정할 수 있게 해주는 함수
        ///	<param name="vec">Vector3(x, 높이, z)</param>
        /// </summary>
        public void SetJumpingPower(Vector3 vec)
        {
            jumpingPowerVector = vec;
        }

        void Update()
        {
            time += Time.deltaTime;

            switch (state)
            {
                case State.Init:
                    Init();
                    break;

                case State.Drop:
                    Drop();
                    break;

                case State.StayOnGround:
                    StayOnGround();
                    break;

                case State.MoveToUI:
                    MoveToUI();
                    break;

                case State.Release:
                    Release();
                    break;
            }
        }

        private void Init()
        {
            CachedTransform.position = home;

            if (cachedRigidbody != null)
            {
                cachedRigidbody.velocity = Vector3.zero;
                cachedRigidbody.AddForce(jumpingPowerVector, ForceMode.VelocityChange);
            }

            ChangeState(State.Drop);
        }

        private void Drop()
        {
            if (CachedTransform.position.y > home.y)
                return;

            home = CachedTransform.position;
            if (cachedRigidbody != null)
            {
                cachedRigidbody.useGravity = false;
                cachedRigidbody.velocity = Vector3.zero;
            }
            DroppedItemPickingManager.Instance.picking.onTouchHit += OnRayCastHit;
            ChangeState(State.StayOnGround);
        }

        private void StayOnGround()
        {
            cachedTweenScale.enabled = true;

            //// 일정 시간이 지나면 코인이 사라진다.
            //if (time > (BattleManager.Instance.DungeonInfo.Chapter == 1 ? 15f : coinRemainTime)) // 첫번째 챕터인 경우에만 동전의 수명 연장.. (튜토리얼을 위해)
            //    ChangeState(State.Release);
        }

        private async void OnRayCastHit(RaycastHit hit)
        {
            if (state != State.StayOnGround)
                return;

            if (!hit.collider.Equals(cachedCollider))
                return;

            ////	프로토콜 송수신 시작
            //// 던전 필드 테이블 아이디를 얻을 때 NullReferenceException 에러를 피할 수 없어서 try catch로 ...
            //try
            //{
            //    DungeonInfo dungeonInfo = BattleManager.Instance?.DungeonInfo;
            //    if (dungeonInfo == null)
            //        throw new System.Exception("Dungeon Info is null");

            //    int fieldDataID = dungeonInfo.FieldDataID;
            //    await Entity.player.User.RequestGetBossBonusZeny(fieldDataID);
            //}
            //catch (System.Exception e)
            //{
            //    Debug.LogError("Error at Protocol.REQUEST_GET_BOSS_BONUS_ZENY : dungeonInfo.FieldDataID is null.\n" + e?.Message + "\n" + e?.StackTrace);
            //    ChangeState(State.Release);
            //    return;
            //}

            ChangeState(State.MoveToUI);
        }

        private void MoveToUI()
        {
            float normalizedTime = Mathf.Clamp01(time / moveDuration);
            Debug.LogWarning("TODO MoveToUI");
            //CachedTransform.position = Vector3.Lerp(home, CameraManager.Instance.world2UICameraSync.zenyAreaTransform.position, normalizedTime * 0.95f);

            if (normalizedTime == 1f)
                ChangeState(State.Release);
        }

        private void ChangeState(State state)
        {
            this.state = state;
            time = 0f;
        }

        private new void Release()
        {
            cachedTweenScale.enabled = false;
            DroppedItemPickingManager.Instance.picking.onTouchHit -= OnRayCastHit;
            base.Release();
        }

        bool IInspectorFinder.Find()
        {
            cachedRigidbody = GetComponent<Rigidbody>();
            cachedCollider = GetComponent<BoxCollider>();
            return true;
        }
    }
}