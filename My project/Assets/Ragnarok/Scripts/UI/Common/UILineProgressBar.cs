using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// progress Line 이 존재
    /// </summary>
    [RequireComponent(typeof(UIProgressBar))]
    public class UILineProgressBar : MonoBehaviour, IAutoInspectorFinder
    {
        GameObject myGameObject;
        UIProgressBar progressBar;

        [SerializeField] UILabel labelLine;
        [SerializeField] int maxLine = 1;
        [SerializeField]
        Color[] colors =
        {
            new Color32(220, 0, 0, 255),
            new Color32(255, 142, 0, 255),
            new Color32(255, 208, 0, 255),
            new Color32(121, 223, 16, 255),
            new Color32(7, 211, 255, 255),
            new Color32(25, 103, 238, 255),
            new Color32(127, 3, 254, 255),
        };

        [SerializeField] float tweenTime = 0.2f;

        private float destProgress;

        void Awake()
        {
            myGameObject = gameObject;
            progressBar = GetComponent<UIProgressBar>();
        }

        void OnDestroy()
        {
            StopKillCoroutine();
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void Set(long cur, long max, int maxLine)
        {
            this.maxLine = Mathf.Max(1, maxLine); // maxLine 은 1 이상
            Set(cur, max);
        }

        public void SetMaxLine(int maxLine)
        {
            this.maxLine = Mathf.Max(1, maxLine); // maxLine 은 1 이상
            Refresh();
        }

        public void Set(long cur, long max)
        {
            StopKillCoroutine();

            destProgress = MathUtils.GetProgress(cur, max);
            Refresh();
        }

        public void Tween(long cur, long max)
        {
            StopKillCoroutine();

            float oldProgress = destProgress;
            float progress = MathUtils.GetProgress(cur, max);
            Timing.RunCoroutine(YieldPlayProgress(oldProgress, progress), myGameObject);
        }

        private void Refresh()
        {
            float valuePerLine = MathUtils.GetProgress(1, maxLine);
            int line = Mathf.CeilToInt(destProgress / valuePerLine);
            float value = (destProgress % valuePerLine) * maxLine;

            if (value == 0f && line == maxLine) // max일 경우
                value = 1f;

            if (progressBar.backgroundWidget)
                progressBar.backgroundWidget.color = GetColor(line - 1);

            if (progressBar.foregroundWidget)
                progressBar.foregroundWidget.color = GetColor(line);

            if (progressBar)
                progressBar.value = value;

            if (labelLine)
                labelLine.text = line > 1 ? string.Concat("x", line.ToString()) : string.Empty;
        }

        private Color GetColor(int line)
        {
            if (colors == null || colors.Length == 0)
                return Color.clear;

            int index = line - 1;
            if (index < 0)
                return Color.clear;

            return colors[index % colors.Length];
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        private void StopKillCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }

        IEnumerator<float> YieldPlayProgress(float oldValue, float value)
        {
            float lastRealTime = Time.realtimeSinceStartup;
            float runningTime;
            float percentage = 0f;
            float timeRate = 1f / tweenTime;

            while (percentage < 1f)
            {
                runningTime = Time.realtimeSinceStartup - lastRealTime;
                percentage = runningTime * timeRate;

                destProgress = Mathf.Lerp(oldValue, value, percentage);
                Refresh();
                yield return Timing.WaitForOneFrame;
            }

            Refresh();
        }
    }
}