using UnityEngine;

namespace Ragnarok
{
    public abstract class UIBaseGuildInfo<TPresenter> : UIInfo<TPresenter, GuildSimpleInfo>
        where TPresenter : ViewPresenter
    {
        [SerializeField] UITextureHelper emblemBackground;
        [SerializeField] UITextureHelper emblemFrame;
        [SerializeField] UITextureHelper emblemIcon;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelMaster;
        [SerializeField] UILabelHelper labelJoin;
        [SerializeField] UILabelHelper labelMember;
        [SerializeField] UIPlayTween twMember;

        protected override void Refresh()
        {
            emblemBackground.SetGuildEmblem(info.EmblemBgName);
            emblemFrame.SetGuildEmblem(info.EmblemframeName);
            emblemIcon.SetGuildEmblem(info.EmblemIconName);
            labelName.Text = $"Lv.{info.Level} {info.Name}";
            labelMaster.Text = info.MasterName;
            labelJoin.Text = info.IsAutoJoin ? LocalizeKey._33027.ToText() : LocalizeKey._33026.ToText(); // 자유가입 : 신청가입
            labelMember.Text = info.MemeberCountName;

            if (info.IsMaxMember)
            {
                twMember.Play(true);
            }
            else
            {
                twMember.Play(false);
            }
        }
    }
}