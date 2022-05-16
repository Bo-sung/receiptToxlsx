using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UISpecialRoulette : UICanvas
    {
        public interface IInput : IUIData
        {
            int TitleKey { get; }
            int DescriptionKey { get; }
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonWithIconHelper btnRouletteItem;
        [SerializeField] UIButtonWithIconHelper btnCatcoin;
        [SerializeField] UIButtonHelper btnRouletteItemInfo;
        [SerializeField] UIButton btnExit;
        [SerializeField] UIButtonWithIconValueHelper btnRoulette;
        [SerializeField] UIButtonHelper btnRouletteFree;
        [SerializeField] UIButtonHelper btnRouletteComplete;
        [SerializeField] UIButtonWithIconValueHelper btnChange;
        [SerializeField] UIButtonHelper btnChangeFree;
        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] UISpecialRouletteElement[] elements;
        [SerializeField] UILabelHelper labelTimeTitle;
        [SerializeField] UILabelHelper labelTime;
        [SerializeField] GameObject touchBlock;
        [SerializeField] GameObject rouletteFx;
        [SerializeField] GameObject slotFx;
        [SerializeField] GameObject backFx;
        [SerializeField] float fxTime = 3f;

        SpecialRoulettePresenter presenter;

        private bool isPlayingEffect;

        private int titleKey = LocalizeKey._5700;
        private int descriptionKey = LocalizeKey._5701;

        protected override void OnInit()
        {
            presenter = new SpecialRoulettePresenter();

            presenter.OnUpdateCatCoin += UpdateCatCoin;
            presenter.OnUpdateUseItem += UpdateUseItem;
            presenter.OnUpdateRoulette += OnUpdateRoulette;
            presenter.OnRouletteChange += OnRouletteChange;
            presenter.OnPlayRouletteEffect += OnPlayRouletteEffect;
            presenter.OnUpdateNotice += OnUpdateNotice;

            presenter.AddEvent();

            EventDelegate.Add(btnRouletteItem.OnClick, OnClickedBtnRouletteItem);
            EventDelegate.Add(btnCatcoin.OnClick, OnClickedBtnCatcoin);
            EventDelegate.Add(btnExit.onClick, OnBack);
            EventDelegate.Add(btnRoulette.OnClick, OnClickedBtnRoulette);
            EventDelegate.Add(btnRouletteFree.OnClick, OnClickedBtnRoulette);
            EventDelegate.Add(btnChange.OnClick, OnClickedBtnChange);
            EventDelegate.Add(btnChangeFree.OnClick, OnClickedBtnChange);
            EventDelegate.Add(btnRouletteItemInfo.OnClick, OnClickedBtnRouletteItemInfo);
            EventDelegate.Add(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnClose()
        {
            presenter.OnUpdateCatCoin -= UpdateCatCoin;
            presenter.OnUpdateUseItem -= UpdateUseItem;
            presenter.OnUpdateRoulette -= OnUpdateRoulette;
            presenter.OnRouletteChange -= OnRouletteChange;
            presenter.OnPlayRouletteEffect -= OnPlayRouletteEffect;
            presenter.OnUpdateNotice -= OnUpdateNotice;

            presenter.RemoveEvent();

            EventDelegate.Remove(btnRouletteItem.OnClick, OnClickedBtnRouletteItem);
            EventDelegate.Remove(btnCatcoin.OnClick, OnClickedBtnCatcoin);
            EventDelegate.Remove(btnExit.onClick, OnBack);
            EventDelegate.Remove(btnRoulette.OnClick, OnClickedBtnRoulette);
            EventDelegate.Remove(btnRouletteFree.OnClick, OnClickedBtnRoulette);
            EventDelegate.Remove(btnChange.OnClick, OnClickedBtnChange);
            EventDelegate.Remove(btnChangeFree.OnClick, OnClickedBtnChange);
            EventDelegate.Remove(btnRouletteItemInfo.OnClick, OnClickedBtnRouletteItemInfo);
            EventDelegate.Remove(btnHelp.OnClick, OnClickedBtnHelp);
        }

        protected override void OnShow(IUIData data = null)
        {
            // 이벤트 기간이 아님
            if (!presenter.IsRemainTimeRoulette())
            {
                OnBack();
                return;
            }

            if (data is IInput input)
            {
                titleKey = input.TitleKey;
                descriptionKey = input.DescriptionKey;
                UpdateText();
            }

            presenter.Initialize();
            SetCatCoinIcon();
            UpdateRewardInfo();
            UpdateOwnedCount();
            UpdateUseItem();
            UpdateRouletteButton();
            UpdateRouletteChangeButton();
            touchBlock.SetActive(false);
            rouletteFx.SetActive(false);
            slotFx.SetActive(true);
            backFx.SetActive(true);
            btnChange.SetIconName(CoinType.CatCoin.IconName());
            isPlayingEffect = false;
        }

        protected override void OnHide()
        {
            if (isPlayingEffect)
            {
                presenter.ShowRewardPopup(); // 보상 팝업 출력
            }
            isPlayingEffect = false;
        }

        protected override void OnBack()
        {
            if (presenter.IsSendRoulette)
                return;

            base.OnBack();
        }

        protected override void OnLocalize()
        {
            UpdateText();
            btnRoulette.LocalKey = LocalizeKey._5702; // 뽑기
            btnRouletteFree.LocalKey = LocalizeKey._5703; // 무료 뽑기
            btnRouletteComplete.LocalKey = LocalizeKey._5705; // 완료
            btnChange.LocalKey = LocalizeKey._5707; // 교체
            btnChangeFree.LocalKey = LocalizeKey._5708; // 무료 교체
            labelTimeTitle.LocalKey = LocalizeKey._5706; // 무료 교체 까지
        }

        private void UpdateText()
        {
            labelTitle.LocalKey = titleKey;
            labelDesc.LocalKey = descriptionKey;
        }

        /// <summary>
        /// 룰렛판 보상 목록 정보 세팅
        /// </summary>
        private void UpdateRewardInfo()
        {
            UISpecialRouletteElement.IInput[] infos = presenter.GetSpecialRouletteElementArray();
            for (int i = 0; i < infos.Length; i++)
            {
                elements[i].SetData(infos[i]);
            }
        }

        private void SetCatCoinIcon()
        {
            btnCatcoin.SetIconName(CoinType.CatCoin.IconName());
        }

        private void UpdateCatCoin(long catCoin)
        {
            btnCatcoin.Text = catCoin.ToString("N0");
        }

        /// <summary>
        /// 소모 아이템 보유수량 표시
        /// </summary>
        private void UpdateOwnedCount()
        {
            btnRouletteItem.Text = presenter.GetOwnedCount().ToString();
        }

        /// <summary>
        /// 소모 아이템 아이콘 정보 표시
        /// </summary>
        private void UpdateUseItem()
        {
            string iconName = presenter.GetOwnedIconName();
            btnRouletteItem.SetIconName(iconName);
            btnRoulette.SetIconName(iconName);
        }

        /// <summary>
        /// 룰렛 뽑기 버튼 상태 표시
        /// </summary>
        private void UpdateRouletteButton()
        {
            int needCount = presenter.GetNeedCount();
            bool isFree = needCount == 0;

            if (presenter.IsMaxUsed())
            {
                btnRouletteComplete.SetActive(true);
                btnRouletteFree.SetActive(false);
                btnRoulette.SetActive(false);
            }
            else if (isFree)
            {
                btnRouletteComplete.SetActive(false);
                btnRouletteFree.SetActive(true);
                btnRoulette.SetActive(false);
                btnRouletteFree.SetNotice(!presenter.IsJobLevelLimit());
            }
            else
            {
                btnRouletteComplete.SetActive(false);
                btnRouletteFree.SetActive(false);
                btnRoulette.SetActive(true);
                btnRoulette.SetIconName(presenter.GetOwnedIconName());
                btnRoulette.SetValue($"x{needCount}");
                btnRoulette.SetNotice(presenter.GetRouletteButtonNotice());
            }
        }

        /// <summary>
        /// 룰렛 변경 버튼 상태 표시
        /// </summary>
        private void UpdateRouletteChangeButton()
        {
            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), nameof(YieldRemainTime), SingletonBehavior.Overwrite);
        }

        /// <summary>
        /// 룰렛 변경까지 남은시간 체크
        /// </summary>
        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                float time = presenter.GetChangeRemainTime().ToRemainTime();
                if (time <= 0)
                    break;

                btnChange.SetActive(true);
                btnChange.SetValue(presenter.GetNeedChangeCatCoin().ToString());
                labelTime.Text = presenter.GetChangeRemainTime().ToStringTime();
                btnChangeFree.SetActive(false);
                yield return Timing.WaitForSeconds(0.5f);
            }
            btnChange.SetActive(false);
            btnChangeFree.SetActive(true);
        }

        /// <summary>
        /// 룰렛판 뽑기 후 정보 갱신
        /// </summary>
        private void OnUpdateRoulette()
        {
            UpdateRewardInfo();
            UpdateOwnedCount();
            UpdateRouletteButton();
        }

        /// <summary>
        /// 룰렛판 변경 후 정보 갱신
        /// </summary>
        private void OnRouletteChange()
        {
            UpdateRewardInfo();
            UpdateOwnedCount();
            UpdateRouletteButton();
            UpdateRouletteChangeButton();
        }

        /// <summary>
        /// 뽑기 연출 시작
        /// </summary>
        void OnPlayRouletteEffect()
        {
            Timing.RunCoroutineSingleton(YieldRouletteEffect().CancelWith(gameObject), nameof(YieldRouletteEffect), SingletonBehavior.Overwrite);
        }

        /// <summary>
        /// 뽑기 연출 후 결과 팝업
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldRouletteEffect()
        {
            isPlayingEffect = true;
            touchBlock.SetActive(true);
            rouletteFx.SetActive(true);
            slotFx.SetActive(false);
            backFx.SetActive(false);

            yield return Timing.WaitForSeconds(fxTime);

            isPlayingEffect = false;
            touchBlock.SetActive(false);
            rouletteFx.SetActive(false);
            slotFx.SetActive(true);
            backFx.SetActive(true);
            presenter.ShowRewardPopup();
        }

        /// <summary>
        /// 룰렛 뽑기 버튼 이벤트
        /// </summary>
        void OnClickedBtnRoulette()
        {
            presenter.RequestRoulette();
        }

        /// <summary>
        /// 룰렛판 변경 버튼 이벤트
        /// </summary>
        void OnClickedBtnChange()
        {
            presenter.RequestChangeRoulette();
        }

        /// <summary>
        /// 소모 아이템 정보 버튼 이벤트
        /// </summary>
        void OnClickedBtnRouletteItemInfo()
        {
            presenter.ShowUseItemInfo();
        }

        /// <summary>
        /// 도움말 버튼 이벤트
        /// </summary>
        void OnClickedBtnHelp()
        {
            presenter.ShowRewerdList();
        }

        void OnUpdateNotice()
        {
            btnRouletteFree.SetNotice(!presenter.IsJobLevelLimit());
        }

        void OnClickedBtnCatcoin()
        {
            UI.ShowCashShop();
        }

        void OnClickedBtnRouletteItem()
        {
            UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.EveryDay);
        }

        public override bool Find()
        {
            elements = GetComponentsInChildren<UISpecialRouletteElement>();
            return true;
        }

        public static void ShowByConfig()
        {
            int index = BasisType.SPECIAL_ROULETTE_SKIN.GetInt();
            ISpecialRouletteConfigContainer container = AssetManager.Instance;
            SpecialRouletteConfig.Config config = container.Get(index);
            if (config == null)
            {
                UI.ShortCut<UISpecialRoulette>();
                return;
            }

            config.ShortcutCanvas();
        }
    }
}