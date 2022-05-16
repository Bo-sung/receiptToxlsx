using Sfs2X.Util;
using UnityEngine;

namespace Ragnarok
{
    public class DamagePacket : IDamageTuple
    {
        /**
         * 유닛아이디
         * 
         * [1] 캐릭터: Cid
         * [2] 큐펫: 큐펫아이디
         * [3] 일반몬스터: 몬스터아이디 (유닛속성, 유닛사이즈, 유닛스탯)
         * [4] 보스몬스터: 몬스터아이디 (유닛속성, 유닛사이즈, 유닛스택)
         * [5] 동료: 동료아이디
         * [6] 셰어캐릭터: 셰어캐릭터Cid
         * [7] 더미셰어캐릭터: 동료아이디
         * 
         * 유닛레벨
         * 
         * [2] 큐펫 => 큐펫레벨
         * [3] 일반몬스터 => 몬스터레벨
         * [4] 보스몬스터 => 몬스터레벨
         */
        public enum DamageUnitType : byte
        {
            Character = 1,
            Cupet = 2,
            NormalMonster = 3,
            BossMonster = 4,
            Agent = 5,
            SharingCharacter = 6,
            DummySharingCharacter = 7,
        }

        private const byte HIT_NORMAL = 1;
        private const byte HIT_CRITICAL = 2;

        #region 유닛
        public int targetId; // 타겟아이디
        public int targetLevel; // 타겟레벨
        public byte targetType; // 타겟타입

        public int attackerId; // 공격자아이디
        public int attackerLevel; // 공격자레벨
        public byte attackerType; // 공격자타입
        #endregion

        #region 추가스탯
        private int[] targetCrowdControlIds; // 타겟의 상태이상 (물방감소, 마방감소)
        private SkillKey[] targetBuffSkills; // 타겟의 버프스킬 (물방증가, 마방증가)
        private bool isTargetAngry; // 타겟의 분노여부

        private int[] attackerCrowdControlIds; // 공격자의 상태이상 (최종댐감소)
        private SkillKey[] attackerBuffSkills; // 공격자의 버프스킬 (근거리물공증가, 원거리물공증가, 마공증가)
        private bool isAttackerAngry; // 공격자의 분노여부
        #endregion

        #region 스킬 및 결과
        private bool isRefBasicActiveSkill; // 공격자의 참조평타스킬 여부 (배우지 않은 스킬)
        private SkillKey skill; // 공격자의 스킬 => 공격타입, 공격속성, 평타여부, 타격횟수, 전투옵션타입
        private byte hitType; // 피격자의 피격타입 (1: 일반피격, 2: 크리티컬피격)

        private int damageRandomSeq; // 물댐 랜덤 시퀀스
        private int damage; // 물댐
        private int mdamageRandomSeq; // 마댐 랜덤 시퀀스
        private int mdamage; // 마댐
        private int totalDamage; // 최종댐 (추가댐 적용)
        private int finalDamage; // 타겟에게 가해지는 대미지 (타격횟수 적용)
        #endregion

        #region 참고 추가 스탯
        private int def; // 타겟의 물방
        private int mdef; // 타겟의 마방
        private int dmgRateResist; // 타겟의 증댐저항
        private int meleeDmgRateResist; // 타겟의 근거리물증댐저항
        private int rangedDmgRateResist; // 타겟의 원거리물증댐저항
        private int neutralDmgRateResist; // 타겟의 무속성증댐저항
        private int fireDmgRateResist; // 타겟의 화속성증댐저항
        private int waterDmgRateResist; // 타겟의 수속성증댐저항
        private int windDmgRateResist; // 타겟의 풍속성증댐저항
        private int earthDmgRateResist; // 타겟의 지속성증댐저항
        private int poisonDmgRateResist; // 타겟의 독속성증댐저항
        private int holyDmgRateResist; // 타겟의 성속성증댐저항
        private int shadowDmgRateResist; // 타겟의 암속성증댐저항
        private int ghostDmgRateResist; // 타겟의 염속성증댐저항
        private int undeadDmgRateResist; // 타겟의 사속성증댐저항

