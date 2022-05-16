using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ragnarok
{
    public class AssetManager : Singleton<AssetManager>
        , ICharacterContainer
        , IMonsterContainer
        , INpcContainer
        , IItemContainer
        , IUIContainer
        , IEffectContainer
        , ICharacterAniContainer
        , IBgmContainer
        , ISfxContainer
        , IUiSfxContainer
        , ISkillSettingContainer
        , IProjectileSettingContainer
        , IHUDContainer
        , IFontContainer
        , ITextureContainer
        , ISpecialRouletteConfigContainer
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            string sceneName = SceneManager.GetActiveScene().name;
            if (!(sceneName.Equals(SceneLoader.PRELOAD) || sceneName.Equals(SceneLoader.INTRO)))
                return;
#endif

            Instance.LoadLocalAssetBundle();
        }

        private const string LOCAL = "local";
        private const string PREFAB_CHARACTER = "prefab_character";
        private const string PREFAB_MONSTER = "prefab_monster";
        private const string PREFAB_NPC = "prefab_npc";
        private const string PREFAB_ITEM = "prefab_item";
        private const string ANIMATION_CHARACTER = "animation_character";
        private const string PREFAB_UI = "prefab_ui";
        private const string PREFAB_EFFECT = "prefab_effect";
        private const string HUD = "hud";
        private const string SOUND = "sound";
        private const string DATA_SKILL = "data/skill_setting";
        private const string DATA_PROJECTILE = "data/projectile_setting";
        private const string DATA_SPECIAL_ROULETTE_CONFIG = "data/special_roulette_config";
        private const string DATA_SPECIAL_ROULETTE_CONFIG_NAME = "SpecialRouletteConfig";

        private readonly AssetLoader assetLoader;
        private readonly ImageDownloader imageDownloader;

        public static event System.Action OnAllAssetReady;

        public event System.Action<float> OnDownloadTotalProgress;
        public event System.Action<float> OnDownloadDetailProgress;
        public event System.Action<int, int> OnDownloadCountProgress;
        public event System.Action<float> OnLoadTotalProgress;
        public event System.Action<float> OnLoadDetailProgress;
        public event System.Action<int, int> OnLoadCountProgress;
        public event System.Action<long> OnLoadloadSpeed;

        /******************** assetBundleNameCollection ********************/
        private readonly BetterList<AssetBundleNameCollection> textureNameCollections;
        private readonly AssetBundleNameCollection textureAdventureNameCollection;
        private readonly AssetBundleNameCollection textureBossNameCollection;
        private readonly AssetBundleNameCollection textureBoxNameCollection;
        private readonly AssetBundleNameCollection textureCardNameCollection;
        private readonly AssetBundleNameCollection textureCardOptionPanalNameCollection;
        private readonly AssetBundleNameCollection textureContentsUnlockNameCollection;
        private readonly AssetBundleNameCollection textureDungeonNameCollection;
        private readonly AssetBundleNameCollection textureEventNameCollection;
        private readonly AssetBundleNameCollection textureGuildEmblemNameCollection;
        private readonly AssetBundleNameCollection textureItemNameCollection;
        private readonly AssetBundleNameCollection textureItemSourceCollection;
        private readonly AssetBundleNameCollection textureJobAgentNameCollection;
        private readonly AssetBundleNameCollection textureJobIconNameCollection;
        private readonly AssetBundleNameCollection textureJobIllustNameCollection;
        private readonly AssetBundleNameCollection textureJobProfileNameCollection;
        private readonly AssetBundleNameCollection textureJobSDNameCollection;
        private readonly AssetBundleNameCollection textureLoadingTipNameCollection;
        private readonly AssetBundleNameCollection textureMonsterNameCollection;
        private readonly AssetBundleNameCollection textureNPCNameCollection;
        private readonly AssetBundleNameCollection textureRankTierNameCollection;
        private readonly AssetBundleNameCollection textureShopNameCollection;
        private readonly AssetBundleNameCollection textureSkillNameCollection;

        private bool isLoadLocalAssetBundle;

        public static bool IsAllAssetReady { get; private set; }

        public AssetManager()
        {
            assetLoader = BuildSettings.Instance.GetAssetLoader();
            imageDownloader = new ImageDownloader();

            textureNameCollections = new BetterList<AssetBundleNameCollection>();
            textureAdventureNameCollection = new AssetBundleNameCollection("adventure", "texture/adventure/", 4);
            textureBossNameCollection = new AssetBundleNameCollection("boss", "texture/boss/", 3);
            textureBoxNameCollection = new AssetBundleNameCollection("box", "texture/box/", 3);
            textureCardNameCollection = new AssetBundleNameCollection("card", "texture/card/", 2);
            textureCardOptionPanalNameCollection = new AssetBundleNameCollection("card_option_panel", "texture/card_option_panel/", 2);
            textureContentsUnlockNameCollection = new AssetBundleNameCollection("contents_unlock", "texture/contents_unlock/", 1);
            textureDungeonNameCollection = new AssetBundleNameCollection("dungeon", "texture/dungeon/", 1);
            textureEventNameCollection = new AssetBundleNameCollection("event", "texture/event/", 10);
            textureGuildEmblemNameCollection = new AssetBundleNameCollection("guild_emblem", "texture/guild_emblem/", 1);
            textureItemNameCollection = new AssetBundleNameCollection("item", "texture/item/", 12);
            textureItemSourceCollection = new AssetBundleNameCollection("item_source", "texture/item_source/", 1);
            textureJobAgentNameCollection = new AssetBundleNameCollection("job_agent", "texture/job_agent/", 2);
            textureJobIconNameCollection = new AssetBundleNameCollection("job_icon", "texture/job_icon/", 1);
            textureJobIllustNameCollection = new AssetBundleNameCollection("job_illust", "texture/job_illust/", 2);
            textureJobProfileNameCollection = new AssetBundleNameCollection("job_profile", "texture/job_profile/", 6);
            textureJobSDNameCollection = new AssetBundleNameCollection("job_sd", "texture/job_sd/", 2);
            textureLoadingTipNameCollection = new AssetBundleNameCollection("loading_tip", "texture/loading_tip/", 1);
            textureMonsterNameCollection = new AssetBundleNameCollection("monster", "texture/monster/", 4);
            textureNPCNameCollection = new AssetBundleNameCollection("npc", "texture/npc/", 1);
            textureRankTierNameCollection = new AssetBundleNameCollection("rank_tier", "texture/rank_tier/", 1);
            textureShopNameCollection = new AssetBundleNameCollection("shop", "texture/shop/", 1);
            textureSkillNameCollection = new AssetBundleNameCollection("skill", "texture/skill/", 6);

            textureNameCollections.Add(textureAdventureNameCollection);
            textureNameCollections.Add(textureBossNameCollection);
            textureNameCollections.Add(textureBossNameCollection);
            textureNameCollections.Add(textureBoxNameCollection);
            textureNameCollections.Add(textureCardNameCollection);
            textureNameCollections.Add(textureCardOptionPanalNameCollection);
            textureNameCollections.Add(textureDungeonNameCollection);
            textureNameCollections.Add(textureEventNameCollection);
            textureNameCollections.Add(textureContentsUnlockNameCollection);
            textureNameCollections.Add(textureGuildEmblemNameCollection);
            textureNameCollections.Add(textureItemNameCollection);
            textureNameCollections.Add(textureItemSourceCollection);
            textureNameCollections.Add(textureJobAgentNameCollection);
            textureNameCollections.Add(textureJobIconNameCollection);
            textureNameCollections.Add(textureJobIllustNameCollection);
            textureNameCollections.Add(textureJobProfileNameCollection);
            textureNameCollections.Add(textureJobSDNameCollection);
            textureNameCollections.Add(textureLoadingTipNameCollection);
            textureNameCollections.Add(textureMonsterNameCollection);
            textureNameCollections.Add(textureNPCNameCollection);
            textureNameCollections.Add(textureRankTierNameCollection);
            textureNameCollections.Add(textureShopNameCollection);
            textureNameCollections.Add(textureSkillNameCollection);

            IsAllAssetReady = false;

            assetLoader.OnDownloadTotalProgress += OnAssetBundleTotalDownload;
            assetLoader.OnDownloadDetailProgress += OnAssetBundleDetailDownload;
            assetLoader.OnDownloadCountProgress += OnAssetBundleDownloadCount;
            assetLoader.OnLoadTotalProgress += OnAssetBundleTotalLoad;
            assetLoader.OnLoadDetailProgress += OnAssetBundleDetailLoad;
            assetLoader.OnLoadCountProgress += OnAssetBundleLoadCount;
            assetLoader.OnAllReady += OnAssetBundleAllReady;
            assetLoader.OnDownloadSpeed += OnAssetDownloadSpeed;
        }

        ~AssetManager()
        {
            assetLoader.OnDownloadTotalProgress -= OnAssetBundleTotalDownload;
            assetLoader.OnDownloadDetailProgress -= OnAssetBundleDetailDownload;
            assetLoader.OnDownloadCountProgress -= OnAssetBundleDownloadCount;
            assetLoader.OnLoadTotalProgress -= OnAssetBundleTotalLoad;
            assetLoader.OnLoadDetailProgress -= OnAssetBundleDetailLoad;
            assetLoader.OnLoadCountProgress -= OnAssetBundleLoadCount;
            assetLoader.OnAllReady -= OnAssetBundleAllReady;
            assetLoader.OnDownloadSpeed -= OnAssetDownloadSpeed;
        }

        protected override void OnTitle()
        {
            if (IsAllAssetReady)
                return;

            //UnloadAssetBundle(); // 어셋 번들 해제
            LoadLocalAssetBundle(); // Local 어셋 준비
            imageDownloader.Clear();
        }

        private void LoadLocalAssetBundle()
        {
            if (isLoadLocalAssetBundle)
                return;

            assetLoader.LoadLocalAssetBundle();

            isLoadLocalAssetBundle = true;
        }

        public Task<long> DownloadPatchList()
        {
            return assetLoader.DownloadPatchList();
        }

        public bool IsShowDownload()
        {
            return assetLoader.isShowDownload;
        }

        public Task Download()
        {
            return assetLoader.DownloadAssetBundles();
        }

        public Task Load()
        {
            return assetLoader.LoadCachedAssetBundles();
        }

        void OnAssetBundleTotalDownload(float progress)
        {
            OnDownloadTotalProgress?.Invoke(progress);
        }

        void OnAssetBundleDetailDownload(float progress)
        {
            OnDownloadDetailProgress?.Invoke(progress);
        }

        void OnAssetBundleDownloadCount(int cur, int max)
        {
            OnDownloadCountProgress?.Invoke(cur, max);
        }

        void OnAssetBundleTotalLoad(float progress)
        {
            OnLoadTotalProgress?.Invoke(progress);
        }

        void OnAssetBundleDetailLoad(float progress)
        {
            OnLoadDetailProgress?.Invoke(progress);
        }

        void OnAssetBundleLoadCount(int cur, int max)
        {
            OnLoadCountProgress?.Invoke(cur, max);
        }

        void OnAssetBundleAllReady()
        {
            IsAllAssetReady = true;
            OnAllAssetReady?.Invoke();
        }

        void OnAssetDownloadSpeed(long downladSize)
        {
            OnLoadloadSpeed?.Invoke(downladSize);
        }

        GameObject ICharacterContainer.Get(string name)
        {
            GameObject go = assetLoader.GetResource<GameObject>(PREFAB_CHARACTER, name);

            if (go == null)
                Debug.LogError($"name에 해당하는 Character가 존재하지 않습니다: name = {name}");

            return go;
        }

        GameObject IMonsterContainer.Get(string name)
        {
            GameObject go = assetLoader.GetResource<GameObject>(PREFAB_MONSTER, name);

            if (go == null)
                Debug.LogError($"name에 해당하는 Monster가 존재하지 않습니다: name = {name}");

            return go;
        }

        GameObject INpcContainer.Get(string name)
        {
            GameObject go = assetLoader.GetResource<GameObject>(PREFAB_NPC, name);

            if (go == null)
                Debug.LogError($"name에 해당하는 NPC가 존재하지 않습니다: name = {name}");

            return go;
        }

        GameObject IItemContainer.Get(string name)
        {
            GameObject go = assetLoader.GetResource<GameObject>(PREFAB_ITEM, name);

            if (go == null)
                Debug.LogError($"name에 해당하는 Item이 존재하지 않습니다: name = {name}");

            return go;
        }

        AnimationClip[] ICharacterAniContainer.GetAniClips()
        {
            return assetLoader.GetResourceAll<AnimationClip>(ANIMATION_CHARACTER);
        }

        GameObject IUIContainer.GetUI(string assetName)
        {
            GameObject go = assetLoader.GetResource<GameObject>(LOCAL, assetName);

            if (go == null)
                go = assetLoader.GetResource<GameObject>(PREFAB_UI, assetName);

            if (go == null)
                Debug.LogError($"UI가 존재하지 않습니다: assetName = {assetName}");

            return go;
        }

        SkillSetting ISkillSettingContainer.Get(int id)
        {
            SkillSetting[] settings = assetLoader.GetResourceAll<SkillSetting>(DATA_SKILL);
            foreach (var item in settings)
            {
                if (item.id == id)
                    return item;
            }

            Debug.LogError($"id에 해당하는 SkillSetting 이 존재하지 않습니다: id = {id}");
            return null;
        }

        ProjectileSetting IProjectileSettingContainer.Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            ProjectileSetting[] settings = assetLoader.GetResourceAll<ProjectileSetting>(DATA_PROJECTILE);
            foreach (var item in settings)
            {
                if (item.name == name)
                    return item;
            }

            Debug.LogError($"ProjectileSetting 이 존재하지 않습니다: name = {name}");
            return null;
        }

        GameObject IEffectContainer.Get(string name)
        {
            return assetLoader.GetResource<GameObject>(PREFAB_EFFECT, name);
        }

        GameObject IHUDContainer.Get(string name)
        {
            GameObject go = assetLoader.GetResource<GameObject>(HUD, name);

            if (go == null)
                Debug.LogError($"HUD가 존재하지 않습니다: name = {name}");

            return go;
        }

        AudioClip IBgmContainer.Get(string name)
        {
            AudioClip clip = assetLoader.GetResource<AudioClip>(LOCAL, name);

            if (clip == null)
                clip = assetLoader.GetResource<AudioClip>(SOUND, name);

            if (clip == null)
                Debug.LogError($"BGM이 존재하지 않습니다: name = {name}");

            return clip;
        }

        AudioClip ISfxContainer.Get(string name)
        {
            AudioClip clip = assetLoader.GetResource<AudioClip>(SOUND, name);

            if (clip == null)
                Debug.LogError($"Sfx가 존재하지 않습니다: name = {name}");

            return clip;
        }

        AudioClip IUiSfxContainer.Get(string name)
        {
            AudioClip clip = assetLoader.GetResource<AudioClip>(LOCAL, name);

            if (clip == null)
                clip = assetLoader.GetResource<AudioClip>(SOUND, name);

            if (clip == null)
                Debug.LogError($"UI_Sfx가 존재하지 않습니다: name = {name}");

            return clip;
        }

        Font IFontContainer.Get(string name)
        {
            return assetLoader.GetResource<Font>(LOCAL, name);
        }

        Texture ITextureContainer.Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (assetLoader.Contains(LOCAL, name))
                return assetLoader.GetResource<Texture>(LOCAL, name);

            foreach (var item in textureNameCollections)
            {
                Texture texture = GetTexture(name, item, isLog: false);
                if (texture != null)
                    return texture;
            }

            Debug.LogError($"Texture가 존재하지 않습니다: {nameof(name)} = {name}");
            return null;
        }

        Texture ITextureContainer.GetAdventure(string name)
        {
            return GetTexture(name, textureAdventureNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetBoss(string name)
        {
            return GetTexture(name, textureBossNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetBox(string name)
        {
#if UNITY_EDITOR
            return GetTexture(name, textureBoxNameCollection, isLog: true);
#else
            return GetTexture(name, textureBoxNameCollection, isLog: false);
#endif
        }

        Texture ITextureContainer.GetCard(string name)
        {
            return GetTexture(name, textureCardNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetCardOptionPanel(string name)
        {
            return GetTexture(name, textureCardOptionPanalNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetContentsUnlock(string name)
        {
            return GetTexture(name, textureContentsUnlockNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetDungeon(string name)
        {
            return GetTexture(name, textureDungeonNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetEvent(string name)
        {
            return GetTexture(name, textureEventNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetGuildEmblem(string name)
        {
            return GetTexture(name, textureGuildEmblemNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetItem(string name)
        {
            return GetTexture(name, textureItemNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetItemSource(string name)
        {
            return GetTexture(name, textureItemSourceCollection, isLog: true);
        }

        Texture ITextureContainer.GetJobAgent(string name)
        {
            return GetTexture(name, textureJobAgentNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetJobIcon(string name)
        {
            return GetTexture(name, textureJobIconNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetJobIllust(string name)
        {
            return GetTexture(name, textureJobIllustNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetJobProfile(string name)
        {
            return GetTexture(name, textureJobProfileNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetJobSD(string name)
        {
            return GetTexture(name, textureJobSDNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetLoadingTip(string name)
        {
            return GetTextureWithLang(name, textureLoadingTipNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetPrologueTip(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            LanguageConfig config = LanguageConfig.GetBytKey(Language.Current);
            if (config != null)
            {
                string langName = string.Concat(name, "_", config.type);
                if (assetLoader.Contains(LOCAL, langName))
                    return assetLoader.GetResource<Texture>(LOCAL, langName);
            }

            if (assetLoader.Contains(LOCAL, name))
                return assetLoader.GetResource<Texture>(LOCAL, name);

            Debug.LogError($"Texture가 존재하지 않습니다: {nameof(name)} = {name}");
            return null;
        }

        Texture ITextureContainer.GetMonster(string name)
        {
            return GetTexture(name, textureMonsterNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetNPC(string name)
        {
            return GetTexture(name, textureNPCNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetRankTier(string name)
        {
            return GetTexture(name, textureRankTierNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetShop(string name)
        {
            return GetTexture(name, textureShopNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetSkill(string name)
        {
            return GetTexture(name, textureSkillNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGet(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            if (assetLoader.Contains(LOCAL, name))
                return assetLoader.GetResourceAsync<Texture>(LOCAL, name);

            foreach (var item in textureNameCollections)
            {
                AssetBundleRequest request = GetAsyncTexture(name, item, isLog: false);
                if (request != null)
                    return request;
            }

            Debug.LogError($"Texture가 존재하지 않습니다: {nameof(name)} = {name}");
            return null;
        }

        AssetBundleRequest ITextureContainer.AsyncGetAdventure(string name)
        {
            return GetAsyncTexture(name, textureAdventureNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetBoss(string name)
        {
            return GetAsyncTexture(name, textureBossNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetBox(string name)
        {
#if UNITY_EDITOR
            return GetAsyncTexture(name, textureBoxNameCollection, isLog: true);
#else
            return GetAsyncTexture(name, textureBoxNameCollection, isLog: false);
#endif
        }

        AssetBundleRequest ITextureContainer.AsyncGetCard(string name)
        {
            return GetAsyncTexture(name, textureCardNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetCardOptionPanel(string name)
        {
            return GetAsyncTexture(name, textureCardOptionPanalNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetContentsUnlock(string name)
        {
            return GetAsyncTexture(name, textureContentsUnlockNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetDungeon(string name)
        {
            return GetAsyncTexture(name, textureDungeonNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetEvent(string name)
        {
            return GetAsyncTexture(name, textureEventNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetGuildEmblem(string name)
        {
            return GetAsyncTexture(name, textureGuildEmblemNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetItem(string name)
        {
            return GetAsyncTexture(name, textureItemNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetItemSource(string name)
        {
            return GetAsyncTexture(name, textureItemSourceCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetJobAgent(string name)
        {
            return GetAsyncTexture(name, textureJobAgentNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetJobIcon(string name)
        {
            return GetAsyncTexture(name, textureJobIconNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetJobIllust(string name)
        {
            return GetAsyncTexture(name, textureJobIllustNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetJobProfile(string name)
        {
            return GetAsyncTexture(name, textureJobProfileNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetJobSD(string name)
        {
            return GetAsyncTexture(name, textureJobSDNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetLoadingTip(string name)
        {
            return GetAsyncTextureWithLang(name, textureLoadingTipNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetPrologueTip(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            LanguageConfig config = LanguageConfig.GetBytKey(Language.Current);
            if (config != null)
            {
                string langName = string.Concat(name, "_", config.type);
                if (assetLoader.Contains(LOCAL, langName))
                    return assetLoader.GetResourceAsync<Texture>(LOCAL, langName);
            }

            if (assetLoader.Contains(LOCAL, name))
                return assetLoader.GetResourceAsync<Texture>(LOCAL, name);

            Debug.LogError($"Texture가 존재하지 않습니다: {nameof(name)} = {name}");
            return null;
        }

        AssetBundleRequest ITextureContainer.AsyncGetMonster(string name)
        {
            return GetAsyncTexture(name, textureMonsterNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetNPC(string name)
        {
            return GetAsyncTexture(name, textureNPCNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetRankTier(string name)
        {
            return GetAsyncTexture(name, textureRankTierNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetShop(string name)
        {
            return GetAsyncTexture(name, textureShopNameCollection, isLog: true);
        }

        AssetBundleRequest ITextureContainer.AsyncGetSkill(string name)
        {
            return GetAsyncTexture(name, textureSkillNameCollection, isLog: true);
        }

        Texture ITextureContainer.GetFromUrl(string url)
        {
            return imageDownloader.Get(url);
        }

        void ITextureContainer.Cache(string name, Texture downloaded)
        {
            imageDownloader.Cache(name, downloaded);
        }

        SpecialRouletteConfig.Config ISpecialRouletteConfigContainer.Get(int index)
        {
            if (index < 0)
                return null;

            SpecialRouletteConfig configs = assetLoader.GetResource<SpecialRouletteConfig>(DATA_SPECIAL_ROULETTE_CONFIG, DATA_SPECIAL_ROULETTE_CONFIG_NAME);
            if (configs == null)
                return null;

            return configs.Get(index);
        }

        private Texture GetTexture(string name, AssetBundleNameCollection collection, bool isLog)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (string assetBundleName in collection)
            {
                if (assetLoader.Contains(assetBundleName, name))
                    return assetLoader.GetResource<Texture>(assetBundleName, name);
            }

            if (isLog)
                Debug.LogError($"Texture가 존재하지 않습니다 ({collection}): {nameof(name)} = {name}");

            return null;
        }

        private AssetBundleRequest GetAsyncTexture(string name, AssetBundleNameCollection collection, bool isLog)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (string assetBundleName in collection)
            {
                if (assetLoader.Contains(assetBundleName, name))
                    return assetLoader.GetResourceAsync<Texture>(assetBundleName, name);
            }

            if (isLog)
                Debug.LogError($"Texture가 존재하지 않습니다 ({collection}): {nameof(name)} = {name}");

            return null;
        }

        private Texture GetTextureWithLang(string name, AssetBundleNameCollection collection, bool isLog)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            LanguageConfig config = LanguageConfig.GetBytKey(Language.Current);
            if (config != null)
            {
                string langName = string.Concat(name, "_", config.type);
                foreach (string assetBundleName in collection)
                {
                    if (assetLoader.Contains(assetBundleName, langName))
                        return assetLoader.GetResource<Texture>(assetBundleName, langName);
                }
            }

            foreach (string assetBundleName in collection)
            {
                if (assetLoader.Contains(assetBundleName, name))
                    return assetLoader.GetResource<Texture>(assetBundleName, name);
            }

            if (isLog)
                Debug.LogError($"Texture가 존재하지 않습니다 ({collection}): {nameof(name)} = {name}");

            return null;
        }

        private AssetBundleRequest GetAsyncTextureWithLang(string name, AssetBundleNameCollection collection, bool isLog)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            LanguageConfig config = LanguageConfig.GetBytKey(Language.Current);
            if (config != null)
            {
                string langName = string.Concat(name, "_", config.type);
                foreach (string assetBundleName in collection)
                {
                    if (assetLoader.Contains(assetBundleName, langName))
                        return assetLoader.GetResourceAsync<Texture>(assetBundleName, langName);
                }
            }

            foreach (string assetBundleName in collection)
            {
                if (assetLoader.Contains(assetBundleName, name))
                    return assetLoader.GetResourceAsync<Texture>(assetBundleName, name);
            }

            if (isLog)
                Debug.LogError($"Texture가 존재하지 않습니다 ({collection}): {nameof(name)} = {name}");

            return null;
        }
    }
}