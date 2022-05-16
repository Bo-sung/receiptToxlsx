using UnityEngine;

namespace Ragnarok
{
    public interface ITextureContainer
    {
        Texture Get(string name);
        Texture GetAdventure(string name);
        Texture GetBoss(string name);
        Texture GetBox(string name);
        Texture GetCard(string name);
        Texture GetCardOptionPanel(string name);
        Texture GetContentsUnlock(string name);
        Texture GetDungeon(string name);
        Texture GetEvent(string name);
        Texture GetGuildEmblem(string name);
        Texture GetItem(string name);
        Texture GetItemSource(string name);
        Texture GetJobAgent(string name);
        Texture GetJobIcon(string name);
        Texture GetJobIllust(string name);
        Texture GetJobProfile(string name);
        Texture GetJobSD(string name);
        Texture GetLoadingTip(string name);
        Texture GetPrologueTip(string name);
        Texture GetMonster(string name);
        Texture GetNPC(string name);
        Texture GetRankTier(string name);
        Texture GetShop(string name);
        Texture GetSkill(string name);

        AssetBundleRequest AsyncGet(string name);
        AssetBundleRequest AsyncGetAdventure(string name);
        AssetBundleRequest AsyncGetBoss(string name);
        AssetBundleRequest AsyncGetBox(string name);
        AssetBundleRequest AsyncGetCard(string name);
        AssetBundleRequest AsyncGetCardOptionPanel(string name);
        AssetBundleRequest AsyncGetContentsUnlock(string name);
        AssetBundleRequest AsyncGetDungeon(string name);
        AssetBundleRequest AsyncGetEvent(string name);
        AssetBundleRequest AsyncGetGuildEmblem(string name);
        AssetBundleRequest AsyncGetItem(string name);
        AssetBundleRequest AsyncGetItemSource(string name);
        AssetBundleRequest AsyncGetJobAgent(string name);
        AssetBundleRequest AsyncGetJobIcon(string name);
        AssetBundleRequest AsyncGetJobIllust(string name);
        AssetBundleRequest AsyncGetJobProfile(string name);
        AssetBundleRequest AsyncGetJobSD(string name);
        AssetBundleRequest AsyncGetLoadingTip(string name);
        AssetBundleRequest AsyncGetPrologueTip(string name);
        AssetBundleRequest AsyncGetMonster(string name);
        AssetBundleRequest AsyncGetNPC(string name);
        AssetBundleRequest AsyncGetRankTier(string name);
        AssetBundleRequest AsyncGetShop(string name);
        AssetBundleRequest AsyncGetSkill(string name);

        Texture GetFromUrl(string url);
        void Cache(string url, Texture downloaded);
    }
}