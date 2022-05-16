using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildSupportSelect"/>
    /// </summary>
    public class UIGuildSupportSelectElement : UIElement<UIGuildSupportSelectElement.IInput>
    {
        public interface IInput
        {
            int Cid { get; }
            int Uid { get; }
            string ProfileName { get; }
            string JobIconName { get; }
            int JobLevel { get; }
            string CharacterName { get; }
            int BattleScore { get; }
        }

        [SerializeField] UITextureHelper profile;
        [SerializeField] UITextureHelper jobIcon;
        [SerializeField] UILabelHelper labelCharacterName;
        [SerializeField] UILabelHelper labelBattleScore;
        [SerializeField] UIButtonHelper btnSelect;

        public System.Action<IInput> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
        }

        protected override void OnLocalize()
        {
            btnSelect.LocalKey = LocalizeKey._34201; // 지원받기
        }

        protected override void Refresh()
        {
            profile.SetJobProfile(info.ProfileName);
            jobIcon.SetJobIcon(info.JobIconName);
            labelCharacterName.Text = StringBuilderPool.Get()
                .Append("Lv. ").Append(info.JobLevel).Append(" ").Append(info.CharacterName)
                .Release();
            labelBattleScore.Text = info.BattleScore.ToString("N0");
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info);
        }
    }
}