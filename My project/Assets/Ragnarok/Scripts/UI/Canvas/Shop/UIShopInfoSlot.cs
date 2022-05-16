using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIShopInfoSlot : UIInfo<ShopPresenter, ShopInfo>
    {
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIRewardHelper itemBase;
        [SerializeField] UIRewardHelper itemInfo;
        [SerializeField] GameObject noneItemBase;
        [SerializeField] UITextureHelper noneItemIcon;
        [SerializeField] UILabelHelper labelNoneItemCount;
        [SerializeField] UILabelHelper labelBuyLimit;
        [SerializeField] UIButtonHelper btnPurchase;
        [SerializeField] UITextureHelper goodsIcon;
        [SerializeField] UILabelHelper labelPrice;
        [SerializeField] UIGrid state;
        [SerializeField] GameObject stateNew, stateBest, stateHot;

        [SerializeField] GameObject time;
        [SerializeField] UILabelHelper labelLimitTime;

        [SerializeField] UISprite backgroundTime;
        [SerializeField] Color32 backgroundTimeDayColor, labelTimeDayColor;
        [SerializeField] Color32 backgroundTimeHourColor, labelTimeHourColor;

        // sale, first, addGoods는 동시에 사용할수 없다.
        [SerializeField] GameObject sale;
        [SerializeField] UILabelHelper labelSalePrice;
        [SerializeField] UILabelHelper labelSalePersent;
        [SerializeField] UILabelHelper labelSaleOff;

        [SerializeField] GameObject first;
        [SerializeField] UILabelHelper LabelFirstBonus;
        [SerializeField] UILabelHelper labelFirstPersent;
        [SerializeField] UILabelHelper labelFirstAddAmount;

        [SerializeField] GameObject addGoods;
        [SerializeField] UILabelHelper labelAddGoodsBonus;
        [SerializeField] UITextureHelper iconAddGoods;
        [SerializeField] UILabelHelper labelAddGoodsCount;

        [SerializeField] GameObject soldOutCover;
        [SerializeField] GameObject lockBase;
        [SerializeField] UILabelHelper labelJobLevel;

        [SerializeField] GameObject limitTime;
        [SerializeField] UILabelHelper labelBuyLimitTime;

        [SerializeField] GameObject fxDailyFree;

        GameObject itemInfoObject;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnPurchase.OnClick, OnClickedBtnPurchase);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnPurchase.OnClick, OnClickedBtnPurchase);
        }

        protected override void OnLocalize()
        {
            LabelFirstBonus.LocalKey = LocalizeKey._8043; // Bonus
            labelAddGoodsBonus.LocalKey = LocalizeKey._8043; // Bonus
            labelSaleOff.LocalKey = LocalizeKey._8044; // SaleOff
        }

        protected override void Refresh()
        {
            // 상점 탭 이동시 기존 코루틴 중지 후 재실행
            // 오브젝트가 꺼졌다 바로 켜질경우 코루틴이 중지되지 않는 현상 방지
            Timing.KillCoroutines(gameObject);

            if (IsInvalid())
                return;

            if (itemInfoObject == null) itemInfoObject = itemInfo.gameObject;

            labelName.Text = info.Name; // 상점 아이템 타이틀        
            SetReward();
            SetPrice();
            SetBuyLimit();
            SetState();
            SetSale();
            SetTime();
            SetFirst();
            SetAddGoods();
            SetSoldOut();
            SetJobLevelLimit();
            SetFxDailyFree();
        }

        /// <summary>
        /// 보상 표시
        /// </summary>
        private void SetReward()
        {
            bool isItemIcon = info.IsValueIconName();

            if (isItemIcon)
            {
                // 재화나, 티켓 종류
                itemBase.SetData(null);

                if (info.Reward == null)
                {
                    noneItemBase.SetActive(false);
                    return;
                }
                noneItemBase.SetActive(true);
                noneItemIcon.Set(info.IconName);
                labelNoneItemCount.Text = info.Reward.Count.ToString();
            }
            else
            {
                itemBase.SetData(info.Reward);
                noneItemBase.SetActive(false);
            }

            itemInfo.SetData(info.Reward);
            itemInfoObject.SetActive(info.RewardType.ToCoinType() == default); // 획득 아이템이 코인 종류가 아닐때만 표시
        }

        /// <summary>
        /// 가격 표시
        /// </summary>
        private void SetPrice()
        {
            bool isGoods = info.CoinType.IsGoods();

            if (isGoods)
            {
                goodsIcon.SetActive(true);
                goodsIcon.Set(info.CoinType.IconName());
            }
            else
            {
                // 광고, 현금은 재화 아이콘을 안씀
                goodsIcon.SetActive(false);
            }
            labelPrice.Text = info.CostText;
        }

        /// <summary>
        /// 구매 제한 표시
        /// </summary>
        private void SetBuyLimit()
        {
            if (info.IsBuyLimit)
            {
                labelBuyLimit.Text = $"{info.GetLimitDayTypeToText()} ({info.BuyLimitCount - info.BuyCount}/{info.BuyLimitCount})";
            }
            else
            {
                labelBuyLimit.Text = info.GetLimitDayTypeToText();
            }
        }

        /// <summary>
        /// 아이템 상태표시
        /// </summary>
        private void SetState()
        {
            bool isNew = info.ShopState.HasFlag(ShopState.New);
            bool isBest = info.ShopState.HasFlag(ShopState.Best);
            bool isHot = info.ShopState.HasFlag(ShopState.Hot);
            stateNew.SetActive(isNew);
            stateBest.SetActive(isBest);
            stateHot.SetActive(isHot);
            state.Reposition();
        }

        /// <summary>
        /// 세일 표시
        /// </summary>
        private void SetSale()
        {
            if (info.IsSale())
            {
                sale.SetActive(true);
                labelSalePrice.Text = info.GetSalePrice();
                labelSalePersent.Text = info.GetSalePersent();
            }
            else
            {
                sale.SetActive(false);
            }
        }

        private void SetTime()
        {
            if (info.IsPeriedItem())
            {
                // 기간제 상품
                time.SetActive(true);
                Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                time.SetActive(false);
            }

            // 단위 기간 상품
            limitTime.SetActive(false);
            if (info.SellType == SellType.ResetPeriodProduct)
            {
                // 남은 시간이 있고 영구구매횟수 제한이 아닐때 시간체크
                if (info.IsSetResetRemainTime && info.LimitDayType != LimitDayType.NoLimit)
                {
                    Timing.RunCoroutineSingleton(YieldBuyLimitResetRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
                }
            }
        }

        /// <summary>
        /// 기간제 아이템 남은시간 체크
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                float time = info.RemainTime.ToRemainTime();
                if (time <= 0)
                    break;

                UpdateLimitTime(time);
                yield return Timing.WaitForSeconds(0.5f);
            }
            UpdateLimitTime(0);
        }

        /// <summary>
        /// 기간제 아이템 남은시간 표시
        /// </summary>
        private void UpdateLimitTime(float time)
        {
            if (time <= 0)
            {
                SetLimitTimeColor(isDay: false);
                labelLimitTime.Text = "00:00";
                return;
            }

            // UI 표시에 1분을 추가해서 보여준다.
            TimeSpan span = TimeSpan.FromMilliseconds(time + 60000);

            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            SetLimitTimeColor(isDay);

            if (isDay)
            {
                labelLimitTime.Text = LocalizeKey._8041.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}
            }
            else
            {
                labelLimitTime.Text = span.ToString(@"hh\:mm");
            }
        }

        /// <summary>
        /// 구매 횟수 제한 상품 초기화까지 남은시간 체크
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldBuyLimitResetRemainTime()
        {
            while (true)
            {
                float time = info.ResetRemainTime.ToRemainTime();

                if (time <= 0)
                    break;

                SetBuyReaminTime(time);
                yield return Timing.WaitForSeconds(0.5f);
            }
            info.SetBuyCount(0);
            SetBuyLimit();
            SetBuyReaminTime(0);
        }

        /// <summary>
        /// 구매 가능까지 남은 시간 표시
        /// </summary>
        private void SetBuyReaminTime(float time)
        {
            if (time <= 0)
            {
                limitTime.SetActive(false);
                return;
            }

            TimeSpan span = TimeSpan.FromMilliseconds(time);

            if (info.IsBuyLimitResetType())
            {
                limitTime.SetActive(true);
                int totalHours = (int)span.TotalHours;
                int minutes = span.Minutes;
                int seconds = span.Seconds;
                labelBuyLimitTime.Text = LocalizeKey._8055.ToText().Replace(ReplaceKey.TIME, $"{totalHours:00}:{minutes:00}:{seconds:00}"); // 남은 시간\n{TIME}
                return;
            }

            limitTime.SetActive(false);
        }

        /// <summary>
        /// 기간제 아이템 남은시간에 따른 표시 방식 변경
        /// 배경색과, 글자색 변경
        /// </summary>
        /// <param name="isDay"></param>
        private void SetLimitTimeColor(bool isDay)
        {
            if (isDay)
            {
                labelLimitTime.uiLabel.color = labelTimeDayColor;
                backgroundTime.color = backgroundTimeDayColor;
            }
            else
            {
                // 하루 미만으로 남았을 경우 표시
                labelLimitTime.uiLabel.color = labelTimeHourColor;
                backgroundTime.color = backgroundTimeHourColor;
            }
        }

        /// <summary>
        /// 첫구매 상품
        /// </summary>
        private void SetFirst()
        {
            // 첫구매 보상 상품이 아님
            if (!info.IsFirstBuyItem())
            {
                first.SetActive(false);
                return;
            }

            float rate = MathUtils.ToPermilleValue(info.FirstRate); // 추가 보상 비율 1000분률
            int amount = MathUtils.ToInt(info.Reward.Count * rate); // 추가 보상 수량
            string persent = MathUtils.GetPercentText(rate);

            labelFirstPersent.Text = persent;
            labelFirstAddAmount.Text = LocalizeKey._8042.ToText().Replace(ReplaceKey.VALUE, amount); // (+{VALUT})           

            if (info.IsSetFirstItemRemainTime)
            {
                // 영구 첫보상
                if (info.FirstDayType == FirstDayType.OneTime)
                {
                    first.SetActive(false);
                    return;
                }
                // 다음 첫구매 추가보상이 없는 경우 시간체크
                Timing.RunCoroutineSingleton(YieldFirstRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else if (info.BuyCount > 0)
            {
                first.SetActive(false);
            }
            else
            {
                first.SetActive(true);
            }
        }

        /// <summary>
        /// 첫구매 추가보상(구매한 상품과 동일) 상품 다음 추가보상까지 남은시간 체크
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldFirstRemainTime()
        {
            while (true)
            {
                float time = info.FirstRemainTime.ToRemainTime();
                if (time <= 0)
                    break;

                // 다음 첫구매까지 남은 시간이 있어 추가 보상이 없다.
                first.SetActive(false);
                yield return Timing.WaitForSeconds(0.5f);
            }
            // 다음 첫구매까지 남은 시간이 없어 추가 보상이 있다.
            info.SetBuyCount(0);
            first.SetActive(true);
        }

        /// <summary>
        /// 구매한 상품 이외의 다른 추가 상품
        /// </summary>
        private void SetAddGoods()
        {
            if (!info.IsAddGoods)
            {
                addGoods.SetActive(false);
                return;
            }

            // 냥다래 구매는 추가 보상을 보여주지 않는다
            if (info.RewardType == RewardType.CatCoin)
            {
                addGoods.SetActive(false);
                return;
            }

            RewardData addGoodsReward = info.GetAddGoodsReward();

            addGoods.SetActive(true);
            labelAddGoodsCount.Text = addGoodsReward.Count.ToString();
            iconAddGoods.Set(addGoodsReward.IconName);
        }

        /// <summary>
        /// 매진 세팅
        /// </summary>
        private void SetSoldOut()
        {
            if (info.IsBuyLimitResetType())
            {
                soldOutCover.SetActive(false);
                return;
            }
            soldOutCover.SetActive(!info.CanBuyLimit);
        }

        /// <summary>
        /// 직업 레벨 제한 세팅
        /// </summary>
        private void SetJobLevelLimit()
        {
            if (info.ShopVisableType == ShopVisableType.JobLevel)
            {
                int jobLevel = presenter.GetJobLevel();

                if (jobLevel >= info.JobLevel)
                {
                    // 구매 가능
                    lockBase.SetActive(false);
                }
                else
                {
                    // 구매 불가 (직업레벨 미도달)
                    lockBase.SetActive(true);
                    labelJobLevel.Text = LocalizeKey._8051.ToText().Replace(ReplaceKey.LEVEL, info.JobLevel); // [FCCC37]JOB Lv.{LEVEL}[-]\필요
                }
            }
            else
            {
                lockBase.SetActive(false);
            }
        }

        /// <summary>
        /// 무료 구매 아이템 이펙트 표시 여부 셋팅
        /// </summary>
        private void SetFxDailyFree()
        {
            // 직업 레벨 미도달시 이펙트 Hide
            if (info.ShopVisableType == ShopVisableType.JobLevel)
            {
                int jobLevel = presenter.GetJobLevel();

                if (jobLevel < info.JobLevel)
                {
                    fxDailyFree.SetActive(false);
                    btnPurchase.SetNotice(false);
                    return;
                }
            }

            // 이미 구매한 경우 이펙트 Hide
            if (!info.CanBuyLimit)
            {
                fxDailyFree.SetActive(false);
                btnPurchase.SetNotice(false);
                return;
            }

            bool isFree = info.CoinType == CoinType.Free;
            fxDailyFree.SetActive(isFree);
            btnPurchase.SetNotice(isFree);
        }

        void OnClickedBtnPurchase()
        {
#if UNITY_EDITOR
            ShopData data = ShopDataManager.Instance.Get(info.ID);
            Debug.Log($"상점ID={info.ID}");
            Debug.Log(data.GetDump());
#endif
            presenter.RequestShopPurchase(info);
        }
    }
}