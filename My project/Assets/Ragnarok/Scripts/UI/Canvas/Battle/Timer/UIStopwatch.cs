using UnityEngine;

namespace Ragnarok
{
    public class UIStopwatch : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UILabel labelTime;
        [SerializeField] UILabel labelMillisecond;

        private bool isRun;
        private float limit;
        private RemainTime remainTime;

        public event System.Action OnFinished;
        public event System.Action<double> OnUpdate;

        void Update()
        {
            if (!isRun)
                return;

            if (remainTime.ToRemainTime() > 0f) // 남은 시간 존재
            {
                Refresh();
                return;
            }

            // Finish
            StopTimer();
            Refresh();

            OnFinished?.Invoke();
        }

        /// <summary>
        /// 남은 시간 세팅 (밀리초)
        /// </summary>
        public void SetLimitTime(float milliseconds)
        {
            limit = milliseconds;
            ResetTimer();
        }

        public void ResetTimer()
        {
            remainTime = limit;
            StopTimer();
            Refresh();
        }

        public void StartTimer()
        {
            isRun = true;
        }

        public void StopTimer()
        {
            isRun = false;
        }

        private void Refresh()
        {
            labelTime.text = remainTime.ToStringTime(@"mm\:ss\:ff");
            labelMillisecond.text = remainTime.ToStringTime("fff");

            OnUpdate?.Invoke(remainTime.ToRemainTime().ToTotalSeconds());
        }
    }
}