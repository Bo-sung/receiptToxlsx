#if UNITY_EDITOR
using Sfs2X.Util;

namespace Ragnarok
{
    public class DamageCheckPacket : IDamageTuple
    {
        /******************** 타겟 (스탯) ********************/
        private int def; // 물방
        private int mdef; // 마방
        private int dmgRateResist; // 증댐저항
        private int meleeDmgRateResist; // 근거리물증댐저항
        private int rangedDmgRateResist; // 원거리물증댐저항
        private int neutralDmgRateResist; // 무속성증댐저항
        private int fireDmgRateResist; // 화속성증댐저항
        private int waterDmgRateResist; // 수속성증댐저항
        private int windDmgRateResist; // 풍속성증댐저항
        private int earthDmgRateResist; // 지속성증댐저항
        private int poisonDmgRateResist; // 독속성증댐저항
        private int holyDmgRateResist; // 성속성증댐저항
        private int shadowDmgRateResist; // 암속성증댐저항
        private int ghostDmgRateResist; // 염속성증댐저항
        private int undeadDmgRateResist; // 사속성증댐저항

        /******************** 타겟 (상태이상) ********************/
        private int defDecreaseRate; // 타겟 (상태이상)
        private int mdefDecreaseRate; // 타겟 (상태이상)

        /******************** 타겟 (버프스킬) ********************/

        /******************** 시전자 (스탯) ********************/
        private int str; // 힘
        private int agi; // 민첩_어질
        private int vit; // 체력_바이탈
        private int inte; // 지력_인트
        private int dex; // 재주_덱스
        private int luk; // 운_럭
        private int meleeAtk; // 근거리물공
        private int rangedAtk; // 원거리물공
        private int matk; // 마공
        private int criDmgRate; // 크리증댐
        private int dmgRate; // 증댐
        private int meleeDmgRate; // 근거리물증댐
        private int rangedDmgRate; // 원거리물증댐
        private int normalMonsterDmgRate; // 일반몹증댐
        private int bossMonsterDmgRate; // 보스몹증댐
        private int smallMonsterDmgRate; // 소형몹증댐
        private int mediumMonsterDmgRate; // 중형몹증댐
        private int largeMonsterDmgRate; // 대형몹증댐
        private int neutralDmgRate; // 무속성증댐
        private int fireDmgRate; // 화속성증댐
        private int waterDmgRate; // 수속성증댐
        private int windDmgRate; // 풍속성증댐
        private int earthDmgRate; // 지속성증댐
        private int poisonDmgRate; // 독속성증댐
        private int holyDmgRate; // 성속성증댐
        private int shadowDmgRate; // 암속성증댐
        private int ghostDmgRate; // 염속성증댐
        private int undeadDmgRate; // 사속성증댐
        private int skillDamageRate; // 특정스킬증댐

        /******************** 시전자 (상태이상) ********************/
        private int totalDmgRateDecreaseRate; // 시전자 (상태이상 - 전체 대미지 감소율) 

        /******************** 시전자 (버프스킬) ********************/

        /******************** 계산 과정 (대미지 상세) ********************/
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
        private int damage; // 물댐

        private int attackerMAtk; // 시전자의 마법공격력
        private int rawMDamageValue; // 시전자의 마법 공격력에 스킬 마법 공격 적용
        private int mdamageRandomValue; // 랜덤 타격 계수
        private int mdamageValue; // 마법 공격력에 랜덤 타격계수 적용
        private int targetMDef; // 방어력은 0 이상
        private int mdamageDecreaseRate; // 마법대미지감소 비율은 0에서 1사이
        private int mdamage; // 마댐

        private int sumDamage; // 대미지총합 = (물리대미지 + 마법대미지)
        private int damageFactor; // 대미지 배율 계산
        private int normalDamage; // 대미지 배율 적용
        private int sumPlusDamageRate; // 증폭대미지 비율 총합
        private int plusDamage; // 증폭대미지 비율 적용
        private int rawTotalDamage; // 시전자의 전체 대미지 비율 감소율 적용
        private int totalDamage; // 최종댐 (추가댐 적용)
        private int finalDamage; // 타겟에게 가해지는 대미지 (타격횟수 적용)

