using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICardReward : UICanvas<CardRewardPresenter>, CardRewardPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        private const string TAG = nameof(UICardReward);

        private const string ANIM_IDLE = "Idle";
        private const string ANIM_PLAY_0 = "PlayAnim_0";

        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIGridHelper gridRate;
        [SerializeField] UITextureHelper iconCard;
        [SerializeField] Animator animator;
        [SerializeField] GameObject goFX;
        [SerializeField] UIButtonHelper backgroundButton;

        private bool canSkip;
        private bool showRewardInfo;
        private RewardPacket[] rewardPackets;

        protected override void OnInit()
        {
            presenter = new CardRewardPresenter(this);
            presenter.AddEvent();
            EventDelegate.Add(backgroundButton.OnClick, OnClickBackground);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(backgroundButton.OnClick, OnClickBackground);
            Timing.KillCoroutines(TAG);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is CardItemInfo.ICardInfoSimple cardInfo) // ItemInfo가 IUIData라서 이쪽으로 들어올 수 있음.
            {
                Show(cardInfo);
            }
        }

        public void Show(CardItemInfo.ICardInfoSimple[] cardInfos, bool canSkip = false, bool showRewardInfo = false)
        {
            rewardPackets = new RewardPacket[cardInfos.Length];
            for (int i = 0; i < cardInfos.Length; ++i)
                rewardPackets[i] = new RewardPacket((byte)RewardType.Item, cardInfos[i].ID, 0, 1);

            this.canSkip = canSkip;
            this.showRewardInfo = showRewardInfo;
            Show();
            Timing.RunCoroutine(YieldPlayAnimations(cardInfos, isCloseUI: true), TAG);
        }

        public void Show(CardItemInfo.ICardInfoSimple cardInfo, bool canSkip = false, bool showRewardInfo = false)
        {
            rewardPackets = new RewardPacket[] { new RewardPacket((byte)RewardType.Item, cardInfo.ID, 0, 1) };

            this.canSkip = canSkip;
            this.showRewardInfo = showRewardInfo;
            Show();
            Timing.RunCoroutine(YieldPlayAnimation(cardInfo, isCloseUI: true), TAG);
        }

        private IEnumerator<float> YieldPlayAnimations(CardItemInfo.ICardInfoSimple[] cardInfos, bool isCloseUI)
        {
            foreach (var cardInfo in cardInfos)
            {
                yield return Timing.WaitUntilDone(YieldPlayAnimation(cardInfo, isCloseUI: false), TAG);
            }

            if (isCloseUI)
                CloseUI();
        }

        private IEnumerator<float> YieldPlayAnimation(CardItemInfo.ICardInfoSimple cardInfo, bool isCloseUI)
        {
            presenter.SetData(cardInfo);
            Refresh();

            // 애니메이션 재생
            animator.Play(ANIM_PLAY_0);
            goFX.SetActive(true);

            yield return Timing.WaitUntilTrue(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

            animator.Play(ANIM_IDLE);
            goFX.SetActive(false);

            if (isCloseUI)
                CloseUI();
        }

        protected override void OnBack()
        {
        }

        protected override void OnHide()
        {
            Timing.KillCoroutines(TAG);
        }

        protected override void OnLocalize()
        {
        }

        public void Refresh()
        {
            labelName.LocalKey = presenter.GetNameId();
            gridRate.SetValue(presenter.GetRating());
            iconCard.Set(presenter.GetIconName(), isAsync: false); // 애니메이션이 Active를 제어하기 때문에 isAsync 를 false 처리
        }

        private void OnClickBackground()
        {
            if (canSkip)
                CloseUI();
        }

        void CloseUI()
        {
            UI.Close<UICardReward>();

            if (showRewardInfo)
                UI.RewardInfo(rewardPackets);
        }
    }
}