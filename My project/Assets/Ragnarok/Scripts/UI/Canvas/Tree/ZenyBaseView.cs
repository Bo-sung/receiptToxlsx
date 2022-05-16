using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class ZenyBaseView : UISubCanvas<DailyCheckPresenter>, IAutoInspectorFinder
    {
        [SerializeField] UILabelValue labelCumulativeZeny;
        [SerializeField] UILabelValue labelRewardZeny;
        [SerializeField] UIButtonHelper btnGetReward;    
        [SerializeField] GameObject treePackImage;

        protected override void OnInit()
        {
            
            EventDelegate.Add(btnGetReward.OnClick, OnClickedBtnGetReward);
        }
        
        protected override void OnClose()
        {
            EventDelegate.Remove(btnGetReward.OnClick, OnClickedBtnGetReward);
        }
        
        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnHide() { }
        
        protected override void OnLocalize()
        {
            labelRewardZeny.TitleKey = LocalizeKey._9010; // 누적 제니: 
            btnGetReward.LocalKey = LocalizeKey._9009; // 보상 획득
            labelCumulativeZeny.TitleKey = LocalizeKey._9012; // 누적 접속 시간: 
        }

        public void Refresh()
        {
            Timing.RunCoroutineSingleton(YieldCumulativeTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldCumulativeTime()
        {
            while (true)
            {
                btnGetReward.IsEnabled = presenter.ZenyTreeReward > 0;
                labelRewardZeny.Value = presenter.ZenyTreeReward.ToString("N0");
                labelCumulativeZeny.Value = LocalizeKey._9007.ToText()
                    .Replace("{MINUTE}", presenter.CurZenyTreeTime.ToTotalMinute()); // {MINUTE}분
                yield return Timing.WaitForSeconds(1f);
            }
        }

        private IEnumerator<float> YieldRemainTime()
        {
            while (true)
            {
                float time = presenter.GetTreePackRemainTime();
                if (time <= 0)
                    break;

                treePackImage.SetActive(true);
                yield return Timing.WaitForSeconds(0.5f);
            }
            treePackImage.SetActive(false);
        }

        void OnClickedBtnGetReward()
        {
            presenter.RequestGetTreeReward(isZenyTree: true);
        }        
    }
}
