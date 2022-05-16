using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRewardLauncher : UICanvas
    {
        private const int GOODS_MIN_SIZE = 1; // 최소 개수
        private const int GOODS_MAX_SIZE = 10; // 최대 개수
        private const int GOODS_SIZE_PER_VALUE = 20; // 특정값 당 발사체 개수
        private const float GOODS_DELAY_PER_SIZE = 0.04f; // 발사체 당 딜레이 (개수가 늘어날 때마다 딜레이 추가)
        private const float GOODS_RANDOM_RADIUS = 80f; // 발사체 랜덤 반경 위치

        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        public enum GoodsDestination
        {
            Basic,
            Mail,
        }

        [SerializeField] PlayTweenPoolManager goodsPool;
        [SerializeField] PlayTweenPoolManager itemPool;

        GoodsDestination destination;

        protected override void OnInit()
        {
            itemPool.OnDespawn += OnItemDespawn;
        }

        protected override void OnClose()
        {
            itemPool.OnDespawn -= OnItemDespawn;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void Clear()
        {
            goodsPool.Clear();
            itemPool.Clear();
        }

        public void LaunchGoods(Vector3 from, RewardType rewardType, int itemValue, GoodsDestination dest)
        {
            int poolSize = GetPoolSize(itemValue);
            UIMainTop.MenuContent contentType = GetMenuContent(rewardType);
            Vector3 destination = GetGoodsDestination(contentType, dest);
            float delay = 0f;
            string itemIcon = rewardType.IconName();
            while (--poolSize >= 0)
            {
                goodsPool.Spawn()
                    .SetPosition(from, destination, GOODS_RANDOM_RADIUS)
                    .AddDelay(delay)
                    .Initialize(itemIcon)
                    .Launch();

                delay += GOODS_DELAY_PER_SIZE; // 딜레이 추가
            }
        }

        public void LaunchItem(Vector3 from, string itemIcon, int itemCount, GoodsDestination destination)
        {
            itemPool.Spawn()
                .SetPosition(from, GetItemDestination(destination))
                .Initialize(itemIcon, itemCount)
                .Launch();
        }

        private int GetPoolSize(int value)
        {
            return Mathf.Clamp(value / GOODS_SIZE_PER_VALUE, GOODS_MIN_SIZE, GOODS_MAX_SIZE);
        }

        private UIMainTop.MenuContent GetMenuContent(RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.Zeny:
                    return UIMainTop.MenuContent.Zeny;

                case RewardType.JobExp:
                    return UIMainTop.MenuContent.JobExp;

                case RewardType.LevelExp:
                    return UIMainTop.MenuContent.Exp;

                case RewardType.CatCoin:
                    return UIMainTop.MenuContent.CatCoin;

                case RewardType.ROPoint:
                    return UIMainTop.MenuContent.RoPoint;

                    // TODO: 길드코인 추가
                    //case RewardType.GuildCoin:
                    //    return 
            }

            return default;
        }

        private Vector3 GetGoodsDestination(UIMainTop.MenuContent content, GoodsDestination type)
        {
            destination = type;

            switch (destination)
            {
                default:
                case GoodsDestination.Basic:
                    return GetMainTop(content);

                case GoodsDestination.Mail:
                    return GetMailPosition();
            }
        }

        private Vector3 GetItemDestination(GoodsDestination type)
        {
            destination = type;

            switch (destination)
            {
                default:
                case GoodsDestination.Basic:
                    return GetInvenPosition();

                case GoodsDestination.Mail:
                    return GetMailPosition();
            }
        }

        void OnItemDespawn()
        {
            switch (destination)
            {
                default:
                case GoodsDestination.Basic:
                    PlayInvenTween();
                    break;

                case GoodsDestination.Mail:
                    PlayMailTween();
                    break;
            }
        }

        private Vector3 GetMainTop(UIMainTop.MenuContent content)
        {
            UIMainTop uiMainTop = UI.GetUI<UIMainTop>();

            if (uiMainTop == null)
                return Vector3.zero;

            return uiMainTop.GetPosition(content);
        }

        private Vector3 GetInvenPosition()
        {
            UIMain ui = UI.GetUI<UIMain>();

            if (ui == null)
                return Vector3.zero;

            return ui.GetPosition(UIMain.MenuContent.Inven);
        }

        private Vector3 GetMailPosition()
        {
            UIMainShortcut ui = UI.GetUI<UIMainShortcut>();

            if (ui == null)
                return Vector3.zero;

            return ui.GetPosition(UIMainShortcut.MenuContent.Mail);
        }

        private void PlayInvenTween()
        {
            UIMain ui = UI.GetUI<UIMain>();

            if (ui == null)
                return;

            ui.PlayTweenEffect(UIMain.MenuContent.Inven);
        }

        private void PlayMailTween()
        {
            UIMainShortcut ui = UI.GetUI<UIMainShortcut>();

            if (ui == null)
                return;

            ui.PlayTweenEffect(UIMainShortcut.MenuContent.Mail);
        }
    }
}