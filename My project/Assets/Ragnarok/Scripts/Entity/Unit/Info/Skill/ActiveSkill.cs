using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Ragnarok
{
    public class ActiveSkill : SkillInfo
    {
        public enum Type
        {
            /// <summary>
            /// 공격형 스킬 (물댐, 마댐, 상태이상)
            /// </summary>
            Attack,
            /// <summary>
            /// 회복형 스킬
            /// </summary>
            RecoveryHp,
            /// <summary>
            /// 버프형 스킬
            /// </summary>
            Buff,
        }

        private readonly ActiveBattleOptionList options;

        ObscuredLong slotNo; // 서버 슬롯 고유 값
        ObscuredInt slotPos; // 슬롯 장착 위치
        ObscuredInt skillRate; // 스킬 발동 확률
        ObscuredBool isChainedSkill; // 연계로 인해 발동된 스킬 여부

        /// <summary>
        /// 끝나는 시간
        /// </summary>
        private BattleRemainTime endTime;

        /// <summary>
        /// 스킬 사용했을 때 쿨타임 시간
        /// cooltime은 쿨감옵션에 의해 바뀔 수 있다
        /// </summary>
        private float savedRealCooldownTime;

        /// <summary>
        /// 서버 쿨타임 응답 대기 상태
        /// </summary>
        private bool isResponseCooldownCheckState;

        public override long SlotNo => slotNo;
        public override int EffectID => data.effect_id;
        public override Type ActiveSkillType => GetActiveSkillType();
        public override float RemainCooldownTime => endTime * 0.001f;
        public override AttackType AttackType => data.attack_type.ToEnum<AttackType>();
        public override ElementType ElementType => data.element_type.ToEnum<ElementType>();
        public override int BlowCount => data.blow_count;
        public override bool IsRush => data.rush_check;
        public override EffectPointType PointType => data.point_type.ToEnum<EffectPointType>();
        public override TargetType TargetType => data.target_type.ToEnum<TargetType>();
        public override int SkillRange => data.skill_range;
        public override int SkillArea => data.skill_area;
        public override float Duration => data.duration * 0.001f;
        public override ActiveBattleOptionList ActiveOptions => options;
        public override int SlotPos => slotPos;

        public ActiveSkill()
        {
            options = new ActiveBattleOptionList();
        }

        public override bool IsValid(EquipmentClassType weaponType)
        {
            // 장착하지 않은 스킬
            if (!IsEquippedSkill())
                return false;

            return base.IsValid(weaponType);
        }

        public override bool GetIsAutoSkill()
        {
            // slotNo > 0L => 슬롯 고유값이 존재(플레이어 스킬)
            if (BattleManager.isAntiAutoSkill)
            {
                // slotNo > 0L => 슬롯 고유값이 존재 (플레이어 스킬)
                if (slotNo > 0L || RefSlotNo > 0L)
                    return false;
            }

            return skillRate == 10000;
        }

        public override void ResetSlotInfo()
        {
            slotNo = 0L;
            slotPos = -1;
        }

        public override void SetSlotInfo(long slotNo, int slotPos)
        {
            this.slotNo = slotNo;
            this.slotPos = slotPos;
        }

        public override void SetSkillRate(int skillRate)
        {
            this.skillRate = skillRate;
        }

        public override void SetChainableSkill(bool isChainedSkill)
        {
            this.isChainedSkill = isChainedSkill;
        }

        public override bool IsEqualSlotPos(int slotPos)
        {
            return this.slotPos == slotPos;
        }

        public override bool HasRemainCooldownTime()
        {
            // 서버 대기 중에는 쿨타임 존재
            if (isResponseCooldownCheckState)
                return true;

            return RemainCooldownTime > 0f;
        }

        public override bool IsCheckSkillRate()
        {
            // Active 형식의 스킬은 무조건 자동스킬이 아니다 (평타는 기존과 동일하게 처리)
            if (BattleManager.isAntiAutoSkill && SkillType == SkillType.Active)
            {
                // slotNo > 0L => 슬롯 고유값이 존재 (플레이어 스킬)
                if (slotNo > 0L || RefSlotNo > 0L)
                    return false;
            }

            return MathUtils.IsCheckPermyriad(skillRate);
        }

        public override bool IsChainedSkill()
        {
            return isChainedSkill;
        }

        /// <summary>
        /// 쿨타임 적용 (클라 전용)
        /// </summary>
        public override void StartCooldownWithCooldownRate(int cooldownRate)
        {
            long calculatedRemainTime = GetRealCooldownTime(cooldownRate); // 쿨타임 계산 (밀리초)
            StartCooldown(calculatedRemainTime);
        }

        /// <summary>
        /// 쿨타임 적용 서버 응답 대기 상태
        /// </summary>
        public override void SetResponseCooldownCheckState()
        {
            isResponseCooldownCheckState = true;
        }

        /// <summary>
        /// 쿨타임 적용 (밀리초)
        /// </summary>
        public override void StartCooldown(long remainTime)
        {
            isResponseCooldownCheckState = false;

            endTime = remainTime;

            if (RefBattleOption == BattleOptionType.None)
            {
                savedRealCooldownTime = RemainCooldownTime;
            }
            else
            {
                savedRealCooldownTime = GetRealCooldownTime(0) * 0.001f;
            }
        }

        public override float GetCooldownProgress()
        {
            // 쿨타임 대기중
            if (isResponseCooldownCheckState)
                return 1f;

            // 쿨타임이 없는 스킬
            if (savedRealCooldownTime == 0f)
                return 0f;

            return RemainCooldownTime / savedRealCooldownTime;
        }

        public override float GetSkillRange(float atkRangeRate)
        {
            float skillRange = GetSkillRange();

            // 사정거리가 전체 or 사정거리가 없음 or 사정거리 증가율이 없음
            if (skillRange == -1f || skillRange == 0f || atkRangeRate == 1f)
                return skillRange;

            float realSkillRange = skillRange * atkRangeRate;
            if (realSkillRange <= 0f)
                return 0f;

            return realSkillRange;
        }

        public override float GetSkillArea()
        {
            int skillArea = data.skill_area;

            // 스킬범위가 전체
            if (skillArea == -1)
                return -1f;

            // 스킬범위가 없음
            if (skillArea == 0f)
                return 0f;

            return skillArea * 0.01f;
        }

        /// <summary>
        /// 쿨타임 감소에 따른 쿨타임 시간 계산 (밀리초)
        /// </summary>
        public override int GetRealCooldownTime(int cooldownRate)
        {
            int cooldownTime = data.cooldown;

            // 쿨타임이 존재하지 않음 or 쿨타임 감소율이 없음
            if (cooldownTime == 0f || cooldownRate == 0)
                return cooldownTime;

            int realCooldownTime = MathUtils.ToInt(cooldownTime * (1 - MathUtils.ToPermyriadValue(cooldownRate)));
            return Mathf.Max(0, realCooldownTime); // 0 이상의 값
        }

        private Type GetActiveSkillType()
        {
            if (options.HasDamageValue || options.HasCrowdControl)
                return Type.Attack;

            if (options.HasRecoveryValue)
                return Type.RecoveryHp;

            if (data.duration > 0)
                return Type.Buff;

            Debug.LogError($"[정의되지 않은 ActiveSkillType] {nameof(SkillId)} = {SkillId}");
            return default;
        }

        /// <summary>
        /// 장착된 스킬 여부
        /// </summary>
        private bool IsEquippedSkill()
        {
            return slotPos != -1;
        }

        /// <summary>
        /// 사정거리 반환
        /// </summary>
        private float GetSkillRange()
        {
            int skillRange = data.skill_range;

            // 사정거리가 전체
            if (skillRange == -1)
                return -1f;

            // 사정거리가 없음
            if (skillRange == 0)
                return 0f;

            return skillRange * 0.01f;
        }

        public override void SetData(SkillData data)
        {
            base.SetData(data);

            options.Clear();
            options.AddRange(this);
        }

        public override void ResetData()
        {
            base.ResetData();

            options.Clear();
        }
    }
}