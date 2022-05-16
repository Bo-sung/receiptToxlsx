using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="EndlessTowerDataManager"/>
    /// </summary>
    public sealed class EndlessTowerData : IData, IBossMonsterSpawnData, UIBossComing.IInput, UIEndlessTowerElement.IInput
    {
        private const int FLOOR_NORMAL_TYPE = 1;
        private const int FLOOR_BOSS_TYPE = 2;

        private readonly ObscuredInt floorType;
        private readonly ObscuredInt floor;
        private readonly ObscuredInt boss_monster_scale;
        private readonly ObscuredInt boss_monster_level;
        private readonly ObscuredInt boss_monster_id;
        private readonly byte boss_drop1_type;
        private readonly int boss_drop1_reward;
        private readonly int boss_drop1_count;
        private readonly byte boss_drop2_type;
        private readonly int boss_drop2_reward;
        private readonly int boss_drop2_count;
        public readonly ObscuredInt normal_monster_level;
        private readonly ObscuredInt normal_monster_id_1;
        private readonly ObscuredInt normal_monster_id_2;
        private readonly ObscuredInt normal_monster_id_3;
        private readonly ObscuredInt normal_monster_id_4;
        private readonly byte normal_drop_type;
        private readonly int normal_drop_reward;
        private readonly int normal_drop_count;
        private readonly ObscuredInt clear_recovery;

        int IBossMonsterSpawnData.BossMonsterId => boss_monster_id;
        int IBossMonsterSpawnData.Level => boss_monster_level;
        float IBossMonsterSpawnData.Scale => MathUtils.ToPercentValue(boss_monster_scale);

        public EndlessTowerData(IList<MessagePackObject> data)
        {
            byte index = 0;
            int id = data[index++].AsInt32();
            floorType = data[index++].AsInt32();
            floor = data[index++].AsInt32();
            boss_monster_scale = data[index++].AsInt32();
            boss_monster_level = data[index++].AsInt32();
            boss_monster_id = data[index++].AsInt32();
            boss_drop1_type = (byte)data[index++].AsInt32();
            boss_drop1_reward = data[index++].AsInt32();
            boss_drop1_count = data[index++].AsInt32();
            boss_drop2_type = (byte)data[index++].AsInt32();
            boss_drop2_reward = data[index++].AsInt32();
            boss_drop2_count = data[index++].AsInt32();
            normal_monster_level = data[index++].AsInt32();
            normal_monster_id_1 = data[index++].AsInt32();
            normal_monster_id_2 = data[index++].AsInt32();
            normal_monster_id_3 = data[index++].AsInt32();
            normal_monster_id_4 = data[index++].AsInt32();
            normal_drop_type = (byte)data[index++].AsInt32();
            normal_drop_reward = data[index++].AsInt32();
            normal_drop_count = data[index++].AsInt32();
            clear_recovery = data[index++].AsInt32();
        }

        public int GetFloor()
        {
            return floor;
        }

        public bool IsBossFloor()
        {
            return floorType == FLOOR_BOSS_TYPE;
        }

        public RewardData[] GetRewards()
        {
            if (IsBossFloor())
            {
                return new RewardData[2]
                {
                    new RewardData(boss_drop1_type, boss_drop1_reward, boss_drop1_count),
                    new RewardData(boss_drop2_type, boss_drop2_reward, boss_drop2_count),
                };
            }

            return new RewardData[1]
            {
                new RewardData(normal_drop_type, normal_drop_reward, normal_drop_count),
            };
        }

        public RewardPacket[] GetNormalRewards()
        {
            if (IsBossFloor())
                return null;

            return new RewardPacket[1]
            {
                new RewardPacket(normal_drop_type, normal_drop_reward, 0, normal_drop_count),
            };
        }

        public IEnumerable<int> GetNormalMonsterId()
        {
            if (normal_monster_id_1 > 0)
                yield return normal_monster_id_1;

            if (normal_monster_id_2 > 0)
                yield return normal_monster_id_2;

            if (normal_monster_id_3 > 0)
                yield return normal_monster_id_3;

            if (normal_monster_id_4 > 0)
                yield return normal_monster_id_4;
        }

        /// <summary>
        /// 회복률
        /// </summary>
        public float GetRecoveryRate()
        {
            return MathUtils.ToPercentValue(clear_recovery);
        }

        int UIBossComing.IInput.GetMonsterId()
        {
            return boss_monster_id;
        }

        string UIBossComing.IInput.GetDescription()
        {
            return LocalizeKey._39302.ToText(); // 보스 출현!
        }

        string UIBossComing.IInput.GetSpriteName()
        {
            return "Ui_Common_Icon_Boss4";
        }
    }
}