using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattlePotionButton : UICostButtonHelper
    {
        [SerializeField] UISprite cooldown;

        float max;
        float cur;

        public bool HasCooldownTime => cur > 0f;

        void Update()
        {
            if (!HasCooldownTime)
                return;

            cur -= Time.deltaTime;
            Refresh();
        }

        public void InitCooldownTime(float cooldownTime)
        {
            max = cooldownTime;
            ResetCooldown();

            Refresh();
        }

        public void StartCooldown()
        {
            cur = max;

            Refresh();
        }

        public void ResetCooldown()
        {
            cur = 0f;

            Refresh();
        }

        private void Refresh()
        {
            cooldown.fillAmount = MathUtils.GetProgress(cur, max);
        }
    }
}