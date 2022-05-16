using AnimationOrTween;
using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleIntro : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] Animator animationBase;
        [SerializeField] BattleIntroView battleIntroView;

        public event System.Action OnFinished;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {

        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Show(UICharacterIntroduction.IInput input1, UICharacterIntroduction.IInput input2)
        {
            Show();

            battleIntroView.SetData(input1, input2);
            Play();
        }

        private void Play()
        {
            const string CLIP_NAME = "UI_DuelMatch[IN]"; // 재생 이름
            const Direction PLAY_DIRECTION = Direction.Forward; // 정방향 플레이
            const EnableCondition ENABLE_BEFORE_PLAY = EnableCondition.EnableThenPlay;
            const DisableCondition DISABLE_CONDITION = DisableCondition.DoNotDisable;

            ActiveAnimation aa = ActiveAnimation.Play(animationBase, CLIP_NAME, PLAY_DIRECTION, ENABLE_BEFORE_PLAY, DISABLE_CONDITION);
            EventDelegate.Add(aa.onFinished, OnFinishedAnimation);
        }

        private void OnFinishedAnimation()
        {
            OnFinished?.Invoke();
            Hide();
        }
    }
}