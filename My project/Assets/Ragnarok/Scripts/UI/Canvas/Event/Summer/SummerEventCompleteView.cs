using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UISummerEvent"/>
    /// </summary>
    public class SummerEventCompleteView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIGrid grid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UIButtonHelper btnComplete;
        [SerializeField] UILabelHelper labelNotice;

        public event System.Action OnSelectBtnComplete;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnComplete.OnClick, OnClickedBtnComplete);            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnComplete.OnClick, OnClickedBtnComplete);            
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._7300; // 전체 완료 보상
            labelNotice.LocalKey = LocalizeKey._7302; // 일일 퀘스트 10회 완료 시 보상을 받을 수 있습니다.
        }

        public void SetData(QuestInfo info)
        {
            for (int i = 0; i < rewards.Length; i++)
            {
                rewards[i].SetData(info.RewardDatas[i]);
            }
            grid.Reposition();
            switch (info.CompleteType)
            {
                case QuestInfo.QuestCompleteType.InProgress: // 진행중
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnComplete.IsEnabled = false;
                    break;

                case QuestInfo.QuestCompleteType.StandByReward: // 보상 대기
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnComplete.IsEnabled = true;
                    break;

                case QuestInfo.QuestCompleteType.ReceivedReward: // 보상 받음
                    btnComplete.LocalKey = LocalizeKey._10014; // 획득완료
                    btnComplete.IsEnabled = false;
                    break;
            }
        }

        void OnClickedBtnComplete()
        {
            OnSelectBtnComplete?.Invoke();
        }
    }
}