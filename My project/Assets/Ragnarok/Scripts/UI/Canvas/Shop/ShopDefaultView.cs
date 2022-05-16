using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ShopDefaultView : UISubCanvas<ShopPresenter>
    {
        private ShopTabType tabType = ShopTabType.Package;

        [SerializeField] NPCStyle NPC;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UITabHelper tab;
        [SerializeField] GameObject normalList, packageList;
        [SerializeField] SuperScrollListWrapper normalWrapper, packageWrapper;
        [SerializeField] GameObject normalShopSlot, packageShopSlot;
        [SerializeField] UIButtonHelper btnSecret;

        ShopInfo[] arrayInfo;

        protected override void OnInit()
        {
            normalWrapper.SetRefreshCallback(OnItemNormalShopRefresh);
            normalWrapper.SpawnNewList(normalShopSlot, 0, 0);

            packageWrapper.SetRefreshCallback(OnItemPackageShopRefresh);
            packageWrapper.SpawnNewList(packageShopSlot, 0, 0);

            NPC.SetStyle();

            EventDelegate.Add(tab[0].OnChange, OnClickedTabPackage);
            EventDelegate.Add(tab[1].OnChange, OnClickedTabGoods);
            EventDelegate.Add(tab[2].OnChange, OnClickedTabBox);
            EventDelegate.Add(tab[3].OnChange, OnClickedTabEveryDay);
            EventDelegate.Add(tab[4].OnChange, OnClickedTabConsumable);
            EventDelegate.Add(btnSecret.OnClick, presenter.ShowShopSecret);

            labelNotice.SetActive(GameServerConfig.IsKoreaLanguage());
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(tab[0].OnChange, OnClickedTabPackage);
            EventDelegate.Remove(tab[1].OnChange, OnClickedTabGoods);
            EventDelegate.Remove(tab[2].OnChange, OnClickedTabBox);
            EventDelegate.Remove(tab[3].OnChange, OnClickedTabEveryDay);
            EventDelegate.Remove(tab[4].OnChange, OnClickedTabConsumable);
            EventDelegate.Remove(btnSecret.OnClick, presenter.ShowShopSecret);
        }

        protected override void OnShow()
        {
            Refresh();
        }

        protected override void OnHide()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            labelNotice.LocalKey = LocalizeKey._8000; // 미사용 상품은 7일 내 청약철회가 가능하며\n법정대리인 미동의 결제는 취소 가능합니다.
            tab[0].LocalKey = LocalizeKey._8024; // 유료
            tab[1].LocalKey = LocalizeKey._8025; // 제니
            tab[2].LocalKey = LocalizeKey._8033; // 코스튬
            tab[3].LocalKey = LocalizeKey._8026; // 아이템
            tab[4].LocalKey = LocalizeKey._8027; // 상자
            btnSecret.LocalKey = LocalizeKey._8030; // 비밀 상점
        }

        public void Set(ShopTabType type)
        {
            tab[(int)type].Value = true;
            Refresh();
        }

        public void Refresh()
        {
            arrayInfo = presenter.GetShopInfos((int)tabType);
            if (tabType == ShopTabType.Package)
            {
                System.Array.Sort(arrayInfo, SortByPackage);
            }
            else
            {
                System.Array.Sort(arrayInfo, SortByCustom);
            }

            if (tabType == ShopTabType.Package)
            {
                normalList.SetActive(false);
                packageList.SetActive(true);
                packageWrapper.Resize(arrayInfo.Length);
            }
            else
            {
                packageList.SetActive(false);
                normalList.SetActive(true);
                normalWrapper.Resize(arrayInfo.Length);
            }

            // 비밀상점 상품 변경까지 남은시간
            Timing.RunCoroutineSingleton(CheckFreeChangeSecretShop().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        IEnumerator<float> CheckFreeChangeSecretShop()
        {
            while (true)
            {
                btnSecret.SetNotice(presenter.IsSecretShopFree() && presenter.IsOpenSecretShop());
                yield return Timing.WaitForSeconds(1f);
            }
        }

        public void ShowTab(ShopTabType type)
        {
            if (UIToggle.current != null && !UIToggle.current.value)
                return;

            tabType = type;
            Refresh();
            NPC.PlayTalk();
            if (tabType == ShopTabType.Package)
            {
                packageWrapper.SetProgress(0);
            }
            else
            {
                normalWrapper.SetProgress(0);
            }
        }

        void OnClickedTabPackage()
        {
            ShowTab(ShopTabType.Package);
        }

        void OnClickedTabGoods()
        {
            ShowTab(ShopTabType.Goods);
        }

        void OnClickedTabBox()
        {
            ShowTab(ShopTabType.Box);
        }

        void OnClickedTabEveryDay()
        {
            ShowTab(ShopTabType.EveryDay);
        }

        void OnClickedTabConsumable()
        {
            ShowTab(ShopTabType.Consumable);
        }        

        private void OnItemNormalShopRefresh(GameObject go, int index)
        {
            UIShopInfoSlot ui = go.GetComponent<UIShopInfoSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        private void OnItemPackageShopRefresh(GameObject go, int index)
        {
            PackageShopInfoSlot ui = go.GetComponent<PackageShopInfoSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        private int SortByCustom(ShopInfo x, ShopInfo y)
        {
            int result = y.IsSortCanbuyLimit().CompareTo(x.IsSortCanbuyLimit()); // 매진된 상품 뒤로 보내기
            return result == 0 ? x.SortIndex.CompareTo(y.SortIndex) : result;
        }

        private int SortByPackage(ShopInfo x, ShopInfo y)
        {
            int result0 = presenter.HasPackageNotice(y.PackageType).CompareTo(presenter.HasPackageNotice(x.PackageType));
            int result1 = result0 == 0 ? y.IsSortCanbuyLimit().CompareTo(x.IsSortCanbuyLimit()) : result0; // 매진된 상품 뒤로 보내기
            int result2 = result1 == 0 ? x.SortIndex.CompareTo(y.SortIndex) : result1;
            return result2;
        }

        public void UpdatePackageTabNotice()
        {
            bool hasNotice = HasPackageNotice();
            tab[0].SetNotice(hasNotice);
        }

        private bool HasPackageNotice()
        {
            foreach (PackageType item in System.Enum.GetValues(typeof(PackageType)))
            {
                if (item == PackageType.None)
                    continue;

                if (presenter.GetHasPackageNotice(item))
                    return true;
            }

            return false;
        }

        public void UpdateDefaultTabNotice()
        {
            for (int i = 0; i < tab.Count; i++)
            {
                if (i < ShopModel.TAB_MIN_VALUE || i > ShopModel.TAB_MAX_VALUE)
                    continue;

                tab[i].SetNotice(presenter.GetHasNotice(i));
            }
        }
    }
}