        private int str; // 공격자의 힘
        private int agi; // 공격자의 민첩_어질
        private int vit; // 공격자의 체력_바이탈
        private int inte; // 공격자의 지력_인트
        private int dex; // 공격자의 재주_덱스
        private int luk; // 공격자의 운_럭
        private int meleeAtk; // 공격자의 근거리물공
        private int rangedAtk; // 공격자의 원거리물공
        private int matk; // 공격자의 마공
        private int criDmgRate; // 공격자의 크리증댐
        private int dmgRate; // 공격자의 증댐
        private int meleeDmgRate; // 공격자의 근거리물증댐
        private int rangedDmgRate; // 공격자의 원거리물증댐
        private int normalMonsterDmgRate; // 공격자의 일반몹증댐
        private int bossMonsterDmgRate; // 공격자의 보스몹증댐
        private int smallMonsterDmgRate; // 공격자의 소형몹증댐
        private int mediumMonsterDmgRate; // 공격자의 중형몹증댐
        private int largeMonsterDmgRate; // 공격자의 대형몹증댐
        private int neutralDmgRate; // 공격자의 무속성증댐
        private int fireDmgRate; // 공격자의 화속성증댐
        private int waterDmgRate; // 공격자의 수속성증댐
        private int windDmgRate; // 공격자의 풍속성증댐
        private int earthDmgRate; // 공격자의 지속성증댐
        private int poisonDmgRate; // 공격자의 독속성증댐
        private int holyDmgRate; // 공격자의 성속성증댐
        private int shadowDmgRate; // 공격자의 암속성증댐
        private int ghostDmgRate; // 공격자의 염속성증댐
        private int undeadDmgRate; // 공격자의 사속성증댐
        private int skillDamageRate; // 공격자의 특정스킬증댐

        private int targetDefDecreaseRate; // 타겟의 상태이상으로 인한 물방감소
        private int targetMdefDecreaseRate; // 타겟의 상태이상으로 인한 마방감소

        public int attackerTotalDmgDecreaseRate; // 시전자의 상태이상으로 인한 최종댐 감소율
        #endregion

        #region 참고 추가 계산과정
        private int criDamageRate; // 치명타 증폭대미지 비율
        private int elementFactor; // 속성간 대미지 배율 (기초데이터)
        private int attackerElementDamageRate; // 시전자의 속성 증폭대미지 비율
        private int targetElementDamageRateResist; // 타겟의 속성 증폭대미지 비율 저항
        private int elementDamageRate; // 속성 증폭대미지 비율
        private int damageRate; //전체 증폭대미지 비율
        private int distDamageRate; // 거리 증폭대미지 비율 = (시전자의 거리 증폭대미지 비율 - 타겟의 거리 증폭대미지 비율 저항)
        private int monsterDamageRate; // 몬스터 증폭대미지 비율
        private int attackerWeaponType; // 장착한 무기 타입
        private int unitSizeFactor; // 유닛사이즈 대미지 배율 (기초 데이터)
        private int unitSizeDamageRate; // 유닛사이즈 증폭대미지 비율

        private int rawDamageValue; // 시전자의 물리 공격력에 스킬 물리 공격 적용
        private int damageRandomValue; // 랜덤 타격계수
        private int damageValue; // 물리 공격력에 랜덤 타격계수 적용
        private int targetDef; // 타겟 방어력
        private int damageDecreaseRate; // 물리대미지감소 비율은 0에서 1사이

        private int attackerMAtk; // 시전자의 마법공격력
        private int rawMDamageValue; // 시전자의 마법 공격력에 스킬 마법 공격 적용
        private int mdamageRandomValue; // 랜덤 타격 계수
        private int mdamageValue; // 마법 공격력에 랜덤 타격계수 적용
        private int targetMDef; // 방어력은 0 이상
        private int mdamageDecreaseRate; // 마법대미지감소 비율은 0에서 1사이

