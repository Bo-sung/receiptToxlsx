using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public sealed class CustomerRewardView : UIView, IInspectorFinder
    {
        public interface IInput
        {
            UICustomerRewardSlot.IInput[] Inputs { get; }
            RemainTime RemainTime { get; }
            bool IsAllReceived { get; }
        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UICustomerRewardSlot[] slots;
        [SerializeField] UIGrid grid;
        [SerializeField] UILabelHelper labelRemainTime;

        public event System.Action OnSelect;

        private int titleLocalKey;

        protected override void Awake()
        {
            base.Awake();

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].OnSelect += OnSelectSlot;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].OnSelect -= OnSelectSlot;
            }
        }

        protected override void OnLocalize()
        {
            UpdateText();
        }

        public void Initialize(int titleLocalKey)
        {
            this.titleLocalKey = titleLocalKey;
            UpdateText();
        }

        public void SetData(IInput input)
        {
            int length = input.Inputs == null ? 0 : input.Inputs.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetData(i < length ? input.Inputs[i] : null);
            }

            grid.Reposition();

            Timing.KillCoroutines(gameObject);
            if (input.IsAllReceived)
            {
                labelRemainTime.LocalKey = LocalizeKey._8209; // 모든 보상을 수령하였습니다.
            }
            else
            {
                Timing.RunCoroutineSingleton(YieldRemainTime(input.RemainTime).CancelWith(gameObject), Segment.RealtimeUpdate, gameObject, SingletonBehavior.Overwrite);
            }
        }

        void OnSelectSlot()
        {
            OnSelect?.Invoke();
        }

        private void UpdateText()
        {
            labelTitle.LocalKey = titleLocalKey;
        }

        private IEnumerator<float> YieldRemainTime(RemainTime remainTime)
        {
            while (true)
            {
                float time = remainTime.ToRemainTime();
                if (time <= 0f)
                    break;

                UpdateRemainTimeText(time.ToTimeSpan());
                yield return Timing.WaitForSeconds(0.5f);
            }

            UpdateRemainTimeText(System.TimeSpan.Zero);
        }

        private void UpdateRemainTimeText(System.TimeSpan timeSpan)
        {
            if (timeSpan.Ticks > 0L)
            {
                string timeText = StringBuilderPool.Get()
                    .Append("[c][5792FF]")
                    .Append(timeSpan.ToString(@"hh\:mm\:ss"))
                    .Append("[-][/c]")
                    .Release();

                labelRemainTime.Text = LocalizeKey._8207.ToText() // 다음 보상까지 남은 시간 : {TIME}
                    .Replace(ReplaceKey.TIME, timeText);
            }
            else
            {
                labelRemainTime.LocalKey = LocalizeKey._8208; // 보상 획득이 가능합니다.
            }
        }

        bool IInspectorFinder.Find()
        {
            slots = GetComponentsInChildren<UICustomerRewardSlot>();
            return true;
        }
    }
}