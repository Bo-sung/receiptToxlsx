using UnityEngine;

namespace Ragnarok
{
    public class UIConsumableInfo : UICanvas, ConsumableInfoPresenter.IView
    {
        public enum SelectType
        {
            None = 0,
            Count = 1,
            Slider = 2,
        }

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UISprite header;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UILabelHelper labelOwned;
        [SerializeField] UILabelHelper labelOwnedValue;

        [SerializeField] UIConsumableProfile itemProfile;
        [SerializeField] UIBattleOptionList optionList;

        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnUseSource;
        [SerializeField] UIButtonHelper btnGetSource;
        [SerializeField] GameObject goGetSourceLine;
        [SerializeField] GameObject goUseSourceLine;

        [SerializeField] GameObject goButtonBase; // 일반 확인/사용 버튼
        [SerializeField] UIButtonHelper btnConfirm;

        [SerializeField] SelectType default_CountType;

        [SerializeField] GameObject goSliderBase; // 슬라이더
        [SerializeField] UISlider slider;  //슬라이더
        [SerializeField] UIButtonHelper btnConfirm_Slider; //슬라이더 모드 확인 버튼
        [SerializeField] UIButton btnMinus, btnPlus; //슬라이더 버튼
        [SerializeField] UILabel sliderLabel; // 슬라이더 라벨
        [SerializeField] GameObject maxSprite; // 맥스 라벨
        [SerializeField] UISprite prograssBar;//슬라이더 스프라이트

        [SerializeField] GameObject goCountBase; // [상자] 사용 수량 + 사용 버튼
        [SerializeField] UIButtonHelper btnConfirm_Count; // [상자] 사용 버튼

        [SerializeField] UIButtonHelper btnBoxReward;

        [SerializeField] ElementCount elementCountSetter;

        private ConsumableInfoPresenter presenter;
        int useCount;
        private SelectType type;

        /// <summary>
        /// 기초데이터로 불러온 최댓값
        /// </summary>
        int BaseMaxCount
        {
            //기초데이터 생기면 대체. 임시로 최대 잡레벨만큼으로
            get => BasisType.CONSUMABLE_ITEM_USE_LIMIT.GetInt();
        }

        int CurMaxCount
        {
            get => Mathf.Min(BaseMaxCount, presenter.info.ItemCount);
        }

        protected override void OnInit()
        {
            presenter = new ConsumableInfoPresenter(this);
            presenter.AddEvent();

            EventDelegate.Add(background.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, OnClickedBtnUse);
            EventDelegate.Add(btnConfirm_Count.OnClick, OnClickedBtnUse);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Add(btnUseSource.OnClick, OnClickBtnUseSource);
            EventDelegate.Add(btnBoxReward.OnClick, OnClickedBtnBoxReward);

            EventDelegate.Add(btnConfirm_Slider.OnClick, OnClickedBtnUse);
            EventDelegate.Add(slider.onChange, OnChangedSlider);
            EventDelegate.Add(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Add(btnMinus.onClick, OnClickedBtnMinus);

            elementCountSetter.SetInputEnable(false);
            elementCountSetter.OnRefresh += OnSetterRefresh;
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(background.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, OnClickedBtnUse);
            EventDelegate.Remove(btnConfirm_Count.OnClick, OnClickedBtnUse);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnGetSource.OnClick, OnClickBtnGetSource);
            EventDelegate.Remove(btnUseSource.OnClick, OnClickBtnUseSource);
            EventDelegate.Remove(btnBoxReward.OnClick, OnClickedBtnBoxReward);

            EventDelegate.Remove(btnConfirm_Slider.OnClick, OnClickedBtnUse);
            EventDelegate.Remove(slider.onChange, OnChangedSlider);
            EventDelegate.Remove(btnPlus.onClick, OnClickedBtnPlus);
            EventDelegate.Remove(btnMinus.onClick, OnClickedBtnMinus);


            elementCountSetter.OnRefresh -= OnSetterRefresh;
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._44000; // 아이템 정보
            labelOwned.LocalKey = LocalizeKey._44005; // 보유;
            btnBoxReward.LocalKey = LocalizeKey._44006; // 구성품 보기
            btnConfirm_Slider.LocalKey = LocalizeKey._2906;// 사용
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is ItemInfo)
            {
                SetUseCount(1);
                presenter.SelectInfo(data as ItemInfo);
                itemProfile.SetData(presenter.info);
                return;
            }

            Debug.LogError("data가 존재하지 않습니다");
            CloseUI();
        }

