using UnityEngine;

namespace Ragnarok
{
    public class NpcAnimator : AdvancedUnitAnimator
    {
        protected override void Initialize()
        {
        }

        public override bool CanPlayRun()
        {
            return false;
        }

        public override bool IsPlayVictory()
        {
            return false;
        }

        public override void PlayDebuff()
        {
        }

        public override void PlayDie()
        {
        }

        public override void PlayHit()
        {
        }

        public override void PlayIdle()
        {
        }

        public override void PlayRun()
        {
        }

        public override void PlayVictory()
        {
        }

        public override float? GetClipLength(string aniName)
        {
            return null;
        }

        public override void Play(string name, AniType speedType)
        {
        }

        protected override void Stop()
        {
        }

        public override float PlayEmotion(EmotionType type, Gender gender)
        {
            return default;
        }
    }
}