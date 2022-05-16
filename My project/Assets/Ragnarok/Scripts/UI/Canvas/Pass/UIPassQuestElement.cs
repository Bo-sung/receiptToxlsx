﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class UIPassQuestElement : UIElement<UIPassQuestElement.IInput>
    {
        public interface IInput
        {
            event System.Action OnUpdateQuest;

            int ID { get; }
            RewardData[] RewardDatas { get; }
            int CurrentValue { get; }
            int MaxValue { get; }
            string Name { get; }
            string ConditionText { get; }
            QuestInfo.QuestCompleteType CompleteType { get; }
            QuestCategory Category { get; }
        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnComplete;
        [SerializeField] UIRewardHelper[] rewardHelpers;
        [SerializeField] UIProgressBar progressBar;
        [SerializeField] UILabelHelper labelProgress;
        [SerializeField] GameObject completeBase;

        public event System.Action<int> OnSelect;

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
            for (int i = 0; i < rewardHelpers.Length; i++)
            {
                rewardHelpers[i].SetData(info.RewardDatas[i]);
            }
            int currentValue = info.CurrentValue;
            int maxValue = info.MaxValue;
            labelTitle.Text = info.ConditionText;
            progressBar.value = MathUtils.GetProgress(currentValue, maxValue);
            labelProgress.Text = $"({currentValue}/{maxValue})";
            switch (info.CompleteType)
            {
                case QuestInfo.QuestCompleteType.InProgress: // 진행중
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnComplete.IsEnabled = false;
                    completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.StandByReward: // 보상 대기
                    btnComplete.LocalKey = LocalizeKey._10012; // 보상획득
                    btnComplete.IsEnabled = true;
                    completeBase.SetActive(false);
                    break;

                case QuestInfo.QuestCompleteType.ReceivedReward: // 보상 받음
                    btnComplete.LocalKey = LocalizeKey._10014; // 획득완료
                    btnComplete.IsEnabled = false;
                    completeBase.SetActive(true);
                    break;
            }
        }

        void OnClickedBtnComplete()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.ID);
        }
    }
}