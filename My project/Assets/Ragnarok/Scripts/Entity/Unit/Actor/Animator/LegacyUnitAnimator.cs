using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CharacterAnimator"/>
    /// <see cref="MonsterAnimator"/>
    /// <see cref="CupetAnimator"/>
    /// </summary>
    public abstract class LegacyUnitAnimator : UnitAnimator
    {
        Animation aniBody;

        AnimationState moveAniState; // 이동
        AnimationState attackAniState; // 평타

        public bool IsIdle { get; private set; }

        /// <summary>
        /// 몸통 세팅
        /// </summary>
        public override void SetBody(GameObject go)
        {
            aniBody = GetAnimation(go);

            string saved = curPlayingAnim;
            Initialize();

            if (saved != null && CanPlay(saved))
                Play(saved);
        }

        protected virtual Animation GetAnimation(GameObject go)
        {
            Animation ani = go.GetComponent<Animation>();

            if (ani == null)
                ani = go.GetComponentInChildren<Animation>();

            return ani ?? go.AddMissingComponent<Animation>();
        }

        public override float? GetClipLength(string aniName)
        {
            AnimationClip clip = aniBody.GetClip(aniName);
            if (clip == null)
                return null;

            return clip.length;
        }

        protected void Play(string name)
        {
            Play(name, default);
        }

        public override void Play(string name, AniType speedType)
        {
            if (string.IsNullOrEmpty(name))
                return;

            switch (speedType)
            {
                case AniType.Run:
                    if (!IsEqual(moveAniState, name)) // 다른 애니메이션의 경우
                    {
                        SetSpeed(moveAniState, 1f); // 기존 애니메이션 속도 초기화
                        moveAniState = aniBody[name]; // 애니메이션 세팅
                        SetSpeed(moveAniState, moveSpeed); // 애니메이션 속도 세팅
                    }
                    break;

                case AniType.BasicAttack:
                    if (!IsEqual(attackAniState, name)) // 다른 애니메이션의 경우
                    {
                        SetSpeed(attackAniState, 1f); // 기존 애니메이션 속도 초기화
                        attackAniState = aniBody[name]; // 애니메이션 세팅
                        SetSpeed(attackAniState, attackSpeed); // 애니메이션 속도 세팅
                    }
                    break;
            }

            // crossFade 는 0.3 이하의 값으로만 세팅
            const float MAX_CROSS_FADE_LENGTH = 0.15f;
            aniBody.CrossFade(name, Mathf.Min(MAX_CROSS_FADE_LENGTH, MAX_CROSS_FADE_LENGTH / attackSpeed));
            curPlayingAnim = name;
        }

        protected void PlayBlend(string name)
        {
            aniBody.Blend(name);
            curPlayingAnim = name;
        }

        protected void PlayQueued(string name)
        {
            aniBody.PlayQueued(name);
            curPlayingAnim = name;
        }

        protected override void Stop()
        {
            aniBody.Stop();
        }

        public override bool IsPlay(string name)
        {
            return base.IsPlay(name) && aniBody.IsPlaying(name);
        }

        protected bool CanPlay(string name)
        {
            return aniBody.GetClip(name);
        }

        // 기본 클립 세팅
        protected void SetClip(string name)
        {
            aniBody.clip = aniBody.GetClip(name);
        }

        protected override bool IsReady()
        {
            if (aniBody == null)
                return false;

            return true;
        }

        protected override void SetMoveSpeedRate(float speedRate)
        {
            base.SetMoveSpeedRate(speedRate);

            SetSpeed(moveAniState, speedRate);
        }

        protected override void SetAttackSpeedRate(float speedRate)
        {
            base.SetAttackSpeedRate(speedRate);

            SetSpeed(attackAniState, speedRate);
        }

        private bool IsEqual(AnimationState animationState, string name)
        {
            // 존재하지 않음
            if (animationState == null)
                return false;

            return animationState.name.Equals(name);
        }

        private bool SetSpeed(AnimationState animationState, float speed)
        {
            // 존재하지 않음
            if (animationState == null)
                return false;

            // 같은 스피드
            if (animationState.speed == speed)
                return false;

            animationState.speed = speed;
            return true;
        }
    }
}