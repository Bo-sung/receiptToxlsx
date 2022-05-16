using System.Collections.Generic;

namespace Ragnarok
{
    public class UICupetBattleOptionList : UIBattleOptionList
    {
        public void SetData(CupetEntity entity)
        {
            // TODO BattleOption을 보여주는 것이 아니라 Status 를 보여주는 UI를 따로 만들어야..
            // TODO UIBattleOptionList 대신 다른 것을 사용해야..
            SetData(GetOptionCollection(entity));
        }

        private IEnumerable<BattleOption> GetOptionCollection(CupetEntity entity)
        {
            yield return new BattleOption(BattleOptionType.MaxHp, entity.battleStatusInfo.MaxHp, value2: 0);
            yield return new BattleOption(BattleOptionType.Atk, entity.battleStatusInfo.MeleeAtk, value2: 0);
            yield return new BattleOption(BattleOptionType.MAtk, entity.battleStatusInfo.MAtk, value2: 0);
            yield return new BattleOption(BattleOptionType.Def, entity.battleStatusInfo.Def, value2: 0);
            yield return new BattleOption(BattleOptionType.MDef, entity.battleStatusInfo.MDef, value2: 0);
            yield return new BattleOption(BattleOptionType.MoveSpd, entity.battleStatusInfo.MoveSpd, value2: 0);
            yield return new BattleOption(BattleOptionType.AtkSpd, entity.battleStatusInfo.AtkSpd, value2: 0);
            yield return new BattleOption(BattleOptionType.AtkRange, entity.battleStatusInfo.AtkRange, value2: 0);
        }
    }
}