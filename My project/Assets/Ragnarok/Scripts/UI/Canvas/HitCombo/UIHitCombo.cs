using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIHitCombo : UICanvas<HitComboPresenter>
        , HitComboPresenter.IView
        , IInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UILabel labelComboCount;
        [SerializeField] Animation hitCombo;
        [SerializeField] UIWidget widget, effect;

        CoroutineHandle fadeOutHandle, showEffectHandle;

        protected override void OnInit()
        {
            presenter = new HitComboPresenter(this);
            presenter.AddEvent();
            effect.alpha = 0f;
        }

        protected override void OnClose()
        {
            Timing.KillCoroutines(gameObject);

            presenter.RemoveEvent();
            if (presenter != null)
                presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            Hide();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void HitComboPresenter.IView.Show(int combo)
        {
            if (gameObject == null)
                return;

            gameObject.SetActive(true);

            Timing.KillCoroutines(fadeOutHandle);

            if (combo > 0)
            {
                widget.alpha = 1f;

                labelComboCount.text = combo.ToString();
                hitCombo.Play();

                if (combo % 10 == 0)
                {
                    Timing.KillCoroutines(showEffectHandle);
                    showEffectHandle = Timing.RunCoroutine(YieldShowEffect(1.0f), gameObject);
                }
            }
            else
            {
                fadeOutHandle = Timing.RunCoroutine(YieldFadeOut(0.5f), gameObject);
            }
        }

        IEnumerator<float> YieldFadeOut(float duration)
        {
            float lastRealTime = Time.realtimeSinceStartup;
            float runningTime = 0f;
            float percentage = 0f;
            float timeRate = 1f / duration;

            while (percentage < 1f)
            {
                runningTime = Time.realtimeSinceStartup - lastRealTime;
                percentage = runningTime * timeRate;
                widget.alpha = Mathf.Lerp(1f, 0f, percentage);
                yield return Timing.WaitForOneFrame;
            }

            widget.alpha = 0f;
        }

        IEnumerator<float> YieldShowEffect(float duration)
        {
            float lastRealTime = Time.realtimeSinceStartup;
            float runningTime = 0f;
            float percentage = 0f;
            float timeRate = 1f / duration;

            while (percentage < 1f)
            {
                runningTime = Time.realtimeSinceStartup - lastRealTime;
                percentage = runningTime * timeRate;
                effect.alpha = Mathf.Lerp(1f, 0f, percentage);
                yield return Timing.WaitForOneFrame;
            }

            effect.alpha = 0f;
        }

        bool IInspectorFinder.Find()
        {
            hitCombo = GetComponentInChildren<Animation>();
            widget = GetComponent<UIWidget>();
            return true;
        }
    }
}