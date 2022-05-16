using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class MaterialBaseView : UISubCanvas<DailyCheckPresenter>, IAutoInspectorFinder
    {
        [SerializeField] UILabelValue labelCumulativeMaterial;
        [SerializeField] UIButtonHelper btnGetReward;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] GameObject treePackImage;

        MaterialRewardInfo[] arrayInfo;

        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
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
            labelCumulativeMaterial.TitleKey = LocalizeKey._9012; // 누적 접속 시간: 
            btnGetReward.LocalKey = LocalizeKey._9009; // 보상 획득
            labelDescription.LocalKey = LocalizeKey._9014; // 등장 아이템
        }

        public void Refresh()
        {
            arrayInfo = presenter.GetMaterialRewardInfos();
            wrapper.Resize(arrayInfo.Length);
            Timing.RunCoroutineSingleton(YieldCumulativeTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            Timing.RunCoroutineSingleton(YieldRemainTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private IEnumerator<float> YieldCumulativeTime()
        {
            while (true)
            {
                btnGetReward.IsEnabled = presenter.IsMaterialTreeReward;
                labelCumulativeMaterial.Value = LocalizeKey._9007.ToText()
                    .Replace("{MINUTE}", presenter.CurMaterialTreeTime.ToTotalMinute()); // {MINUTE}분
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
            presenter.RequestGetTreeReward(isZenyTree: false);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            TreeRewardSlot ui = go.GetComponent<TreeRewardSlot>();
            ui.SetData(arrayInfo[index]);
        }
    }
}
