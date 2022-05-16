using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="UIGuildBattleReady"/>
    /// </summary>
    public sealed class GuildBattleBuffView : UIView
    {
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GuildBattleBuffSelectElement element;

        private SuperWrapContent<GuildBattleBuffSelectElement, GuildBattleBuffSelectElement.IInput> wrapContent;

        public event System.Action<int> OnSelect;

        protected override void Awake()
        {
            base.Awake();
            wrapContent = wrapper.Initialize<GuildBattleBuffSelectElement, GuildBattleBuffSelectElement.IInput>(element);
            foreach (var item in wrapContent)
            {
                item.OnSelect += OnSelectElement;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in wrapContent)
            {
                item.OnSelect -= OnSelectElement;
            }
        }

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._33809; // 버프 강화
        }

        public void SetData(GuildBattleBuffSelectElement.IInput[] inputs)
        {
            wrapContent.SetData(inputs);
        }

        void OnSelectElement(int skillId)
        {
            OnSelect?.Invoke(skillId);
        }        
    }
}