using UnityEngine;

namespace Ragnarok
{
    public class UIHourglass : MonoBehaviour
    {
        [SerializeField] UIProgressBar progress;

        private float limitTime;
        private System.TimeSpan timeSpan;
        private bool isRun;

        public event System.Action OnFinished;
        public event System.Action<double> OnUpdate;

        void Update()
        {
            if (!isRun)
                return;

            timeSpan -= System.TimeSpan.FromSeconds(Time.deltaTime);
            if (timeSpan.Ticks > 0L) // 남은 시간 존재
            {
                Refresh();
                return;
            }

            // Finish
            StopTimer();
            timeSpan = System.TimeSpan.Zero;
            Refresh();

            OnFinished?.Invoke();
        }

        public void SetLimitTime(float limitTime)
        {
            this.limitTime = limitTime;
            ResetTimer();
        }

        public void ResetTimer()
        {
            timeSpan = System.TimeSpan.FromSeconds(limitTime);
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
            progress.value = 1f - MathUtils.GetProgress((int)timeSpan.TotalSeconds, (int)limitTime);

            OnUpdate?.Invoke(timeSpan.TotalSeconds);
        }
    } 
}
