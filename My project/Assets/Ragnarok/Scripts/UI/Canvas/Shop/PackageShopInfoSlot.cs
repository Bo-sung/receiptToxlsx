using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class PackageShopInfoSlot : UIInfo<ShopPresenter, ShopInfo>
    {
        [SerializeField] UILabelHelper labelTitleName;
        [SerializeField] UITextureHelper itemIcon;
        [SerializeField] UIButtonHelper btnPurchase;
        [SerializeField] UILabelHelper labelPrice;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] GameObject time;
        [SerializeField] UILabelHelper labelLimitTime;
        [SerializeField] UILabelHelper labelBuyLimit;
        [SerializeField] GameObject lockBase;
        [SerializeField] UILabelHelper labelJobLevel;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnPurchase.OnClick, OnClickedBtnPurchase);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnPurchase.OnClick, OnClickedBtnPurchase);

            if (presenter != null)
            {
                presenter.OnUpdateRewardPackageAchieve -= UpdateNotice;
                presenter.OnUpdateEveryDayGoods -= UpdateNotice;
            }
        }

        public override void Initialize(ShopPresenter presenter)
        {
            base.Initialize(presenter);

            presenter.OnUpdateRewardPackageAchieve += UpdateNotice;
            presenter.OnUpdateEveryDayGoods += UpdateNotice;
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            labelTitleName.Text = info.Name;
            itemIcon.SetShop(info.IconName);
            labelPrice.Text = info.CostText;
            labelDescription.Text = info.Description;

            UpdateNotice();

            SetBuyLimit();
            SetTime();
            SetJobLevelLimit();
        }

        private void UpdateNotice()
        {
            btnPurchase.SetNotice(presenter.GetHasPackageNotice(info.PackageType));
        }

        /// <summary>
        /// 구매 제한 표시
        /// </summary>
        private void SetBuyLimit()
        {
            // 1회 구매 제한이 있는 패키지
            if (info.PackageType.IsBuyLimitPackage())
            {
                string limitText = info.ShopLimitType == ShopLimitType.Account ? LocalizeKey._8054.ToText() : string.Empty; // (계정)
                if (presenter.CanBuyLimit(info))
                {
                    labelBuyLimit.Text = LocalizeKey._8046.ToText().Replace(ReplaceKey.NAME, limitText); // 구매 한정{NAME} (1/1)
                }
                else
                {
                    labelBuyLimit.Text = LocalizeKey._8047.ToText().Replace(ReplaceKey.NAME, limitText); // 구매 한정{NAME} (0/1)
                }
            }
            else if (info.PackageType.IsPeriodProductPackage())
            {
                string limitText = info.ShopLimitType == ShopLimitType.Account ? LocalizeKey._8054.ToText() : string.Empty; // (계정)
                if (presenter.CanBuyLimit(info))
                {
                    labelBuyLimit.Text = LocalizeKey._8052.ToText().Replace(ReplaceKey.NAME, limitText); // 기간 한정{NAME} (1/1)
                }
                else
                {
                    labelBuyLimit.Text = LocalizeKey._8053.ToText().Replace(ReplaceKey.NAME, limitText); // 기간 한정{NAME} (0/1)
                }
            }
            else if (info.IsBuyLimit)
            {
                labelBuyLimit.Text = $"{info.GetLimitDayTypeToText()} ({info.BuyLimitCount - info.BuyCount}/{info.BuyLimitCount})";
            }
            else
            {
                labelBuyLimit.Text = string.Empty;
            }
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
        /// <param name="time"></param>
        private void UpdateLimitTime(float time)
        {
            if (time <= 0)
            {
                labelLimitTime.Text = "00:00";
                return;
            }

            // UI 표시에 1분을 추가해서 보여준다.
            TimeSpan span = TimeSpan.FromMilliseconds(time + 60000);

            int totalDays = (int)span.TotalDays;
            bool isDay = totalDays > 0;

            if (isDay)
            {
                labelLimitTime.Text = LocalizeKey._8045.ToText().Replace(ReplaceKey.TIME, totalDays); // D-{TIME}
            }
            else
            {
                labelLimitTime.Text = span.ToString(@"hh\:mm");
            }
        }

        void OnClickedBtnPurchase()
        {
            presenter.ShowPackagePopup(info);
        }
    }
}