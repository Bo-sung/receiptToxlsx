using UnityEngine;

namespace Ragnarok
{
    public class HudCircleTimer : HUDObject, IAutoInspectorFinder
    {
        [SerializeField] UITexture circle;

        private bool isPlaying;
        private long destTick;
        private long curTick;
        private float startTime;

        /// <summary>
        /// Fill Value 설정 (0.0 ~ 1.0)
        /// </summary>
        public void SetValue(float t)
        {
            isPlaying = false;
            ApplyValue(t);
        }

        private void ApplyValue(float t)
        {
            circle.fillAmount = t;
        }

        /// <summary>
        /// 자동으로 채워지는 타이머 실행
        /// </summary>
        /// <param name="countTick"></param>
        /// <param name="startTick"></param>
        public void StartTimer(long countTick, long startTick = 0L)
        {
            isPlaying = true;

            destTick = countTick;
            curTick = startTick;
            startTime = Time.realtimeSinceStartup;

            float t = Mathf.Clamp01((float)curTick / destTick);
            ApplyValue(t);
        }

        public void Stop()
        {
            isPlaying = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!isPlaying)
                return;

            curTick += (long)(Time.deltaTime * 1000f);

            float t = Mathf.Clamp01((float)curTick / destTick);
            ApplyValue(t);
        }
    }
}