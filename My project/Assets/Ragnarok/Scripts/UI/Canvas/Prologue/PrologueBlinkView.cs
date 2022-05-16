using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class PrologueBlinkView : UIView
    {
        [Serializable]
        enum BlinkState
        {
            wait,
            open,
            close,
        }

        [Serializable]
        struct BlinkStatus
        {
            public BlinkState state;
            public float waitTime;
            public float heightValue;
            public float tweenSpeed;

            /// <summary>대기중일때</summary>
            public BlinkStatus(BlinkState state, float waitTime)
            {
                this.state = state;
                this.waitTime = waitTime;
                this.heightValue = default;
                this.tweenSpeed = default;
            }

            /// <summary>트윈사용</summary>
            public BlinkStatus(BlinkState state, float heightValue, float tweenSpeed)
            {
                this.state = state;
                this.waitTime = default;
                this.heightValue = heightValue;
                this.tweenSpeed = tweenSpeed;
            }
        }

        [SerializeField] int closeHeight = 20;
        [SerializeField] float closeSpeed = 0.2f;
        [SerializeField] float maxBGAlpha = 0.3f;
        [SerializeField] float fadeOutSpeed = 0.02f;
        
        [SerializeField] UISprite backgroundAlpha;
        [SerializeField] UISprite backgroundBlink;

        [SerializeField] UITweener.Method openTweenMethod = UITweener.Method.EaseOut;
        [SerializeField] UITweener.Method closeTweenMethod = UITweener.Method.EaseOut;

        [SerializeField] List<BlinkStatus> blinkStatusList = new List<BlinkStatus>();
        TweenHeight tweenHeight;

        public event System.Action OnHideView;

        float maxWidth;
        float maxHeight;
        
        float tempAlpha = 0;
        float BgAlpha
        {
            get { return tempAlpha; }
            set
            {
                tempAlpha = value;
                if (tempAlpha < 0) tempAlpha = 0;
                if (tempAlpha > maxBGAlpha) tempAlpha = maxBGAlpha;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            InitBlinkList();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            KillAllCoroutines();
        }

        public override void Hide()
        {
            base.Hide();

            KillAllCoroutines();
        }

        public override void Show()
        {
            base.Show();
        
            KillAllCoroutines();
            
            backgroundBlink.height = closeHeight;

            TweenAlpha.Begin(backgroundBlink.cachedGameObject, 0, 1);
            TweenAlpha.Begin(backgroundAlpha.cachedGameObject, 0, 1);

            Timing.RunCoroutine(YieldBlink(), gameObject);
        }

        protected override void OnLocalize()
        {
        }

        private void InitBlinkList()
        {
            // 깜빡임 리스트 셋팅
            blinkStatusList.Clear();

            blinkStatusList.Add(new BlinkStatus(BlinkState.open, 0.5f, 0.4f));
            blinkStatusList.Add(new BlinkStatus(BlinkState.wait, 0.1f));
            blinkStatusList.Add(new BlinkStatus(BlinkState.close, default, default));
            blinkStatusList.Add(new BlinkStatus(BlinkState.open, 0.5f, 0.4f));
            blinkStatusList.Add(new BlinkStatus(BlinkState.wait, 0.5f));
            blinkStatusList.Add(new BlinkStatus(BlinkState.close, default, default));
            blinkStatusList.Add(new BlinkStatus(BlinkState.open, 0.5f, 0.3f));
            blinkStatusList.Add(new BlinkStatus(BlinkState.wait, 0.1f));
            blinkStatusList.Add(new BlinkStatus(BlinkState.close, default, default));
            blinkStatusList.Add(new BlinkStatus(BlinkState.open, 1f, 0.3f));
        }

        private IEnumerator<float> YieldBlink()
        {
            yield return Timing.WaitForOneFrame;

            maxWidth = backgroundAlpha.width * 1.5f;
            maxHeight = backgroundAlpha.height;
            backgroundBlink.width = (int)maxWidth;

            for (int i = 0; i < blinkStatusList.Count; i++)
            {
                switch (blinkStatusList[i].state)
                {
                    case BlinkState.open:
                        int tempHeight;
                        if (blinkStatusList[i].heightValue < 1) tempHeight = (int)(blinkStatusList[i].heightValue * maxWidth);
                        else tempHeight = (int)maxHeight;
                        BgAlpha = 1 - blinkStatusList[i].heightValue;

                        // open
                        PlayTweenHeight(backgroundBlink, blinkStatusList[i].tweenSpeed, tempHeight, openTweenMethod);
                        TweenAlpha.Begin(backgroundAlpha.cachedGameObject, blinkStatusList[i].tweenSpeed, BgAlpha);
                        yield return Timing.WaitForSeconds(blinkStatusList[i].tweenSpeed);
                        break;

                    case BlinkState.close:
                        PlayTweenHeight(backgroundBlink, closeSpeed, closeHeight, closeTweenMethod);
                        TweenAlpha.Begin(backgroundAlpha.cachedGameObject, closeSpeed, 1);
                        yield return Timing.WaitForSeconds(closeSpeed);
                        break;

                    //case BlinkState.wait:
                    default:
                        yield return Timing.WaitForSeconds(blinkStatusList[i].waitTime);
                        break;
                }
            }

            // 깜빡임 제거
            TweenAlpha.Begin(backgroundAlpha.cachedGameObject, fadeOutSpeed, 0);
            TweenAlpha.Begin(backgroundBlink.cachedGameObject, fadeOutSpeed, 0);
            yield return Timing.WaitForSeconds(fadeOutSpeed);

            OnHideView?.Invoke();
        }

        private void PlayTweenHeight(UIWidget widget, float duration, int height, UITweener.Method method)
        {
            tweenHeight = TweenHeight.Begin(widget, duration, height);
            tweenHeight.method = method;
            tweenHeight.animationCurve = null;
            tweenHeight.PlayForward();
        }

        private void KillAllCoroutines()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}