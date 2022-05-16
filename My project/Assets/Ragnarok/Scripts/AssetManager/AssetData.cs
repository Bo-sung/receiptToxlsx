using UnityEngine;

namespace Ragnarok
{
    [System.Serializable]
    public class AssetData
    {
        public string name; // 번들이름
        public Hash128 hash; // 해쉬
        public long size; // 용량
    }
}