        public void Refresh()
        {
            ItemInfo info = presenter.info;
            if (info == null)
                return;

            optionList.SetData(info);

#if UNITY_EDITOR
            labelItemName.Text = $"{info.Name}({info.ItemId})";
#else
            labelItemName.Text = info.Name;
#endif
            labelDesc.Text = info.Description;

            if ((info.ItemType == ItemType.Box || (info.IsBuff && info.Cooldown <= 1)) && !info.IsCooldown())
                type = default_CountType;
            else
                type = SelectType.None;
            SetSelectType();

            bool hasGetSource = (info.Get_ClassBitType != ItemSourceCategoryType.None);
            btnGetSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasGetSource))
                .Append(LocalizeKey._44002.ToText()).Release(); // 획득처 보기
            goGetSourceLine.SetActive(hasGetSource);

            bool hasUseSource = (info.Use_ClassBitType != ItemSourceCategoryType.None);
            btnUseSource.Text = StringBuilderPool.Get()
                .Append(UIItemSource.GetActivationColorString(isActive: hasUseSource))
                .Append(LocalizeKey._44001.ToText()).Release(); // 사용처 보기
            goUseSourceLine.SetActive(hasUseSource);

            labelOwnedValue.Text = presenter.GetItemCount().ToString();

            bool isBox = info.ItemType == ItemType.Box;
            bool isJobBox = info.BoxType == BoxType.JobRef || info.BoxType == BoxType.JobGradeRef;

            // 직업 전용 상자가 아닐때
            if (isBox && !isJobBox)
            {
                btnBoxReward.SetActive(true);
            }
            else
            {
                btnBoxReward.SetActive(false);
            }
        }

        public void CloseUI()
        {
            UI.Close<UIConsumableInfo>();
        }

        // 소모품 사용, 확인
        void OnClickedBtnUse()
        {
            ItemInfo info = presenter.info;

            if (info.IsCooldown())
            {
                CloseUI();
                return;
            }

            // 셰어부스트 아이템 컨텐츠 언락 체크
            foreach (var option in info.GetBattleOptionCollection())
            {
                if (option.battleOptionType == BattleOptionType.ShareBoost)
                {
                    if (!Entity.player.Quest.IsOpenContent(ContentType.ShareLevelUp))
                    {
                        CloseUI();
                        return;
                    }
                }
            }

            switch (type)
            {
                case SelectType.None:
                    {
                        presenter.RequestUseConsumableItem().WrapNetworkErrors();
                    }
                    break;
                case SelectType.Count:
                    {
                        presenter.RequestUseConsumableItem(useCount).WrapNetworkErrors();
                    }
                    break;
                case SelectType.Slider:
                    {
                        presenter.RequestUseConsumableItem(useCount).WrapNetworkErrors();
                    }
                    break;
            }
        }

        /// <summary>
        /// 획득처 보기
        /// </summary>
        void OnClickBtnGetSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.GetSource, presenter.info));
        }

        /// <summary>
        /// 사용처 보기
        /// </summary>
        void OnClickBtnUseSource()
        {
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.Use, presenter.info));
        }

        void OnClickedBtnPlus()
        {
            SetUseCount(Mathf.Min(++useCount, CurMaxCount));
        }

        void OnClickedBtnMinus()
        {
            SetUseCount(Mathf.Max(--useCount, 1));
        }

        /// <summary>
        /// useCount 설정
        /// </summary>
        void SetUseCount(int count)
        {
            useCount = count;
            RefreshSlider(useCount);
        }

        /// <summary>
        /// 구성품 보기 버튼
        /// </summary>
        void OnClickedBtnBoxReward()
        {
            if (presenter.info == null)
                return;

            UI.Show<UIBoxRewardList>().Set(presenter.info.ItemId);
        }

        void OnSetterRefresh()
        {
            if (presenter.info == null)
                return;
            //선택된 수 받아옴.
            SetUseCount(elementCountSetter.Count);
        }

        void SetSelectType()
        {
            switch (type)
            {
                case SelectType.None:
                    {
                        header.height = 535;
                        goButtonBase.SetActive(true);
                        goCountBase.SetActive(false);
                        goSliderBase.SetActive(false);

                        if (presenter.info.IsCooldown())
                            btnConfirm.LocalKey = LocalizeKey._44003; // 확인
                        else
                            btnConfirm.LocalKey = LocalizeKey._44004; // 사용
                        return;
                    }
                case SelectType.Count:
                    {
                        header.height = 535;
                        goButtonBase.SetActive(false);
                        goCountBase.SetActive(true);
                        goSliderBase.SetActive(false);
                        SetUseCount(useCount);

                        elementCountSetter.Initiallize(useCount, CurMaxCount);
                        btnConfirm_Count.LocalKey = LocalizeKey._44004; // 사용
                        return;
                    }
                case SelectType.Slider:
                    {
                        header.height = 585;
                        goButtonBase.SetActive(false);
                        goCountBase.SetActive(false);
                        goSliderBase.SetActive(true);
                        SetUseCount(useCount);
                        btnConfirm_Count.LocalKey = LocalizeKey._44004; // 사용
                        btnConfirm_Slider.IsEnabled = useCount != 0;
                        return;
                    }
            }
        }


        void OnChangedSlider()
        {
            int curMax = Mathf.Min(BaseMaxCount, presenter.info.ItemCount);
            useCount = (int)(curMax * slider.value) + 1;
            int current = Mathf.Min(curMax, useCount);
            sliderLabel.text = useCount.ToString();
            SetUseCount(current);
            maxSprite.SetActive(current == curMax);
            btnConfirm_Slider.IsEnabled = current != 0;
        }

        private void RefreshSlider(int input)
        {
            if (type != SelectType.Slider)
                return;

            if (BaseMaxCount == 0)
            {
                slider.value = 0f;
            }
            else
            {
                // 1부터 시작
                slider.value = MathUtils.GetProgress(input - 1, CurMaxCount - 1);
                sliderLabel.text = input.ToString();
                maxSprite.SetActive(input == CurMaxCount);
                btnConfirm_Slider.IsEnabled = input != 0;
            }
        }

    }
}