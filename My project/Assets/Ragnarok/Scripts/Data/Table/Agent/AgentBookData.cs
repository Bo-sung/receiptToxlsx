using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="AgentBookDataManager"/>
    /// </summary>
    public sealed class AgentBookData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt agent_id_1;
        public readonly ObscuredInt agent_id_2;
        public readonly ObscuredInt agent_id_3;
        public readonly ObscuredInt agent_id_4;
        public readonly ObscuredInt battle_option_type_1;
        public readonly ObscuredInt value1_b1;
        public readonly ObscuredInt value2_b1;
        public readonly ObscuredInt battle_option_type_2;
        public readonly ObscuredInt value1_b2;
        public readonly ObscuredInt value2_b2;
        public readonly ObscuredInt battle_option_type_3;
        public readonly ObscuredInt value1_b3;
        public readonly ObscuredInt value2_b3;
        public readonly ObscuredInt battle_option_type_4;
        public readonly ObscuredInt value1_b4;
        public readonly ObscuredInt value2_b4;
        public readonly ObscuredInt reward_type;
        public readonly ObscuredInt reward_value;
        public readonly ObscuredInt reward_count;

        public AgentBookData(IList<MessagePackObject> data)
        {
            int index = 0;
            id                   = data[index++].AsInt32();
            name_id              = data[index++].AsInt32();
            agent_id_1           = data[index++].AsInt32();
            agent_id_2           = data[index++].AsInt32();
            agent_id_3           = data[index++].AsInt32();
            agent_id_4           = data[index++].AsInt32();
            battle_option_type_1 = data[index++].AsInt32();
            value1_b1            = data[index++].AsInt32();
            value2_b1            = data[index++].AsInt32();
            battle_option_type_2 = data[index++].AsInt32();
            value1_b2            = data[index++].AsInt32();
            value2_b2            = data[index++].AsInt32();
            battle_option_type_3 = data[index++].AsInt32();
            value1_b3            = data[index++].AsInt32();
            value2_b3            = data[index++].AsInt32();
            battle_option_type_4 = data[index++].AsInt32();
            value1_b4            = data[index++].AsInt32();
            value2_b4            = data[index++].AsInt32();
            reward_type          = data[index++].AsInt32();
            reward_value         = data[index++].AsInt32();
            reward_count         = data[index++].AsInt32();
        }

        public IEnumerable<int> GetAgentIDs()
        {
            if (agent_id_1 != 0)
                yield return agent_id_1;
            if (agent_id_2 != 0)
                yield return agent_id_2;
            if (agent_id_3 != 0)
                yield return agent_id_3;
            if (agent_id_4 != 0)
                yield return agent_id_4;
            yield break;
        }

        public IEnumerable<BattleOption> GetBattleOptions()
        {
            if (battle_option_type_1 != 0)
                yield return new BattleOption(battle_option_type_1, value1_b1, value2_b1);
            if (battle_option_type_2 != 0)
                yield return new BattleOption(battle_option_type_2, value1_b2, value2_b2);
            if (battle_option_type_3 != 0)
                yield return new BattleOption(battle_option_type_3, value1_b3, value2_b3);
            if (battle_option_type_4 != 0)
                yield return new BattleOption(battle_option_type_4, value1_b4, value2_b4);
            yield break;
        }
    }
}