        private int sumDamage; // 대미지총합 = (물리대미지 + 마법대미지)
        private int damageFactor; // 대미지 배율 계산
        private int normalDamage; // 대미지 배율 적용
        private int sumPlusDamageRate; // 증폭대미지 비율 총합
        private int plusDamage; // 증폭대미지 비율 적용
        private int rawTotalDamage; // 시전자의 전체 대미지 비율 감소율 적용
        private int applyedDecreaseRateDamage; // 보정값이 적용된 대미지
        #endregion

        #region 추가 상세 정보
        private int attackerElementType; // 시전자의 속성 타입
        private int attackerElementLevel; // 시전자의 속성 레벨
        private int targetElementType; // 피격자의 속성 타입
        private int targetElementLevel; // 피격자의 속성 레벨
        #endregion

        int IDamageTuple.Def => def;
        int IDamageTuple.Mdef => mdef;
        int IDamageTuple.DmgRateResist => dmgRateResist;
        int IDamageTuple.MeleeDmgRateResist => meleeDmgRateResist;
        int IDamageTuple.RangedDmgRateResist => rangedDmgRateResist;
        int IDamageTuple.NeutralDmgRateResist => neutralDmgRateResist;
        int IDamageTuple.FireDmgRateResist => fireDmgRateResist;
        int IDamageTuple.WaterDmgRateResist => waterDmgRateResist;
        int IDamageTuple.WindDmgRateResist => windDmgRateResist;
        int IDamageTuple.EarthDmgRateResist => earthDmgRateResist;
        int IDamageTuple.PoisonDmgRateResist => poisonDmgRateResist;
        int IDamageTuple.HolyDmgRateResist => holyDmgRateResist;
        int IDamageTuple.ShadowDmgRateResist => shadowDmgRateResist;
        int IDamageTuple.GhostDmgRateResist => ghostDmgRateResist;
        int IDamageTuple.UndeadDmgRateResist => undeadDmgRateResist;
        int IDamageTuple.DefDecreaseRate => targetDefDecreaseRate;
        int IDamageTuple.MdefDecreaseRate => targetMdefDecreaseRate;
        int IDamageTuple.Str => str;
        int IDamageTuple.Agi => agi;
        int IDamageTuple.Vit => vit;
        int IDamageTuple.Inte => inte;
        int IDamageTuple.Dex => dex;
        int IDamageTuple.Luk => luk;
        int IDamageTuple.MeleeAtk => meleeAtk;
        int IDamageTuple.RangedAtk => rangedAtk;
        int IDamageTuple.Matk => matk;
        int IDamageTuple.CriDmgRate => criDmgRate;
        int IDamageTuple.DmgRate => dmgRate;
        int IDamageTuple.MeleeDmgRate => meleeDmgRate;
        int IDamageTuple.RangedDmgRate => rangedDmgRate;
        int IDamageTuple.NormalMonsterDmgRate => normalMonsterDmgRate;
        int IDamageTuple.BossMonsterDmgRate => bossMonsterDmgRate;
        int IDamageTuple.SmallMonsterDmgRate => smallMonsterDmgRate;
        int IDamageTuple.MediumMonsterDmgRate => mediumMonsterDmgRate;
        int IDamageTuple.LargeMonsterDmgRate => largeMonsterDmgRate;
        int IDamageTuple.NeutralDmgRate => neutralDmgRate;
        int IDamageTuple.FireDmgRate => fireDmgRate;
        int IDamageTuple.WaterDmgRate => waterDmgRate;
        int IDamageTuple.WindDmgRate => windDmgRate;
        int IDamageTuple.EarthDmgRate => earthDmgRate;
        int IDamageTuple.PoisonDmgRate => poisonDmgRate;
        int IDamageTuple.HolyDmgRate => holyDmgRate;
        int IDamageTuple.ShadowDmgRate => shadowDmgRate;
        int IDamageTuple.GhostDmgRate => ghostDmgRate;
        int IDamageTuple.UndeadDmgRate => undeadDmgRate;
        int IDamageTuple.SkillDamageRate => skillDamageRate;
        int IDamageTuple.TotalDmgRateDecreaseRate => attackerTotalDmgDecreaseRate;

