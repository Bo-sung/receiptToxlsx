using UnityEngine;

namespace Ragnarok
{
    public class NexusEffectPlayer : MonsterEffectPlayer
    {
        protected Color hudNameColor_Ally = Color.yellow; // new Color(0.2621095f, 0.8584906f, 0.2065237f);
        protected Color hudNameColor_Enemy = new Color(0.764151f, 0.2703364f, 0.2703364f);

        private HudCircleTimer hudCircleTimer;

        public override void OnRelease()
        {
            base.OnRelease();

            if (hudCircleTimer)
            {
                hudCircleTimer.Release();
                hudCircleTimer = null;
            }
        }

        public override void ShowName()
        {
            if (hudName == null)
                hudName = hudPool.SpawnUnitName(CachedTransform);

            hudName.Initialize(entity.GetNameId(), entity.type);
            hudName.SetColor(entity.IsEnemy ? hudNameColor_Enemy : hudNameColor_Ally);
        }

        public void ShowCircleTimer(long countTick, long startTick = 0L)
        {
            if (hudCircleTimer is null)
                hudCircleTimer = hudPool.SpawnCircleTimer(actor?.CachedTransform) as HudCircleTimer;

            hudCircleTimer.StartTimer(countTick, startTick);
            hudCircleTimer.Show();
        }

        public void HideCircleTimer()
        {
            hudCircleTimer?.Hide();
        }
    }
}
