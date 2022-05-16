using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "MakeSubTab", menuName = "Config/MakeSubTab/Create")]
    public sealed class MakeSubTabConfig : ScriptableObject, IDisplayDirty
    {
        [SerializeField]
        int[] subTabIds;

        public int GetCount()
        {
            return subTabIds == null ? 0 : subTabIds.Length;
        }

        public int GetSubTabNameId(int index)
        {
            return subTabIds[index];
        }
    }
}