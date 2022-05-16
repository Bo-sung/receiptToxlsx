namespace Ragnarok
{
    public class ItemStatusValuePacket : IPacket<Response>, BattleItemInfo.IValue
    {
        public int atk;
        public int matk;
        public int def;
        public int mdef;

        int BattleItemInfo.IValue.TotalItemAtk => atk;
        int BattleItemInfo.IValue.TotalItemMatk => matk;
        int BattleItemInfo.IValue.TotalItemDef => def;
        int BattleItemInfo.IValue.TotalItemMdef => mdef;

        void IInitializable<Response>.Initialize(Response response)
        {
            atk = response.GetInt("1");
            matk = response.GetInt("2");
            def = response.GetInt("3");
            mdef = response.GetInt("4");
        }
    }
}