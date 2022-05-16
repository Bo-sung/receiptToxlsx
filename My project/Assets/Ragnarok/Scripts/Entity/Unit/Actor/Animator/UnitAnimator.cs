using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="LegacyUnitAnimator"/>
    /// <see cref="AdvancedUnitAnimator"/>
    /// </summary>
    public abstract class UnitAnimator : MonoBehaviour, IEntityActorElement<UnitEntity>
    {
        public enum AniType
        {
            Run = 1,
            BasicAttack,
        }

        private UnitEntity unitEntity;
        protected float moveSpeed;
        protected float attackSpeed;
        protected string curPlayingAnim;

        public virtual void OnReady(UnitEntity entity)
        {
            unitEntity = entity;

            SetMoveSpeedRate(unitEntity.MoveSpeedRate);
            SetAttackSpeedRate(unitEntity.AttackSpeedRate);
        }

        public virtual void OnRelease()
        {
            unitEntity = null;
        }

        public virtual void AddEvent()
        {
            if (unitEntity)
            {
                unitEntity.OnChangeMoveSpeedRate += SetMoveSpeedRate;
                unitEntity.OnChangeAttackSpeedRate += SetAttackSpeedRate;
            }
        }

        public virtual void RemoveEvent()
        {
            if (unitEntity)
            {
                unitEntity.OnChangeMoveSpeedRate -= SetMoveSpeedRate;
                unitEntity.OnChangeAttackSpeedRate -= SetAttackSpeedRate;
            }
        }

        protected virtual void SetMoveSpeedRate(float speedRate)
        {
            moveSpeed = speedRate;
        }

        protected virtual void SetAttackSpeedRate(float speedRate)
        {
            attackSpeed = speedRate;
        }

        /// <summary>
        /// 전투 애니메이션 초기화
        /// </summary>
        protected abstract void Initialize();

        public abstract void PlayIdle();

        public abstract void PlayRun();

        public abstract bool CanPlayRun();

        public abstract void PlayHit();

        public abstract void PlayDebuff();

        public abstract void PlayDie();

        public abstract void PlayVictory();

        public abstract bool IsPlayVictory();

        public abstract float PlayEmotion(EmotionType type, Gender gender);

        /// <summary>
        /// 애니메이션 길이 반환
        /// </summary>
        public abstract float? GetClipLength(string aniName);

        public abstract void SetBody(GameObject go);

        public abstract void Play(string name, AniType speedType);

        public virtual bool IsPlay(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return !string.IsNullOrEmpty(curPlayingAnim) && curPlayingAnim.Equals(name);
        }

        protected abstract bool IsReady();

        [System.Obsolete("Die 후에 Idle로 돌아가기 때문에 임시로 넣었음. 추후 확인 후 뺄 것")]
        protected abstract void Stop();
    }
}