        int IDamageTuple.CriDamageRate => criDamageRate;
        int IDamageTuple.ElementFactor => elementFactor;
        int IDamageTuple.AttackerElementDamageRate => attackerElementDamageRate;
        int IDamageTuple.TargetElementDamageRateResist => targetElementDamageRateResist;
        int IDamageTuple.ElementDamageRate => elementDamageRate;
        int IDamageTuple.DamageRate => damageRate;
        int IDamageTuple.DistDamageRate => distDamageRate;
        int IDamageTuple.MonsterDamageRate => monsterDamageRate;
        int IDamageTuple.AttackerWeaponType => attackerWeaponType;
        int IDamageTuple.UnitSizeFactor => unitSizeFactor;
        int IDamageTuple.UnitSizeDamageRate => unitSizeDamageRate;
        int IDamageTuple.RawDamageValue => rawDamageValue;
        int IDamageTuple.DamageRandomValue => damageRandomValue;
        int IDamageTuple.DamageValue => damageValue;
        int IDamageTuple.TargetDef => targetDef;
        int IDamageTuple.DamageDecreaseRate => damageDecreaseRate;
        int IDamageTuple.Damage => damage;
        int IDamageTuple.AttackerMAtk => attackerMAtk;
        int IDamageTuple.RawMDamageValue => rawMDamageValue;
        int IDamageTuple.MdamageRandomValue => mdamageRandomValue;
        int IDamageTuple.MdamageValue => mdamageValue;
        int IDamageTuple.TargetMDef => targetMDef;
        int IDamageTuple.MdamageDecreaseRate => mdamageDecreaseRate;
        int IDamageTuple.Mdamage => mdamage;
        int IDamageTuple.SumDamage => sumDamage;
        int IDamageTuple.DamageFactor => damageFactor;
        int IDamageTuple.NormalDamage => normalDamage;
        int IDamageTuple.SumPlusDamageRate => sumPlusDamageRate;
        int IDamageTuple.PlusDamage => plusDamage;
        int IDamageTuple.RawTotalDamage => rawTotalDamage;
        int IDamageTuple.TotalDamage => totalDamage;
        int IDamageTuple.FinalDamage => finalDamage;
        int IDamageTuple.AttackerElementType => attackerElementType;
        int IDamageTuple.AttackerElementLevel => attackerElementLevel;
        int IDamageTuple.TargetElementType => targetElementType;
        int IDamageTuple.TargetElementLevel => targetElementLevel;
        int IDamageTuple.SkillId => skill.skillId;
        int IDamageTuple.SkillLevel => skill.skillLevel;

