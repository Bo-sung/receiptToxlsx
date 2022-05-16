using UnityEngine;

namespace Ragnarok
{
    public class GVGMultiPlayerEffectPlayer : CharacterEffectPlayer
    {
        protected Color hudNameColor_Ally = new Color(0.2621095f, 0.8584906f, 0.2065237f);
        protected Color hudNameColor_Enemy = new Color(0.764151f, 0.2703364f, 0.2703364f);

        public override void ShowName()
        {
            if (hudName == null)
                hudName = hudPool.SpawnCharacterName(CachedTransform);

            hudName.Initialize(entity.GetName(), entity.Character.JobLevel, entity.type);
            hudName.Initialize(GetGuildName(), GetTitleName());
            if (!ReferenceEquals(Entity.player, entity))
            {
                hudName.SetColor(entity.IsEnemy ? hudNameColor_Enemy : hudNameColor_Ally);
            }
        }
    }
}
