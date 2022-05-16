using UnityEngine;

namespace Ragnarok
{
    public class TweenPositionAdvanced : TweenPosition
    {
        [HideInInspector]
        public AnimationCurve animationCurveY = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

        protected override void OnUpdate(float factor, bool isFinished)
        {
            float x = GetValue(from.x, to.x, animationCurve.Evaluate(factor));
            float y = GetValue(from.y, to.y, animationCurveY.Evaluate(factor));

            value = new Vector3(x, y);
        }

        private float GetValue(float from, float to, float factor)
        {
            return from * (1f - factor) + to * factor;
        }
    }
}