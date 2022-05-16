using UnityEngine;

namespace Ragnarok.View
{
    public class UIAdventureMazeElement : UIElement<UIAdventureMazeElement.IInput>
    {
        public interface IInput
        {
            int Id { get; }
            int LocalKey { get; }
            string IconName { get; }
            RewardData Reward { get; }
            int RecommandedBattleScore { get; } // 권장 전투력

            bool IsLock { get; }
            string OpenConditionMessage { get; } // 오픈 조건 메시지
            bool IsCurrentGuideQuest { get; }
        }

        [SerializeField] UIButtonWithIconValueHelper btnEnter;
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelLock;
        [SerializeField] GameObject goLocation, goArrow;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
        }

        void OnClickedBtnEnter()
        {
            if (info.IsLock)
                return;

            OnSelect?.Invoke(info.Id);
        }

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            btnEnter.LocalKey = info.LocalKey;
            btnEnter.SetIconName(info.IconName);
            rewardHelper.SetData(info.Reward);

            int recommandedBattleScore = info.RecommandedBattleScore;
            bool isShowBattleScore = recommandedBattleScore > 0;
            btnEnter.SetActiveValue(isShowBattleScore);
            if (isShowBattleScore)
            {
                string valueText = LocalizeKey._48800.ToText() // 권장 전투력 {VALUE}
                    .Replace(ReplaceKey.VALUE, recommandedBattleScore);
                btnEnter.SetValue(valueText);
            }

            labelLock.SetActive(info.IsLock);
            if (info.IsLock)
            {
                labelLock.Text = info.OpenConditionMessage;
            }

            goLocation.SetActive(info.IsCurrentGuideQuest);
            goArrow.SetActive(!info.IsCurrentGuideQuest);
        }

        #region Tutorial
        public int GetId()
        {
            return info == null ? 0 : info.Id;
        }
        #endregion
    }
}