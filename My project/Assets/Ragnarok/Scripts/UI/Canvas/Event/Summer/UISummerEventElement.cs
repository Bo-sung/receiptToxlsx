using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="SummerEventView"/>
    /// </summary>
    public class UISummerEventElement : UIElement<UISummerEventElement.IInput>
    {
        public interface IInput
        {
            event System.Action OnUpdateQuest;

            int ID { get; }
            RewardData[] RewardDatas { get; }
            QuestInfo.QuestCompleteType CompleteType { get; }
            QuestType QuestType { get; }
        }

        [SerializeField] UITextureHelper icon;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] GameObject goComplete;
        [SerializeField] UIButtonHelper btnComplete;

        public event System.Action<int> OnSelectBtnComplete;

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

        protected override void AddEvent()
        {
            base.AddEvent();
            info.OnUpdateQuest += Refresh;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();
            info.OnUpdateQuest -= Refresh;
        }

        protected override void OnLocalize()
        {
            
        }

        protected override void Refresh()
        {
            icon.Set(info.RewardDatas[0].IconName);
            labelCount.Text = info.RewardDatas[0].Count.ToString("N0");
            icon.SetActive(info.CompleteType != QuestInfo.QuestCompleteType.ReceivedReward);
            goComplete.SetActive(info.CompleteType == QuestInfo.QuestCompleteType.ReceivedReward);
            btnComplete.SetActive(info.CompleteType == QuestInfo.QuestCompleteType.StandByReward);
        }

        void OnClickedBtnComplete()
        {
            OnSelectBtnComplete?.Invoke(info.ID);
        }
    }
}