using System;
using System.Collections;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// 현재 적용중인 버프 - 아이템 (소모품)
    /// </summary>
    public class BuffItemInfo : DataInfo<ItemData>, IEnumerable<BattleOption>, IBuff, UIApplyBuffContent.IBuffInfo
    {
        /// <summary>
        /// 끝나는 시간 (버프 종료 시간)
        /// </summary>
        private RemainTime endTime;

        /// <summary>
        /// 지속시간
        /// </summary>
        public float Duration => GetDuration();

        /// <summary>
        /// 아이콘 이름
        /// </summary>
        public string IconName => data.icon_name;

        /// <summary>
        /// 이름
        /// </summary>
        public string Name => data.name_id.ToText();

        /// <summary>
        /// 남은 시간
        /// </summary>
        public float RemainTime => GetRemainTime();

        /// <summary>
        /// 남은 시간 텍스트
        /// </summary>
        public string RemainTimeText => GetRemainTimeText();

        /// <summary>
        /// 아이템 ID
        /// </summary>
        public int Itemid => data.id;

        public virtual bool IsEventBuff => false;

        public virtual bool HasRemainTime => true;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(float remainDuration)
        {
            endTime = remainDuration;
        }

        /// <summary>
        /// 유효성
        /// </summary>
        public bool IsValid()
        {
            return endTime.ToRemainTime() > 0f;
        }

        /// <summary>
        /// 진행도
        /// </summary>
        public float GetProgress()
        {
            float duration = GetDuration();
            if (duration == 0f)
                return 1f;

            float remainTime = endTime.ToRemainTime() * 0.001f;
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

        /// <summary>
        /// 특정 버프 옵션 타입이 있는지 체크
        /// </summary>
        public bool HasBattleOptionType(BattleOptionType type)
        {
            foreach (var item in this)
            {
                if (item.battleOptionType == type)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 특정 버프 옵션 타입의 값
        /// </summary>
        public int[] GetBattleOptionTypeValues(BattleOptionType type)
        {
            foreach (var item in this)
            {
                if (item.battleOptionType == type)
                {
                    return new int[] { item.value1, item.value2 };
                }
            }

            return new int[] { 0, 0 }; // 버프가 없을경우..
        }

        private float GetRemainTime()
        {
            return endTime.ToRemainTime();
        }

        private string GetRemainTimeText()
        {
            float time = endTime.ToRemainTime();

            if (time == 0)
                return "00:00";

            // UI 표시에 1분을 추가해서 보여준다.
            TimeSpan span = TimeSpan.FromMilliseconds(time + 60000);

            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            if (isDay)
                return LocalizeKey._8041.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}

            return span.ToString(@"hh\:mm");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
