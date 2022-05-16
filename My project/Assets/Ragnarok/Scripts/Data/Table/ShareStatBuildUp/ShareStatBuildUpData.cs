using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ShareStatBuildUpDataManager"/>
    /// </summary>
    public sealed class ShareStatBuildUpData : IData
    {
        public readonly ObscuredInt id;
        public readonly int group;
        public readonly ObscuredInt need_shareforce;
        public readonly int stat_lv;
        public readonly ObscuredInt battle_option_type;
        public readonly ObscuredInt value1;
        public readonly ObscuredInt value2;

        public ShareStatBuildUpData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            group = data[index++].AsInt32();
            need_shareforce = data[index++].AsInt32();
            stat_lv = data[index++].AsInt32();
            battle_option_type = data[index++].AsInt32();
            value1 = data[index++].AsInt32();
            value2 = data[index++].AsInt32();
        }

        public BattleOption GetBattleOption()
        {
            return new BattleOption(battle_option_type, value1, value2);
        }

        public int GetNeedPoint()
        {
            return need_shareforce;
        }
    }
}