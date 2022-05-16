using UnityEngine;

namespace Ragnarok
{
    public class HudCharacterName : HudUnitName, IAutoInspectorFinder
    {
        [SerializeField] UILabel labelGuildName;
        [SerializeField] UILabel labelTitle;

        public void Initialize(string guildName, string titleName)
        {
            labelGuildName.text = guildName;
            labelTitle.text = titleName;
        }
    }
}