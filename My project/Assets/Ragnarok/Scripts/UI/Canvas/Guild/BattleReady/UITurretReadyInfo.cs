using UnityEngine;

namespace Ragnarok.View
{
    /// <summary>
    /// <see cref="GuildBattleEntryPopupView"/>
    /// </summary>
    public class UITurretReadyInfo : UITurretInfo
    {
        [SerializeField] UILabelHelper labelName;

        public void SetName(string text)
        {
            labelName.Text = text;
        }
    }
}