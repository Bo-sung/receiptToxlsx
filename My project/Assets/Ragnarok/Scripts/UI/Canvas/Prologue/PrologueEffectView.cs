using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class PrologueEffectView : UIView
    {
        int lightSize = 0;

        [SerializeField] float tweenWidthTime = 2;
        [SerializeField] int targetSize = 1000;

        [SerializeField] float tweenAlphaTime = 1f;
        [SerializeField] float targetAlpha = 0.9f;

        [SerializeField] GameObject backgroundAlpha;
        [SerializeField] UITexture fx_Light;

        public event System.Action OnHideView;
        
        protected override void Awake()
        {
            base.Awake();

            lightSize = fx_Light.width;
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

            fx_Light.width =  lightSize;
            TweenAlpha.Begin(backgroundAlpha, 0, 0);

            Timing.RunCoroutine(YieldFx(), gameObject);
        }

        protected override void OnLocalize()
        {
        }

        private IEnumerator<float> YieldFx()
        {
            TweenWidth.Begin(fx_Light, tweenWidthTime, targetSize);
            yield return Timing.WaitForSeconds(tweenWidthTime);

            TweenAlpha.Begin(backgroundAlpha, tweenAlphaTime, targetAlpha);
            yield return Timing.WaitForSeconds(tweenAlphaTime);

            OnHideView?.Invoke();
        }

        private void KillAllCoroutines()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}