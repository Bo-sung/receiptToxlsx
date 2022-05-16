using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class CardSmeltProgressView : UIView
    {
        [SerializeField] float duration = 1f; // 다음 연속 제련 대기시간
        [SerializeField] UIButtonHelper btnStopContinuousSmelt;
        [SerializeField] UISlider slider;
        [SerializeField] UILabelHelper labelSlider;
        [SerializeField] GameObject fxSlider;

        public event System.Action OnStopSmelt;
        public event System.Action OnFinishProgress;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnStopContinuousSmelt.OnClick, OnClickedBtnStopSmelt);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnStopContinuousSmelt.OnClick, OnClickedBtnStopSmelt);
        }

        protected override void OnLocalize()
        {
            labelSlider.LocalKey = LocalizeKey._18513; // 연속 제련 진행 중...
            btnStopContinuousSmelt.LocalKey = LocalizeKey._18514; // 연속 제련 중단
        }

        public override void Show()
        {
            base.Show();           
        }

        public void StartWaitSmelt()
        {
            Timing.RunCoroutine(YieldWaitSmelt(), gameObject);
        }

        IEnumerator<float> YieldWaitSmelt()
        {
            fxSlider.SetActive(true);
            float time = 0;
            while (time <= duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                slider.value = t;
                yield return Timing.WaitForOneFrame;
            }
            OnFinishProgress?.Invoke();
        }

        void OnClickedBtnStopSmelt()
        {
            OnStopSmelt?.Invoke();
        }
    }
}