namespace Ragnarok
{
    public sealed class WebAssetLoader : AssetLoader
    {
        public WebAssetLoader(string baseURL, int version, bool isShowDownload)
            : base($"{baseURL}/AssetBundles/{version}/{Config.PLATFORTM_NAME}/", isShowDownload)
        {
        }
    }
}