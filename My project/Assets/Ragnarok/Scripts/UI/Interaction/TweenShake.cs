using UnityEngine;

namespace Ragnarok
{
    public class TweenShake : UITweener
    {
        public Vector3 center;
        public float power = 4f;

        Transform mTrans;
        UIRect mRect;

        public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

        /// <summary>
        /// Tween's current value.
        /// </summary>
        public Vector3 value
        {
            get
            {
                return cachedTransform.localPosition;
            }
            set
            {
                if (mRect == null || !mRect.isAnchored)
                {
                    cachedTransform.localPosition = value;
                }
                else
                {
                    value -= cachedTransform.localPosition;
                    NGUIMath.MoveRect(mRect, value.x, value.y);
                }
            }
        }

        void Awake() { mRect = GetComponent<UIRect>(); }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            float currentPower = power * (1f - factor);
            value = new Vector3(center.x + GetRandomValue(currentPower), center.y + GetRandomValue(currentPower));
        }

        private float GetRandomValue(float input)
        {
            return Random.Range(-input, input);
        }

        [ContextMenu("Set 'Center' to current value")]
        public override void SetStartToCurrentValue()
        {
            center = value;
        }
    }
}