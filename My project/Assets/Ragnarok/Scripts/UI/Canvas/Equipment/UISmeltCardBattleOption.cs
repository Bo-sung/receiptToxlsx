using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UISmeltCardBattleOption : MonoBehaviour, IAutoInspectorFinder, IInspectorFinder
    {
        [SerializeField] UILabelHelper labelOptionName;
        [SerializeField] UILabelHelper labelOptionValue;
        [SerializeField] UILabelHelper labelValue;
        [SerializeField] GameObject fxSlot;
        [SerializeField] AnimationCurve curve;
        [SerializeField] float duration;
        [SerializeField] TweenAlpha tweenAlpha;
        [SerializeField] float alphaDuration = 0.9f;
        private CardBattleOption option;

        public void Set(string title, string optionValue, string value)
        {
            if (tweenAlpha)
            {
                tweenAlpha.enabled = false;
                tweenAlpha.value = 1;
            }

            labelOptionName.Text = title;
            labelOptionValue.Text = optionValue;
            labelValue.SetActive(!string.IsNullOrEmpty(value));
            labelValue.Text = value;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SetFx(bool isActive)
        {
            if (fxSlot)
                fxSlot.SetActive(isActive);
        }

        public IEnumerator<float> SetValueFx(CardBattleOption before, CardBattleOption after)
        {
            option = before;

            long startValue = before.GetValue();
            long increaseValue = after.GetValue();
            long endValue = before.GetValue() + increaseValue;

            if (before.battleOptionType.IsConditionalSkill())
            {
                SetValue(startValue, 0);
                yield break;
            }

            SetValue(startValue, increaseValue);
            tweenAlpha.value = 1;
            tweenAlpha.enabled = true;
            yield return Timing.WaitForSeconds(alphaDuration);

            //Debug.LogError($"{before.GetTitleText()} startValue={startValue}, endValue={endValue}, increaseValue={increaseValue}");
            tweenAlpha.enabled = false;
            tweenAlpha.value = 1;
            Timing.RunCoroutine(YieldFx(startValue, endValue, increaseValue, SetValue), gameObject);
        }

        IEnumerator<float> YieldFx(long min, long max, long increase, Action<long, long> OnChangeValue)
        {
            float normalizedTime = 0;
            float time = 0;
            while (normalizedTime < 1f)
            {
                normalizedTime = Mathf.Clamp01(time / duration);
                float t = curve.Evaluate(normalizedTime);
                int optionValue = Mathf.RoundToInt(Mathf.Lerp(min, max, t));
                int value = Mathf.CeilToInt(Mathf.Lerp(increase, 0, t));
                //Debug.LogError($"{option.GetTitleText()}, {optionValue}, {value}");
                OnChangeValue?.Invoke(optionValue, value);
                time += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }
        }

        void SetValue(long optionValue, long value)
        {
            labelOptionValue.Text = option.GetValeText(optionValue, true);
            labelValue.SetActive(value != 0);
            labelValue.Text = option.GetValeText(value, true);
        }

        bool IInspectorFinder.Find()
        {
#if UNITY_EDITOR
            transform.name = transform.GetSiblingIndex().ToString();
#endif
            return true;
        }
    }
}