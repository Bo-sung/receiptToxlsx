using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// 잔상이 남는 효과
    /// </summary>
    public class UIAniProgressBar : MonoBehaviour, IAutoInspectorFinder
    {
        public UIProgressBar behind, front;
        [SerializeField] UILabel labelText;
        public float tweenTime = 0.5f;

        GameObject myGameObject;

        void Awake()
        {
            myGameObject = gameObject;
        }

        void OnDestroy()
        {
            StopAllCoroutine();
        }

        public void Show()
        {
            SetActive(true);
        }

        public void Hide()
        {
            SetActive(false);
        }

        public void Set(int cur, int max)
        {
            StopAllCoroutine();

            float progress = MathUtils.GetProgress(cur, max);

            if (labelText)
            {
                labelText.text = StringBuilderPool.Get()
                    .Append(cur).Append("/").Append(max)
                    .Release();
            }

            behind.value = progress;
            front.value = progress;
        }

        public void Tween(int cur, int max)
        {
            StopAllCoroutine();

            float oldProgress = front.value;
            float progress = MathUtils.GetProgress(cur, max);

            if (labelText)
            {
                labelText.text = StringBuilderPool.Get()
                    .Append(cur).Append("/").Append(max)
                    .Release();
            }

            // 감소되었을 경우
            if (progress < oldProgress)
            {
                front.value = progress; // 앞 ProgressBar 세팅
                Timing.RunCoroutine(YieldPlayProgress(behind, progress), myGameObject); // 뒤 ProgressBar 애니메이션
            }
            else // 증가되었을 경우
            {
                behind.value = progress; // 뒤 ProgressBar 세팅
                Timing.RunCoroutine(YieldPlayProgress(front, progress), myGameObject); // 앞 ProgressBar 애니메이션
            }
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        private void StopAllCoroutine()
        {
            Timing.KillCoroutines(myGameObject);
        }

        IEnumerator<float> YieldPlayProgress(UIProgressBar progressBar, float value)
        {
            float lastRealTime = Time.realtimeSinceStartup;
            float runningTime;
            float percentage = 0f;
            float timeRate = 1f / tweenTime;

            float startValue = progressBar.value;
            while (percentage < 1f)
            {
                runningTime = Time.realtimeSinceStartup - lastRealTime;
                percentage = runningTime * timeRate;

                progressBar.value = Mathf.Lerp(startValue, value, percentage);
                yield return Timing.WaitForOneFrame;
            }

            progressBar.value = value;
        }
    }
}