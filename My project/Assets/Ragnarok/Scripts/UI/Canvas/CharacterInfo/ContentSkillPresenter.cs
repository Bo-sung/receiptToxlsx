using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public class ContentSkillPresenter : ViewPresenter
    {
        public interface IView
        {
        }

        private readonly IView view;
        private CharacterEntity player;
        private CharacterModel characterModel;
        private SkillModel skillModel;

        SkillDataManager.ISkillDataRepoImpl skillDataRepo;


        private SkillModel.ISkillSimpleValue[] skillList;
        private SkillModel.ISkillSimpleValue[] skillSlotList;

        public ContentSkillPresenter(IView view)
        {
            this.view = view;
            skillDataRepo = SkillDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void SetPlayer(CharacterEntity player)
        {
            this.player = player;
            this.characterModel = this.player.Character;
            this.skillModel = this.player.Skill;

            var skillList = this.skillModel.GetAllPossessedSkills().ToList();
            skillList.Sort((x, y) => y.SkillId - x.SkillId);
            this.skillList = skillList.ToArray();

            this.skillSlotList = GetSkillSlotInfos();
        }


        public string GetJobIcon() => this.characterModel.Job.GetJobIcon();
        public string GetJobName() => this.characterModel.Job.GetJobName();
        public int GetJobLevel() => this.characterModel.JobLevel;

        public SkillModel.ISkillSimpleValue[] GetSkillArray() => skillList;
        public SkillModel.ISkillSimpleValue[] GetSkillSlotArray() => skillSlotList;

        /// <summary>
        /// 스킬슬롯 리스트 반환
        /// </summary>
        private SkillModel.ISkillSimpleValue[] GetSkillSlotInfos()
        {
            List<SkillModel.ISkillSimpleValue> skillInfoList = new List<SkillModel.ISkillSimpleValue>();

            int slotCount = BasisType.MAX_CHAR_SKILL_SLOT.GetInt();
            for (int i = 0; i < slotCount; i++)
            {
                SkillModel.ISlotValue slotInfo = this.skillModel.GetSlotInfo(i);
                if (slotInfo is null)
                    continue;

                var skillInfo = this.skillModel.GetSkill(slotInfo.SkillNo, isBattleSkill: true);
                skillInfoList.Add(skillInfo);
            }

            return skillInfoList.ToArray();
        }
    }
}