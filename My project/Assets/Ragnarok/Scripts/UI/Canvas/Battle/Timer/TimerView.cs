using UnityEngine;

namespace Ragnarok.View
{
    public class TimerView : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIStopwatch stopwatch;

        public event System.Action OnFinished;
        public event System.Action<double> OnUpdate;

        void Awake()
        {
            stopwatch.OnFinished += OnFinishedStopwatch;
            stopwatch.OnUpdate += OnUpdateStopwatch;
        }

        void OnDestroy()
        {
            stopwatch.OnFinished -= OnFinishedStopwatch;
            stopwatch.OnUpdate -= OnUpdateStopwatch;
        }

        void OnFinishedStopwatch()
        {
            OnFinished?.Invoke();
        }

        void OnUpdateStopwatch(double totalSeconds)
        {
            OnUpdate?.Invoke(totalSeconds);
        }

        /// <summary>
        /// 남은 시간 세팅 (밀리초)
        /// </summary>
        public void Initialize(long limitTime)
        {
            stopwatch.SetLimitTime(limitTime);
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
    }
}