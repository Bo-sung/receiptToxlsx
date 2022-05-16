using Ragnarok.View;

using UnityEngine;

namespace Ragnarok
{
    public sealed class UICardStatusInfo : UIInfo<ItemInfo>, IAutoInspectorFinder
    {
        [SerializeField] UICardProfile cardProfile;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelHelper labelWeight;
        [SerializeField] UISprite iconClassBitType;
        [SerializeField] UICardBattleOptionList effectList;
        [SerializeField] UIToolTipHelper classTypeToolTip;

        protected override void Refresh()
        {
            cardProfile.SetData(info);
            var level = LocalizeKey._18005.ToText(). // Lv. {LEVEL}
                    Replace(ReplaceKey.LEVEL, info.GetCardLevelView());

#if UNITY_EDITOR
            labelName.Text = $"{level} {info.Name}({info.ItemId})";
#else
            labelName.Text = $"{level} {info.Name}";
#endif
            labelDescription.Text = info.Description;
            labelWeight.Text = info.TotalWeightText;
            iconClassBitType.spriteName = info.ClassType.GetIconName(info.ItemDetailType);
            classTypeToolTip.SetToolTipLocalizeKey(info.ClassType.ToLocalizeKey());

            effectList.SetData(info, info);
        }
    }
}