using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class CupetSetBuffData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt des_id;
        public readonly ObscuredString icon_name;
        public readonly ObscuredInt cupet_id_1;
        public readonly ObscuredInt cupet_id_2;
        public readonly ObscuredInt cupet_id_3;
        public readonly ObscuredInt cupet_id_4;
        public readonly ObscuredByte target_type;
        public readonly BattleOption battleOption1;
        public readonly BattleOption battleOption2;
        public readonly BattleOption battleOption3;
        public readonly BattleOption battleOption4;

        public CupetSetBuffData(IList<MessagePackObject> data)
        {
            id                       = data[0].AsInt32();
            name_id                  = data[1].AsInt32();
            des_id                   = data[2].AsInt32();
            icon_name                = data[3].AsString();
            cupet_id_1               = data[4].AsInt32();
            cupet_id_2               = data[5].AsInt32();
            cupet_id_3               = data[6].AsInt32();
            cupet_id_4               = data[7].AsInt32();
            target_type              = data[8].AsByte();
            int battle_option_type_1 = data[9].AsInt32();
            int value1_b1            = data[10].AsInt32();
            int value2_b1            = data[11].AsInt32();
            int battle_option_type_2 = data[12].AsInt32();
            int value1_b2            = data[13].AsInt32();
            int value2_b2            = data[14].AsInt32();
            int battle_option_type_3 = data[15].AsInt32();
            int value1_b3            = data[16].AsInt32();
            int value2_b3            = data[17].AsInt32();
            int battle_option_type_4 = data[18].AsInt32();
            int value1_b4            = data[19].AsInt32();
            int value2_b4            = data[20].AsInt32();

            battleOption1 = new BattleOption(battle_option_type_1, value1_b1, value2_b1);
            battleOption2 = new BattleOption(battle_option_type_2, value1_b2, value2_b2);
            battleOption3 = new BattleOption(battle_option_type_3, value1_b3, value2_b3);
            battleOption4 = new BattleOption(battle_option_type_4, value1_b4, value2_b4);
        }

        public int GetCupetID(int index)
        {
            switch (index)
            {
                case 0: return cupet_id_1;
                case 1: return cupet_id_2;
                case 2: return cupet_id_3;
                case 3: return cupet_id_4;

                default:
                    Debug.LogError($"[올바르지 않은 {nameof(index)}] {nameof(index)} = {index}");
                    break;
            }

            return 0;
        }
    }
}