        public void Set(UnitKey target, UnitKey attacker, Battle.SkillInput settings, SkillKey[] targetBuffSkills, bool isTargetAngry, SkillKey[] attackerBuffSkills, bool isAttackerAngry, Battle.SkillOutput result)
        {
            targetId = target.id;
            targetLevel = target.level;
            targetType = (byte)target.type;
            attackerId = attacker.id;
            attackerLevel = attacker.level;
            attackerType = (byte)attacker.type;

            targetCrowdControlIds = settings.crowdControlTargetValue.GetCrowdControlIds();
            this.targetBuffSkills = targetBuffSkills;
            this.isTargetAngry = isTargetAngry;
            attackerCrowdControlIds = settings.crowdControlAttackerValue.GetCrowdControlIds();
            this.attackerBuffSkills = attackerBuffSkills;
            this.isAttackerAngry = isAttackerAngry;

            isRefBasicActiveSkill = !settings.skillValue.IsInPossession; // 배우지 않은 평타 스킬 여부
            skill = new SkillKey(settings.skillValue.SkillId, settings.skillValue.SkillLevel);

            hitType = result.isCritical ? HIT_CRITICAL : HIT_NORMAL;
            damageRandomSeq = result.damageRandomSeq;
            damage = result.damage;
            mdamageRandomSeq = result.mdamageRandomSeq;
            mdamage = result.mdamage;
            totalDamage = result.totalDamage;
            finalDamage = totalDamage * settings.skillValue.BlowCount;

            def = settings.statusTargetValue.Def;
            mdef = settings.statusTargetValue.MDef;
            dmgRateResist = settings.statusTargetValue.DmgRateResist;
            meleeDmgRateResist = settings.statusTargetValue.MeleeDmgRateResist;
            rangedDmgRateResist = settings.statusTargetValue.RangedDmgRateResist;
            neutralDmgRateResist = settings.statusTargetValue.NeutralDmgRateResist;
            fireDmgRateResist = settings.statusTargetValue.FireDmgRateResist;
            waterDmgRateResist = settings.statusTargetValue.WaterDmgRateResist;
            windDmgRateResist = settings.statusTargetValue.WindDmgRateResist;
            earthDmgRateResist = settings.statusTargetValue.EarthDmgRateResist;
            poisonDmgRateResist = settings.statusTargetValue.PoisonDmgRateResist;
            holyDmgRateResist = settings.statusTargetValue.HolyDmgRateResist;
            shadowDmgRateResist = settings.statusTargetValue.ShadowDmgRateResist;
            ghostDmgRateResist = settings.statusTargetValue.GhostDmgRateResist;
            undeadDmgRateResist = settings.statusTargetValue.UndeadDmgRateResist;

            str = settings.statusAttackerValue.Str;
            agi = settings.statusAttackerValue.Agi;
            vit = settings.statusAttackerValue.Vit;
            inte = settings.statusAttackerValue.Int;
            dex = settings.statusAttackerValue.Dex;
            luk = settings.statusAttackerValue.Luk;
            meleeAtk = settings.statusAttackerValue.MeleeAtk;
            rangedAtk = settings.statusAttackerValue.RangedAtk;
            matk = settings.statusAttackerValue.MAtk;
            criDmgRate = settings.statusAttackerValue.CriDmgRate;
            dmgRate = settings.statusAttackerValue.DmgRate;
            meleeDmgRate = settings.statusAttackerValue.MeleeDmgRate;
            rangedDmgRate = settings.statusAttackerValue.RangedDmgRate;
            normalMonsterDmgRate = settings.statusAttackerValue.NormalMonsterDmgRate;
            bossMonsterDmgRate = settings.statusAttackerValue.BossMonsterDmgRate;
            smallMonsterDmgRate = settings.statusAttackerValue.SmallMonsterDmgRate;
            mediumMonsterDmgRate = settings.statusAttackerValue.MediumMonsterDmgRate;
            largeMonsterDmgRate = settings.statusAttackerValue.LargeMonsterDmgRate;
            neutralDmgRate = settings.statusAttackerValue.NeutralDmgRate;
            fireDmgRate = settings.statusAttackerValue.FireDmgRate;
            waterDmgRate = settings.statusAttackerValue.WaterDmgRate;
            windDmgRate = settings.statusAttackerValue.WindDmgRate;
            earthDmgRate = settings.statusAttackerValue.EarthDmgRate;
            poisonDmgRate = settings.statusAttackerValue.PoisonDmgRate;
            holyDmgRate = settings.statusAttackerValue.HolyDmgRate;
            shadowDmgRate = settings.statusAttackerValue.ShadowDmgRate;
            ghostDmgRate = settings.statusAttackerValue.GhostDmgRate;
            undeadDmgRate = settings.statusAttackerValue.UndeadDmgRate;
            skillDamageRate = settings.statusAttackerValue.GetSkillDamageRate(settings.skillValue.SkillId);

            targetDefDecreaseRate = settings.crowdControlTargetValue.DefDecreaseRate;
            targetMdefDecreaseRate = settings.crowdControlTargetValue.MdefDecreaseRate;

            attackerTotalDmgDecreaseRate = settings.crowdControlAttackerValue.TotalDmgDecreaseRate;

            criDamageRate = result.criDamageRate;
            elementFactor = result.elementFactor;
            attackerElementDamageRate = result.attackerElementDamageRate;
            targetElementDamageRateResist = result.targetElementDamageRateResist;
            elementDamageRate = result.elementDamageRate;
            damageRate = result.damageRate;
            distDamageRate = result.distDamageRate;
            monsterDamageRate = result.monsterDamageRate;
            attackerWeaponType = result.attackerWeaponType;
            unitSizeFactor = result.unitSizeFactor;
            unitSizeDamageRate = result.unitSizeDamageRate;

            rawDamageValue = result.rawDamageValue;
            damageRandomValue = result.damageRandomValue;
            damageValue = result.damageValue;
            targetDef = result.targetDef;
            damageDecreaseRate = (int)(result.damageDecreaseRate * 10000.0f);

            attackerMAtk = result.attackerMAtk;
            rawMDamageValue = result.rawMDamageValue;
            mdamageRandomValue = result.mdamageRandomValue;
            mdamageValue = result.mdamageValue;
            targetMDef = result.targetMDef;
            mdamageDecreaseRate = (int)(result.mdamageDecreaseRate * 10000.0f);

            sumDamage = result.sumDamage;
            damageFactor = result.damageFactor;
            normalDamage = result.normalDamage;
            sumPlusDamageRate = result.sumPlusDamageRate;
            plusDamage = result.plusDamage;
            rawTotalDamage = result.rawTotalDamage;
            applyedDecreaseRateDamage = result.applyedDecreaseRateDamage;

            attackerElementType = (int)settings.skillElementType;
            attackerElementLevel = settings.skillElementLevel;
            targetElementType = (int)settings.unitTargetValue.UnitElementType;
            targetElementLevel = settings.unitTargetValue.UnitElementLevel;
        }

