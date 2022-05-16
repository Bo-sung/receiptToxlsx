using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using Ragnarok.View;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="EventQuizDataManager"/>
    /// </summary>
    public sealed class EventQuizData : IData, QuizQuizView.IInput
    {
        private const int ALL_CORRECT_ANSWER = 3; // 모두 정답으로 처리

        public readonly ObscuredInt id;
        public readonly int group_id;
        //public readonly int daily_group;
        public readonly int start_date;
        public readonly ObscuredInt seq;
        public readonly string image_name;
        public readonly int quiz_text;
        public readonly ObscuredInt answer;
        //public readonly int fail_reward_type;
        //public readonly int fail_reward_value;
        //public readonly int fail_reward_count;

        private readonly RewardData reward;
        private ObscuredInt maxSeq;

        int QuizQuizView.IInput.Id => id;
        int QuizQuizView.IInput.StartDate => start_date;
        string QuizQuizView.IInput.ImageName => image_name;
        int QuizQuizView.IInput.QuizLocalKey => quiz_text;
        RewardData QuizQuizView.IInput.Reward => reward;
        int QuizQuizView.IInput.Seq => seq;
        int QuizQuizView.IInput.MaxSeq => maxSeq;

        public EventQuizData(IList<MessagePackObject> data)
        {
            int index = 0;
            id = data[index++].AsInt32();
            group_id = data[index++].AsInt32();
            int daily_group = data[index++].AsInt32();
            start_date = data[index++].AsInt32();
            seq = data[index++].AsInt32();
            image_name = data[index++].AsString();
            quiz_text = data[index++].AsInt32();
            answer = data[index++].AsInt32();
            int success_reward_type = data[index++].AsInt32();
            int success_reward_value = data[index++].AsInt32();
            int success_reward_count = data[index++].AsInt32();
            int fail_reward_type = data[index++].AsInt32();
            int fail_reward_value = data[index++].AsInt32();
            int fail_reward_count = data[index++].AsInt32();

            reward = new RewardData((byte)success_reward_type, success_reward_value, success_reward_count);
        }

        public void SetMaxSeq(int maxSeq)
        {
            this.maxSeq = maxSeq;
        }

        bool QuizQuizView.IInput.GetAnswer(byte answer)
        {
            // 모두 정답으로 인정
            if (this.answer == ALL_CORRECT_ANSWER)
                return true;

            return this.answer == answer;
        }
    }
}