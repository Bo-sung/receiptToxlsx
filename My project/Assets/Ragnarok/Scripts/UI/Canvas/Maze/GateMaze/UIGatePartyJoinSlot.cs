using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIGatePartyJoinSlot : UIElement<UIGatePartyJoinSlot.IInput>
    {
        public delegate void JoinEvent(int channelId);

        public interface IInput
        {
            int ChannelId { get; }

            bool HasCharacter { get; }
            bool IsLeader { get; }
            int Uid { get; }
            int Cid { get; }
            string ProfileName { get; }
            string JobIconName { get; }
            int Level { get; }
            string Name { get; }
            int BattleScore { get; }
        }

        [Header("Character")]
        [SerializeField] GameObject goCharacter;
        [SerializeField] UIButton btnSlot;
        [SerializeField] UITextureHelper profile;
        [SerializeField] GameObject goLeader;
        [SerializeField] UITextureHelper iconJob;
        [SerializeField] UILabelHelper labelLevel, labelName;
        [SerializeField] UILabelValue battleScore;

        [Header("Empty")]
        [SerializeField] GameObject goEmpty;
        [SerializeField] UILabelHelper labelWait;
        [SerializeField] UIButtonHelper btnJoin;

        public event UserModel.UserInfoEvent OnSelectUserInfo;
        public event JoinEvent OnSelectJoin;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnSlot.onClick, OnClickedBtnSlot);
            EventDelegate.Add(btnJoin.OnClick, OnClickedBtnJoin);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnSlot.onClick, OnClickedBtnSlot);
            EventDelegate.Remove(btnJoin.OnClick, OnClickedBtnJoin);
        }

        protected override void OnLocalize()
        {
            battleScore.TitleKey = LocalizeKey._48000; // 전투력
            labelWait.LocalKey = LocalizeKey._6909; // 대기중...
            btnJoin.LocalKey = LocalizeKey._6910; // 참여하기
        }

        protected override void Refresh()
        {
            goCharacter.SetActive(info.HasCharacter);

            if (info.HasCharacter)
            {
                profile.SetJobProfile(info.ProfileName);
                goLeader.SetActive(info.IsLeader);
                iconJob.SetJobIcon(info.JobIconName);
                labelLevel.Text = LocalizeKey._48238.ToText() // Lv. {LEVEL}
                    .Replace(ReplaceKey.LEVEL, info.Level);
                labelName.Text = info.Name;
                battleScore.Value = info.BattleScore.ToString();
            }

            goEmpty.SetActive(!info.HasCharacter);
        }

        void OnClickedBtnSlot()
        {
            OnSelectUserInfo?.Invoke(info.Uid, info.Cid);
        }

        void OnClickedBtnJoin()
        {
            OnSelectJoin?.Invoke(info.ChannelId);
        }
    }
}