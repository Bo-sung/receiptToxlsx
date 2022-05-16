using Ragnarok.AI;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 전투 대기 상태 (깜빡임)
    /// </summary>
    public class ReadyWaitState : UnitFsmState
    {
        private bool isHide;
        private Vector3 originScale;

        public ReadyWaitState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            isHide = false;
            originScale = actor.CachedTransform.localScale;

            actor.AI.ResetTarget(); // 타겟 초기화 (Group Camera로 인한 움직임 방지)
            actor.Animator.PlayIdle();
            actor.Movement.Stop();
        }

        public override void Update() // 60FPS 기준 0.1초에 한번 업데이트
        {
            base.Update();

            if (isHide)
            {
                ShowActor();
            }
            else
            {
                HideActor();
            }

            isHide = !isHide;
        }

        public override void End()
        {
            ShowActor();

            base.End();
        }

        private void ShowActor()
        {
            actor.CachedTransform.localScale = originScale;
        }

        private void HideActor()
        {
            actor.CachedTransform.localScale = Vector3.zero;
        }

    }
}