using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIGatePartyReadySlot : UIElement<UIGatePartyReadySlot.IInput>, IInspectorFinder
    {
        public delegate void BanEvent(int cid);

        public interface IInput
        {
            bool HasCharacter { get; }

            bool IsLeader { get; }
            int Uid { get; }
            int Cid { get; }

            string ProfileName { get; }
            string JobIconName { get; }
            int Level { get; }
            string Name { get; }
            int BattleScore { get; }

            string GetSkillIconName(int index);
        }

        [Header("Character")]
        [SerializeField] GameObject goCharacter;
        [SerializeField] UIButton btnSlot;
        [SerializeField] UITextureHelper profile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabelHelper labelPower;
        [SerializeField] UIGrid skillGrid;
        [SerializeField] UITextureHelper[] skills;
        [SerializeField] UIButtonHelper btnBan;
        [SerializeField] GameObject goLeader;

        [Header("Empty")]
        [SerializeField] GameObject goEmpty;
        [SerializeField] UILabelHelper labelWait;

        public event UserModel.UserInfoEvent OnSelectUserInfo;
        public event BanEvent OnSelectUserBan;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSlot.onClick, OnClickedBtnSlot);
            EventDelegate.Add(btnBan.OnClick, OnClickedBtnBan);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnSlot.onClick, OnClickedBtnSlot);
            EventDelegate.Remove(btnBan.OnClick, OnClickedBtnBan);
        }

        protected override void OnLocalize()
        {
            labelWait.LocalKey = LocalizeKey._6915; // 파티원을 기다리는 중입니다.
        }

        protected override void Refresh()
        {
            goCharacter.SetActive(info.HasCharacter);

            if (info.HasCharacter)
            {
                profile.SetJobProfile(info.ProfileName);
                goLeader.SetActive(info.IsLeader);
                labelName.Text = LocalizeKey._19007.ToText() // Lv. {LEVEL} {NAME}
                    .Replace(ReplaceKey.LEVEL, info.Level)
                    .Replace(ReplaceKey.NAME, info.Name);
                iconJob.SetJobIcon(info.JobIconName);
                labelPower.Text = LocalizeKey._32021.ToText() // 전투력 : {VALUE}
                    .Replace(ReplaceKey.VALUE, info.BattleScore);

                string skillName;
                for (int i = 0; i < skills.Length; i++)
                {
                    skillName = info.GetSkillIconName(i);

                    if (string.IsNullOrEmpty(skillName))
                    {
                        skills[i].SetActive(false);
                    }
                    else
                    {
                        skills[i].SetActive(true);
                        skills[i].SetSkill(skillName);
                    }
                }
                skillGrid.Reposition();
                btnBan.SetActive(!info.IsLeader);
            }

            goEmpty.SetActive(!info.HasCharacter);
        }

        void OnClickedBtnSlot()
        {
            OnSelectUserInfo?.Invoke(info.Uid, info.Cid);
        }

        void OnClickedBtnBan()
        {
            OnSelectUserBan?.Invoke(info.Cid);
        }

        public void SetLeader(bool isLeader)
        {
            // 현재 슬롯이 방장일 경우
            if (info.IsLeader)
                return;

            btnBan.SetActive(isLeader); // 방장만 강퇴 기능 활성화
        }

        bool IInspectorFinder.Find()
        {
            skills = skillGrid == null ? null : skillGrid.GetComponentsInChildren<UITextureHelper>();
            return true;
        }
    }
}