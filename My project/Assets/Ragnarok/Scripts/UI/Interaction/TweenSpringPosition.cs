using UnityEngine;

namespace Ragnarok
{
    public class TweenSpringPosition : UITweener
    {
        public Vector3 from;
        public Vector3 to;

        [HideInInspector]
        public bool worldSpace = false;

        public bool updateScrollView = false;

        Transform mTrans;
        UIScrollView mSv;

        public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

        /// <summary>
        /// Tween's current value.
        /// </summary>
        public Vector3 value
        {
            get
            {
                return worldSpace ? cachedTransform.position : cachedTransform.localPosition;
            }
            set
            {
                if (worldSpace)
                {
                    cachedTransform.position = value;
                }
                else
                {
                    cachedTransform.localPosition = value;
                }
            }
        }

        void Awake()
        {
            if (updateScrollView)
            {
                mSv = NGUITools.FindInParents<UIScrollView>(gameObject);
            }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from * (1f - factor) + to * factor;

            if (mSv != null)
            {
                mSv.UpdateScrollbars(true);
            }
        }

        static public TweenSpringPosition Begin(GameObject go, float duration, Vector3 pos)
        {
            TweenSpringPosition comp = UITweener.Begin<TweenSpringPosition>(go, duration);
            comp.from = comp.value;
            comp.to = pos;

            if (duration <= 0f)
            {
                comp.Sample(1f, true);
                comp.enabled = false;
            }
            return comp;
        }

        static public TweenSpringPosition Begin(GameObject go, float duration, Vector3 pos, bool worldSpace)
        {
            TweenSpringPosition comp = UITweener.Begin<TweenSpringPosition>(go, duration);
            comp.worldSpace = worldSpace;
            comp.from = comp.value;
            comp.to = pos;

            if (duration <= 0f)
            {
                comp.Sample(1f, true);
                comp.enabled = false;
            }
            return comp;
        }

        [ContextMenu("Set 'From' to current value")]
        public override void SetStartToCurrentValue()
        {
            from = value;
        }

        [ContextMenu("Set 'To' to current value")]
        public override void SetEndToCurrentValue()
        {
            to = value;
        }

        [ContextMenu("Assume value of 'From'")]
        void SetCurrentValueToStart()
        {
            value = from;
        }

        [ContextMenu("Assume value of 'To'")]
        void SetCurrentValueToEnd()
        {
            value = to;
        }
    }
}