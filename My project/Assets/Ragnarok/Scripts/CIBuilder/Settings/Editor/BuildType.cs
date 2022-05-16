namespace Ragnarok.CIBuilder
{
    public enum BuildType
    {
        Custom,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("203.84.246.242", 9933, "http://203.84.246.242:8080")]
        [AssetBundleConfig("", AssetBundleSettings.Mode.Editor)]
        Local242,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("203.84.246.249", 13001, "http://203.84.246.249:13002")]
        [AssetBundleConfig("", AssetBundleSettings.Mode.Editor)]
        Local249,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("35.72.123.7", 9933, "http://35.72.126.217:8080")]
        [AssetBundleConfig("https://d1hbvxmma206od.cloudfront.net/test/asset", AssetBundleSettings.Mode.Editor)]
        TestGlobal,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("elbglobal.labyrinthragnarok.com", 9933, "https://d1hbvxmma206od.cloudfront.net/real")]
        [AssetBundleConfig("https://d1hbvxmma206od.cloudfront.net/real/asset", AssetBundleSettings.Mode.Editor)]
        RealGlobal,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("stagingglobal.labyrinthragnarok.com", 9933, "https://d1hbvxmma206od.cloudfront.net/stage")]
        [AssetBundleConfig("https://d1hbvxmma206od.cloudfront.net/stage/asset", AssetBundleSettings.Mode.Editor)]
        StageGlobal,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("8.214.0.255", 9933, "http://8.214.14.123:8080")]
        [AssetBundleConfig("https://ftp-labyrinthnft.gnjoy.id/test/asset", AssetBundleSettings.Mode.Editor)]
        TestNFT,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("elb-labyrinthnft.gnjoy.id", 9933, "https://ftp-labyrinthnft.gnjoy.id/real")]
        [AssetBundleConfig("https://ftp-labyrinthnft.gnjoy.id/real/asset", AssetBundleSettings.Mode.Editor)]
        RealNFT,

        [StoreTypeConfig(StoreType.GOOGLE)]
        [AuthServerConfig("staging-labyrinthnft.gnjoy.id", 9933, "https://ftp-labyrinthnft.gnjoy.id/stage")]
        [AssetBundleConfig("https://ftp-labyrinthnft.gnjoy.id/stage/asset", AssetBundleSettings.Mode.Editor)]
        StageNFT,
    }
}