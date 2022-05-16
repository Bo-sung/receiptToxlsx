using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UILabel))]
    public class UILoopNotice : MonoBehaviour
    {
        UILabel label;

        [SerializeField] float fadeDuration = 0.8f;
        [SerializeField] float noticeDelay = 2f;

        private BetterList<string> noticeList;
        private int noticeIndex;

        private float lastRealTime, runningTime, percentage;
        private bool isReverse;
        private bool isFade;
        private RelativeRemainTime delayRemainTime;

        void Awake()
        {
            label = GetComponent<UILabel>();
            noticeList = new BetterList<string>();
        }

        void OnEnable()
        {
            UpdateLastTime();
        }

        void Update()
        {
            if (delayRemainTime.GetRemainTime() > 0f)
                return;

            if (!isFade)
            {
                isFade = true;
                UpdateLastTime();
            }

            if (isReverse)
            {
                if (percentage > 0f)
                {
                    TweenUpdate();
                }
                else
                {
                    TweenComplete();
                }
            }
            else
            {
                if (percentage < 1f)
                {
                    TweenUpdate();
                }
                else
                {
                    TweenComplete();
                }
            }

            label.alpha = Mathf.Lerp(0f, 1f, percentage); // Alpha Update
        }

        public void Clear()
        {
            noticeList.Clear();
        }

        public void AddNotice(int localKey)
        {
            AddNotice(localKey.ToText());
        }

        public void AddNotice(string notice)
        {
            noticeList.Add(notice);
        }

        public void RemoveNotice(int localKey)
        {
            RemoveNotice(localKey.ToText());
        }

        public void RemoveNotice(string notice)
        {
            noticeList.Remove(notice);
        }

        public void Refresh()
        {
            label.text = GetNotice();
        }

        private void TweenUpdate()
        {
            runningTime += (Time.realtimeSinceStartup - lastRealTime);

            if (isReverse)
            {
                percentage = 1 - (runningTime / fadeDuration);
            }
            else
            {
                percentage = (runningTime / fadeDuration);
            }

            UpdateLastTime();
        }

        private void UpdateLastTime()
        {
            lastRealTime = Time.realtimeSinceStartup;
        }

        private void TweenComplete()
        {
            if (isReverse)
            {
                percentage = 0f;
                Next();
                Refresh(); // Show Next Notice
            }
            else
            {
                percentage = 1f;
                delayRemainTime = noticeDelay; // delay
                isFade = false;
            }

            isReverse = !isReverse;
            runningTime = 0;
        }

        private void Next()
        {
            if (++noticeIndex >= noticeList.size)
                noticeIndex = 0;
        }

        private string GetNotice()
        {
            if (noticeList.size == 0)
                return string.Empty;

            return noticeList[noticeIndex];
        }
    }
}