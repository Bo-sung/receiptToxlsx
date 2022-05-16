using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public class UIResultClear : UICanvas, ResultClearPresenter.IView
    {
        protected override UIType uiType => UIType.Hide;

        public enum ResultType
        {
            Clear = 1,
            Failed,
            Result,
        }

        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelValue labelCount;
        [SerializeField] UIButtonHelper btnRetry;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIGrid gridBtn;
        [SerializeField] UIDuelAlphabetCollection duelAlphabetTemplates;
        [SerializeField] GameObject goClear, goFailed, goResult;
        [SerializeField] UILabel noticeLabel;

        // 보상 특수연출
        [SerializeField] Animator animSecondBackground; // 팝업 위로 어둡게 덮는 Animator
        [SerializeField] BoxCollider2D colFxPanel; // fx패널 Collider (스크롤 방지용)
        //[SerializeField] SuperScrollListWrapper wrapperFx;
        [SerializeField] GameObject[] prefabFx;


        ResultClearPresenter presenter;

        private DuelRewardData[] duelRewardDatas;
        private RewardData[] rewards;
        private DungeonType dungeonType;
        private bool showRewardFX; // 보상 특수연출 팝업 여부

        public event System.Action OnRetryDungeon;
        public event System.Action OnFinishDungeon;

        protected override void OnInit()
        {
            presenter = new ResultClearPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            //wrapperFx.SpawnNewList(prefabFx, 0, 0);

            EventDelegate.Add(btnRetry.OnClick, OnClickedBtnRetry);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnConfirm);

            duelRewardDatas = DuelRewardDataManager.Instance.GetDatas();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnRetry.OnClick, OnClickedBtnRetry);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnConfirm);
        }

        protected override void OnShow(IUIData data)
        {
        }

        protected override void OnHide()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            labelDesc.LocalKey = LocalizeKey._7500; // 도전에 성공하셨습니다.
            labelTitle.LocalKey = LocalizeKey._7501; // 보상 목록
            labelCount.TitleKey = LocalizeKey._7502; // 남은 도전 횟수
            btnRetry.LocalKey = LocalizeKey._7504; // 다시하기
            btnConfirm.LocalKey = LocalizeKey._7505; // 확인

            Refresh();
        }

        public void Show(RewardData[] rewards, DungeonType dungeonType, bool isRetry = true, ResultType resultType = ResultType.Clear, bool showRewardFX = false)
        {
            ShowUI();

            this.rewards = rewards;
            this.dungeonType = dungeonType;
            this.showRewardFX = showRewardFX;

            btnRetry.SetActive(isRetry);
            gridBtn.repositionNow = true;
            labelCount.SetActive(dungeonType != default);

            // 특수 보상 연출
            animSecondBackground.gameObject.SetActive(this.showRewardFX);
            animSecondBackground.Update(0f);
            animSecondBackground.enabled = false;
            colFxPanel.enabled = this.showRewardFX;
            //wrapperFx.Resize(0);
            for (int i = 0; i < prefabFx.Length; i++)
            {
                prefabFx[i].SetActive(false);
            }

            if (this.showRewardFX)
            {
                wrapper.Resize(0);
            }
            else
            {
                wrapper.Resize(rewards == null ? 0 : rewards.Length);
            }

            NGUITools.SetActive(goClear, resultType == ResultType.Clear);
            NGUITools.SetActive(goFailed, resultType == ResultType.Failed);
            NGUITools.SetActive(goResult, resultType == ResultType.Result);
            noticeLabel.gameObject.SetActive(false);

            Refresh();

            if (this.showRewardFX && rewards != null)
            {
                Timing.RunCoroutine(YieldShowRewardFX(), gameObject);
            }

            SoundManager.Instance.PlayUISfx(Sfx.UI.BigSuccess);
        }

        public void Show(string label, DungeonType dungeonType, bool isRetry = true, ResultType resultType = ResultType.Clear, bool showRewardFX = false)
        {
            Show(new RewardData[0], dungeonType, isRetry, resultType, showRewardFX);
            noticeLabel.gameObject.SetActive(true);
            noticeLabel.text = label;
        }

        /// <summary>
        /// 보상 연출
        /// </summary>
        private IEnumerator<float> YieldShowRewardFX()
        {
            const float POPUP_ANIM_DURATION = 1f;
            const float REWARD_SHOW_INTERVAL = 0.5f;
            const float FINISH_DELAY = 0.25f;

            yield return Timing.WaitForSeconds(POPUP_ANIM_DURATION);

            for (int i = 0; i < rewards.Length; i++)
            {
                wrapper.Resize(i + 1);
                prefabFx[i].SetActive(true);

                yield return Timing.WaitForSeconds(REWARD_SHOW_INTERVAL);
            }

            yield return Timing.WaitForSeconds(FINISH_DELAY);

            animSecondBackground.enabled = true;
            animSecondBackground.gameObject.SetActive(true);
            colFxPanel.enabled = false;
        }

        public void SetDescription(string description)
        {
            labelDesc.Text = description;
        }

        void OnClickedBtnRetry()
        {
            OnRetryDungeon?.Invoke();
            HideUI();
        }

        public void OnClickedBtnConfirm()
        {
            OnFinishDungeon?.Invoke();
            HideUI();
        }

        public void Refresh()
        {
            if (dungeonType == default)
                return;

            int freeEntryCount = presenter.GetDungeonFreeEntryCount(dungeonType);
            int freeEntryMaxCount = presenter.GetFreeEntryMaxCount(dungeonType);

            labelCount.Value = LocalizeKey._7503.ToText() // {COUNT}/{MAX}
                 .Replace(ReplaceKey.COUNT, freeEntryCount)
                 .Replace(ReplaceKey.MAX, freeEntryMaxCount);
        }

        void OnItemRefresh(GameObject go, int index)
        {
            UIRewardHelper rewardHelper = go.GetComponentInChildren<UIRewardHelper>(true);
            UIResultClearAlphabetReward alphabetReward = go.GetComponentInChildren<UIResultClearAlphabetReward>(true);

            var data = rewards[index];

            if (data.RewardType == RewardType.DuelAlphabet)
            {
                DuelRewardInfo info = data.UserCustomValue as DuelRewardInfo;

                var duelRewardData = duelRewardDatas.FirstOrDefault(v => v.check_value == info.chapter && v.step_value == info.rewardedCount + 1);

                if (duelRewardData == null)
                    return;

                int alphabetIndex = 0;
                for (int i = 0; i < 8; ++i)
                    if ((info.alphabetBit & (1 << i)) > 0)
                    {
                        alphabetIndex = i;
                        break;
                    }

                char character = duelRewardData.GetWord().ElementAt(alphabetIndex);

                rewardHelper.gameObject.SetActive(false);
                alphabetReward.gameObject.SetActive(true);
                alphabetReward.SetData(duelAlphabetTemplates.GetTemplate(duelRewardData.color_index), character);
            }
            else
            {
                rewardHelper.gameObject.SetActive(true);
                alphabetReward.gameObject.SetActive(false);
                rewardHelper.SetData(data);
            }

        }

        private void HideUI()
        {
            Hide();
        }

        private void ShowUI()
        {
            Show();
        }

        protected override void OnBack()
        {
            OnClickedBtnConfirm();
        }
    }
}