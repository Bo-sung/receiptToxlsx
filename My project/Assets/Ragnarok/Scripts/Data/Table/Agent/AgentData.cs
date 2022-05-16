using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="AgentDataManager"/>
    /// </summary>
    public class AgentData : IData, IInfo
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt job_id;
        public readonly ObscuredInt gender;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt weapon_id;
        public readonly ObscuredInt skill_id_1;
        public readonly ObscuredInt skill_id_2;
        public readonly ObscuredInt skill_id_3;
        public readonly ObscuredInt skill_id_4;
        public readonly ObscuredInt agent_type;
        public readonly ObscuredInt agent_rating;
        public readonly ObscuredString icon_name;
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
        public readonly ObscuredInt explore_bit_type;
        public readonly ObscuredInt dummy_chapter;
        public readonly ObscuredInt dummy_lv;
        public readonly ObscuredInt dummy_battle_score;
        public readonly ObscuredInt costume_id_1; // 장착 코스튬 id(무기) 아이디
        public readonly ObscuredInt costume_id_2; // 장착 코스튬 id(상단 모자) 아이디
        public readonly ObscuredInt costume_id_3; // 장착 코스튬 id(하단 얼굴 장식) 아이디
        public readonly ObscuredInt costume_id_4; // 장착 코스튬 id(망토) 아이디

        public bool IsInvalidData => id == 0;
        public event Action OnUpdateEvent;

        public string GetIconName(AgentIconType iconType)
        {
            if (iconType == AgentIconType.RectIcon)
                return string.Concat("Info_", icon_name);
            else if (iconType == AgentIconType.CircleIcon)
                return icon_name;
            else if (iconType == AgentIconType.HexaIcon)
                return string.Concat("Agent_", icon_name);
            return string.Empty;
        }

        public IEnumerable<ExploreType> GetExploreTypes()
        {
            ExploreType val = (ExploreType)(int)explore_bit_type;

            if (val.HasFlag(ExploreType.Trade))
            {
                yield return ExploreType.Trade;
            }

            if (val.HasFlag(ExploreType.Collect))
            {
                yield return ExploreType.Collect;
            }

            if (val.HasFlag(ExploreType.Dig))
            {
                yield return ExploreType.Dig;
            }

            if (val.HasFlag(ExploreType.Production))
            {
                yield return ExploreType.Production;
            }
            yield break;
        }

        public AgentData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            job_id = data[index++].AsInt32();
            gender = data[index++].AsInt32();
            name_id = data[index++].AsInt32();
            weapon_id = data[index++].AsInt32();
            skill_id_1 = data[index++].AsInt32();
            skill_id_2 = data[index++].AsInt32();
            skill_id_3 = data[index++].AsInt32();
            skill_id_4 = data[index++].AsInt32();
            agent_type = data[index++].AsInt32();
            agent_rating = data[index++].AsInt32();
            icon_name = data[index++].AsString();
            battle_option_type_1 = data[index++].AsInt32();
            value1_b1 = data[index++].AsInt32();
            value2_b1 = data[index++].AsInt32();
            battle_option_type_2 = data[index++].AsInt32();
            value1_b2 = data[index++].AsInt32();
            value2_b2 = data[index++].AsInt32();
            battle_option_type_3 = data[index++].AsInt32();
            value1_b3 = data[index++].AsInt32();
            value2_b3 = data[index++].AsInt32();
            battle_option_type_4 = data[index++].AsInt32();
            value1_b4 = data[index++].AsInt32();
            value2_b4 = data[index++].AsInt32();
            explore_bit_type = data[index++].AsInt32();
            dummy_chapter = data[index++].AsInt32();
            dummy_lv = data[index++].AsInt32();
            dummy_battle_score = data[index++].AsInt32();
            costume_id_1 = data[index++].AsInt32();
            costume_id_2 = data[index++].AsInt32();
            costume_id_3 = data[index++].AsInt32();
            costume_id_4 = data[index++].AsInt32();
        }

        public int GetSkillId(int index)
        {
            switch (index)
            {
                case 0: return skill_id_1;
                case 1: return skill_id_2;
                case 2: return skill_id_3;
                case 3: return skill_id_4;
            }

            return 0;
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