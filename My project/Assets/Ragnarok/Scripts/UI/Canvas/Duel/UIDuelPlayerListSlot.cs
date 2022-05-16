using Ragnarok.View;
using System;
using UnityEngine;

namespace Ragnarok
{
    public class UIDuelPlayerListSlot : UIView
    {
        public interface IInput
        {
            UIDuel.State State { get; }
            string Name { get; }
            Job Job { get; }
            short JobLevel { get; }
            int BattleScore { get; }
            int WinCount { get; }
            int DefeatCount { get; }
            int CID { get; }
            int UID { get; }
            Gender Gender { get; }
            string ProfileName { get; }
            int ArenaPoint { get; }
        }

        [SerializeField] UILabelHelper nameLabel;
        [SerializeField] UILabelHelper battleScoreLabel;
        [SerializeField] UILabelHelper recordLabel;
        [SerializeField] UIButtonHelper jobProfileButton;
        [SerializeField] UIButtonHelper battleButton;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UITextureHelper profile;
        [SerializeField] UILabelHelper labelArenaFlag;

        private Action<IInput> onClickBattle;
        private IInput curInfo;

        protected override void Start()
        {
            base.Start();

            EventDelegate.Add(battleButton.OnClick, OnClickBattle);
            EventDelegate.Add(jobProfileButton.OnClick, ShowPlayerInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(battleButton.OnClick, OnClickBattle);
            EventDelegate.Remove(jobProfileButton.OnClick, ShowPlayerInfo);
        }

        protected override void OnLocalize()
        {
            battleButton.LocalKey = LocalizeKey._47901; // 전투
        }

        public void SetData(IInput input, Action<IInput> onClickBattle)
        {
            curInfo = input;
            this.onClickBattle = onClickBattle;

            nameLabel.Text = $"Lv.{curInfo.JobLevel} {curInfo.Name} [c][BEBEBE]({MathUtils.CidToHexCode(curInfo.CID)})[-][/c]";
            battleScoreLabel.Text = LocalizeKey._10204.ToText()
                .Replace(ReplaceKey.VALUE, curInfo.BattleScore.ToString("n0"));
            profile.Set(curInfo.ProfileName);
            jobIcon.Set(curInfo.Job.GetJobIcon());
            labelArenaFlag.SetActive(input.ArenaPoint > 0);
            labelArenaFlag.Text = input.ArenaPoint.ToString();

            switch (curInfo.State)
            {
                case UIDuel.State.Chapter:
                    recordLabel.Text = LocalizeKey._47812.ToText() // {WIN}승 / {LOSE}패
                        .Replace(ReplaceKey.WIN, curInfo.WinCount)
                        .Replace(ReplaceKey.LOSE, curInfo.DefeatCount);
                    break;

                case UIDuel.State.Event:
                    recordLabel.Text = string.Empty; // 보여주지 않는다.
                    break;

                default:
                    throw new InvalidOperationException($"유효하지 않은 처리: {nameof(UIDuel.State)} = {curInfo.State}");
            }
        }

        private void OnClickBattle()
        {
            onClickBattle(curInfo);
        }

        private void ShowPlayerInfo()
        {
            if (curInfo == null)
                return;

            if (curInfo.State == UIDuel.State.Event)
                return;

            Entity.player.User.RequestOtherCharacterInfo(curInfo.UID, curInfo.CID).WrapNetworkErrors();
        }
    }
}