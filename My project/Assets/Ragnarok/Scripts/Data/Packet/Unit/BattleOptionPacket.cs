namespace Ragnarok
{
    public class BattleOptionPacket : IPacket<Response>, IBattleOption
    {
        public int battle_option_type;
        public int value_1;
        public int value_2;

        BattleOptionType IBattleOption.BattleOptionType => battle_option_type.ToEnum<BattleOptionType>();
        int IBattleOption.Value1 => value_1;
        int IBattleOption.Value2 => value_2;

        void IInitializable<Response>.Initialize(Response response)
        {
            battle_option_type = response.GetInt("1");
            value_1 = response.GetInt("2");
            value_2 = response.GetInt("3");
        }
    }
}