        public ByteArray ToByteArray()
        {
            ByteArray output = new ByteArray();

            output.WriteInt(targetId); // 타겟아이디
            output.WriteInt(targetLevel); // 타겟레벨
            output.WriteByte(targetType); // 타겟타입

            output.WriteInt(attackerId); // 공격자아이디
            output.WriteInt(attackerLevel); // 공격자레벨
            output.WriteByte(attackerType); // 공격자타입

            int targetCrowdControlIdCount = targetCrowdControlIds == null ? 0 : targetCrowdControlIds.Length;
            output.WriteInt(targetCrowdControlIdCount); // 타겟의 상태이상 배열 수
            for (int i = 0; i < targetCrowdControlIdCount; i++)
            {
                output.WriteInt(targetCrowdControlIds[i]); // 타겟의 상태이상 아이디
            }

            int targetBuffSkillIdCount = targetBuffSkills == null ? 0 : targetBuffSkills.Length;
            output.WriteInt(targetBuffSkillIdCount); // 타겟의 버프스킬 배열 수
            for (int i = 0; i < targetBuffSkillIdCount; i++)
            {
                output.WriteInt(targetBuffSkills[i].skillId); // 타겟의 버프스킬 아이디
                output.WriteInt(targetBuffSkills[i].skillLevel); // 타겟의 버프스킬 레벨
            }
            output.WriteBool(isTargetAngry); // 타겟의 분노 여부

            int attackerCrowdControlIdCount = attackerCrowdControlIds == null ? 0 : attackerCrowdControlIds.Length;
            output.WriteInt(attackerCrowdControlIdCount); // 공격자의 상태이상 배열 수
            for (int i = 0; i < attackerCrowdControlIdCount; i++)
            {
                output.WriteInt(attackerCrowdControlIds[i]); // 공격자의 상태이상 아이디
            }

            int attackerBuffSkillIdCount = attackerBuffSkills == null ? 0 : attackerBuffSkills.Length;
            output.WriteInt(attackerBuffSkillIdCount); // 공격자의 버프스킬 배열 수
            for (int i = 0; i < attackerBuffSkillIdCount; i++)
            {
                output.WriteInt(attackerBuffSkills[i].skillId); // 공격자의 버프스킬 아이디
                output.WriteInt(attackerBuffSkills[i].skillLevel); // 공격자의 버프스킬 레벨
            }
            //output.WriteBool(isAttackerAngry); // 공격자의 분노 여부

            output.WriteBool(isRefBasicActiveSkill); // 공격자의 참조평타스킬 여부
            output.WriteInt(skill.skillId); // 공격자의 스킬아이디
            output.WriteInt(skill.skillLevel); // 공격자의 스킬레벨

            output.WriteByte(hitType); // 피격타입
            output.WriteInt(damageRandomSeq); // 물댐 랜덤 시퀀스
            output.WriteInt(damage); // 물댐
            output.WriteInt(mdamageRandomSeq); // 마댐 랜덤 시퀀스
            output.WriteInt(mdamage); // 마댐
            output.WriteInt(totalDamage); // 최종댐 (추가댐 적용)
            output.WriteInt(finalDamage); // 타겟에게 가해지는 대미지 (타격횟수 적용)

            /******************** 타겟 (스탯) ********************/
            output.WriteInt(def); // 물방
            output.WriteInt(mdef); // 마방
            output.WriteInt(dmgRateResist); // 증댐저항
            output.WriteInt(meleeDmgRateResist); // 근거리물증댐저항
            output.WriteInt(rangedDmgRateResist); // 원거리물증댐저항
            output.WriteInt(neutralDmgRateResist); // 무속성증댐저항
            output.WriteInt(fireDmgRateResist); // 화속성증댐저항
            output.WriteInt(waterDmgRateResist); // 수속성증댐저항
            output.WriteInt(windDmgRateResist); // 풍속성증댐저항
            output.WriteInt(earthDmgRateResist); // 지속성증댐저항
            output.WriteInt(poisonDmgRateResist); // 독속성증댐저항
            output.WriteInt(holyDmgRateResist); // 성속성증댐저항
            output.WriteInt(shadowDmgRateResist); // 암속성증댐저항
            output.WriteInt(ghostDmgRateResist); // 염속성증댐저항
            output.WriteInt(undeadDmgRateResist); // 사속성증댐저항

            /******************** 시전자 (스탯) ********************/
            output.WriteInt(str); // 힘
            output.WriteInt(agi); // 민첩_어질
            output.WriteInt(vit); // 체력_바이탈
            output.WriteInt(inte); // 지력_인트
            output.WriteInt(dex); // 재주_덱스
            output.WriteInt(luk); // 운_럭
            output.WriteInt(meleeAtk); // 근거리물공
            output.WriteInt(rangedAtk); // 원거리물공
            output.WriteInt(matk); // 마공
            output.WriteInt(criDmgRate); // 크리증댐
            output.WriteInt(dmgRate); // 증댐
            output.WriteInt(meleeDmgRate); // 근거리물증댐
            output.WriteInt(rangedDmgRate); // 원거리물증댐
            output.WriteInt(normalMonsterDmgRate); // 일반몹증댐
            output.WriteInt(bossMonsterDmgRate); // 보스몹증댐
            output.WriteInt(smallMonsterDmgRate); // 소형몹증댐
            output.WriteInt(mediumMonsterDmgRate); // 중형몹증댐
            output.WriteInt(largeMonsterDmgRate); // 대형몹증댐
            output.WriteInt(neutralDmgRate); // 무속성증댐
            output.WriteInt(fireDmgRate); // 화속성증댐
            output.WriteInt(waterDmgRate); // 수속성증댐
            output.WriteInt(windDmgRate); // 풍속성증댐
            output.WriteInt(earthDmgRate); // 지속성증댐
            output.WriteInt(poisonDmgRate); // 독속성증댐
            output.WriteInt(holyDmgRate); // 성속성증댐
            output.WriteInt(shadowDmgRate); // 암속성증댐
            output.WriteInt(ghostDmgRate); // 염속성증댐
            output.WriteInt(undeadDmgRate); // 사속성증댐
            output.WriteInt(skillDamageRate); // 특정스킬증댐

            return output;
        }

