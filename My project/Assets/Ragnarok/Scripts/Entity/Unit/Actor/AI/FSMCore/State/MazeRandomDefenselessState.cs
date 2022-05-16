namespace Ragnarok.AI
{
    /// <summary>
    /// 미로 행동불능상태 (랜덤 그로기)
    /// </summary>
    public class MazeRandomDefenselessState : UnitFsmState
    {
        public MazeRandomDefenselessState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        public override void Begin()
        {
            base.Begin();

            actor.Movement.Stop();
            actor.Animator.PlayDebuff();

            actor.EffectPlayer.ShowRandomGroggy(isPlaySfx: true);

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