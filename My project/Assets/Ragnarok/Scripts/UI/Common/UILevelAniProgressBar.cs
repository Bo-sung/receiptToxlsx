using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// 잔상이 남는 효과 (순환 가능)
    /// </summary>
    public class UILevelAniProgressBar : MonoBehaviour, IAutoInspectorFinder
    {
        public delegate void LevelEvent(int level, int cur, int max);

        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabel labelText;

        private int[] maxValues;

        /// <summary>
        /// 레벨과 같은 개념
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// 현재 포인트
        /// </summary>
        public int Cur { get; private set; }

        /// <summary>
        /// 최대 포인트
        /// </summary>
        public int Max { get; private set; }

        /// <summary>
        /// 최대 레벨
        /// </summary>
        public int MaxLevel => GetMaxLevel();

        public event LevelEvent OnUpdateLevel;

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

        public void Set(int value, int maxValue, int maxLevel)
        {
            InitMaxValues(maxValue, maxLevel);
            Set(value);
        }

        public void Set(int value, int[] maxValues)
        {
            this.maxValues = maxValues;
            Set(value);
        }

        public void Set(int value)
        {
            StopAllCoroutine();

            Refresh(value);
            float progress = MathUtils.GetProgress(Cur, Max);
            progressBar.value = progress;
        }

        public void Tween(int value, int maxValue, int maxLevel)
        {
            InitMaxValues(maxValue, maxLevel);
            Tween(value);
        }

        public void Tween(int value, int[] maxValues)
        {
            this.maxValues = maxValues;
            Tween(value);
        }

        public void Tween(int value)
        {
            StopAllCoroutine();

            float oldProgress = MathUtils.GetProgress(Cur, Max);
            progressBar.value = oldProgress;

            bool isOldMaxLevel = Cur == Max;
            int oldLevel = Level;
            Refresh(value);

            float progress = MathUtils.GetProgress(Cur, Max);

            if (Level < oldLevel) // 레벨 감소
            {
                if (oldLevel - Level > 1) // 1 이상 차이
                {
                    progress -= 1f;
                }
                else
                {
                    if (Cur == 0)
                    {
                        // Do Nothing
                    }
                    else if (isOldMaxLevel)
                    {
                        // Do Nothing (이전 최대 레벨 달성)
                    }
                    else
                    {
                        progress -= 1f;
                    }
                }
            }
            else if (Level > oldLevel) // 레벨이 증가되었을 경우
            {
                if (Level - oldLevel > 1) // 1 이상 차이
                {
                    progress += 1f;
                }
                else
                {
                    if (Cur == Max)
                    {
                        // Do Nothing (최대 레벨 달성)
                    }
                    else
                    {
                        progress += 1f;
                    }
                }
            }

            Timing.RunCoroutine(YieldPlayProgress(progress), myGameObject); // 뒤 ProgressBar 애니메이션
        }

        private void InitMaxValues(int maxValue, int maxLevel)
        {
            int level = Mathf.Max(1, maxLevel); // level 은 1 이상

            maxValues = new int[level];
            for (int i = 0; i < maxValues.Length; i++)
            {
                maxValues[i] = maxValue;
            }
        }

        private int GetMaxLevel()
        {
            int length = maxValues == null ? 0 : maxValues.Length;
            return Mathf.Max(1, length);
        }

        private void Refresh(int value)
        {
            int length = maxValues == null ? 0 : maxValues.Length;

            int level = 0;
            int max = 0;
            for (int i = 0; i < length; i++)
            {
                max = maxValues[i];

                // max를 넘지 못함
                if (value < max)
                    break;

                ++level; // 레벨 증가

                // 최대 레벨 도달
                if (level == MaxLevel)
                    break;

                value -= max; // 레벨에 해당하는 cur로 변경
            }

            Level = level;
            Cur = value;
            Max = max;

            if (labelText)
            {
                labelText.text = StringBuilderPool.Get()
                    .Append(Cur).Append("/").Append(Max)
                    .Release();
            }

            OnUpdateLevel?.Invoke(Level, Cur, Max);
        }

        private void SetActive(bool isActive)
        {
            NGUITools.SetActive(myGameObject, isActive);
        }

        private void StopAllCoroutine()
        {
            Timing.KillCoroutines(myGameObject);
        }

        IEnumerator<float> YieldPlayProgress(float value)
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
                progressBar.value = GetProgress01(Mathf.Lerp(startValue, value, percentage));
                yield return Timing.WaitForOneFrame;
            }

            progressBar.value = GetProgress01(MathUtils.GetProgress(Cur, Max));
        }

        private float GetProgress01(float value)
        {
            if (value < 0f)
                return value + 1f;

            if (value > 1f)
                return value - 1f;

            return value;
        }
    }
}