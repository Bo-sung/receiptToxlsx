using UnityEngine;

namespace Ragnarok.View
{
    public class EventMazeHelpView : SkillFreeFightHelpView
    {
        private const int TAB_MULTI = 0; // 멀티
        private const int TAB_FREEFIGHT = 1; // 난전

        [SerializeField] UITabHelper tab;
        [SerializeField] UILabelValue labelMultiHelp;

        protected override void OnLocalize()
        {
            base.OnLocalize();

            tab[TAB_MULTI].LocalKey = LocalizeKey._49501; // 멀티
            tab[TAB_FREEFIGHT].LocalKey = LocalizeKey._49502; // 난전
        }

        public void SetDetailText(string multiTitle, string multiDesc)
        {
            labelMultiHelp.Title = multiTitle;
            labelMultiHelp.Value = multiDesc;
        }
    }
}