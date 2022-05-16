﻿namespace Ragnarok
{
    public sealed class CupetAnimator : LegacyUnitAnimator
    {
        protected override void Initialize()
        {
            if (!IsReady())
                return;

            // 기본 상태 Idle
            PlayIdle();
        }

        public override void PlayIdle()
        {
            Play("Idle");
        }

        public override void PlayRun()
        {
            Play("Run", AniType.Run);
        }

        public override bool CanPlayRun()
        {
            return CanPlay("Run");
        }

        public override void PlayHit()
        {
            Play("Hit");
        }

        public override void PlayDebuff()
        {
            Stop();
            PlayBlend("Debuff");
        }

        public override void PlayDie()
        {
            Stop();
            PlayBlend("Die");

            if (!CanPlay("Dead"))
                return;

            PlayQueued("Dead");
        }

        public override void PlayVictory()
        {
            Play("Victory");
        }

        public override bool IsPlayVictory()
        {
            return IsPlay("Victory");
        }

        public override float PlayEmotion(EmotionType type, Gender gender)
        {
            return default;
        }
    }
}