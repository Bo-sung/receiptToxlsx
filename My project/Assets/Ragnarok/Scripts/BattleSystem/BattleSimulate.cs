using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleSimulate
    {
        public delegate void ChangeHpEvent(UnitEntity unit, int cur, int max);
        public delegate void DieEvent(UnitEntity unit, UnitEntity attacker);

        private readonly Buffer<UnitEntity> unitList;
        private readonly IRandomDamage randomDamage; // 랜덤 지수

        public event ChangeHpEvent OnChangeHp;
        public event DieEvent OnDie;

        public BattleSimulate()
        {
            unitList = new Buffer<UnitEntity>();
            randomDamage = RandomTableDataManager.Instance;
        }

        /// <summary>
        /// 유닛 초기화
        /// </summary>
        public void Clear()
        {
            unitList.Clear();
        }

        /// <summary>
        /// 시작
        /// </summary>
        public void Run()
        {
        }

        /// <summary>
        /// 아군 등록
        /// </summary>
        public void AddAlly(UnitEntity ally)
        {
            ally.SetEnemy(isEnemy: false);
            unitList.Add(ally);
        }

        /// <summary>
        /// 적군 등록
        /// </summary>
        public void AddEnemy(UnitEntity enemy)
        {
            enemy.SetEnemy(isEnemy: true);
            unitList.Add(enemy);
        }

        /// <summary>
        /// 아군 등록
        /// </summary>
        public void AddAllies(IEnumerable<UnitEntity> allies)
        {
            foreach (var item in allies)
            {
                AddAlly(item);
            }
        }

        /// <summary>
        /// 적군 등록
        /// </summary>
        public void AddEnemies(IEnumerable<UnitEntity> enemies)
        {
            foreach (var item in enemies)
            {
                AddEnemy(item);
            }
        }

        private void Simulate()
        {
            /**
            UnitEntity unit = null;
            UnitEntity attacker = null;
            SkillInfo skillInfo = null;

            Battle.SkillInput settings = new Battle.SkillInput
            {
                unitTargetValue = unit.battleUnitInfo,
                statusTargetValue = unit.battleStatusInfo,
                crowdControlTargetValue = unit.battleCrowdControlInfo,

                itemAttackerValue = attacker.battleItemInfo,
                statusAttackerValue = attacker.battleStatusInfo,
                crowdControlAttackerValue = attacker.battleCrowdControlInfo,

                skillValue = skillInfo,
                // 스킬이 평타이면서 속성이 존재하지 않을 경우 => 장착한 무기의 속성으로 대체
                skillElementType = (skillInfo.IsBasicActiveSkill && skillInfo.ElementType == default) ? attacker.battleItemInfo.WeaponElementType : skillInfo.ElementType,
                randomDamage = randomDamage,
            };

            Battle.SkillOutput result = Battle.ApplyActiveSkill(settings, unit);
            **/
        }
    }
}