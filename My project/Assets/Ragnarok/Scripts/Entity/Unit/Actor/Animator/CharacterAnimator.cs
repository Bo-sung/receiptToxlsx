using UnityEngine;

namespace Ragnarok
{
    public sealed class CharacterAnimator : LegacyUnitAnimator
    {
        public const int SKILL_COUNT = 71;

        CharacterActor characterActor;
        ICharacterAniContainer characterAniContainer;

        private struct AniName
        {
            public string idle;
            public string run;
            public string hit;
        }

        private AniName aniName;
        private Gender savedGender;
        private EquipmentClassType savedWeaponType;

        void Awake()
        {
            characterActor = GetComponent<CharacterActor>();
            characterAniContainer = AssetManager.Instance;
        }

        /// <summary>
        /// 성별 세팅
        /// </summary>
        public void SetGender(Gender gender)
        {
            if (savedGender == gender)
                return;

            savedGender = gender;
            Initialize();
        }

        /// <summary>
        /// 아이템 타입 세팅
        /// </summary>
        public void SetWeaponType(EquipmentClassType weaponType)
        {
            if (savedWeaponType == weaponType)
                return;

            savedWeaponType = weaponType;
            Initialize();
        }

        protected override Animation GetAnimation(GameObject go)
        {
            Animation ani = base.GetAnimation(go);

            AnimationClip[] clips = characterAniContainer.GetAniClips();

            foreach (AnimationClip clip in clips)
            {
                ani.AddClip(clip, clip.name);
            }

            return ani;
        }

        protected override void Initialize()
        {
            if (!IsReady())
                return;

            int id = GetItemAnimationID(savedWeaponType); // 아이템 아이디 (101, 201, 301 ...)
            int groupID = id / 100; // 아이템 그룹 아이디 (1, 2, 3 ...)
            // 애니메이션 강제로 남자껄로 세팅함
            string gender = "M";

            // 애니메이션 이름 세팅
            aniName.idle = $"G_{groupID}_Idle{gender}"; // G_1_IdleM, G_1_IdleF, G_2_IdleM ...
            aniName.run = $"G_{groupID}_Run{gender}"; // G_1_RunM, G_1_RunF, G_2_RunM ...
            aniName.hit = $"G_{groupID}_Hit"; // G_1_Hit, G_2_Hit, G_3_Hit

            // 기본 애니메이션 세팅
            SetClip(aniName.idle);

            // UI 표시 캐릭터는 무기 변경기 Idle로 변경
            if (characterActor.Entity.type == UnitEntityType.UI)
            {
                // 기본 상태 Idle
                PlayIdle();
                return;
            }

            // 죽어있을때 성별 변경, 무기 타입 변경 시 기존 Die 애니메이션 계속 실행
            if (characterActor.Entity.IsDie && !string.IsNullOrEmpty(curPlayingAnim))
                return;

            // 기본 상태 Idle
            PlayIdle();
        }

        public override void PlayIdle()
        {
            Play(aniName.idle);
        }

        public void QueueIdle()
        {
            PlayQueued(aniName.idle);
        }

        public override void PlayRun()
        {
            Play(aniName.run, AniType.Run);
        }

        public override bool CanPlayRun()
        {
            return CanPlay(aniName.run);
        }

        public override void PlayHit()
        {
            Play(aniName.hit);
        }

        public override void PlayDebuff()
        {
            Stop();
            PlayBlend("G_Debuff");
        }

        public override void PlayDie()
        {
            Stop();
            PlayBlend("G_Die");
            PlayQueued("G_Dead");
        }

        public override void PlayVictory()
        {
            Play("G_Victory");
        }

        public override bool IsPlayVictory()
        {
            return IsPlay("G_Victory");
        }

        public override float PlayEmotion(EmotionType type, Gender gender)
        {
            string genderName = "";
            if (type == EmotionType.Sit)
            {
                genderName = gender == Gender.Male ? "M" : "F";
            }

            var animName = $"G_{type}{genderName}";

            Play(animName);
            if (type != EmotionType.Sit)
            {
                QueueIdle();
            }

            var animTime = GetClipLength(animName);
            return animTime.HasValue ? animTime.Value : 0;
        }

        public bool IsPlayHit()
        {
            return IsPlay(aniName.hit);
        }

        public void PlayChangeJobUp()
        {
            Play("G_ChangeJobUp");
        }

        public void PlayChangeJobDown()
        {
            Play("G_ChangeJobDown");
        }

        public void PlayChangeJobLook()
        {
            Play("G_ChangeJobLook");
        }

        public void PlayThrowStone()
        {
            Play("ThrowStone");
        }

        protected override bool IsReady()
        {
            if (!base.IsReady())
                return false;

            if (savedGender == default || savedWeaponType == default)
                return false;

            return true;
        }

        private int GetItemAnimationID(EquipmentClassType weaponType)
        {
            switch (weaponType)
            {
                case EquipmentClassType.OneHandedSword: // 한손검
                    return 102;

                case EquipmentClassType.OneHandedStaff: // 지팡이
                    return 103;

                case EquipmentClassType.Dagger: // 단검
                    return 101;

                case EquipmentClassType.Bow: // 활
                    return 104;

                case EquipmentClassType.TwoHandedSword: // 양손검
                    return 301;

                case EquipmentClassType.TwoHandedSpear: // 양손창
                    return 203;
            }

            throw new System.ArgumentException($"[올바르지 않은 {nameof(weaponType)}] {nameof(weaponType)} = {weaponType}");
        }
    }
}