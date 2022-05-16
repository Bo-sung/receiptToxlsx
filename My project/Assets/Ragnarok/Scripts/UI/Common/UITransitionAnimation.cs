using AnimationOrTween;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UITransitionAnimation : TransitionComponent
    {
        [SerializeField] Animator animator;
        [SerializeField] Animation target;
        [SerializeField] string clipName;
        [SerializeField] Direction direction;
        [SerializeField] EnableCondition enableCondition;
        [SerializeField] DisableCondition disableCondition;
        [SerializeField] UIWidget widget;
        private bool isSkip;

        AdvancedActiveAnimation aaa;

        public override void Animate(bool skip)
        {
            isSkip = skip;

            // 주의! 존재할 경우 Reset을 하지 않으면 Finish 가 제대로 작동하지 않음
            if (aaa)
                aaa.Reset();

            if (animator)
            {
                aaa = AdvancedActiveAnimation.Play(animator, clipName, direction, enableCondition, disableCondition);
                if (!animator.gameObject.activeSelf)
                    isSkip = true;
            }
            else if (target)
            {
                aaa = AdvancedActiveAnimation.Play(target, clipName, direction, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
                if (!target.gameObject.activeSelf)
                    isSkip = true;
            }

            if (aaa == null)
                return;

            EventDelegate.Add(aaa.onFinished, OnFinished, true);

            if (isSkip)
            {
                aaa.Finish();

                if (widget)
                {
                    Timing.KillCoroutines(gameObject);
                    Timing.RunCoroutine(WidgetAlpha(), gameObject);
                }
            }
        }

        public override bool IsSkip()
        {
            return isSkip;
        }

        void OnFinished()
        {
            EventDelegate.Execute(onFinished);
        }

        public override void Finish()
        {
            Animate(skip: true);
        }

        IEnumerator<float> WidgetAlpha()
        {
            widget.alpha = 0.9f;
            yield return Timing.WaitForOneFrame;
            widget.alpha = 1f;
        }

        public override bool IsPlaying()
        {
            return aaa == null ? false : aaa.isPlaying;
        }
    }
}