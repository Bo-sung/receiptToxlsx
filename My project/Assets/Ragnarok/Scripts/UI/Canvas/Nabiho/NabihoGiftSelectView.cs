using UnityEngine;

namespace Ragnarok.View
{
    public sealed class NabihoGiftSelectView : SelectPopupView
    {
        [SerializeField] UILabelHelper labelGiftMaterial;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UIButton btnMinus, btnPlus;
        [SerializeField] UISlider slider;
        [SerializeField] UILabel labelGiftValue;
        [SerializeField] GameObject goLock;
        [SerializeField] UILabelHelper labelPreLevel, labelNextLevel;
        [SerializeField] UIAniProgressBar aniProgressBar;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabel labelProgress;
        [SerializeField] UILabelHelper labelNotice;

        private int materialNameId = 194239; // 도람 강아지풀

        private int currentLevel, cur, max;
        private int ownGiftCount = 0; // 보유 재료
        private int maxGiftCount; // 최대 재료
        private int giftCount = -1; // 선택 재료 수

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Add(slider.onChange, OnChangedSlider);

            MainTitleLocalKey = LocalizeKey._10920; // 선물 하기
            ConfirmLocalKey = LocalizeKey._10922; // 선물
            CancelLocalKey = LocalizeKey._2; // 취소

            OnConfirm += ExecuteConfirm;
            OnConfirm += Hide;
            OnCancel += Hide;
            OnExit += Hide;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnMinus.onClick, OnClickedBtnMinus);
            EventDelegate.Remove(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Remove(slider.onChange, OnChangedSlider);

            OnConfirm -= ExecuteConfirm;
            OnConfirm -= Hide;
            OnCancel -= Hide;
            OnExit -= Hide;
        }

        void OnClickedBtnMinus()
        {
            RefreshSlider(giftCount - 1);
        }

        void OnClickedBtnPlus()
        {
            RefreshSlider(giftCount + 1);
        }

        void OnChangedSlider()
        {
            int value = (int)(maxGiftCount * UIProgressBar.current.value);
            int current = Mathf.Min(ownGiftCount, value);
            SetGiftCount(current);
        }

        void ExecuteConfirm()
        {
            OnSelect?.Invoke(giftCount);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelGiftMaterial.LocalKey = LocalizeKey._10921; // 선물 재료
            UpdateLevelText();
            UpdateNoticeText();
        }

        public override void Show()
        {
            base.Show();

            int value = ownGiftCount > 0 ? 1 : 0;
            RefreshSlider(value);
            SetGiftCount(value);
            UpdateProgress();
        }

        public override void Hide()
        {
            base.Hide();

            slider.value = 0f;
            aniProgressBar.Set(cur, max);
        }

        public void Initialize(int materialNameId)
        {
            if (this.materialNameId == materialNameId)
                return;

            this.materialNameId = materialNameId;
            UpdateNoticeText();
        }

        public void UpdateMaterial(RewardData rewardData)
        {
            rewardHelper.SetData(rewardData);
            ownGiftCount = rewardData.RewardCount;

            RefreshMaxCount();
        }

        public void UpdateLevel(int currentLevel, int cur, int max)
        {
            this.currentLevel = currentLevel;
            this.cur = cur;
            this.max = max;

            progressBar.value = MathUtils.GetProgress(cur, max);
            aniProgressBar.Set(cur, max);

            RefreshMaxCount();
            UpdateLevelText();
            UpdateProgress();
        }

        private void RefreshMaxCount()
        {
            int value = Mathf.Min(ownGiftCount, max - cur); // 1부터 시작
            maxGiftCount = Mathf.Max(0, value);
            slider.numberOfSteps = maxGiftCount + 1;

            NGUITools.SetActive(goLock, maxGiftCount == 0);
        }

        private void UpdateLevelText()
        {
            labelPreLevel.Text = LocalizeKey._10902.ToText() // {VALUE}단계
                .Replace(ReplaceKey.VALUE, currentLevel);

            labelNextLevel.Text = LocalizeKey._10902.ToText() // {VALUE}단계
                .Replace(ReplaceKey.VALUE, currentLevel + 1);
        }

        private void UpdateProgress()
        {
            labelProgress.text = StringBuilderPool.Get()
                .Append(cur + giftCount).Append('/').Append(max)
                .Release();

            aniProgressBar.Tween(cur + giftCount, max);
        }

        private void RefreshSlider(int input)
        {
            if (maxGiftCount == 0)
            {
                slider.value = 0f;
            }
            else
            {
                slider.value = MathUtils.GetProgress(input, maxGiftCount); // 1부터 시작
            }
        }

        private void SetGiftCount(int input)
        {
            if (giftCount == input)
                return;

            giftCount = input;
            labelGiftValue.text = giftCount.ToString();

            btnMinus.enabled = giftCount > 0;
            btnPlus.enabled = giftCount < maxGiftCount;
            SetIsEnabledBtnConfirm(giftCount > 0);

            UpdateProgress();
        }

        private void UpdateNoticeText()
        {
            labelNotice.Text = LocalizeKey._10923.ToText() // {NAME}은 일일 퀘스트 전체 완료 시 획득할 수 있습니다.
                .Replace(ReplaceKey.NAME, materialNameId.ToText());
        }
    }
}