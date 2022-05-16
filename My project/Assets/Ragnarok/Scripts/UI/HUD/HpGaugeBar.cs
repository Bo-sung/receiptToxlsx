using UnityEngine;
using Ragnarok.View;

namespace Ragnarok
{
    public class HpGaugeBar : HUDObject
    {
        [SerializeField] UIAniProgressBar progressBar;

        [SerializeField, Rename(displayName = "아군 HP 색상")]
        Color alliesColor = new Color32(95, 233, 238, 255);

        [SerializeField, Rename(displayName = "적군 HP 색상")]
        Color enemyColor = new Color32(229, 142, 152, 255);

        private bool isInitialize;

        public void Initialize(bool isEnemy)
        {
            isInitialize = true;

            Color color = isEnemy ? enemyColor : alliesColor;
            progressBar.front.foregroundWidget.color = color;
            progressBar.behind.foregroundWidget.SetColorNoAlpha(color);
        }

        public void SetProgress(int curHP, int maxHP)
        {
            if (isInitialize)
            {
                isInitialize = false;
                progressBar.Set(curHP, maxHP);
            }
            else
            {
                progressBar.Tween(curHP, maxHP);
            }
        }
    }
}