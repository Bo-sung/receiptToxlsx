using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using MEC;

namespace Ragnarok
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PrologueCharacter : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] PrologueTargetingArrow prologueTargetingArrow;

        private const float DELAY_TO_IDLE_MOTION = 15f;

        private enum AniState { Run, Idle }
        private enum AniTrigger { IdleMotion, }

        GameObject myGameObject;
        NavMeshAgent agent;
        Animator animator;

        Transform target;

        public Transform CachedTransform { get; private set; }

        public event System.Action<GameObject> OnPortal;

        void Awake()
        {
            myGameObject = gameObject;
            CachedTransform = transform;
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
        }

        void Start()
        {
            //MoveStop();
        }

        private void LateUpdate()
        {
            if (prologueTargetingArrow.isActiveAndEnabled)
            {
                if(target != null) prologueTargetingArrow.SetPosition(transform.position, target.position);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tag.PORTAL))
                OnPortal?.Invoke(other.gameObject);
        }

        public void Show()
        {
            NGUITools.SetActive(myGameObject, true);
        }

        public void Hide()
        {
            NGUITools.SetActive(myGameObject, false);
        }

        public void FirstIdle()
        {
            PlayAniState(AniState.Idle, true);
        }

        public void MoveStop()
        {
            // MoveStop
            agent.isStopped = true;

            // PlayAnimation
            PlayIdle();
        }

        public void Move(Vector3 motion)
        {
            // Move
            agent.isStopped = false;
            CachedTransform.rotation = Quaternion.LookRotation(motion);
            agent.Move(motion * agent.speed * Time.deltaTime);

            // PlayAnimation
            PlayRun();
        }

        public void ActiveTargetArrow(bool isActive, Transform target)
        {
            this.target = target;
            prologueTargetingArrow.SetActive(isActive);
        }

        private void PlayIdle()
        {
            PlayAniState(AniState.Run, false);

            Timing.RunCoroutine(YieldIdleMotion().CancelWith(gameObject), nameof(YieldIdleMotion)); // Idle Motion 반복 코루틴 시작
        }

        private void PlayRun()
        {
            PlayAniState(AniState.Run, true);

            Timing.KillCoroutines(nameof(YieldIdleMotion)); // Idle Motion 반복 코루틴 종료
            PlayAniTrigger(AniTrigger.IdleMotion, false);
        }

        private void PlayAniState(AniState state, bool value)
        {
            animator.SetBool(state.ToString(), value);
        }

        private void PlayAniTrigger(AniTrigger trigger, bool value)
        {
            if (value)
            {
                animator.SetTrigger(trigger.ToString());
            }
            else
            {
                animator.ResetTrigger(trigger.ToString());
            }
        }

        IEnumerator<float> YieldIdleMotion()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(DELAY_TO_IDLE_MOTION);
                PlayAniTrigger(AniTrigger.IdleMotion, true);
            }
        }
    }
}