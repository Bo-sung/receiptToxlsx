using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UISharevice : MonoBehaviour, IAutoInspectorFinder
    {
        [SerializeField] UIButtonHelper btnCancel;
        [SerializeField] UIButtonHelper btnLevelUp;
        [SerializeField] UIButtonHelper btnLevelUpProgress;
        [SerializeField] UIButtonHelper btnLevelUpComplete;
        [SerializeField] UILabelHelper labCost;

        [SerializeField] GameObject prefab;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] UIShareviceItem element;

        [SerializeField] UITexture gage;
        [SerializeField] UITexture preview;
        [SerializeField] Transform gage_FX;
        [SerializeField] Transform preview_FX;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabel labelNotice;
        [SerializeField] UILabelHelper labelNoItem;
        [SerializeField] GameObject ItemListMask;

        [SerializeField] UIButtonHelper btnLevelUpUseAll;

        // Normal Sharevice
        [SerializeField] UILabelHelper normalInfo;
        [SerializeField] UILabelHelper levelNormal;
        [SerializeField] UILabelValue levelNormalPreview;

        [SerializeField] UILabelValue maxPower;

        [SerializeField] UILabelHelper labelExpNeed;
        [SerializeField] UILabelHelper labelExpCurrent;
        [SerializeField] UILabelHelper labelExpSelect;

        // Progress Sharevice
        [SerializeField] UILabelHelper progressInfo;
        [SerializeField] UILabelHelper levelProgress;
        [SerializeField] UILabelValue levelProgressRemain;

        // Complete Sharevice
        [SerializeField] UILabelHelper completeInfo;
        [SerializeField] UILabelHelper levelComplete;
        [SerializeField] UILabelHelper labelCompleteMessage;


        [SerializeField] UIWidget widgetFirstExperianceItem;
        [SerializeField] UIWidget widgetLevelUp;
        [SerializeField] UIWidget widgetClose;

        SharevicePresenter presenter;

        private int curLevel;
        private int curExp;
        private int needExp;
        private int maxBP;
        private bool onceLevelUp = false;

        private SuperWrapContent<UIShareviceItem, ShareviceItem> wrapContent;

        public void OnInit()
        {
            wrapContent = wrapper.Initialize<UIShareviceItem, ShareviceItem>(element);
            foreach (UIShareviceItem item in wrapContent)
            {
                item.OnChangeSelect += PreviewGage;
            }

            presenter = new SharevicePresenter();
            presenter.OnLevelUpSharevice += OnLevelUpShareVice;

            EventDelegate.Add(btnCancel.OnClick, Hide);
            EventDelegate.Add(btnLevelUp.OnClick, LevelUpVice);
            EventDelegate.Add(btnLevelUpProgress.OnClick, LevelUpImmediately);
            EventDelegate.Add(btnLevelUpComplete.OnClick, LevelupComplete);
            EventDelegate.Add(btnLevelUpUseAll.OnClick, LevelUpUseAll);
        }

        public void OnClose()
        {
            presenter.OnLevelUpSharevice -= OnLevelUpShareVice;
            foreach (UIShareviceItem item in wrapContent)
            {
                item.OnChangeSelect -= PreviewGage;
            }

            EventDelegate.Remove(btnCancel.OnClick, Hide);
            EventDelegate.Remove(btnLevelUp.OnClick, LevelUpVice);
            EventDelegate.Remove(btnLevelUpProgress.OnClick, LevelUpImmediately);
            EventDelegate.Remove(btnLevelUpComplete.OnClick, LevelupComplete);
            EventDelegate.Remove(btnLevelUpUseAll.OnClick, LevelUpUseAll);
        }

        public void Show()
        {
            gameObject.SetActive(true);

            InitShareviceExperience(true);
            onceLevelUp = false;
        }

        public void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._10246; // Sharevice
            normalInfo.LocalKey = LocalizeKey._10254; // Level
            progressInfo.LocalKey = LocalizeKey._10254; // Level
            levelProgressRemain.TitleKey = LocalizeKey._10258; // 레벨업 소요 시간
            completeInfo.LocalKey = LocalizeKey._10254; // Level
            maxPower.TitleKey = LocalizeKey._10247; // Max Power BP
            labelNotice.text = LocalizeKey._10264.ToText(); // 사냥 필드와 중앙실험실에서 레벨업 재료를 획득할 수 있습니다.
            labelNoItem.LocalKey = LocalizeKey._10265; // 사용 가능한 재료가 없습니다.
            btnCancel.LocalKey = LocalizeKey._7606; // 돌아가기
            btnLevelUp.LocalKey = LocalizeKey._10243; // 레벨업
            btnLevelUpProgress.LocalKey = LocalizeKey._10259; // 즉시 완료
            btnLevelUpComplete.LocalKey = LocalizeKey._10260; // 완료
            labelCompleteMessage.LocalKey = LocalizeKey._10263; // 쉐어바이스 레벨업이 완료되었습니다.
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void InitShareviceExperience(bool isShow = false)
        {
            wrapContent.SetData(presenter.GetShareviceItems());
            labelNoItem.SetActive(presenter.IsEmptyShareViceItem());

            curLevel = presenter.GetShareviceLevel();
            curExp = presenter.GetShareviceExp();
            needExp = presenter.GetShareviceNeedExp();
            maxBP = presenter.GetShareviceMaxBP();

            levelNormal.Text = curLevel.ToString();
            levelNormalPreview.Title = curLevel.ToString();
            maxPower.Value = maxBP.ToString("#,##0");

            btnLevelUp.IsEnabled = false; // 레벨업 버튼 비활성화

            SetGage(isShow);
        }

        private IEnumerator<float> YieldRepositionGage(UITexture texture, Transform fx, float amount, float tweenTime = 0.2f)
        {
            tweenTime *= 1000; // 밀리초
            RemainTime remainTime = tweenTime;

            float preValue = texture.fillAmount;
            float tweenValue = amount - preValue;

            while (remainTime.ToRemainTime() > 0)
            {
                var targetVal = preValue + tweenValue * (tweenTime - remainTime.ToRemainTime()) / tweenTime;
                RepositionGage(texture, fx, targetVal);

                yield return Timing.WaitForOneFrame;
            }

            RepositionGage(texture, fx, amount);

            // 레벨업상태 체크..
            if (curExp + presenter.GetTotalSelectedExp() >= needExp)
            {
                Timing.KillCoroutines(GetTag(levelNormalPreview));
                Timing.RunCoroutine(YieldLevelUpPreview().CancelWith(gameObject), GetTag(levelNormalPreview));
            }
        }

        private IEnumerator<float> YieldLevelUpPreview()
        {
            var destLevel = presenter.GetShareviceLevel(presenter.GetTotalSelectedExp());
            var count = destLevel - curLevel;
            var waitForSeconds = count > 0 ? 1 / count : 1;

            levelNormal.Text = "";
            levelNormalPreview.SetActive(true);

            for (int i = 0; i < count; i++)
            {
                levelNormalPreview.Value = (curLevel + i + 1).ToString();
                yield return waitForSeconds;
            }
        }

        private IEnumerator<float> YieldRemainTime()
        {
            // 미리보기 게이지는 최소치로 바꿔줌
            var amountMax = AmountPadding(0);
            RepositionGage(preview, preview_FX, amountMax);

            // 경험치 게이지를 타이머로 사용
            var totalTime = presenter.GetLevelUpTotalRemainTime(GetShareviceTargetLevel());
            var remainTime = presenter.GetShareviceLevelUpRemainTime();

            while (remainTime.ToRemainTime() > 0)
            {
                labCost.Text = GetCatCoinImmediatlyLevelUp(remainTime).ToString(); // 즉시완료 캣코인 수량 갱신

                var span = remainTime.ToRemainTime().ToTimeSpan();
                span += System.TimeSpan.FromMinutes(1); // 최소 표시단위가 분단위라 올림해서 보여주기 위함.
                levelProgressRemain.Value = LocalizeKey._10262.ToText() // {HOURS}H {MINUTES}M
                    .Replace(ReplaceKey.HOURS, (int)span.TotalHours).Replace(ReplaceKey.MINUTES, span.Minutes.ToString("00"));

                var amount = AmountPadding((float)remainTime.ToRemainTime() / totalTime);
                RepositionGage(gage, gage_FX, amount);

                yield return Timing.WaitForSeconds(1);
            }

            RefreshShareviceState();
        }

        private void RepositionGage(UITexture texture, Transform fx, float amount)
        {
            texture.fillAmount = amount;
            fx.localEulerAngles = Vector3.forward * (-90 - 360 * amount);
        }

        /// <summary>
        /// 현재 게이지 수치 갱신
        /// </summary>
        private void SetGage(bool isShow = false)
        {
            var amount = AmountPadding((float)curExp / needExp);
            if (presenter.GetShareviceState() == ShareviceState.LevelUpProgress) amount = AmountPadding(1f); // 레벨업 진행중에는 게이지를 타이머로 사용.
            RepositionGage(preview, preview_FX, amount);

            Timing.KillCoroutines(GetTag(gage));
            if (isShow) Timing.RunCoroutine(YieldRepositionGage(gage, gage_FX, amount, 0f).CancelWith(gameObject), GetTag(gage));
            else Timing.RunCoroutine(YieldRepositionGage(gage, gage_FX, amount).CancelWith(gameObject), GetTag(gage));


            levelNormal.Text = curLevel.ToString();
            levelNormalPreview.Title = curLevel.ToString();
            levelNormalPreview.SetActive(false);

            labelExpSelect.Text = "";
            labelExpCurrent.Text = curExp.ToString();
            labelExpNeed.Text = string.Format("/{0}", needExp);

            // 셰어바이스 상태 갱신
            RefreshShareviceState();
        }

        /// <summary>
        /// 아이템 적용 게이지 미리보기
        /// </summary>
        private void PreviewGage()
        {
            btnLevelUp.IsEnabled = true; // 레벨업 버튼 활성화

            var amount = AmountPadding((float)(curExp + presenter.GetTotalSelectedExp()) / needExp);
            Timing.KillCoroutines(GetTag(preview));
            Timing.RunCoroutine(YieldRepositionGage(preview, preview_FX, amount).CancelWith(gameObject), GetTag(preview));

            // 미리보기 경험치 수치 갱신
            labelExpSelect.Text = (curExp + presenter.GetTotalSelectedExp()).ToString();
            labelExpCurrent.Text = "";
        }

        private void RefreshShareviceState()
        {
            SetViceState(presenter.GetShareviceState());
        }

        private float AmountPadding(float value)
        {
            if (value > 1) value = 1;

            return value * (1 - 0.144f) + 0.072f;
        }

        private string GetTag(UITexture texture)
        {
            return string.Concat(gameObject.GetInstanceID(), texture.GetInstanceID());
        }

        private string GetTag(UILabelValue labelValue)
        {
            return string.Concat(gameObject.GetInstanceID(), labelValue.GetInstanceID());
        }

        private void SetViceState(ShareviceState state)
        {
            normalInfo.SetActive(state == ShareviceState.Normal);
            progressInfo.SetActive(state == ShareviceState.LevelUpProgress);
            completeInfo.SetActive(state == ShareviceState.LevelUpComplete);

            btnLevelUp.SetActive(state == ShareviceState.Normal);
            btnLevelUpProgress.SetActive(state == ShareviceState.LevelUpProgress);
            btnLevelUpComplete.SetActive(state == ShareviceState.LevelUpComplete);
            btnLevelUpUseAll.SetActive(state == ShareviceState.Normal);
            ItemListMask.SetActive(state != ShareviceState.Normal); // 경험치 아이템은 레벨업중에는 사용 불가능, 마스크 추가

            Timing.KillCoroutines(GetTag(levelProgressRemain));

            switch (state)
            {
                case ShareviceState.Normal:
                    // 이땐 별거 없음
                    break;
                case ShareviceState.LevelUpProgress:
                    // 이때는 남은시간 코루틴으로 체크
                    levelProgress.Text = GetShareviceTargetLevel().ToString();
                    Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), GetTag(levelProgressRemain), SingletonBehavior.Overwrite);
                    break;
                case ShareviceState.LevelUpComplete:
                    // 레벨업할 레벨수치 갱신
                    levelComplete.Text = GetShareviceTargetLevel().ToString();

                    var amountMin = AmountPadding(0);
                    var amountMax = AmountPadding(1);
                    RepositionGage(preview, preview_FX, amountMax);
                    RepositionGage(gage, gage_FX, amountMin);
                    break;
            }
        }

        /// <summary>
        /// 경험치 증가시의 결과 레벨
        /// </summary>
        private int GetShareviceTargetLevel(bool isSelectedItem = false)
        {
            if (isSelectedItem) return presenter.GetShareviceLevel(presenter.GetTotalSelectedExp()); // 선택중인 아이템들의 총 경험치
            else return presenter.GetShareviceLevel(presenter.GetShareviceTempExp()); // 임시저장 경험치
        }

        /// <summary>
        /// 즉시 완료에 필요한 냥다래
        /// </summary>
        private int GetCatCoinImmediatlyLevelUp(RemainTime remainTime)
        {
            var rt = remainTime.ToRemainTime();
            if (rt > 0f)
            {
                var timeSpan = rt.ToTimeSpan();
                var initMin = BasisType.SHAREVICE_LEVELUP_INIT_TIME.GetInt() * 0.001f / 60f; // 초기화 시간 단위(분)
                return BasisType.SHAREVICE_IMMEDIATLY_LEVELUP_CATCOIN.GetInt() * Mathf.CeilToInt((float)timeSpan.TotalMinutes / initMin); // 5분당 10냥다래
            }
            else
            {
                return 0; // 남은시간 없음..
            }
        }

        /// <summary>
        /// 레벨업 버튼
        /// </summary>
        private void LevelUpVice()
        {
            presenter.LevelUpSharevice();
        }

        /// <summary>
        /// 레벨업 아이템 전부 사용
        /// </summary>
        private void LevelUpUseAll()
        {
            if (!presenter.SetAutoMaxSelect())
                return;

            wrapper.RefreshAllItems();
            PreviewGage();
        }

        private async void LevelUpImmediately()
        {
            var remainTime = presenter.GetShareviceLevelUpRemainTime();
            if (remainTime.ToRemainTime() > 0)
            {
                // 레벨업 / 냥다래를 소모하여 즉시 레벨업 하시겠습니까 ?
                if (!await UI.CostPopup(CoinType.CatCoin, GetCatCoinImmediatlyLevelUp(remainTime), LocalizeKey._10243.ToText(), LocalizeKey._10261.ToText()))
                    return;
            }

            LevelupComplete();
        }

        private void LevelupComplete()
        {
            presenter.LevelUpCompleteSharevice();
        }

        private void OnLevelUpShareVice()
        {
            InitShareviceExperience();
            onceLevelUp = true;
        }

        public UIWidget GetBtnFirstExperienceItem()
        {
            return widgetFirstExperianceItem;
        }

        public bool IsFirstExperienceItemSelected()
        {
            return presenter.IsSelectedExp();
        }

        public UIWidget GetBtnLevelUp()
        {
            return widgetLevelUp;
        }

        public UIWidget GetBtnClose()
        {
            return widgetClose;
        }

        public bool IsLevelUpAccomplished()
        {
            return onceLevelUp;
        }
    }
}