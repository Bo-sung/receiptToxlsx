using UnityEngine;

namespace Ragnarok
{

    public class PanelBuffEffect : PoolObject
    {
        [SerializeField] UIPlayTween tweenPlay;
        [SerializeField] TweenScale tweenScale_appear;
        [SerializeField] TweenScale tweenScale_disappear;

        [SerializeField, Rename(displayName = "등장 시간")]
        float appearDuration = 0.5f;

        [SerializeField, Rename(displayName = "등장 후 대기 시간")]
        float waitDuration = 0.5f;

        [SerializeField, Rename(displayName = "사라지는 시간")]
        float disappearDuration = 0.5f;

        [SerializeField, Rename(displayName = "사라지는 크기")]
        Vector3 disappearScale = new Vector3(0f, 3f, 0f);

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(tweenScale_disappear.onFinished, Release);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(tweenScale_disappear.onFinished, Release);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            // 세팅 후 연출 재생
            tweenScale_appear.duration = appearDuration;

            tweenScale_disappear.delay = appearDuration + waitDuration;
            tweenScale_disappear.duration = disappearDuration;
            tweenScale_disappear.to = disappearScale;

            tweenPlay.Play();
        }

        private void LateUpdate()
        {
            CachedTransform.eulerAngles = Vector3.zero;
        }
    }
}