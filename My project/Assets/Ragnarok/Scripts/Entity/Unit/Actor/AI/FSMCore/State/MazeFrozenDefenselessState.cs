namespace Ragnarok.AI
{
    /// <summary>
    /// 미로 행동불능상태 (얼음)
    /// </summary>
    public class MazeFrozenDefenselessState : UnitFsmState
    {
        public MazeFrozenDefenselessState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop();
            actor.Animator.PlayDebuff();

            actor.EffectPlayer.ShowFrozen(isPlaySfx: true);

            actor.AI.ResetTarget(); // 타겟 초기화 (Group Camera로 인한 움직임 방지)
            actor.AI.SetInputMove(isControl: false); // 이동 상태 취소
        }

        public override void End()
        {
            base.End();

            actor.EffectPlayer.HideGroggy();
            actor.Animator.PlayIdle();
        }
    }
}