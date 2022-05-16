using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public class PanelBuffInfo : DataInfo<PanelBuffData>
    {
        /// <summary>
        /// 장착한 큐펫 id 값
        /// </summary>
        private ObscuredInt equippedCupetID;

        /// <summary>
        /// 장착한 큐펫 id 값
        /// </summary>
        public int EquippedCupetID => equippedCupetID;

        /// <summary>
        /// 전투 효과
        /// </summary>
        public BattleOption GetBattleOption()
        {
            return new BattleOption(data.battle_option_type, data.value1, data.value2);
        }

        public void Initialize(int equippedCupetID)
        {
            this.equippedCupetID = equippedCupetID;
        }

        public void Reset()
        {
            SetData(null);
            equippedCupetID = 0;
        }
    }
}