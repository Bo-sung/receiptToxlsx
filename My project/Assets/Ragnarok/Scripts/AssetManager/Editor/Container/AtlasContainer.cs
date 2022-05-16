using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    [CreateAssetMenu(fileName = "Container", menuName = "AssetBundle/Container/Atlas")]
    public sealed class AtlasContainer : StringAssetContainer<NGUIAtlas>
    {
        Dictionary<string, string> subDic;

        protected override string ConvertKey(NGUIAtlas t)
        {
            return t.name;
        }

        public override void Ready()
        {
            base.Ready();

            subDic = new Dictionary<string, string>(System.StringComparer.Ordinal);
            NGUIAtlas[] array = GetArray();
            foreach (var atlas in array)
            {
                foreach (var item in atlas.spriteList)
                {
                    subDic.Add(item.name, atlas.name);
                }
            }
        }

        public override void Clear()
        {
            base.Clear();

            subDic.Clear();
        }

        public override NGUIAtlas Get(string key, bool isLog = true)
        {
            if (!subDic.ContainsKey(key))
            {
                Debug.LogError($"Icon에 해당하는 Atlas가 존재하지 않습니다: {nameof(key)} = {key}");
                return null;
            }

            return base.Get(subDic[key]);
        }        
    }
}