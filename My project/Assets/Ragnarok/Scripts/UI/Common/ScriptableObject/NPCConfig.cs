using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "NPC", menuName = "Config/NPC/Create")]
    public sealed class NPCConfig : ScriptableObject, IDisplayDirty
    {               
        [SerializeField]
        int npcNameId;
         
        [SerializeField]
        int[] npcTalkId;
        
        public string NPCName => npcNameId.ToText();
        public string NPCTalk => npcTalkId.Length == 0 ? string.Empty : npcTalkId[Random.Range(0, npcTalkId.Length)].ToText();
        
        public int[] GetNpcTalkIDs()
        {
            return npcTalkId;
        }
    }
}
