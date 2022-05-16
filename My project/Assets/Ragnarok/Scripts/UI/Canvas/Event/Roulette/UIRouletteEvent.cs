using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRouletteEvent : UICanvas<RouletteEventPresenter>, RouletteEventPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UIButtonHelper btnStart;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonWithIcon btnCost;
        [SerializeField] Animation animRoulette;
        [SerializeField] GameObject[] goFxLights;
        [SerializeField] UITextureHelper iconItem;
        [SerializeField] UILabelHelper labelNeedItemCount;
        [SerializeField] GameObject goFinger;
        [SerializeField] UIButtonHelper btnChange;
        [SerializeField] GameObject boardNormal, boardRare;
        [SerializeField] GameObject startbtnRare;
        [SerializeField] GameObject backgroundNormal, backgroundRare;
        [SerializeField] UILabelHelper labelNotice;

        private bool isPlaying;

        protected override void OnInit()
        {
            presenter = new RouletteEventPresenter(this);
            presenter.AddEvent();

            presenter.OnCloseReward += ReposAndRefresh;

            EventDelegate.Add(btnCost.OnClick, presenter.ShowCostItemData);
            EventDelegate.Add(btnStart.OnClick, OnClickedBtnStart);
            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnChange.OnClick, OnClickedBtnChange);
        }

        protected override void OnClose()
        {
            presenter.OnCloseReward -= ReposAndRefresh;

            presenter.RemoveEvent();

            EventDelegate.Remove(btnCost.OnClick, presenter.ShowCostItemData);
            EventDelegate.Remove(btnStart.OnClick, OnClickedBtnStart);
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnChange.OnClick, OnClickedBtnChange);

            Timing.KillCoroutines(gameObject);
            Timing.KillCoroutines("Light");
        }

        protected override void OnShow(IUIData data = null)
        {
            isPlaying = false;
            presenter.isSendRquest = false;

            ResetRoulettePostion();
            // 라이트 애니메이션 시작
            StopLightAnimation();
            PlayLightAnimation();
            SetActiveObFinger(IsAvailableRoulette());

            Refresh();
        }

        protected override void OnHide()
        {
            if (isPlaying)
            {
                presenter.ShowRewardUI(); // 보상 팝업 출력
            }

            isPlaying = false;
            StopLightAnimation();
            Timing.KillCoroutines(gameObject);
            Timing.KillCoroutines("Light");
        }

        protected override void OnLocalize()
        {
            btnChange.LocalKey = LocalizeKey._11105; // 돌림판 교체
            labelNotice.Text = LocalizeKey._11106.ToText() // 이벤트 주화는 최대 {COUNT}개까지 획득할 수 있습니다.
                .Replace(ReplaceKey.COUNT, presenter.eventCoinMaxCount);
        }

        public void OnUpdateCost()
        {
            int hasCostCount = presenter.GetCostItemCount();
            int needCostCount = presenter.GetNeedCount();

            btnCost.Text = StringBuilderPool.Get().Append(hasCostCount < needCostCount ? "[FF3333]" : "").Append(hasCostCount).Release();
            iconItem.Set(presenter.CostItem.icon_name);
            labelNeedItemCount.Text = LocalizeKey._11100.ToText().Replace(ReplaceKey.VALUE, needCostCount); // 소모 재화량 : {VALUE}
            btnChange.SetActive(presenter.IsActiveRareRoulette());
            btnChange.SetNotice(presenter.HasNoticeChangeButton());
            btnCost.SetActiveIcon(presenter.IsCheckMaxEventCoin() && hasCostCount >= presenter.eventCoinMaxCount);
        }

        private void Refresh()
        {
            if (!IsVisible)
                return;

            if (presenter.IsInvalid)
                return;

            OnUpdateCost();

            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(presenter.ConvertedRewardDatas[i]);
            }

            labelTitle.Text = presenter.GetTitle();
            boardNormal.SetActive(!presenter.IsRareRoulette());
            boardRare.SetActive(presenter.IsRareRoulette());
            backgroundNormal.SetActive(!presenter.IsRareRoulette());
            backgroundRare.SetActive(presenter.IsRareRoulette());
            startbtnRare.SetActive(presenter.IsRareRoulette());
            labelNotice.SetActive(presenter.IsCheckMaxEventCoin());
        }

        void ReposAndRefresh()
        {
            ResetRoulettePostion();
            Refresh();
        }

        /// <summary>
        /// 룰렛 돌리기
        /// </summary>
        void RouletteEventPresenter.IView.PlayRoulette(int rewardIndex)
        {
            Timing.KillCoroutines(gameObject);
            Timing.KillCoroutines("Light");
            Timing.RunCoroutine(YieldPlayRoulette(rewardIndex), gameObject);
        }

        private IEnumerator<float> YieldPlayRoulette(int rewardIndex)
        {
            const float ROULETTE_ANIM_WAIT_DURATION = 0.25f;

            Refresh();

            presenter.PlayRouletteSfx();

            // 룰렛 애니메이션 대기
            string CLIP_NAME = StringBuilderPool.Get().Append("Center_0").Append(rewardIndex).Release(); // 재생 이름
            animRoulette.Play(CLIP_NAME);

            yield return Timing.WaitUntilFalse(IsPlayinigRoulette);
            yield return Timing.WaitForSeconds(ROULETTE_ANIM_WAIT_DURATION);

            presenter.ShowRewardUI(); // 보상 팝업 출력

            // 다시 룰렛 가능하도록 변경
            isPlaying = false;
            PlayLightAnimation();
            SetActiveObFinger(IsAvailableRoulette());
        }

        private bool IsPlayinigRoulette()
        {
            return animRoulette.isPlaying;
        }

        private bool IsAvailableRoulette()
        {
            int hasCostCount = presenter.GetCostItemCount();
            int needCostCount = presenter.GetNeedCount();

            return hasCostCount >= needCostCount ? true : false;
        }

        void OnClickedBtnStart()
        {
            if (presenter.IsInvalid)
                return;

            // 레어 룰렛판 비활성화
            if (presenter.IsRareRoulette() && !presenter.IsActiveRareRoulette())
                return;

            if (isPlaying)
                return;

            if (IsAvailableRoulette() == false)
            {
                UI.ShowToastPopup(LocalizeKey._11102.ToText()); // 재화가 부족합니다.
                return;
            }

            StopLightAnimation();
            isPlaying = true;

            presenter.RequestPlayRoulette();

            SetActiveObFinger(false);
        }

        void OnClickedBtnExit()
        {
            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIRouletteEvent>();
        }

        private void StopLightAnimation()
        {
            Timing.KillCoroutines("Light");

            for (int i = 0; i < goFxLights.Length; i++)
            {
                goFxLights[i].SetActive(true);
            }
        }

        private void PlayLightAnimation()
        {
            Timing.RunCoroutine(YieldPlayLightFx(), "Light");
        }

        private IEnumerator<float> YieldPlayLightFx()
        {
            // 불빛 : 한 칸 당 3개, 총 30개
            const float LIGHT_MOVE_DELAY = 0.15f;
            const int MAX_LIGHT_SET = 2;

            int lightSetIndex = 0;
            while (true)
            {
                for (int i = 0; i < goFxLights.Length; i++)
                {
                    goFxLights[i].SetActive(i % MAX_LIGHT_SET == lightSetIndex);
                }

                yield return Timing.WaitForSeconds(LIGHT_MOVE_DELAY);

                lightSetIndex = (lightSetIndex + 1) % MAX_LIGHT_SET;
            }
        }

        protected override void OnBack()
        {
            if (presenter.isSendRquest)
                return;

            base.OnBack();
        }

        private void SetActiveObFinger(bool isActive)
        {
            if (goFinger == null)
                return;

            NGUITools.SetActive(goFinger, isActive);
        }

        /// <summary>
        /// 룰렛판 교체 버튼 이벤트
        /// </summary>
        void OnClickedBtnChange()
        {
            if (presenter.IsInvalid)
                return;

            if (isPlaying)
                return;

            presenter.ChangeRoulette();
            ResetRoulettePostion();
            Refresh();
            SetActiveObFinger(IsAvailableRoulette());
        }

        /// <summary>
        /// 룰렛판 위치 초기화
        /// </summary>
        private void ResetRoulettePostion()
        {
            animRoulette.GetClip("Center_00").SampleAnimation(animRoulette.gameObject, 0f);
            animRoulette.Stop();
        }

        public override bool Find()
        {
            base.Find();
            rewards = GetComponentsInChildren<UIRewardHelper>();
            return true;
        }
    }
}