using AnimationOrTween;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DailyCheckSlot : UIInfo<DailyCheckPresenter, DailyCheckInfo>, IAutoInspectorFinder, IInspectorFinder
    {
        const float PLAY_ANIM_DELAY = 0.25f;

        [SerializeField] UILabelHelper labelTime;
        [SerializeField] UIRewardHelper reward;
        [SerializeField] UILabelHelper labelReward;
        [SerializeField] GameObject completeBase;
        [SerializeField] Animator fxCompleteBase;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Timing.KillCoroutines(gameObject);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            labelTime.Text = LocalizeKey._9001.ToText().Replace("{DAY}", info.Day.ToString()); // {DAY}일
            reward.SetData(info.RewardData);
            labelReward.Text = info.Name;
        }

        public void SetActiveComplete(bool isActive)
        {
            completeBase.SetActive(isActive);
        }

        public void SetFxCompleteBase(bool isActive)
        {
            if (isActive)
                Timing.RunCoroutine(_PlayEffect(), gameObject);
        }

        IEnumerator<float> _PlayEffect()
        {
            // UI Loading 창이 보이지 않을 때까지 숨죽여 기다림
            yield return Timing.WaitUntilFalse(IsVisibleUI<UIFade>);

            // UI DailyCheckBox 창이 보이지 않을 때까지 숨죽여 기다림
            yield return Timing.WaitUntilFalse(IsVisibleUI<UIDailyCheckBox>);

            // 셰어 결과 UI 대기
            yield return Timing.WaitUntilFalse(IsVisibleUI<UICharacterShare>);

            // 셰어 보상 목록 UI 대기
            yield return Timing.WaitUntilFalse(IsVisibleUI<UICharacterShareReward>);

            yield return Timing.WaitForSeconds(PLAY_ANIM_DELAY);

            const string CLIP_NAME = "UI_DailyCheckComplete";
            const Direction PLAY_DIRECTION = Direction.Forward;
            const EnableCondition ENABLE_BEFORE_PLAY = EnableCondition.EnableThenPlay;
            const DisableCondition DISABLE_CONDITION = DisableCondition.DoNotDisable;
            ActiveAnimation.Play(fxCompleteBase, CLIP_NAME, PLAY_DIRECTION, ENABLE_BEFORE_PLAY, DISABLE_CONDITION);
        }

        private bool IsVisibleUI<T>()
            where T : UICanvas
        {
            T ui = UI.GetUI<T>();
            if (ui == null)
                return false;

            return ui.IsVisible;
        }

        bool IInspectorFinder.Find()
        {
            fxCompleteBase = completeBase.GetComponent<Animator>();
            return true;
        }
    }
}