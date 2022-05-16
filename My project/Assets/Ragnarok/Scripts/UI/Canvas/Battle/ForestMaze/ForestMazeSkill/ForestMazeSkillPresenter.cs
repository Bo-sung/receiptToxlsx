using Ragnarok.View;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIForestMazeSkill"/>
    /// </summary>
    public class ForestMazeSkillPresenter : ViewPresenter
    {
        // <!-- Repositories --!>
        private readonly ForestRewardDataManager forestRewardDataRepo;

        private readonly Buffer<UIForestMazeSkillElement.IInput> buffer;

        public ForestMazeSkillPresenter()
        {
            forestRewardDataRepo = ForestRewardDataManager.Instance;
            buffer = new Buffer<UIForestMazeSkillElement.IInput>();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// id에 해당하는 데이터 반환
        /// </summary>
        public UIForestMazeSkillElement.IInput GetData(int id)
        {
            return forestRewardDataRepo.Get(id);
        }

        /// <summary>
        /// id에 해당하는 데이터 반환 (배열)
        /// </summary>
        public UIForestMazeSkillElement.IInput[] GetData(int[] ids)
        {
            if (ids == null)
                return null;

            for (int i = 0; i < ids.Length; i++)
            {
                buffer.Add(GetData(ids[i]));
            }

            buffer.Sort(SortBySkill);
            return buffer.GetBuffer(isAutoRelease: true);
        }

        /// <summary>
        /// id에 해당하는 이름 반환
        /// </summary>
        public string GetName(int id)
        {
            ForestRewardData data = forestRewardDataRepo.Get(id);
            if (data == null)
                return string.Empty;

            RewardData rewardData = data.GetReward();
            if (rewardData != null)
                return rewardData.ItemName;

            ForestMazeSkill skillData = data.GetSkill();
            if (skillData != null)
                return skillData.NameId.ToText();

            return string.Empty;
        }

        /// <summary>
        /// id에 해당하는 설명 반환
        /// </summary>
        public string GetDesc(int id)
        {
            ForestRewardData data = forestRewardDataRepo.Get(id);
            if (data == null)
                return string.Empty;

            RewardData rewardData = data.GetReward();
            if (rewardData != null)
                return rewardData.GetDescription();

            ForestMazeSkill skillData = data.GetSkill();
            if (skillData != null)
                return skillData.DescId.ToText();

            return string.Empty;
        }

        /// <summary>
        /// 스킬 우선으로 정렬
        /// </summary>
        private int SortBySkill(UIForestMazeSkillElement.IInput a, UIForestMazeSkillElement.IInput b)
        {
            if (a.Skill == null && b.Skill == null)
                return 0;

            if (a.Skill != null)
                return -1;

            if (b.Skill != null)
                return 1;

            return a.Id.CompareTo(b.Id);
        }
    }
}