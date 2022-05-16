using System;

namespace Ragnarok.CIBuilder
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetBundleConfigAttribute : Attribute
    {
        public static readonly AssetBundleConfigAttribute DEFAULT = new AssetBundleConfigAttribute(string.Empty, default(AssetBundleSettings.Mode));

        public readonly string baseURL;
        public readonly int mode;

        /// <summary>
        /// 어셋번들 설정
        /// </summary>
        /// <param name="baseURL">어셋번들 다운로드 할 기본 주소</param>
        /// <param name="mode">어셋번들 로드 방식</param>
        public AssetBundleConfigAttribute(string baseURL, AssetBundleSettings.Mode mode)
        {
            this.baseURL = baseURL;
            this.mode = (int)mode;
        }

        public bool Equals(string baseURL, int assetBundleMode)
        {
            if (!string.Equals(this.baseURL, baseURL))
                return false;

            if (!this.mode.Equals(assetBundleMode))
                return false;

            return true;
        }
    }
}