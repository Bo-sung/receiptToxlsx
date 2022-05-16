using UnityEngine;

namespace Ragnarok.View
{
    public abstract class UIBaseGuildElement<T> : UIElement<T>
        where T : class, IGuildElementInput
    {
        [SerializeField] UIGuildEmblemInfo guildEmblem;
        [SerializeField] UILabelHelper labelName;

        protected override void Refresh()
        {
            int emblemBg = MathUtils.GetBitFieldValue(info.Emblem, 0);
            int emblemframe = MathUtils.GetBitFieldValue(info.Emblem, 6);
            int emblemIcon = MathUtils.GetBitFieldValue(info.Emblem, 12);
            guildEmblem.SetData(emblemBg, emblemframe, emblemIcon);
            labelName.Text = StringBuilderPool.Get()
                .Append("Lv. ").Append(info.GuildLevel).Append(" ").Append(info.GuildName)
                .Release();
        }
    }
}