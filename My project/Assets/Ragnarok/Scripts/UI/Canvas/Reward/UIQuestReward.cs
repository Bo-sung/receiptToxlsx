using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIQuestReward : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        public class Input : IUIData
        {
            public readonly QuestInfo quest;

            public readonly KafraType kafraType;
            public readonly RewardData[] rewards;

            public Input(QuestInfo quest)
            {
                this.quest = quest;
            }

            public Input(KafraType kafraType, RewardData[] rewards)
            {
                this.kafraType = kafraType;
                this.rewards = rewards;
            }
        }

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIGrid rewardGrid;
        [SerializeField] UIRewardHelper[] rewards;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UIButtonHelper btnConfirm;

        Input input;
        ContentType contentType;

        protected override void OnInit()
        {
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
        }

        protected override void OnShow(IUIData data = null)
        {
            if (data is Input input)
            {
                this.input = input;
                Refresh();
            }
            else
            {
                this.input = null;
                CloseUI();
            }

            if (this.input == null || this.input.quest == null)
            {
                contentType = default;
            }
            else
            {
                contentType = this.input.quest.OpenContent;
            }
        }

        protected override void OnHide()
        {
            foreach (var item in rewards)
            {
                item.Launch(UIRewardLauncher.GoodsDestination.Basic);
            }

            if (contentType == default)
                return;

            UIContentsUnlock.attackPowerInfoDelay = 3;

            // 이미 표시중인 UI 가 있다면 지워줍니다.
            UIPowerUpdate uiPowerUpdate = UI.GetUI<UIPowerUpdate>();
            if (uiPowerUpdate && uiPowerUpdate.IsVisible)
                uiPowerUpdate.Hide();

            if (!contentType.IsContentsUnlockViaTutorial())
                UI.Show<UIContentsUnlock>().Set(contentType);

            contentType = default; // Reset
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._30503; // 퀘스트 보상
            btnConfirm.LocalKey = LocalizeKey._30502; // 확 인
        }

        void Refresh()
        {
            PlaySfx(Constants.SFX.UI.OPEN_QUEST_REWARD);

            string questName;
            string questDesc;
            switch (input.kafraType)
            {
                case KafraType.RoPoint:
                    questName = LocalizeKey._19511.ToText(); // 귀금속 전달
                    questDesc = LocalizeKey._19523.ToText(); // 귀금속 전달 클리어
                    break;

                case KafraType.Zeny:
                    questName = LocalizeKey._19512.ToText(); // 긴급! 도움 요청
                    questDesc = LocalizeKey._19524.ToText(); // 긴급! 도움 요청 클리어
                    break;

                default:
                    questName = input.quest.Name;
                    questDesc = input.quest.Description;
                    break;
            }

            labelName.Text = questName;
            labelDesc.Text = questDesc;

            bool isAddReward = false;
            for (int i = 0; i < 4; i++)
            {
                RewardData reward;
                if (input.rewards != null)
                {
                    reward = i < input.rewards.Length ? input.rewards[i] : null;
                }
                else
                {
                    reward = input.quest.RewardDatas[i];

                    if (reward.RewardType == RewardType.Agent)
                    {
                        if (Entity.player.Agent.IsNewAgent(reward.RewardValue))
                            reward = new RewardData(RewardType.Agent, reward.RewardValue, reward.Count - 1); // 새로 얻은 Agent 일 경우, "New + 증가한 중복 동료 수" 의 형태로 표기 되기에 Count 를 1 줄입니다.
                    }

                    // 타임슈트 보상 정보 추가
                    if (input.quest.Category == QuestCategory.TimePatrol && !isAddReward)
                    {
                        if (reward.RewardType == RewardType.None)
                        {
                            List<int> questSeqList = BasisType.TIME_SUIT_QUEST_ID.GetKeyList();
                            if (questSeqList.Contains(input.quest.Group))
                            {
                                int timeSuit = BasisType.TIME_SUIT_QUEST_ID.GetInt(input.quest.Group);

                                switch (timeSuit.ToEnum<ShareForceType>())
                                {
                                    case ShareForceType.ShareForce1:
                                        reward = new RewardData(RewardType.ShareForce1, 1, 0);
                                        break;
                                    case ShareForceType.ShareForce2:
                                        reward = new RewardData(RewardType.ShareForce2, 1, 0);
                                        break;
                                    case ShareForceType.ShareForce3:
                                        reward = new RewardData(RewardType.ShareForce3, 1, 0);
                                        break;
                                    case ShareForceType.ShareForce4:
                                        reward = new RewardData(RewardType.ShareForce4, 1, 0);
                                        break;
                                    case ShareForceType.ShareForce5:
                                        reward = new RewardData(RewardType.ShareForce5, 1, 0);
                                        break;
                                    case ShareForceType.ShareForce6:
                                        reward = new RewardData(RewardType.ShareForce6, 1, 0);
                                        break;
                                }
                            }

                            if (reward != null)
                                isAddReward = true;
                        }
                    }
                }

                rewards[i].SetData(reward);
            }

            rewardGrid.repositionNow = true;
        }

        void CloseUI()
        {
            UI.Close<UIQuestReward>();
        }

        protected override void OnBack()
        {
            base.OnBack();

            StartTutorial();
        }

        private void StartTutorial()
        {
            if (Tutorial.Run(TutorialType.SkillLearn)) // 스킬 튜토리얼 시작
                return;

            if (Tutorial.Run(TutorialType.BossSummon)) // 보스도전 튜토리얼
                return;

            if (Tutorial.Run(TutorialType.ItemEnchant)) // 아이템 강화 튜토리얼
                return;

            if (Tutorial.Run(TutorialType.JobChange)) // 전직 튜토리얼 시작
                return;

            if (Tutorial.Run(TutorialType.Duel)) // 듀얼 튜토리얼
                return;

            if (Tutorial.Run(TutorialType.MazeOpen)) // 미로섬 튜토리얼
                return;

            if (Tutorial.Run(TutorialType.MultiMazeEnter)) // 멀티미로 입장 튜토리얼
                return;

            if (Tutorial.Run(TutorialType.ShareVice2ndOpen)) // 2세대 쉐어바이스 오픈 튜토리얼
                return;
        }
    }
}