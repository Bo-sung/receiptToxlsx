using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIMvpReward : UICanvas
    {
        [System.Serializable]
        private struct RewardList
        {
            [SerializeField] UIScrollView scroll;
            [SerializeField] UIGrid contentsGrid;
            [SerializeField] UILabelHelper labelTitle;

            public void SetRewards(UIRewardHelper template, List<RewardData> rewardDatas)
            {
                template.GetComponent<UIDragScrollView>().scrollView = scroll;

                for (int i = 0; i < rewardDatas.Count; ++i)
                {
                    var rewardHelper = Instantiate(template);
                    rewardHelper.transform.parent = contentsGrid.transform;
                    rewardHelper.transform.localScale = Vector3.one;
                    rewardHelper.SetData(rewardDatas[i]);
                }

                contentsGrid.Reposition();
                scroll.UpdatePosition();
                scroll.SetDragAmount(0, 0, false);
            }

            public void SetTitle(int localKey)
            {
                labelTitle.LocalKey = localKey;
            }
        }

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIRewardHelper uiRewardHelperTemplate;
        [SerializeField] UIButtonHelper[] closeButton;
        [SerializeField] RewardList[] rewardLists;

        protected override void OnInit()
        {
            EventDelegate.Add(closeButton[0].OnClick, CloseUI);
            EventDelegate.Add(closeButton[1].OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(closeButton[0].OnClick, CloseUI);
            EventDelegate.Remove(closeButton[1].OnClick, CloseUI);
        }

        protected override void OnLocalize()
        {
            closeButton[1].LocalKey = LocalizeKey._1; // 확인

            rewardLists[0].SetTitle(LocalizeKey._48901); // 집결 보상
            rewardLists[1].SetTitle(LocalizeKey._48902); // 노말 보상
            rewardLists[2].SetTitle(LocalizeKey._48903); // 레어 보상
            rewardLists[3].SetTitle(LocalizeKey._48904); // 유니크 보상
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        public void SetData(int chapterID)
        {
            var datas = MvpRewardUIDataManager.Instance.Get(chapterID);
            if (datas == null)
            {
                UI.Close<UIMvpReward>();
                return;
            }

            List<RewardData>[] rewardDatas = new List<RewardData>[4];
            for (int i = 0; i < rewardDatas.Length; ++i)
                rewardDatas[i] = new List<RewardData>();

            foreach (var each in datas)
                rewardDatas[each.markType - 1].Add(new RewardData(RewardType.Item, each.itemReward, 0, 0));

            for (int i = 0; i < rewardDatas.GetLength(0); ++i)
                rewardLists[i].SetRewards(uiRewardHelperTemplate, rewardDatas[i]);

            string chapterName = BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(chapterID).ToText();
            labelMainTitle.Text = LocalizeKey._48900.ToText() // {NAME} MVP 보상
                .Replace(ReplaceKey.NAME, chapterName);
        }

        protected override void OnHide()
        {
        }

        private void CloseUI()
        {
            UI.Close<UIMvpReward>();
        }
    }
}