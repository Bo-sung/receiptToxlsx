using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIFade : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField]
        [HideInInspector] // Editor 에서 따로 처리
        AnimationCurve curve_fadeOver = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField]
        [HideInInspector] // Editor 에서 따로 처리
        AnimationCurve curve_fadeOut = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField] UITextureHelper texture;
        [SerializeField] GameObject goActive;

        [SerializeField] GameObject tipActive;
        [SerializeField] UIWidget tipActiveWidget;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelTip;
        [SerializeField] Texture2D loadingTipBack;

        FadePresenter presenter;

        protected override void OnInit()
        {
            presenter = new FadePresenter();
            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            StopAllCoroutine();
        }

        protected override void OnShow(IUIData data = null)
        {
            StopAllCoroutine();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._3300; // [6EAFDD]TIP[-]
        }

        private void SetLoadingTip()
        {
            bool isTextureTip = presenter.IsTextureTip();

            if (isTextureTip)
            {
                texture.SetLoadingTip(presenter.GetRandomTipTextureName(), false);
                tipActive.SetActive(false);
            }
            else
            {
                texture.SetTexture(loadingTipBack, loadingTipBack.name);
                tipActive.SetActive(true);
                labelTip.Text = presenter.GetRandomTipText();
            }
        }

        public void PlayFadeIn(System.Action onFinished, float duration = 0.3f)
        {
            Timing.RunCoroutine(YieldFadeIn(duration)
                .Append(onFinished), gameObject);
        }

        public void PlayFadeOut(System.Action onFinished = null, float duration = 0.3f)
        {
            Timing.RunCoroutine(YieldFadeOut(duration)
                .Append(CloseUI)
                .Append(onFinished), gameObject);
        }

        public void PlayFadeInOut(System.Action onFadeIn, System.Action onFadeOut = null, float duration = 0.3f)
        {
            Timing.RunCoroutine(YieldFadeIn(duration)
                .Append(onFadeIn)
                .Append(YieldFadeOut(duration))
                .Append(CloseUI)
                .Append(onFadeOut), gameObject);
        }

        /// <summary>
        /// 페이드 인
        /// </summary>
        public IEnumerator<float> YieldFadeIn(float duration = 0.3f)
        {
            SetLoadingTip();

            goActive.SetActive(true);

            if (duration == 0f)
            {
                SetTexture("_Fade", 1f);
                SetTexture("_Reverse", 2f);
                tipActiveWidget.alpha = 1f;
                yield break;
            }

            SetTexture("_Fade", -1f);
            SetTexture("_Reverse", 2f);
            tipActiveWidget.alpha = 0f;

            float elapsedTime = 0f;
            while (true)
            {
                elapsedTime += Time.deltaTime;

                float timeRate = Mathf.Clamp01(elapsedTime / duration);
                float curveValue = curve_fadeOver.Evaluate(timeRate);
                float value = Mathf.Lerp(-1f, 1f, curveValue);
                SetTexture("_Fade", value);
                float alpha = Mathf.Lerp(0f, 1f, curveValue);
                tipActiveWidget.alpha = alpha;

                if (timeRate == 1f)
                    break;

                yield return Timing.WaitForOneFrame;
            }
        }

        /// <summary>
        /// 페이드 아웃
        /// </summary>
        IEnumerator<float> YieldFadeOut(float duration)
        {
            SetTexture("_Fade", 1f);
            SetTexture("_Reverse", 2f);
            tipActiveWidget.alpha = 1f;

            float elapsedTime = 0f;
            while (true)
            {
                elapsedTime += Time.deltaTime;

                float timeRate = Mathf.Clamp01(elapsedTime / duration);
                float curveValue = curve_fadeOut.Evaluate(timeRate);
                float value = Mathf.Lerp(2f, 0f, curveValue);
                SetTexture("_Reverse", value);
                float alpha = Mathf.Lerp(1f, 0f, curveValue);
                tipActiveWidget.alpha = alpha;

                if (timeRate == 1f)
                    break;

                yield return Timing.WaitForOneFrame;
            }

            goActive.SetActive(false);
        }

        private void CloseUI()
        {
            Hide();
        }

        private void SetTexture(string propertyName, float value)
        {
            texture.material.SetFloat(propertyName, value);
            texture.cachedGameObject.SetActive(false);
            texture.cachedGameObject.SetActive(true);
        }

        private void StopAllCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}