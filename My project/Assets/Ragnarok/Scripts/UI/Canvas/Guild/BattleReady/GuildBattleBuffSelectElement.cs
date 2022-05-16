using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="GuildBattleBuffView"/>
    /// </summary>
    public class GuildBattleBuffSelectElement : GuildBattleBuffElement<GuildBattleBuffSelectElement.IInput>
    {
        public interface IInput : GuildBattleBuffElement.IInput
        {
            float ExpProgressValue { get; }
            string ExpProgressText { get; }
            bool IsExpMax { get; }
        }

        [SerializeField] UIProgressBar exp;
        [SerializeField] UILabelHelper labelValue;
        [SerializeField] UIButtonhWithGrayScale btnSelect;

        public event System.Action<int> OnSelect;

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
            base.OnLocalize();

            btnSelect.LocalKey = LocalizeKey._33810; // 기부
        }

        protected override void Refresh()
        {
            base.Refresh();

            exp.value = info.ExpProgressValue;
            labelValue.Text = info.ExpProgressText;

            if (info.IsExpMax)
            {
                btnSelect.IsEnabled = false;
                btnSelect.SetMode(UIGraySprite.SpriteMode.Grayscale);
            }
            else
            {
                btnSelect.IsEnabled = true;
                btnSelect.SetMode(UIGraySprite.SpriteMode.None);
            }
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            OnSelect?.Invoke(info.SkillId);
        }
    }
}