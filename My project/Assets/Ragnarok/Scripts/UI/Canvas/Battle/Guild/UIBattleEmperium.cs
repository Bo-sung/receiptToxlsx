using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleEmperium : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIAniProgressBar hp;
        [SerializeField] UILabelHelper labelName;

        private int level;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            UpdateLevelText();
        }

        /// <summary>
        /// 레벨 세팅
        /// </summary>
        public void SetLevel(int level)
        {
            this.level = level;
            UpdateLevelText();
        }

        /// <summary>
        /// 처음 Hp 세팅
        /// </summary>
        public void SetHp(int cur, int max)
        {
            hp.Set(cur, max);
        }

        /// <summary>
        /// Hp 변화
        /// </summary>
        public void TweenHp(int cur, int max)
        {
            hp.Tween(cur, max);
        }

        private void UpdateLevelText()
        {
            labelName.Text = LocalizeKey._38100.ToText() // Lv.{LEVEL}
                .Replace(ReplaceKey.LEVEL, level);
        }
    }
}