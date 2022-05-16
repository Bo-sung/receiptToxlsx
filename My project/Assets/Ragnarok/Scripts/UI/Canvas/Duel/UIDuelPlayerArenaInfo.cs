using UnityEngine;

namespace Ragnarok.View
{
    public sealed class UIDuelPlayerArenaInfo : UIView
    {
        [SerializeField] UILabelHelper labelArena;
        [SerializeField] UILabelValue arenaFlag;

        private int titleKey = LocalizeKey._47866; // 아레나

        protected override void OnLocalize()
        {
            arenaFlag.TitleKey = LocalizeKey._47867; // 보유 아레나 깃발
            UpdateArenaText();
        }

        public void SetData(int titleKey, int arenaFlagCount)
        {
            this.titleKey = titleKey;
            arenaFlag.Value = arenaFlagCount.ToString();
            UpdateArenaText();
        }

        private void UpdateArenaText()
        {
            labelArena.LocalKey = titleKey;
        }
    }
}