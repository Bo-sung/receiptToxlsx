using System.Collections.Generic;

namespace Ragnarok
{
    public class AssetBundleNameCollection
    {
        private readonly string tag;
        private readonly string[] assetBundleNames;

        /// <summary>
        /// 중복된 어셋번들이름 세팅
        /// 예) texture/skill/01~13
        /// </summary>
        /// <param name="tag">단순로그 용</param>
        /// <param name="basePath">texture/skill/</param>
        /// <param name="maxIndex">13</param>
        public AssetBundleNameCollection(string tag, string basePath, int maxIndex)
        {
            this.tag = tag;
            assetBundleNames = new string[maxIndex];
            for (int i = 0; i < maxIndex; i++)
            {
                assetBundleNames[i] = string.Concat(basePath, (i + 1).ToString());
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (string item in assetBundleNames)
            {
                yield return item;
            }
        }

        public override string ToString()
        {
            return tag;
        }
    }
}