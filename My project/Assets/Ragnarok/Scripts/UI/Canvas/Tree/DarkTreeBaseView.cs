using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DarkTreeBaseView : UISubCanvas<DailyCheckPresenter>
    {
        [SerializeField] UIButtonHelper btnStart, btnReceive;
        [SerializeField] GameObject goSelectReward;
        [SerializeField] UILabelHelper labelSelectReward;
        [SerializeField] UIButtonHelper btnSelectReward;
        [SerializeField] GameObject goMaterialBase;
        [SerializeField] UILabelValue timeBase;
        [SerializeField] UILabelHelper labelSelectedReward;
        [SerializeField] UIRewardHelper selectedReward;
        [SerializeField] UIButtonHelper btnSelectedReward;
        [SerializeField] UILabelValue needPoint;
        [SerializeField] UIAniProgressBar point;
        [SerializeField] UILabelHelper labelPoint;
        [SerializeField] UIButtonHelper btnSelectMaterial;
        [SerializeField] GameObject goResultBase;
        [SerializeField] UILabelValue remainTimeBase;
        [SerializeField] UILabelHelper labelWait, labelFinished;
        [SerializeField] UIRewardHelper result;

        protected override void OnInit()
        {
            EventDelegate.Add(btnStart.OnClick, OnClickedBtnStart);
            EventDelegate.Add(btnReceive.OnClick, OnClickedBtnReceive);
            EventDelegate.Add(btnSelectReward.OnClick, OnClickedBtnSelectReward);
            EventDelegate.Add(btnSelectedReward.OnClick, OnClickedBtnSelectedReward);
            EventDelegate.Add(btnSelectMaterial.OnClick, OnClickedBtnSelectMaterial);

            presenter.OnUpdateDarkTree += Refresh;
        }

        protected override void OnClose()
        {
            presenter.OnUpdateDarkTree -= Refresh;

            EventDelegate.Remove(btnStart.OnClick, OnClickedBtnStart);
            EventDelegate.Remove(btnReceive.OnClick, OnClickedBtnReceive);
            EventDelegate.Remove(btnSelectReward.OnClick, OnClickedBtnSelectReward);
            EventDelegate.Remove(btnSelectedReward.OnClick, OnClickedBtnSelectedReward);
            EventDelegate.Remove(btnSelectMaterial.OnClick, OnClickedBtnSelectMaterial);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnLocalize()
        {
            labelSelectReward.LocalKey = LocalizeKey._9018; // 보상
            labelSelectedReward.LocalKey = LocalizeKey._9018; // 보상
            timeBase.TitleKey = LocalizeKey._9019; // 수확 시간
            needPoint.TitleKey = LocalizeKey._9020; // 필요 포인트
            btnSelectMaterial.LocalKey = LocalizeKey._9021; // 재료 선택
            btnStart.LocalKey = LocalizeKey._9022; // 수확 시작
            remainTimeBase.TitleKey = LocalizeKey._9023; // 남은 수확 시간 :
            labelWait.LocalKey = LocalizeKey._9024; // 수확 중...
            labelFinished.LocalKey = LocalizeKey._9025; // 수확할 만큼 충분히 자랐다!
            btnReceive.LocalKey = LocalizeKey._9026; // 보상 받기
        }

        protected override void OnHide()
        {
        }

        /// <summary>
        /// 수확 시작
        /// </summary>
        void OnClickedBtnStart()
        {
            RewardData reward = presenter.DarkTree.GetSelectedReward(); // 선택한 보상
            if (reward == null)
            {
                UI.ShowToastPopup(LocalizeKey._9027.ToText()); // 수확할 보상 아이템을 선택하세요.
                return;
            }

            int curPoint = presenter.DarkTree.GetCurPoint();
            int maxPoint = presenter.DarkTree.GetMaxPoint();
            if (curPoint < maxPoint)
            {
                UI.ShowToastPopup(LocalizeKey._9028.ToText()); // 수확에 필요한 포인트가 부족합니다.\n수확에 필요한 재료를 선택하세요.
                return;
            }

            presenter.RequestDarkTreeStart(); // 수확 시작
        }

        /// <summary>
        /// 보상 받기
        /// </summary>
        void OnClickedBtnReceive()
        {
            presenter.RequestDarkTreeGetReward(); // 보상 받기
        }

        /// <summary>
        /// 보상 아이템 선택
        /// </summary>
        void OnClickedBtnSelectReward()
        {
            UI.Show<UIDarkTreeRewardSelect>();
        }

        /// <summary>
        /// 보상 아이템 변경
        /// </summary>
        void OnClickedBtnSelectedReward()
        {
            int curPoint = presenter.DarkTree.GetCurPoint();
            if (curPoint > 0)
            {
                UI.ShowToastPopup(LocalizeKey._9042.ToText()); // 수확이 진행중일 경우에는 보상 아이템을 변경할 수 없습니다.
                return;
            }

            UI.Show<UIDarkTreeRewardSelect>();
        }

        /// <summary>
        /// 재료 선택
        /// </summary>
        void OnClickedBtnSelectMaterial()
        {
            int curPoint = presenter.DarkTree.GetCurPoint();
            int maxPoint = presenter.DarkTree.GetMaxPoint();
            if (curPoint >= maxPoint)
            {
                UI.ShowToastPopup(LocalizeKey._9041.ToText()); // 더 이상 추가할 수 없습니다.
                return;
            }

            UI.Show<UIDarkTreeMaterialSelect>();
        }

        private void Refresh()
        {
            RewardData reward = presenter.DarkTree.GetSelectedReward(); // 선택한 보상
            bool hasRemainTime = presenter.DarkTree.HasRemainTime(); // 남은 시간 존재
            bool hasStandByReward = presenter.DarkTree.HasStandByReward(); // 받을 보상 존재

            // 선택한 보상 음슴
            if (reward == null)
            {
                NGUITools.SetActive(goSelectReward, true); // 선택한보상 음슴
                NGUITools.SetActive(goMaterialBase, false); // 선택한보상존재, 재료선택필요
                NGUITools.SetActive(goResultBase, false); // 선택한보상존재, 재료선택완료
                btnStart.SetActive(true);
                btnReceive.SetActive(false);
            }
            else
            {
                bool isFinishedSelectMaterial = hasRemainTime || hasStandByReward; // 재료선택완료

                NGUITools.SetActive(goSelectReward, false);
                NGUITools.SetActive(goMaterialBase, !isFinishedSelectMaterial); // 선택한보상존재, 재료선택필요
                NGUITools.SetActive(goResultBase, isFinishedSelectMaterial); // 선택한보상존재, 재료선택완료
                btnStart.SetActive(!isFinishedSelectMaterial);
                btnReceive.SetActive(isFinishedSelectMaterial);

                selectedReward.SetData(reward);
                result.SetData(reward);
            }
            
            timeBase.Value = LocalizeKey._9039.ToText() // {MINUTES}분
                .Replace(ReplaceKey.MINUTES, presenter.DarkTree.GetTotalMinutes());

            int curPoint = presenter.DarkTree.GetCurPoint();
            int maxPoint = presenter.DarkTree.GetMaxPoint();
            needPoint.Value = maxPoint.ToString("N0"); // 필요 포인트 세팅
            point.Set(curPoint, maxPoint);
            labelPoint.Text = StringBuilderPool.Get()
                .Append(curPoint).Append("/").Append(maxPoint)
                .Release();

            labelWait.SetActive(!hasStandByReward);
            labelFinished.SetActive(hasStandByReward);
            btnReceive.IsEnabled = hasStandByReward;
            btnReceive.SetNotice(hasStandByReward);

            UpdateRemainTimeText(hasRemainTime);

            Timing.KillCoroutines(gameObject);
            if (hasRemainTime)
            {
                Timing.RunCoroutine(YieldUpdateRemainTime().CancelWith(gameObject), gameObject);
            }
        }

        private IEnumerator<float> YieldUpdateRemainTime()
        {
            while (presenter.DarkTree.HasRemainTime())
            {
                UpdateRemainTimeText(true);
                yield return Timing.WaitForSeconds(1f);
            }

            UpdateRemainTimeText(false);
        }

        /// <summary>
        /// 남은 시간 업데이트
        /// </summary>
        private void UpdateRemainTimeText(bool hasRemainTime)
        {
            int remainMinitues = presenter.DarkTree.GetRemainMinitues();

            // 시간이 남아있을 경우에 한하여 1분을 추가해서 보여준다.
            if (hasRemainTime)
                remainMinitues += 1;

            remainTimeBase.Value = LocalizeKey._9039.ToText() // {MINUTES}분
                .Replace(ReplaceKey.MINUTES, remainMinitues);
        }
    }
}