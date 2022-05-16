using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleDarkMaze : UICanvas
    {
        public enum MonsterType
        {
            Lude,
            Gate1,
            Gate2,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UISprite icon;
        [SerializeField] UIAniProgressBar hp;

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
        }

        public void Set(MonsterType type, int cur, int max)
        {
            icon.spriteName = GetSpriteName(type);
            hp.Set(cur, max);
        }

        public void Tween(int cur, int max)
        {
            hp.Tween(cur, max);
        }

        private string GetSpriteName(MonsterType type)
        {
            switch (type)
            {
                case MonsterType.Lude: return "Ui_Lude";
                case MonsterType.Gate1: return "Ui_Gate071";
                case MonsterType.Gate2: return "Ui_Gate166";
            }

            return string.Empty;
        }
    }
}