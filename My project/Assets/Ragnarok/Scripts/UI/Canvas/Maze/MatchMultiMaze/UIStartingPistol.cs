using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIStartingPistol : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UILabel labelCountDown;
        [SerializeField] GameObject root;
        [SerializeField] Transform scale;
        [SerializeField] UITweener tween;

        private enum State
        {
            None,
            Standby,
            Ready,
            Finish,
        }

        public event System.Action OnFinish;

        private State state;
        private RelativeRemainTime remainTime;
        private int countdown;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            countdown = -1;
            SetState(State.Standby);
        }

        protected override void OnHide()
        {
            SetState(State.None);
        }

        protected override void OnLocalize()
        {
        }

        void Update()
        {
            if (state == State.None || state == State.Standby)
                return;

            UpdateTime();
        }

        public void Run(float remainTime)
        {
            Show();

            this.remainTime = remainTime;
            SetState(State.Ready);
            UpdateTime();
        }

        private void SetState(State state)
        {
            if (this.state == state)
                return;

            this.state = state;
            StopAllCoroutine();

            switch (state)
            {
                case State.Standby:
                    tween.Finish();
                    NGUITools.SetActive(root, false);
                    break;

                case State.Ready:
                    NGUITools.SetActive(root, true);
                    root.transform.localScale = Vector3.one;
                    scale.localScale = Vector3.one;
                    break;

                case State.Finish:
                    Timing.RunCoroutine(YieldFinish(), gameObject);
                    break;
            }
        }

        private void UpdateTime()
        {
            float time = remainTime.GetRemainTime();
            if (time <= 0f)
            {
                SetState(State.Finish);
                return;
            }

            int countdown = Mathf.CeilToInt(time);
            SetCountdown(countdown);
        }

        private void SetCountdown(int countdown)
        {
            if (this.countdown == countdown)
                return;

            this.countdown = countdown;
            labelCountDown.text = this.countdown.ToString();

            tween.ResetToBeginning();
            tween.PlayForward();
        }

        private IEnumerator<float> YieldFinish()
        {
            float timer = 0;
            float animProg = 0;

            while (animProg < 1)
            {
                timer += Time.deltaTime;
                animProg = Mathf.Clamp01(timer / 0.2f);
                root.transform.localScale = new Vector3(1 - animProg, 1 - animProg, 1);
                yield return 0f;
            }

            OnFinish?.Invoke();

            Hide();
        }

        private void StopAllCoroutine()
        {
            Timing.KillCoroutines(gameObject);
        }
    }
}