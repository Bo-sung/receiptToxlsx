using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class DuelRewardData : IData
    {
        public const int STAGE_DUEL_TYPE = 1;
        public const int EVENT_DUEL_TYPE = 2;
        public const int ARENA_DUEL_TYPE = 3;

        public readonly ObscuredInt id;
        public readonly ObscuredInt check_type; // 1. 일반듀얼, 2. 이벤트듀얼(서버대항전), 3. 아레나듀얼
        public readonly ObscuredInt check_value;
        public readonly ObscuredInt step_value;
        /// <summary>
        /// 1. 일반듀얼일 경우에 bitType
        /// 2. 이벤트듀얼 (필요 승수)
        /// </summary>
        public readonly ObscuredInt need_bit_type;
        public readonly ObscuredString words_value;
        public readonly ObscuredInt color_index;
        public readonly ObscuredInt reward_type;
        public readonly ObscuredInt reward_value;
        public readonly ObscuredInt reward_count;

        public IEnumerable<char> GetWord()
        {
            int index = 0;
            int bit = need_bit_type;
            string word = words_value;

            if ((bit & 0x0001) > 0)
                yield return word[index++];
            else
                yield return ' ';

            if ((bit & 0x0002) > 0)
                yield return word[index++];
            else
                yield return ' ';

            if ((bit & 0x0004) > 0)
                yield return word[index++];
            else
                yield return ' ';

            if ((bit & 0x0008) > 0)
                yield return word[index++];
            else
                yield return ' ';

            if ((bit & 0x0010) > 0)
                yield return word[index++];
            else
                yield return ' ';

            if ((bit & 0x0020) > 0)
                yield return word[index++];
            else
                yield return ' ';

            if ((bit & 0x0040) > 0)
                yield return word[index++];
            else
                yield return ' ';

            if ((bit & 0x0080) > 0)
                yield return word[index++];
            else
                yield return ' ';

            yield break;
        }

        public DuelRewardData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            check_type = data[index++].AsInt32();
            check_value = data[index++].AsInt32();
            step_value = data[index++].AsInt32();
            need_bit_type = data[index++].AsInt32();
            words_value = data[index++].AsString();
            color_index = data[index++].AsInt32();
            reward_type = data[index++].AsInt32();
            reward_value = data[index++].AsInt32();
            reward_count = data[index++].AsInt32();
        }

        public RewardData GetReward()
        {
            return new RewardData(reward_type, reward_value, reward_count);
        }
    }
}