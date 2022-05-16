using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIEventQuestBanner : UIInfo<EventBannerPresenter, EventQuestGroupInfo>, IAutoInspectorFinder
    {
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelRemainTime;
        [SerializeField] UITextureHelper texBanner;
        [SerializeField] UIButtonHelper btnBanner;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] GameObject notice;
        [SerializeField] GameObject dateBase;
        [SerializeField] GameObject onBuffPointBase;
        [SerializeField] UILabelHelper labelOnBuffPointTitle;
        [SerializeField] UILabelHelper labelOnBuffPoint;

        protected override void Awake()
        {
            base.Awake();

            if (btnBanner)
                EventDelegate.Add(btnBanner.OnClick, OnClickedBtnBanner);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (btnBanner)
                EventDelegate.Remove(btnBanner.OnClick, OnClickedBtnBanner);

            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            if (labelOnBuffPointTitle)
            {
                labelOnBuffPointTitle.LocalKey = LocalizeKey._11438; // 남은 OnBuff 포인트
            }
        }

        protected override void Refresh()
        {
            if (IsInvalid())
                return;

            labelName.LocalKey = LocalizeKey._11001; // EVENT
            labelTitle.Text = info.Name;
            labelDescription.Text = info.Description;
            if (notice)
            {
                if (info.ShortcutType == ShortCutType.Bingo)
                {
                    notice.SetActive(presenter.IsBingoQuestStandByReward());
                }
                else if (info.ShortcutType == ShortCutType.SpecialRoulette)
                {
                    notice.SetActive(presenter.IsSpecialRouletteNotice());
                }
                else if (info.ShortcutType == ShortCutType.AttendEvent)
                {
                    notice.SetActive(presenter.IsAttendEventStandByReward());
                }
                else if (info.ShortcutType == ShortCutType.WordCollectionEvent)
                {
                    notice.SetActive(presenter.IsWordCollectionStandByReward());
                }
                else
                {
                    notice.SetActive(info.IsEventQuestStandByReward());
                }
            }

            bool isOnBuffPoint = false;

            if (onBuffPointBase)
            {
                if (info.ShortcutType == ShortCutType.OnBuffEvent)
                {
                    isOnBuffPoint = true;
                    onBuffPointBase.SetActive(true);
                    labelOnBuffPoint.Text = info.GetOnBuffTotalRemainPoint().ToString("N0");
                }
                else
                {
                    isOnBuffPoint = false;
                    onBuffPointBase.SetActive(false);
                }
            }

            if (isOnBuffPoint)
            {
                Timing.KillCoroutines(gameObject);
                dateBase.SetActive(false);
            }
            else if (info.RemainTime.ToRemainTime() > 0)
            {
                Timing.KillCoroutines(gameObject);
                Timing.RunCoroutine(ShowRemainTime(), gameObject);
            }

            texBanner.SetFromUrl(info.ImageUrl);
        }

        IEnumerator<float> ShowRemainTime()
        {
            while (info.RemainTime.ToRemainTime() > 0)
            {
                var time = info.RemainTime.ToRemainTime().ToTimeSpan();

                // 1000일 이상 남은 이벤트는 시간 표시 X
                if (time.Days >= 1000)
                {
                    dateBase.SetActive(false);
                }
                else
                {
                    dateBase.SetActive(true);
                    labelRemainTime.Text = LocalizeKey._11005.ToText()
                        .Replace("{DAY}", time.Days.ToString())
                        .Replace("{HOUR}", time.Hours.ToString())
                        .Replace("{MINUTE}", time.Minutes.ToString());
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        void OnClickedBtnBanner()
        {
            if (info == null)
                return;

            if (Tutorial.isInProgress)
            {
                UI.ShowToastPopup(LocalizeKey._26030.ToText()); // 튜토리얼 중에는 이용할 수 없습니다.
                return;
            }

            ShortCutType shortCutType = info.ShortcutType;
            if (shortCutType == ShortCutType.None || shortCutType == ShortCutType.OnBuffEvent)
            {
                UI.Show<UIEvent>(info);
            }
            else if (shortCutType == ShortCutType.Empty)
            {
                // Empty
            }
            else if (shortCutType == ShortCutType.Coupon)
            {
                presenter.ShowCouponPopup();
            }
            else
            {
                presenter.CloseUI();
                shortCutType.GoShortCut(info.ShortcutValue, info.RemainTime);
            }
        }
    }
}