        public ByteArray ToByteArrayForCentralLab()
        {
            ByteArray output = new ByteArray();

            int targetCrowdControlIdCount = targetCrowdControlIds == null ? 0 : targetCrowdControlIds.Length;
            output.WriteInt(targetCrowdControlIdCount); // 타겟의 상태이상 배열 수
            for (int i = 0; i < targetCrowdControlIdCount; i++)
            {
                output.WriteInt(targetCrowdControlIds[i]); // 타겟의 상태이상 아이디
            }

            int targetBuffSkillIdCount = targetBuffSkills == null ? 0 : targetBuffSkills.Length;
            output.WriteInt(targetBuffSkillIdCount); // 타겟의 버프스킬 배열 수
            for (int i = 0; i < targetBuffSkillIdCount; i++)
            {
                output.WriteInt(targetBuffSkills[i].skillId); // 타겟의 버프스킬 아이디
                output.WriteInt(targetBuffSkills[i].skillLevel); // 타겟의 버프스킬 레벨
            }
            output.WriteBool(isTargetAngry); // 타겟의 분노 여부

            int attackerCrowdControlIdCount = attackerCrowdControlIds == null ? 0 : attackerCrowdControlIds.Length;
            output.WriteInt(attackerCrowdControlIdCount); // 공격자의 상태이상 배열 수
            for (int i = 0; i < attackerCrowdControlIdCount; i++)
            {
                output.WriteInt(attackerCrowdControlIds[i]); // 공격자의 상태이상 아이디
            }

            int attackerBuffSkillIdCount = attackerBuffSkills == null ? 0 : attackerBuffSkills.Length;
            output.WriteInt(attackerBuffSkillIdCount); // 공격자의 버프스킬 배열 수
            for (int i = 0; i < attackerBuffSkillIdCount; i++)
            {
                output.WriteInt(attackerBuffSkills[i].skillId); // 공격자의 버프스킬 아이디
                output.WriteInt(attackerBuffSkills[i].skillLevel); // 공격자의 버프스킬 레벨
            }
            //output.WriteBool(isAttackerAngry); // 공격자의 분노 여부

            output.WriteBool(isRefBasicActiveSkill); // 공격자의 참조평타스킬 여부
            output.WriteInt(skill.skillId); // 공격자의 스킬아이디
            output.WriteInt(skill.skillLevel); // 공격자의 스킬레벨

            output.WriteByte(hitType); // 피격타입
            output.WriteInt(damageRandomSeq); // 물댐 랜덤 시퀀스
            output.WriteInt(damage); // 물댐
            output.WriteInt(mdamageRandomSeq); // 마댐 랜덤 시퀀스
            output.WriteInt(mdamage); // 마댐
            output.WriteInt(totalDamage); // 최종댐 (추가댐 적용)
            output.WriteInt(finalDamage); // 타겟에게 가해지는 대미지 (타격횟수 적용)

            return output;
        }

        public void ShowLog()
        {
            Debug.LogError("대미지: " + attackerType.ToEnum<DamageUnitType>() + " => " + targetType.ToEnum<DamageUnitType>());
        }

        public struct UnitKey
        {
            public readonly DamageUnitType type;
            public readonly int id;
            public readonly int level;

            public UnitKey(DamageUnitType type, int id, int level)
            {
                this.type = type;
                this.id = id;
                this.level = level;
            }
        }

        public struct SkillKey
        {
            public readonly int skillId;
            public readonly int skillLevel;

            public SkillKey(int skillId, int skillLevel)
            {
                this.skillId = skillId;
                this.skillLevel = skillLevel;
            }
        }
    }
}