using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIGuildContent : UIElement<UIGuildContent.IInput>, IInspectorFinder
    {
        public interface IInput
        {
            int NameId { get; }
            System.DateTime StartTime { get; }
        }

        [SerializeField] UIButton button;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;

        public event System.Action OnSelect;

        protected override void Awake()
        {
            base.Awake();

            button = GetComponent<UIButton>();

            EventDelegate.Add(button.onClick, OnClickedButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(button.onClick, OnClickedButton);
        }

        protected override void OnLocalize()
        {
            if (info == null)
            {
                labelName.Text = string.Empty;
                labelDescription.Text = string.Empty;
                return;
            }

            labelName.LocalKey = info.NameId;
        }

        protected override void Refresh()
        {
            Timing.RunCoroutineSingleton(YieldRemainTime(info.StartTime).CancelWith(gameObject), Segment.RealtimeUpdate, gameObject, SingletonBehavior.Overwrite);
        }

        void OnClickedButton()
        {
            OnSelect?.Invoke();
        }

        private IEnumerator<float> YieldRemainTime(System.DateTime startTime)
        {
            while (true)
            {
                System.TimeSpan span = startTime - ServerTime.Now;
                if (span.Ticks <= 0)
                    break;

                UpdateLimitTime(span);
                yield return Timing.WaitForSeconds(0.5f);
            }
            UpdateLimitTime(System.TimeSpan.Zero);
        }

        private void UpdateLimitTime(System.TimeSpan span)
        {
            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            if (isDay)
            {
                labelDescription.Text = LocalizeKey._8041.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}
            }
            else
            {
                labelDescription.Text = span.ToString(@"hh\:mm\:ss");
            }
        }

        bool IInspectorFinder.Find()
        {
            button = GetComponent<UIButton>();
            return true;
        }
    }
}