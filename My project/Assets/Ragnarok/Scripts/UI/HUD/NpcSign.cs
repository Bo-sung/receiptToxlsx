using UnityEngine;

namespace Ragnarok
{
    public class NpcSign : HUDObject, IAutoInspectorFinder
    {
        [SerializeField] UIButton button;
        [SerializeField] UISprite background;
        [SerializeField] UISprite iconNpc;
        [SerializeField] UILabel labelName;
        [SerializeField] UIPlayTween tween;

        private Npc npc;
        private System.Action<NpcType> onClick;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(button.onClick, OnClickedButton);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(button.onClick, OnClickedButton);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();

            tween.Play();
        }

        public override void OnDespawn()
        {
            base.OnDespawn();

            onClick = null;
        }

        public void Initialize(NpcType npcType, System.Action<NpcType> onClick)
        {
            npc = Npc.GetByKey(npcType);
            this.onClick = onClick;

            if (npc == null)
            {
                iconNpc.spriteName = string.Empty;
                background.gradientTop = Color.white;
                labelName.color = Color.black;

                Debug.LogError($"유효하지 않은 처리: {nameof(npcType)} = {npcType}");
            }
            else
            {
                iconNpc.spriteName = npc.spriteName;
                background.gradientTop = npc.GetBackgroundColor();
                labelName.color = npc.GetLabelColor();
            }

            OnLocalize();
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelName.text = npc == null ? string.Empty : npc.contentLocalKey.ToText();
        }

        void OnClickedButton()
        {
            if (npc == null)
                return;

            onClick?.Invoke(npc.Key);
        }
    }
}