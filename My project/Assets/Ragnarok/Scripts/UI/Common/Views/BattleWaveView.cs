using UnityEngine;

namespace Ragnarok.View
{
    public class BattleWaveView : UIView
    {
        [SerializeField] UIStopwatch stopwatch;
        [SerializeField] UILabelHelper labelWave;

        public event System.Action OnFinished;
        public event System.Action<double> OnUpdate;

        private int titleLocalKey = LocalizeKey._37000; // Wave {VALUE}
        private int wave;

        protected override void Awake()
        {
            base.Awake();

            stopwatch.OnFinished += OnFinishedStopwatch;
            stopwatch.OnUpdate += OnUpdateStopwatch;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            stopwatch.OnFinished -= OnFinishedStopwatch;
            stopwatch.OnUpdate -= OnUpdateStopwatch;
        }

        protected override void OnLocalize()
        {
            UpdateWaveText();
        }

        void OnFinishedStopwatch()
        {
            OnFinished?.Invoke();
        }

        void OnUpdateStopwatch(double totalSeconds)
        {
            OnUpdate?.Invoke(totalSeconds);
        }

        public void SetTitle(int titleLocalKey)
        {
            this.titleLocalKey = titleLocalKey;
            UpdateWaveText();
        }

        /// <summary>
        /// 남은 시간 세팅 (밀리초)
        /// </summary>
        public void Initialize(float milliseconds)
        {
            stopwatch.SetLimitTime(milliseconds);
        }

        public void RestartTimer()
        {
            stopwatch.StartTimer();
        }

        public void ResetTimer()
        {
            stopwatch.ResetTimer();
        }

        public void StopTimer()
        {
            stopwatch.StopTimer();
        }

        public void SetWave(int wave)
        {
            this.wave = wave;
            UpdateWaveText();
        }

        private void UpdateWaveText()
        {
            labelWave.Text = titleLocalKey.ToText()
                .Replace(ReplaceKey.VALUE, wave)
                .Replace(ReplaceKey.INDEX, wave)
                .Replace(ReplaceKey.COUNT, wave);
        }
    }
}