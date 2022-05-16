using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="ElementDataManager"/>
    /// </summary>
    public sealed class EventRpsData : IData, RpsResultView.IInput
    {
        public readonly ObscuredInt id;
        public readonly int monster_name;
        public readonly string provoke_image_name;
        public readonly string win_image_name;
        public readonly string lose_image_name;
        public readonly int provoke_text_id;
        public readonly int win_text_id;
        public readonly int lose_text_id;
        public readonly int rate_win;
        public readonly int rate_draw;
        public readonly int rate_lose;
        public readonly int try_cost;
        public readonly int retry_cost;
        public readonly int draw_text_id;

        public readonly RewardData reward_data;

        public EventRpsData(IList<MessagePackObject> data)
        {
            int index          = 0;
            id                 = data[index++].AsInt32();
            monster_name       = data[index++].AsInt32();
            provoke_image_name = data[index++].AsString();
            win_image_name     = data[index++].AsString();
            lose_image_name    = data[index++].AsString();
            provoke_text_id    = data[index++].AsInt32();
            win_text_id        = data[index++].AsInt32();
            lose_text_id       = data[index++].AsInt32();
            int reward_type    = data[index++].AsInt32();
            int reward_value   = data[index++].AsInt32();
            int reward_count   = data[index++].AsInt32();
            rate_win           = data[index++].AsInt32();
            rate_draw          = data[index++].AsInt32();
            rate_lose          = data[index++].AsInt32();
            try_cost           = data[index++].AsInt32();
            retry_cost         = data[index++].AsInt32();
            draw_text_id       = data[index++].AsInt32();

            reward_data = new RewardData(reward_type, reward_value, reward_count);
        }

        int RpsResultView.IInput.GetDialogueTextId(RpsResultType type)
        {
            switch (type)
            {
                case RpsResultType.Draw:
                    return draw_text_id;
                case RpsResultType.Defeat:
                    return lose_text_id;
                //case RpsResultType.Ready:
                default:
                    return win_text_id;
            }
        }

        string RpsResultView.IInput.GetPoringImage(RpsResultType type)
        {
            switch (type)
            {
                case RpsResultType.Draw:
                    return provoke_image_name;
                case RpsResultType.Defeat:
                    return lose_image_name;
                //case RpsResultType.Ready:
                default:
                    return win_image_name;
            }
        }

        RewardData RpsResultView.IInput.GetRewardData(RpsResultType type)
        {
            if(type == RpsResultType.Ready)
            {
                return reward_data;
            }

            return null;
        }
    }
}