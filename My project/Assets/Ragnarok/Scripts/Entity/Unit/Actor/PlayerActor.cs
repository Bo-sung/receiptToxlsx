using UnityEngine;

namespace Ragnarok
{
    public class PlayerActor : CharacterActor
    {
        protected override bool IsPickingEnable()
        {
            return true;
        }

        public override bool IsRequestSkillCooltime()
        {
            return true;
        }

        public override void RequestSkillCooltimeCheck(SkillInfo info)
        {
            base.RequestSkillCooltimeCheck(info);

            // 평타는 쿨타임 체크를 하지 않는다.
            if (info.IsBasicActiveSkill)
                return;

            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", 0); // cupet Id

            if (info.SlotNo > 0L)
            {
                // 사용해서 나간 액티브 스킬
                sfs.PutLong("2", info.SlotNo); // Skill SlotNo

                info.SetResponseCooldownCheckState(); // 쿨타임 대기 상태로 변경
                sfs.PutLong(SkillModel.SKILL_COOLDOWN_CHECK_SKILL_NO_KEY, info.SkillNo);
            }
            else
            {
                // 참조되서 나간 패시브 스킬
                sfs.PutInt("4", (int)info.RefBattleOption); // 참조 옵션
                sfs.PutInt("5", info.SkillId); // 참조 스킬 아이디

                if (info.RefBattleOption == BattleOptionType.SkillOverride)
                    sfs.PutInt("6", info.SkillLevel); // 참조 스킬 레벨

                int cooldownRate = Entity.battleStatusInfo.CooldownRate; // 쿨타임 감소율
                int clientCoondownTime = info.GetRealCooldownTime(cooldownRate); // 클라가 계산한 쿨타임
                if (clientCoondownTime > 0)
                {
                    Entity.battleSkillInfo.SetRefSkillResponseCooldownCheckState(info.SkillId); // 쿨타임 대기 상태로 변경
                    sfs.PutInt(SkillModel.SKILL_COOLDOWN_CHECK_REF_SKILL_ID_KEY, info.SkillId);
                    sfs.PutInt(SkillModel.SKILL_COOLDOWN_CHECK_REF_SKILL_COOLDOWN_TIME_KEY, clientCoondownTime);
                }

                if (DebugUtils.IsLogUseSkill)
                {
                    Debug.LogError($"참조되서 나간 스킬: {nameof(info.RefBattleOption)} = {info.RefBattleOption}, {nameof(info.SkillId)} = {info.SkillId}");
                }
            }

            Protocol.SKILL_COOLTIME_CHECK.SendAsync(sfs).WrapNetworkErrors();
        }

        protected override ISkillArea ShowSkillAreaCircle(SkillInfo skillInfo, UnitActor target)
        {
            UnitActor pointTarget = skillInfo.PointType == EffectPointType.Target ? target : this;
            var skillArea = skillInfo.GetSkillArea();
            ISkillArea ret = new EmptySkillArea(skillInfo, Entity.LastPosition, target.Entity.LastPosition);

            if (skillArea > 0f) // -1 인 SkillArea 가 있을 수 있다.
            {
                bool isAttackSkill = skillInfo.ActiveOptions.HasDamageValue || skillInfo.ActiveOptions.HasCrowdControl; // 공격형 스킬
                ret = EffectPlayer.ShowSkillAreaCircle(lifeCycle, skillInfo, Entity.LastPosition, target.Entity.LastPosition);
            }

            return ret;
        }
    }
}