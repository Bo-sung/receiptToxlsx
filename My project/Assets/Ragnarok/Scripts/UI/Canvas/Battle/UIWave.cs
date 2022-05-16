using AnimationOrTween;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIWave : UICanvas, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabel labelWave;
        [SerializeField] Animation waveAlarm;

        public event System.Action OnFinished;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data)
        {
            NGUITools.SetActive(waveAlarm.gameObject, false); // 시작과 동시에 Hide
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Show(int wave, int maxWave)
        {
            labelWave.text = LocalizeKey._37001.ToText() // Wave {VALUE}/{MAX}
                .Replace(ReplaceKey.VALUE, wave)
                .Replace(ReplaceKey.MAX, maxWave);

            const string CLIP_NAME = "WaveAlarm"; // 재생 이름
            const Direction PLAY_DIRECTION = Direction.Forward; // 정방향 플레이
            const EnableCondition ENABLE_BEFORE_PLAY = EnableCondition.EnableThenPlay; // Enable 후에 플레이
            const DisableCondition DISABLE_CONDITION = DisableCondition.DisableAfterForward; // 정방향-플레이 후에 Disable

            ActiveAnimation aa = ActiveAnimation.Play(waveAlarm, CLIP_NAME, PLAY_DIRECTION, ENABLE_BEFORE_PLAY, DISABLE_CONDITION);
            EventDelegate.Add(aa.onFinished, OnFinishedAnimation);
        }

        void OnFinishedAnimation()
        {
            OnFinished?.Invoke();
        }

        bool IInspectorFinder.Find()
        {
            waveAlarm = GetComponent<Animation>();
            return true;
        }
    }
}