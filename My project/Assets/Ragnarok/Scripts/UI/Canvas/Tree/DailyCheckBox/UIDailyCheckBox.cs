using AnimationOrTween;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDailyCheckBox : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Destroy;

        [SerializeField] UIEventTrigger blind;
        [SerializeField] Animator animationBase;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] GameObject completeEffect;
        [SerializeField] UILabelHelper labelMessage;
        [SerializeField] float closeDelay = 1f;

        bool isFinished;

        protected override void OnInit()
        {
            EventDelegate.Add(blind.onClick, OnClickedBtnBlind);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(blind.onClick, OnClickedBtnBlind);

            Timing.KillCoroutines(gameObject);
        }

        protected override void OnShow(IUIData data = null)
        {
            NGUITools.SetActive(animationBase.gameObject, false);
            NGUITools.SetActive(completeEffect, false);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMessage.LocalKey = LocalizeKey._9013; // 특별한 선물을 준비했어요.\n받아 주실 꺼죠?
        }

        void OnClickedBtnBlind()
        {
            if (!isFinished)
                return;

            OnBack();
        }

        public void Show(RewardData data)
        {
            rewardHelper.SetData(data);
            Timing.RunCoroutine(PlayReward(), gameObject);
        }

        private IEnumerator<float> PlayReward()
        {
            // UI Loading 창이 보이지 않을 때까지 숨죽여 기다림
            yield return Timing.WaitUntilFalse(IsVisibleFadeUI);

            const string CLIP_NAME = "UI_DailyCheckBoxAnim"; // 재생 이름
            const Direction PLAY_DIRECTION = Direction.Forward; // 정방향 플레이
            const EnableCondition ENABLE_BEFORE_PLAY = EnableCondition.EnableThenPlay;
            const DisableCondition DISABLE_CONDITION = DisableCondition.DoNotDisable;

            ActiveAnimation aa = ActiveAnimation.Play(animationBase, CLIP_NAME, PLAY_DIRECTION, ENABLE_BEFORE_PLAY, DISABLE_CONDITION);
            EventDelegate.Add(aa.onFinished, OnFinishedAnimation);
        }

        private bool IsVisibleFadeUI()
        {
            UIFade uiFade = UI.GetUI<UIFade>();

            if (uiFade == null)
                return false;

            return uiFade.IsVisible;
        }

        private void OnFinishedAnimation()
        {
            isFinished = true;
            NGUITools.SetActive(completeEffect, true);
            Timing.RunCoroutine(YieldClose(), gameObject);
        }

        private IEnumerator<float> YieldClose()
        {
            yield return Timing.WaitForSeconds(closeDelay);
            OnClickedBtnBlind();
        }
    }
}