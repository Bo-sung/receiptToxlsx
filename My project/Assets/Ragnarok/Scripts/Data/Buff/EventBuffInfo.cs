using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class EventBuffInfo : UIApplyBuffContent.IBuffInfo
    {
        public readonly ObscuredInt expRate;
        public readonly ObscuredInt jobExpRate;
        public readonly ObscuredInt goldRate;
        public readonly ObscuredInt itemDropRate;
        //public readonly ObscuredInt pieceDropRate;

        private readonly string titleText;
        public readonly RemainTime remainTime;

        public event Action OnUpdateEvent;
        bool IInfo.IsInvalidData => false;

        bool UIApplyBuffContent.IBuffInfo.IsEventBuff => true;
        bool UIApplyBuffContent.IBuffInfo.HasRemainTime => true;
        string UIApplyBuffContent.IBuffInfo.IconName => throw new NotImplementedException();
        string UIApplyBuffContent.IBuffInfo.Name => GetTitleText();
        string UIApplyBuffContent.IBuffInfo.RemainTimeText => GetRemainTimeText();

        public EventBuffInfo(EventBuffInfo info)
        {
            expRate = info.expRate;
            jobExpRate = info.jobExpRate;
            goldRate = info.goldRate;
            itemDropRate = info.itemDropRate;

            remainTime = info.remainTime;
            titleText = info.titleText;
        }

        public EventBuffInfo(EventBuff data, long leftTime)
        {
            expRate = data.exp_rate;
            jobExpRate = data.job_exp_rate;
            goldRate = data.gold_rate;
            itemDropRate = data.item_drop_rate;
            //pieceDropRate = data.piece_drop_rate;

            remainTime = leftTime;
            titleText = data.title;
        }

        public EventBuffInfo(EventDuelBuffPacket data)
        {
            expRate = data.buf_exp;
            jobExpRate = data.buf_jobexp;
            goldRate = data.buf_gold;
            itemDropRate = data.buf_item;

            remainTime = data.remain_time;
            titleText = data.buf_script.ToText();
        }

        private string GetTitleText()
        {
            int localKey;
            if (int.TryParse(titleText, out localKey))
            {
                return localKey.ToText(); // key로 받았을 때..
            }
            else
            {
                return titleText; // 영문 텍스트
            }
        }

        private string GetRemainTimeText()
        {
            float time = remainTime.ToRemainTime();

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

        public IEnumerator<BattleOption> GetEnumerator()
        {
            // 아이템 버프 코드를 그대로 사용하기 위해 100을 곱해줌.
            BattleOption option1 = new BattleOption((int)BattleOptionType.ExpDropRate, 0, expRate * 100);
            BattleOption option2 = new BattleOption((int)BattleOptionType.JobExpDropRate, 0, jobExpRate * 100);
            BattleOption option3 = new BattleOption((int)BattleOptionType.ZenyDropRate, 0, goldRate * 100);
            BattleOption option4 = new BattleOption((int)BattleOptionType.ItemDropRate, 0, itemDropRate * 100);

            if (option1.value1 > 0 || option1.value2 > 0)
                yield return option1;

            if (option2.value1 > 0 || option2.value2 > 0)
                yield return option2;

            if (option3.value1 > 0 || option3.value2 > 0)
                yield return option3;

            if (option4.value1 > 0 || option4.value2 > 0)
                yield return option4;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
