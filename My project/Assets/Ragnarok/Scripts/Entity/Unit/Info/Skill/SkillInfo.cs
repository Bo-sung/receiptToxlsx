using CodeStage.AntiCheat.ObscuredTypes;
using Ragnarok.View;
using Ragnarok.View.Skill;
using System.Collections;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// 스킬 정보
    /// <see cref="ActiveSkill"/> 액티브
    /// <see cref="PassiveSkill"/> 패시브
    /// <see cref="GuildSkill"/> 길드스킬
    /// <see cref="ItemSkill"/> 아이템스킬
    /// </summary>
    public abstract class SkillInfo : DataInfo<SkillData>, IEnumerable<BattleOption>, SkillInfo.IValue, BattleBuffSkillInfo.ISettings, SkillModel.ISkillValue, SkillModel.ISkillSimpleValue, UIItemSkillInfo.IInput, UISkillInfo.IInfo
    {
        #region 공통

        ObscuredLong skillNo; // 서버 스킬 고유 값
        ObscuredBool isInPossession; // 실제 스킬 보유 여부

        /// <summary>
        /// [공통] 보유 여부
        /// </summary>
        public bool IsInPossession => isInPossession;

        /// <summary>
        /// [공통] 스킬 서버 고유 값 (프로토콜 등에 사용)
        /// </summary>
        public long SkillNo => skillNo;

        /// <summary>
        /// [공통] 스킬 ID
        /// </summary>
        public int SkillId => data.id;

        /// <summary>
        /// [공통] 스킬 레벨
        /// </summary>
        public int SkillLevel => data.lv;

        /// <summary>
        /// [공통] 스킬 이름
        /// </summary>
        public string SkillName => data.name_id.ToText();

        /// <summary>
        /// [공통] 스킬 설명
        /// </summary>
        public string SkillDescription => data.des_id.ToText();

        /// <summary>
        /// [공통] 스킬 아이콘 이름
        /// </summary>
        public string IconName => data.icon_name;

        /// <summary>
        /// [공통] 호환가능 무기타입
        /// </summary>
        public EquipmentClassType EquipmentClassType => data.class_bit_type.ToEnum<EquipmentClassType>();

        /// <summary>
        /// [공통] 스킬 타입 (액티브, 패시브, 평타)
        /// </summary>
        public SkillType SkillType => data.skill_type.ToEnum<SkillType>();

        /// <summary>
        /// [공통] 스킬 평타 여부
        /// </summary>
        public bool IsBasicActiveSkill => SkillType == SkillType.BasicActiveSkill;

        /// <summary>
        /// [공통] 참조 옵션 타입
        /// </summary>
        public BattleOptionType RefBattleOption { get; private set; }

        /// <summary>
        /// [공통] 참조 옵션 슬롯 No
        /// </summary>
        public long RefSlotNo { get; private set; }

        /// <summary>
        /// [공통] 스킬 등급
        /// </summary>
        public int Grade => data.grade;

        int SkillModel.ISkillValue.OrderId => 0; // 불필요 (Dummy 세팅 용도)
        int SkillModel.ISkillValue.ChangeSkillId => 0; // 불필요 (Dummy 세팅 용도: 바뀐 값으로 Skill 세팅)

        int SkillModel.ISkillSimpleValue.SkillId => SkillId;
        int SkillModel.ISkillSimpleValue.SkillLevel => SkillLevel;
        SkillType SkillModel.ISkillSimpleValue.SkillType => SkillType;

        string SkillModel.ISkillSimpleValue.GetIconName() => IconName;
        string UISkillInfo.IInfo.SkillIcon => IconName;

        #endregion

        /// <summary>
        /// [액티브] 스킬 슬롯 서버 고유 값 (프로토콜 등에 사용)
        /// </summary>
        public virtual long SlotNo => default;

        /// <summary>
        /// [액티브] 이펙트 ID
        /// </summary>
        public virtual int EffectID => default;

        /// <summary>
        /// [액티브] 액티브 스킬 타입
        /// </summary>
        public virtual ActiveSkill.Type ActiveSkillType => default;

        /// <summary>
        /// [액티브] 실제 남은 쿨타임 (초)
        /// </summary>
        public virtual float RemainCooldownTime => default;

        /// <summary>
        /// [액티브] 공격 타입 (근거리/원거리)
        /// </summary>
        public virtual AttackType AttackType => default;

        /// <summary>
        /// [액티브] 스킬 속성
        /// </summary>
        public virtual ElementType ElementType => default;

        /// <summary>
        /// [액티브] 타격 횟수
        /// </summary>
        public virtual int BlowCount => default;

        /// <summary>
        /// [액티브] 돌진 공격 타입
        /// </summary>
        public virtual bool IsRush => default;

        /// <summary>
        /// [액티브] 효과 발동 기준
        /// </summary>
        public virtual EffectPointType PointType => default;

        /// <summary>
        /// [액티브] 타겟 타입
        /// </summary>
        public virtual TargetType TargetType => default;

        /// <summary>
        /// [액티브] 사정거리 (사정 거리에 들어왔을 경우에는 스킬 사용)
        /// </summary>
        public virtual int SkillRange => default;

        /// <summary>
        /// [액티브] 효과 범위 (0보다 클 경우 효과가 EffectPointType 기준으로 광역으로 들어감)
        /// </summary>
        public virtual int SkillArea => default;

        /// <summary>
        /// [액티브] 지속 시간
        /// </summary>
        public virtual float Duration => default;

        /// <summary>
        /// [액티브] 스킬 전투 옵션
        /// </summary>
        public virtual ActiveBattleOptionList ActiveOptions => default;

        /// <summary>
        /// [액티브] 스킬 슬롯 위치
        /// </summary>
        public virtual int SlotPos => default;

        /// <summary>
        /// [액티브] 스킬 마나 코스트 
        /// </summary>
        public int MpCost => data.mp_cost;

        public float CastingTime => data.casting_time * 0.001f;

        public float ChasingTime => data.casting_time_chase * 0.001f;

        public float RangeFrom => data.skill_area_gap * 0.01f;

        public float RangeTo => RangeFrom + data.skill_area * 0.01f;

        public float Angle => data.skill_angle;

        /// <summary>
        /// 데이터
        /// </summary>
        SkillData BattleBuffSkillInfo.ISettings.Data => data;

        /// <summary>
        /// [공통] 스킬 서버 고유 no 세팅
        /// </summary>
        public void SetSkillInfo(long no)
        {
            skillNo = no; // 스킬 서버 고유 no
        }

        /// <summary>
        /// [공통] 보유 여부 세팅
        /// </summary>
        public void SetIsInPossession()
        {
            isInPossession = true;
        }

        public void ResetIsInPossession()
        {
            isInPossession = false;
        }

        /// <summary>
        /// [공통] 전투 옵션 반환
        /// </summary>
        public IEnumerable<BattleOption> GetBattleOptionCollection()
        {
            return GetBattleOptionCollection(SkillLevel);
        }

        /// <summary>
        /// [공통] 특정 레벨의 전투 옵션 반환
        /// </summary>
        public IEnumerable<BattleOption> GetBattleOptionCollection(int level)
        {
            yield return new BattleOption(data.battle_option_type_1, data.value1_b1, data.value2_b1);
            yield return new BattleOption(data.battle_option_type_2, data.value1_b2, data.value2_b2);
            yield return new BattleOption(data.battle_option_type_3, data.value1_b3, data.value2_b3);
            yield return new BattleOption(data.battle_option_type_4, data.value1_b4, data.value2_b4);
        }

        /// <summary>
        /// [공통] 유효한 스킬 여부
        /// </summary>
        public virtual bool IsValid(EquipmentClassType weaponType)
        {
            return IsAvailableWeapon(weaponType); // 호환 가능한 무기의 경우
        }

        /// <summary>
        /// [공통] 참조 옵션 여부
        /// </summary>
        public void SetRefBattleOption(long refSlotNo, BattleOptionType type)
        {
            RefSlotNo = refSlotNo;
            RefBattleOption = type;
        }

        /// <summary>
        /// [액티브] 스킬 슬롯 정보 초기화
        /// </summary>
        public virtual void ResetSlotInfo()
        {
        }

        /// <summary>
        /// [액티브] 슬롯 정보 세팅
        /// </summary>
        public virtual void SetSlotInfo(long slotNo, int slotPos)
        {
        }

        /// <summary>
        /// [액티브] 스킬 발동 확률 세팅
        /// </summary>
        public virtual void SetSkillRate(int skillRate)
        {
        }

        /// <summary>
        /// [액티브] 연계로 인해 발동되는 스킬 여부 세팅
        /// </summary>
        public virtual void SetChainableSkill(bool isChainableSkill)
        {
        }

        /// <summary>
        /// [액티브] Auto 스킬 여부
        /// </summary>
        public virtual bool GetIsAutoSkill()
        {
            return default;
        }

        /// <summary>
        /// [액티브] 같은 슬롯 위치에 해당하는 여부
        /// </summary>
        public virtual bool IsEqualSlotPos(int slotPos)
        {
            return default;
        }

        /// <summary>
        /// [액티브] 쿨타임 남아있는지 여부
        /// </summary>
        public virtual bool HasRemainCooldownTime()
        {
            return default;
        }

        /// <summary>
        /// [액티브] 스킬 발동 확률 체크
        /// </summary>
        public virtual bool IsCheckSkillRate()
        {
            return default;
        }

        /// <summary>
        /// [액티브] 연계로 인해 발동되는 스킬 여부
        /// </summary>
        public virtual bool IsChainedSkill()
        {
            return default;
        }

        /// <summary>
        /// [액티브] 쿨타임 적용
        /// </summary>
        public virtual void StartCooldown(long remainTime)
        {
        }

        /// <summary>
        /// [액티브] 쿨타임 적용 (쿨타임 감소율 고려)
        /// </summary>
        public virtual void StartCooldownWithCooldownRate(int cooldownRate)
        {
        }

        /// <summary>
        /// [액티브] 쿨타임 적용 서버 응답 대기 상태
        /// </summary>
        public virtual void SetResponseCooldownCheckState()
        {
        }

        /// <summary>
        /// [액티브] 쿨타임 진행도
        /// </summary>
        public virtual float GetCooldownProgress()
        {
            return default;
        }

        /// <summary>
        /// [액티브] 사정거리 반환
        /// </summary>
        public virtual float GetSkillRange(float atkRangeRate)
        {
            return default;
        }

        /// <summary>
        /// [액티브] 스킬범위 반환
        /// </summary>
        public virtual float GetSkillArea()
        {
            return default;
        }

        /// <summary>
        /// [액티브] 쿨타임 감소에 따른 쿨타임 시간 계산 (밀리초)
        /// </summary>
        public virtual int GetRealCooldownTime(int cooldownRate)
        {
            return 0;
        }

        /// <summary>
        /// 해당 무기로 사용 가능한 스킬인지의 여부를 반환
        /// </summary>
        public bool IsAvailableWeapon(EquipmentClassType weaponType)
        {
            return EquipmentClassType.HasFlag(weaponType);
        }

        public IEnumerator<BattleOption> GetEnumerator()
        {
            return GetBattleOptionCollection().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public interface IValue
        {
            int SkillId { get; }
            int SkillLevel { get; }
            AttackType AttackType { get; }
            //ElementType ElementType { get; }
            bool IsBasicActiveSkill { get; }
            int BlowCount { get; }
            ActiveBattleOptionList ActiveOptions { get; }
            bool IsInPossession { get; } // 실제로 보유한 스킬
        }
    }
}