using UnityEngine;

namespace Ragnarok
{
    public abstract class PlayTweenPoolObject : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIPlayTween tween;

        public event System.Action<PlayTweenPoolObject> OnFinish;

        Transform myTransform;

        protected virtual void Awake()
        {
            myTransform = transform;
            EventDelegate.Add(tween.onFinished, OnFinishedTween);
        }

        protected virtual void OnDestroy()
        {
            EventDelegate.Remove(tween.onFinished, OnFinishedTween);
        }

        public void ResetDelay(PlayTweenPoolObject original)
        {
            UITweener[] originalTweens = original.tween.tweenTarget.GetComponentsInChildren<UITweener>();
            UITweener[] tweens = tween.tweenTarget.GetComponentsInChildren<UITweener>();

            if (originalTweens.Length != tweens.Length)
                return;

            for (int i = 0; i < originalTweens.Length; i++)
            {
                tweens[i].delay = originalTweens[i].delay;
            }
        }

        public void Launch()
        {
            tween.Play();
        }

        public abstract PlayTweenPoolObject Initialize(string itemIcon);

        public abstract PlayTweenPoolObject Initialize(string itemIcon, int itemCount);

        public PlayTweenPoolObject SetPosition(Vector3 startPos, Vector3 endPos)
        {
            return SetPosition(startPos, endPos, 0f);
        }

        public PlayTweenPoolObject SetPosition(Vector3 startPos, Vector3 endPos, float randRadius)
        {
            Vector3 from = myTransform.InverseTransformPoint(startPos);
            Vector3 to = myTransform.InverseTransformPoint(endPos); // 로컬 Vector로 변환

            if (randRadius > 0f)
                from += GetRandomPos(randRadius);

            return SetTweenPosition(from, to);
        }

        public PlayTweenPoolObject AddDelay(float delay)
        {
            UITweener[] tweens = tween.includeChildren ? tween.tweenTarget.GetComponentsInChildren<UITweener>() : tween.tweenTarget.GetComponents<UITweener>();
            foreach (var item in tweens)
            {
                if (item.tweenGroup == tween.tweenGroup)
                    item.delay += delay;
            }

            return this;
        }

        public void SetActive(bool isActive)
        {
            NGUITools.SetActive(tween.tweenTarget, isActive);
        }

        private Vector3 GetRandomPos(float randRadius)
        {
            if (randRadius == 0f)
                return Vector3.zero;

            float rad360 = Mathf.PI * 2;
            float startRad = Random.Range(0, rad360); // 시작 각도
            float radius = Random.Range(0, randRadius); // 반지름
            float x = Mathf.Sin(startRad) * radius;
            float y = Mathf.Cos(startRad) * radius;
            return new Vector2(x, y);
        }

        private PlayTweenPoolObject SetTweenPosition(Vector3 from, Vector3 to)
        {
            TweenPosition[] tweenPositions = tween.includeChildren ? tween.tweenTarget.GetComponentsInChildren<TweenPosition>() : tween.tweenTarget.GetComponents<TweenPosition>();
            foreach (var item in tweenPositions)
            {
                if (item.tweenGroup == tween.tweenGroup)
                {
                    item.from = from; // 로컬 Vector로 변환
                    item.to = to; // 로컬 Vector로 변환
                }
            }

            return this;
        }

        void OnFinishedTween()
        {
            OnFinish?.Invoke(this);
        }
    }
}