        /******************** 추가 상세 정보 ********************/
        private int attackerElementType; // 시전자의 속성 타입
        private int attackerElementLevel; // 시전자의 속성 레벨
        private int targetElementType; // 피격자의 속성 타입
        private int targetElementLevel; // 피격자의 속성 레벨
        private int skillId; // 시전자의 스킬 id
        private int skillLevel; // 시전자의 스킬 level

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
        int IDamageTuple.DefDecreaseRate => defDecreaseRate;
        int IDamageTuple.MdefDecreaseRate => mdefDecreaseRate;
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
        int IDamageTuple.TotalDmgRateDecreaseRate => totalDmgRateDecreaseRate;

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
        int IDamageTuple.SkillId => skillId;
        int IDamageTuple.SkillLevel => skillLevel;

        public DamageCheckPacket(ByteArray byteArray)
        {
            // byteArray 를 single 로 돌려쓰고 있나봄 (처음에 강제로 Position 초기화)
            byteArray.Position = 0;

            /******************** 타겟 (스탯) ********************/
            def = byteArray.ReadInt(); // 물방
            mdef = byteArray.ReadInt(); // 마방
            dmgRateResist = byteArray.ReadInt(); // 증댐저항
            meleeDmgRateResist = byteArray.ReadInt(); // 근거리물증댐저항
            rangedDmgRateResist = byteArray.ReadInt(); // 원거리물증댐저항
            neutralDmgRateResist = byteArray.ReadInt(); // 무속성증댐저항
            fireDmgRateResist = byteArray.ReadInt(); // 화속성증댐저항
            waterDmgRateResist = byteArray.ReadInt(); // 수속성증댐저항
            windDmgRateResist = byteArray.ReadInt(); // 풍속성증댐저항
            earthDmgRateResist = byteArray.ReadInt(); // 지속성증댐저항
            poisonDmgRateResist = byteArray.ReadInt(); // 독속성증댐저항
            holyDmgRateResist = byteArray.ReadInt(); // 성속성증댐저항
            shadowDmgRateResist = byteArray.ReadInt(); // 암속성증댐저항
            ghostDmgRateResist = byteArray.ReadInt(); // 염속성증댐저항
            undeadDmgRateResist = byteArray.ReadInt(); // 사속성증댐저항

            /******************** 타겟 (상태이상) ********************/
            defDecreaseRate = byteArray.ReadInt(); // 타겟 (상태이상)
            mdefDecreaseRate = byteArray.ReadInt(); // 타겟 (상태이상)

            /******************** 타겟 (버프스킬) ********************/

            /******************** 시전자 (스탯) ********************/
            str = byteArray.ReadInt(); // 힘
            agi = byteArray.ReadInt(); // 민첩_어질
            vit = byteArray.ReadInt(); // 체력_바이탈
            inte = byteArray.ReadInt(); // 지력_인트
            dex = byteArray.ReadInt(); // 재주_덱스
            luk = byteArray.ReadInt(); // 운_럭
            meleeAtk = byteArray.ReadInt(); // 근거리물공
            rangedAtk = byteArray.ReadInt(); // 원거리물공
            matk = byteArray.ReadInt(); // 마공
            criDmgRate = byteArray.ReadInt(); // 크리증댐
            dmgRate = byteArray.ReadInt(); // 증댐
            meleeDmgRate = byteArray.ReadInt(); // 근거리물증댐
            rangedDmgRate = byteArray.ReadInt(); // 원거리물증댐
            normalMonsterDmgRate = byteArray.ReadInt(); // 일반몹증댐
            bossMonsterDmgRate = byteArray.ReadInt(); // 보스몹증댐
            smallMonsterDmgRate = byteArray.ReadInt(); // 소형몹증댐
            mediumMonsterDmgRate = byteArray.ReadInt(); // 중형몹증댐
            largeMonsterDmgRate = byteArray.ReadInt(); // 대형몹증댐
            neutralDmgRate = byteArray.ReadInt(); // 무속성증댐
            fireDmgRate = byteArray.ReadInt(); // 화속성증댐
            waterDmgRate = byteArray.ReadInt(); // 수속성증댐
            windDmgRate = byteArray.ReadInt(); // 풍속성증댐
            earthDmgRate = byteArray.ReadInt(); // 지속성증댐
            poisonDmgRate = byteArray.ReadInt(); // 독속성증댐
            holyDmgRate = byteArray.ReadInt(); // 성속성증댐
            shadowDmgRate = byteArray.ReadInt(); // 암속성증댐
            ghostDmgRate = byteArray.ReadInt(); // 염속성증댐
            undeadDmgRate = byteArray.ReadInt(); // 사속성증댐
            skillDamageRate = byteArray.ReadInt(); // 특정스킬증댐

            /******************** 시전자 (상태이상) ********************/
            totalDmgRateDecreaseRate = byteArray.ReadInt(); // 시전자 (상태이상 - 전체 대미지 감소율) 

            /******************** 시전자 (버프스킬) ********************/

            /******************** 결과 (대미지) ********************/
            damage = byteArray.ReadInt(); // 물댐
            mdamage = byteArray.ReadInt(); // 마댐
            totalDamage = byteArray.ReadInt(); // 최종댐 (추가댐 적용)
            finalDamage = byteArray.ReadInt(); // 타겟에게 가해지는 대미지 (타격횟수 적용)

            criDamageRate = byteArray.ReadInt(); // 치명타 증폭대미지 비율
            elementFactor = byteArray.ReadInt(); // 속성간 대미지 배율 (기초데이터)
            elementDamageRate = byteArray.ReadInt(); // 속성 증폭대미지 비율
            attackerElementDamageRate = byteArray.ReadInt(); // 시전자의 속성 증폭대미지 비율
            targetElementDamageRateResist = byteArray.ReadInt(); // 타겟의 속성 증폭대미지 비율 저항
            damageRate = byteArray.ReadInt(); //전체 증폭대미지 비율
            distDamageRate = byteArray.ReadInt(); // 거리 증폭대미지 비율 = (시전자의 거리 증폭대미지 비율 - 타겟의 거리 증폭대미지 비율 저항)
            monsterDamageRate = byteArray.ReadInt(); // 몬스터 증폭대미지 비율
            unitSizeFactor = byteArray.ReadInt(); // 유닛사이즈 대미지 배율 (기초 데이터)
            unitSizeDamageRate = byteArray.ReadInt(); // 유닛사이즈 증폭대미지 비율
            attackerWeaponType = byteArray.ReadInt(); // 장착한 무기 타입
            rawDamageValue = byteArray.ReadInt(); // 시전자의 물리 공격력에 스킬 물리 공격 적용
            damageRandomValue = byteArray.ReadInt(); // 랜덤 타격계수
            damageValue = byteArray.ReadInt(); // 물리 공격력에 랜덤 타격계수 적용
            targetDef = byteArray.ReadInt(); // 타겟 방어력
            damageDecreaseRate = byteArray.ReadInt(); // 물리대미지감소 비율은 0에서 1사이
            attackerMAtk = byteArray.ReadInt(); // 시전자의 마법공격력
            rawMDamageValue = byteArray.ReadInt(); // 시전자의 마법 공격력에 스킬 마법 공격 적용
            mdamageRandomValue = byteArray.ReadInt(); // 랜덤 타격 계수
            mdamageValue = byteArray.ReadInt(); // 마법 공격력에 랜덤 타격계수 적용
            targetMDef = byteArray.ReadInt(); // 방어력은 0 이상
            mdamageDecreaseRate = byteArray.ReadInt(); // 마법대미지감소 비율은 0에서 1사이
            sumDamage = byteArray.ReadInt(); // 대미지총합 = (물리대미지 + 마법대미지)
            damageFactor = byteArray.ReadInt(); // 대미지 배율 계산
            normalDamage = byteArray.ReadInt(); // 대미지 배율 적용
            sumPlusDamageRate = byteArray.ReadInt(); // 증폭대미지 비율 총합
            plusDamage = byteArray.ReadInt(); // 증폭대미지 비율 적용
            rawTotalDamage = byteArray.ReadInt(); // 시전자의 전체 대미지 비율 감소율 적용

            /******************** 추가 상세 정보 ********************/
            attackerElementType = byteArray.ReadInt(); // 시전자의 속성 타입
            attackerElementLevel = byteArray.ReadInt(); // 시전자의 속성 레벨
            targetElementType = byteArray.ReadInt(); // 피격자의 속성 타입
            targetElementLevel = byteArray.ReadInt(); // 피격자의 속성 레벨
            skillId = byteArray.ReadInt(); // 스킬 id
            skillLevel = byteArray.ReadInt(); // 스킬 Level
        }
    }
}
#endif