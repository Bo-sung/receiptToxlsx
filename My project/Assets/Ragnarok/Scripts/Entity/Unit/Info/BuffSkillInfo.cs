using System.Collections.Generic;
using System.Collections;

namespace Ragnarok
{
    /// <summary>
    /// 현재 적용중인 버프 - 스킬 (액티브)
    /// </summary>
    public class BuffSkillInfo : DataInfo<SkillData>, IEnumerable<BattleOption>, IBuff
    {
        /// <summary>
        /// 끝나는 시간 (버프 종료 시간)
        /// </summary>
        private RelativeRemainTime endTime;

        /// <summary>
        /// 스킬 아이디
        /// </summary>
        public int SkillId => data.id;

        /// <summary>
        /// 스킬 레벨
        /// </summary>
        public int SkillLevel => data.lv;

        /// <summary>
        /// 아이콘 이름
        /// </summary>
        public string IconName => data.icon_name;

        /// <summary>
        /// 유효성
        /// </summary>
        public bool IsValid()
        {
            return endTime.GetRemainTime() > 0f;
        }

        /// <summary>
        /// 진행도
        /// </summary>
        public float GetProgress()
        {
            float duration = GetDuration();
            if (duration == 0f)
                return 1f;

            float remainTime = endTime.GetRemainTime();
            if (remainTime == 0f)
                return 1f;

            return 1 - (remainTime / duration);
        }

        /// <summary>
        /// 지속시간 반환
        /// </summary>
        private float GetDuration()
        {
            if (data.duration == 0)
                return 0f;

            return data.duration * 0.001f;
        }

        public override void SetData(SkillData data)
        {
            base.SetData(data);
            endTime = GetDuration();
        }

        public IEnumerator<BattleOption> GetEnumerator()
        {
            BattleOption option1 = new BattleOption(data.battle_option_type_1, data.value1_b1, data.value2_b1);
            BattleOption option2 = new BattleOption(data.battle_option_type_2, data.value1_b2, data.value2_b2);
            BattleOption option3 = new BattleOption(data.battle_option_type_3, data.value1_b3, data.value2_b3);
            BattleOption option4 = new BattleOption(data.battle_option_type_4, data.value1_b4, data.value2_b4);

            if (option1.battleOptionType != BattleOptionType.None)
                yield return option1;

            if (option2.battleOptionType != BattleOptionType.None)
                yield return option2;

            if (option3.battleOptionType != BattleOptionType.None)
                yield return option3;

            if (option4.battleOptionType != BattleOptionType.None)
                yield return option4;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}