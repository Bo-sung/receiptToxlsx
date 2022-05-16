using UnityEngine;

namespace Ragnarok
{
    public class UIScrollWrapContent : UIWrapContent, IInspectorFinder
    {
        [SerializeField] UIPlayTween tween;
        [SerializeField] TweenSpringPosition tweenPos;

        public event System.Action OnFinishedPlay;

        protected virtual void Awake()
        {
            EventDelegate.Add(tween.onFinished, Finished);
        }

        protected virtual void OnDestroy()
        {
            EventDelegate.Remove(tween.onFinished, Finished);
        }

        /// <summary>
        /// 스크롤 재생
        /// </summary>
        public void PlayScroll(int index)
        {
            ResetPosition();
            Play(index);
        }

        /// <summary>
        /// 위치 초기화
        /// </summary>
        public void ResetPosition()
        {
            if (!CacheScrollView())
                return;

            mScroll.ResetPosition();
            ResetChildPositions();
        }

        private void Play(int index)
        {
            int scrollIndex = Mathf.Abs(index);
            if (scrollIndex == 0)
            {
                Finished();
                return;
            }

            tweenPos.to = Vector3.down * itemSize * index;
            tween.Play();
        }

        private void Finished()
        {
            OnFinishedPlay?.Invoke();
        }

        bool IInspectorFinder.Find()
        {
            CacheScrollView();

            tween = mPanel.GetComponent<UIPlayTween>();
            tweenPos = mPanel.GetComponent<TweenSpringPosition>();
            return true;
        }
    }
}