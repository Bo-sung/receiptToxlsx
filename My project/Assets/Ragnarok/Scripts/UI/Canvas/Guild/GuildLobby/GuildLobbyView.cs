using UnityEngine;

namespace Ragnarok.View
{
    public class GuildLobbyView : UIView
    {
        [SerializeField] UIGuildContent guildAttack;

        public event System.Action OnSelectContent;

        protected override void Awake()
        {
            base.Awake();

            guildAttack.OnSelect += OnSelectGuildAttack;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            guildAttack.OnSelect -= OnSelectGuildAttack;
        }

        protected override void OnLocalize()
        {
        }

        void OnSelectGuildAttack()
        {
            OnSelectContent?.Invoke();
        }

        public void SetData(UIGuildContent.IInput input)
        {
            guildAttack.SetData(input);
        }
    }
}