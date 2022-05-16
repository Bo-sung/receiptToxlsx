using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class EmperiumRemainTimeView : UIView
    {
        [SerializeField] UILabelHelper labelTitleRemainTime;
        [SerializeField] UILabelHelper labelRemainTime;

        protected override void OnLocalize()
        {
            labelTitleRemainTime.LocalKey = LocalizeKey._38411; // 다음 습격까지 남은 시간
        }

        public void SetRemainTime(DateTime startTime)
        {
            Timing.RunCoroutineSingleton(YieldRemainTime(startTime).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldRemainTime(DateTime startTime)
        {
            while (true)
            {
                TimeSpan span = startTime - ServerTime.Now;
                if (span.Ticks <= 0)
                    break;

                UpdateLimitTime(span);
                yield return Timing.WaitForSeconds(0.5f);
            }
            UpdateLimitTime(TimeSpan.Zero);
        }

        private void UpdateLimitTime(TimeSpan span)
        {
            if (span.Ticks <= 0)
            {
                labelRemainTime.Text = "00:00";
                return;
            }

            // UI 표시에 1분을 추가해서 보여준다.
            span = span.Add(TimeSpan.FromMilliseconds(60000));

            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            if (isDay)
            {
                labelRemainTime.Text = LocalizeKey._8041.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}
            }
            else
            {
                labelRemainTime.Text = span.ToString(@"hh\:mm");
            }
        }
    }
}