using UnityEngine;

namespace Ragnarok.View
{
    public class UIForestMazeSkillElement : UIElement<UIForestMazeSkillElement.IInput>
    {
        public interface IInput
        {
            int Id { get; }
            RewardData Reward { get; }
            UISkillInfo.IInfo Skill { get; }
        }

        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UISkillInfo skill;
        [SerializeField] GameObject goSelect;

        public event System.Action<int> OnSelect;

        protected override void OnLocalize()
        {
        }

        protected override void Refresh()
        {
            rewardHelper.SetData(info.Reward);
            skill.Show(info.Skill, isAsync: false);
        }

        public void SetSelect(int id)
        {
            if (info == null)
                return;

            NGUITools.SetActive(goSelect, info.Id == id);
        }

        void OnClick()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.Id);
        }
    }
}