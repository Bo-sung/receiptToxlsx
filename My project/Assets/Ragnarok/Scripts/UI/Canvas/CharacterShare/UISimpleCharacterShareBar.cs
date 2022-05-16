using UnityEngine;

namespace Ragnarok.View.CharacterShare
{
    public class UISimpleCharacterShareBar : UIView, IInspectorFinder
    {
        public interface IInput
        {
            int Cid { get; }
            int Uid { get; }
            byte Job { get; }
            byte Gender { get; }
            int JobLevel { get; }
            string Name { get; }
            int Power { get; }
            string GetSkillIcon(int index);
            SkillType GetSkillType(int index);
            SharingModel.SharingCharacterType SharingCharacterType { get; }
            SharingModel.CloneCharacterType CloneCharacterType { get; }
            string ProfileName { get; }
            string ThumbnailName { get; }
        }

        [SerializeField] UITextureHelper thumbnail;
        [SerializeField] UILabel labelName;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabel labelPower;
        [SerializeField] protected UIGrid skillGrid;
        [SerializeField] protected UITextureHelper[] skillIcons;
        [SerializeField] UIButtonHelper btnSelf;

        public IInput data;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSelf.OnClick, OnClickedBtnSelf);
        }

        protected override void OnDestroy()
        {
            EventDelegate.Remove(btnSelf.OnClick, OnClickedBtnSelf);

            base.OnDestroy();
        }

        protected override void OnLocalize()
        {
            if (data == null)
                return;

            labelPower.text = LocalizeKey._10204.ToText() // 전투력 {VALUE}
                .Replace(ReplaceKey.VALUE, data.Power);
        }

        public void SetData(IInput input)
        {
            data = input;

            Refresh();
            OnLocalize();
        }

        protected virtual void Refresh()
        {
            if (data == null)
                return;

            Job job = data.Job.ToEnum<Job>();
            Gender gender = data.Gender.ToEnum<Gender>();

            UpdateProfile(data.ProfileName); // 프로필 업데이트
            UpdateName(); // 이름 업데이트
            UpdateJobIcon(job); // 직업아이콘 업데이트
            UpdateSkillIcon(); // 스킬아이콘들 업데이트
        }

        protected void UpdateProfile(string profileName)
        {
            thumbnail.Set(profileName);
        }

        protected void UpdateName()
        {
            labelName.text = string.Concat("Lv.", data.JobLevel.ToString(), " ", data.Name);
        }

        protected void UpdateJobIcon(Job job)
        {
            iconJob.Set(job.GetJobIcon());
        }

        protected virtual void UpdateSkillIcon()
        {
            for (int i = 0; i < skillIcons.Length; i++)
            {
                string skillicon = data.GetSkillIcon(i);
                if (string.IsNullOrEmpty(skillicon) || data.GetSkillType(i) != SkillType.Active)
                {
                    skillIcons[i].SetActive(false);
                }
                else
                {
                    skillIcons[i].SetActive(true);
                    skillIcons[i].SetSkill(data.GetSkillIcon(i));
                }
            }
            skillGrid.Reposition();
        }

        void OnClickedBtnSelf()
        {
            if (data is null) // || data.Cid == 0 || data.Uid == 0)
                return;

            Entity.player.User.RequestOtherCharacterInfo(data.Uid, data.Cid).WrapNetworkErrors();
        }

        bool IInspectorFinder.Find()
        {
            if (skillGrid)
            {
                skillIcons = skillGrid.transform.GetComponentsInChildren<UITextureHelper>();
            }

            return true;
        }
    }
}