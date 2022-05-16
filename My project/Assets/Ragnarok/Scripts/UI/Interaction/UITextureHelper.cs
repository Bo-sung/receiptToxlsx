using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Ragnarok
{
    [ExecuteInEditMode]
    public class UITextureHelper : UIRoundTexture
    {
        public enum BundleType
        {
            Common,
            Adeventure,
            Boss,
            Box,
            Card,
            CardOptionPanel,
            ContentsUnlock,
            Dungeon,
            Event,
            GuildEmblem,
            Item,
            ItemSource,
            JobAgent,
            JobIcon,
            JobIllust,
            JobProfile,
            JobSd,
            LoadingTip,
            Monster,
            Npc,
            RankTier,
            Shop,
            Skill,
        }

        [HideInInspector, SerializeField]
        UIGraySprite.SpriteMode mode;

        public UIGraySprite.SpriteMode Mode
        {
            get { return mode; }
            set
            {
                if (mode == value)
                    return;

                mode = value;

                if (panel != null)
                {
                    panel.RemoveWidget(this);
                    panel = null;
                }

                CreatePanel();
            }
        }

        public override Shader shader
        {
            get
            {
                if (mode == UIGraySprite.SpriteMode.None)
                    return base.shader;

                return Shader.Find("Unlit/GrayScale");
            }
            set { base.shader = value; }
        }

        private ITextureContainer textureContainer;
        private bool isInitialized;
        private string savedTextureName;

        [System.Obsolete("Use SetAdventure or SetBoss or SetBox or...")]
        public void Set(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGet(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.Get(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetAdventure(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetAdventure(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetAdventure(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetBoss(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetBoss(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetBoss(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetBox(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetBox(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetBox(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetCard(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetCard(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetCard(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetCardOptionPanel(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetCardOptionPanel(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetCardOptionPanel(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetContentsUnlock(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetContentsUnlock(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetContentsUnlock(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetDungeon(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetDungeon(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetDungeon(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetEvent(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetEvent(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetEvent(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetGuildEmblem(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetGuildEmblem(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetGuildEmblem(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetItem(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetItem(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetItem(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetItemSource(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetItemSource(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetItemSource(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetJobAgent(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetJobAgent(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetJobAgent(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetJobIcon(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetJobIcon(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetJobIcon(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetJobIllust(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetJobIllust(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetJobIllust(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetJobProfile(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetJobProfile(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetJobProfile(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetJobSD(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetJobSD(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetJobSD(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetLoadingTip(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetLoadingTip(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetLoadingTip(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetPrologueTip(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetPrologueTip(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetPrologueTip(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetMonster(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetMonster(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetMonster(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetNPC(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetNPC(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetNPC(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetRankTier(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetRankTier(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetRankTier(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetShop(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetShop(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetShop(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetSkill(string textureName, bool isAsync = true)
        {
            if (EqualsTextureName(textureName))
                return;

            Initialize();

            if (isAsync && cachedGameObject.activeInHierarchy)
            {
                AssetBundleRequest request = textureContainer.AsyncGetSkill(textureName);
                StartSetTexture(request, textureName);
            }
            else
            {
                Texture texture = textureContainer.GetSkill(textureName);
                SetTexture(texture, textureName);
            }
        }

        public void SetFromUrl(string url)
        {
            if (EqualsTextureName(url))
                return;

            StartDownloadTexture(url);
        }

        public void Load(BundleType bundleType, string textureName, bool isAsync = true)
        {
            switch (bundleType)
            {
                case BundleType.Common:
                    Set(textureName, isAsync);
                    break;

                case BundleType.Adeventure:
                    SetAdventure(textureName, isAsync);
                    break;

                case BundleType.Boss:
                    SetBoss(textureName, isAsync);
                    break;

                case BundleType.Box:
                    SetBox(textureName, isAsync);
                    break;

                case BundleType.Card:
                    SetCard(textureName, isAsync);
                    break;

                case BundleType.CardOptionPanel:
                    SetCardOptionPanel(textureName, isAsync);
                    break;

                case BundleType.ContentsUnlock:
                    SetContentsUnlock(textureName, isAsync);
                    break;

                case BundleType.Dungeon:
                    SetDungeon(textureName, isAsync);
                    break;

                case BundleType.Event:
                    SetEvent(textureName, isAsync);
                    break;

                case BundleType.GuildEmblem:
                    SetGuildEmblem(textureName, isAsync);
                    break;

                case BundleType.Item:
                    SetItem(textureName, isAsync);
                    break;

                case BundleType.ItemSource:
                    SetItemSource(textureName, isAsync);
                    break;

                case BundleType.JobAgent:
                    SetJobAgent(textureName, isAsync);
                    break;

                case BundleType.JobIcon:
                    SetJobIcon(textureName, isAsync);
                    break;

                case BundleType.JobIllust:
                    SetJobIllust(textureName, isAsync);
                    break;

                case BundleType.JobProfile:
                    SetJobProfile(textureName, isAsync);
                    break;

                case BundleType.JobSd:
                    SetJobSD(textureName, isAsync);
                    break;

                case BundleType.LoadingTip:
                    SetLoadingTip(textureName, isAsync);
                    break;

                case BundleType.Monster:
                    SetMonster(textureName, isAsync);
                    break;

                case BundleType.Npc:
                    SetNPC(textureName, isAsync);
                    break;

                case BundleType.RankTier:
                    SetRankTier(textureName, isAsync);
                    break;

                case BundleType.Shop:
                    SetShop(textureName, isAsync);
                    break;

                case BundleType.Skill:
                    SetSkill(textureName, isAsync);
                    break;
            }
        }

        public void SetActive(bool isActive)
        {
            cachedGameObject.SetActive(isActive);
        }

        public void SetTexture(Texture texture, string textureName)
        {
            KillCoroutines(); // 코루틴 중지

            mainTexture = texture;
            savedTextureName = textureName;

            if (texture == null)
                Invalidate(false);
        }

        private void Initialize()
        {
            if (isInitialized)
                return;

            textureContainer = AssetManager.Instance;
            isInitialized = true;
        }

        private void StartSetTexture(AssetBundleRequest request, string textureName)
        {
            SetTexture(null, string.Empty);

            if (request == null)
                return;

            Timing.RunCoroutineSingleton(YieldSetTexture(request, textureName).CancelWith(cachedGameObject), cachedGameObject, SingletonBehavior.Overwrite);
        }

        private void StartDownloadTexture(string url)
        {
            Initialize();

            Texture texture = textureContainer.GetFromUrl(url);

            if (texture != null)
            {
                SetTexture(texture, url);
                return;
            }

            SetTexture(Texture2D.grayTexture, string.Empty);

            if (string.IsNullOrEmpty(url))
                return;

            Timing.RunCoroutineSingleton(YieldDownloadTexture(url).CancelWith(cachedGameObject), cachedGameObject, SingletonBehavior.Overwrite);
        }

        private void KillCoroutines()
        {
            Timing.KillCoroutines(cachedGameObject);
        }

        private bool EqualsTextureName(string textureName)
        {
            if (string.Equals(savedTextureName, textureName))
                return true;

            savedTextureName = textureName;
            return false;
        }

        IEnumerator<float> YieldSetTexture(AssetBundleRequest request, string textureName)
        {
            yield return Timing.WaitUntilDone(request);
            SetTexture(request.asset as Texture, textureName);
        }

        IEnumerator<float> YieldDownloadTexture(string url)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return Timing.WaitUntilDone(request.SendWebRequest());

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError($"DownloadTexture Error: {nameof(url)} = {url}\n{nameof(request.error)} = {request.error}");
                }
                else
                {
                    Texture texture = DownloadHandlerTexture.GetContent(request);
                    SetTexture(texture, url);

                    textureContainer.Cache(url, texture); // 캐싱
                }
            }
        }
    }
}