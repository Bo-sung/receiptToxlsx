using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UnitTweenScale : MonoBehaviour
    {
        [SerializeField] Vector3 start = Vector3.one;
        [SerializeField] Vector3 end = Vector3.one * 4f;
        [SerializeField] float duration = 2f;

        GameObject myGameObject;
        Transform myTransform;

        void Awake()
        {
            myGameObject = gameObject;
            myTransform = transform;
        }

        void OnDestroy()
        {
            Timing.KillCoroutines(myGameObject);
        }

        public void Set(Vector3 start, Vector3 end, float duration = 2f)
        {
            this.start = start;
            this.end = end;
            this.duration = duration;
        }

        public void Play()
        {
            Timing.RunCoroutine(YieldPlay(), myGameObject);
        }

        IEnumerator<float> YieldPlay()
        {
            myTransform.localScale = start;

            float lastRealTime = Time.realtimeSinceStartup;
            float runningTime = 0f;
            float percentage = 0f;
            float timeRate = 1f / duration;

            while (percentage < 1f)
            {
                runningTime = Time.realtimeSinceStartup - lastRealTime;
                percentage = runningTime * timeRate;
                myTransform.localScale = Vector3.Lerp(start, end, percentage);
                yield return Timing.WaitForOneFrame;
            }

            myTransform.localScale = end;
        }
    }
}