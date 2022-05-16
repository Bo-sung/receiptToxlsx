using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="GateDataManager"/>
    /// </summary>
    public sealed class GateData : IData, IBossMonsterSpawnData, UIBossComing.IInput
    {
        private const int BOSS_FIXED_TYPE = 1;

        public readonly int id;
        public readonly int name_id;
        public readonly string scene_name;
        public readonly string bgm;
        public readonly string boss_battle_scene_name;
        public readonly string boss_bgm;
        public readonly int max_user;
        public readonly int monster_group;
        public readonly int boss_battle_condition;
        private readonly RewardData failReward;
        private readonly RewardData player1Reward;
        private readonly RewardData player2Reward;
        private readonly RewardData player3Reward;
        private readonly RewardData player4Reward;
        public readonly int play_time;
        public readonly int chapter;

        private ISpawnMonster bossMonster; // 클라 전용

        int IBossMonsterSpawnData.BossMonsterId => bossMonster.Id;
        int IBossMonsterSpawnData.Level => bossMonster.Level;
        float IBossMonsterSpawnData.Scale => bossMonster.Scale;

        private int multiMazeDataId; // 클라 전용

        public GateData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            name_id = data[index++].AsInt32();
            scene_name = data[index++].AsString();
            bgm = data[index++].AsString();
            boss_battle_scene_name = data[index++].AsString();
            boss_bgm = data[index++].AsString();
            int size_x = data[index++].AsInt32();
            int size_y = data[index++].AsInt32();
            string multi_maze_data = data[index++].AsString();
            max_user = data[index++].AsInt32();
            monster_group = data[index++].AsInt32();
            boss_battle_condition = data[index++].AsInt32();
            int fail_reward_type = data[index++].AsInt32();
            int fail_reward_value = data[index++].AsInt32();
            int fail_reward_count = data[index++].AsInt32();
            int win_reward_type = data[index++].AsInt32();
            int player1_reward_value = data[index++].AsInt32();
            int player1_reward_count = data[index++].AsInt32();
            int player2_reward_value = data[index++].AsInt32();
            int player2_reward_count = data[index++].AsInt32();
            int player3_reward_value = data[index++].AsInt32();
            int player3_reward_count = data[index++].AsInt32();
            int player4_reward_value = data[index++].AsInt32();
            int player4_reward_count = data[index++].AsInt32();
            play_time = data[index++].AsInt32();
            chapter = data[index++].AsInt32();

            failReward = new RewardData(fail_reward_type, fail_reward_value, fail_reward_count);
            player1Reward = new RewardData(win_reward_type, player1_reward_value, player1_reward_count);
            player2Reward = new RewardData(win_reward_type, player2_reward_value, player2_reward_count);
            player3Reward = new RewardData(win_reward_type, player3_reward_value, player3_reward_count);
            player4Reward = new RewardData(win_reward_type, player4_reward_value, player4_reward_count);
        }

        public void SetMultiMazeDataId(int multiMazeDataId)
        {
            this.multiMazeDataId = multiMazeDataId;
        }

        public int GetMultiMazeDataId()
        {
            // 따로 세팅되어있지 않다면 id 값이 waitingRoomId 와 동일
            if (multiMazeDataId == 0)
                return id;

            return multiMazeDataId;
        }

        public void SetBoss(ISpawnMonster bossMonster)
        {
            this.bossMonster = bossMonster;
        }

        public string GetIcon()
        {
            return StringBuilderPool.Get()
                .Append("Ui_Common_Icon_Gate").Append(id)
                .Release();
        }

        public float GetCharacterSpeed()
        {
            return 1f;
        }

        public int GetBossId()
        {
            return bossMonster == null ? 0 : bossMonster.Id;
        }

        public RewardData GetReward(int index)
        {
            switch (index)
            {
                case 0: return failReward;
                case 1: return player1Reward;
                case 2: return player2Reward;
                case 3: return player3Reward;
                case 4: return player4Reward;
            }

            throw new System.ArgumentException($"유효하지 않은 처리: {nameof(index)} = {index}");
        }

        /// <summary>
        /// 보스 움직임 처리 (하드코딩)
        /// 실제 움직임은 Scene 에서 고정시킴
        /// </summary>
        public bool IsFixedBoss()
        {
            switch (id)
            {
                // 고정 보스
                case 1:
                    return true;
            }

            return false;
        }

        public int GetDescriptionId()
        {
            switch (id)
            {
                case 1:
                    return LocalizeKey._6906; // GATE 진입을 막고있는\n보스를 처치하라!
            }

            return LocalizeKey._6917; // 생체연구를 막아내고\n보스를 처치하라!
        }

        int UIBossComing.IInput.GetMonsterId()
        {
            return GetBossId();
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