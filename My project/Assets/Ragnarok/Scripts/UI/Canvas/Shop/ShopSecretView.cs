using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ShopSecretView : UISubCanvas<ShopPresenter>
    {
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelValue freeTime;
        [SerializeField] UIButtonWithIconValue btnCatCoin; // 리셋권 구매
        [SerializeField] UIButtonHelper btnChange; // 리셋권 사용
        [SerializeField] NPCStyle NPC;
        [SerializeField] UILabelHelper labelNotice;
        [SerializeField] UIButtonHelper btnShop;
        [SerializeField] UITextureHelper iconResetItem;
        [SerializeField] UILabelHelper labelResetItemCount;
        [SerializeField] UILabelHelper labelDetailRate;
        [SerializeField] UIButton btnDetailRate;

        SecretShopInfo[] arrayInfo;
        protected override void OnInit()
        {
            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            presenter.PlaySlotAnim += PlaySlotAnim;

            EventDelegate.Add(btnCatCoin.OnClick, OnClickedBtnCatCoin);
            EventDelegate.Add(btnChange.OnClick, presenter.SecetShopInitUseResetItem);
            EventDelegate.Add(btnShop.OnClick, presenter.ShowShopDefault);
            EventDelegate.Add(btnDetailRate.onClick, OnClickedBtnDetailRate);
            NPC.SetStyle();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnCatCoin.OnClick, OnClickedBtnCatCoin);
            EventDelegate.Remove(btnChange.OnClick, presenter.SecetShopInitUseResetItem);
            EventDelegate.Remove(btnShop.OnClick, presenter.ShowShopDefault);
            EventDelegate.Remove(btnDetailRate.onClick, OnClickedBtnDetailRate);

            presenter.PlaySlotAnim -= PlaySlotAnim;
        }

        protected override void OnShow()
        {
            Refresh();
            labelDetailRate.SetActive(GameServerConfig.IsKorea()); // 한국일 경우에만 노출
        }

        protected override void OnHide()
        {
            Timing.KillCoroutines(gameObject);
        }

        protected override void OnLocalize()
        {
            freeTime.TitleKey = LocalizeKey._8020; // 무료 변경까지 남은 시간
            btnCatCoin.LocalKey = LocalizeKey._8050; // 구매
            btnChange.LocalKey = LocalizeKey._8021; // 변경
            btnCatCoin.SetIconName(CoinType.CatCoin.IconName());
            btnCatCoin.SetLabelValue(presenter.SecretShopInitCatCoin.ToString("N0"));
            labelNotice.LocalKey = LocalizeKey._8023; // 냥다래로 장비를 구매할 수 있습니다.
            btnShop.LocalKey = LocalizeKey._8031; // 일반 상점
            labelDetailRate.LocalKey = LocalizeKey._90301; // 상세 확률
        }

        void Refresh()
        {
            NPC.PlayTalk();
            arrayInfo = presenter.GetSecretShopInfos();
            wrapper.Resize(arrayInfo.Length);

            btnCatCoin.SetNotice(presenter.IsSecretShopFree());

            int resetItemCount = presenter.GetSecretShopResetItemCount();
            btnCatCoin.SetActive(resetItemCount == 0);
            btnChange.SetActive(resetItemCount != 0);
            iconResetItem.Set(presenter.GetSecretShopResetItemIconName());
            labelResetItemCount.Text = resetItemCount > 99 ? "99+" : resetItemCount.ToString();

            // 비밀상점 상품 변경까지 남은시간
            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(CheckFreeChangeSecretShop(), gameObject);
        }

        public void PlaySlotAnim()
        {
            foreach (var info in wrapper.GetComponentsInChildren<UISecretShopInfo>())
            {
                info.PlaySlotAnimation();
            }
        }

        void OnClickedBtnCatCoin()
        {
            presenter.ShowPurchaseSecretShopResetItem();
        }

        void OnClickedBtnDetailRate()
        {
            BasisUrl.SecretShopRate.OpenUrl();
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UISecretShopInfo ui = go.GetComponent<UISecretShopInfo>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        IEnumerator<float> CheckFreeChangeSecretShop()
        {
            while (true)
            {
                float remainTime = presenter.GetRemainFreeChangeSecretShopTime();

                if (remainTime <= 0)
                    break;

                freeTime.Value = remainTime.ToStringTime();
                yield return Timing.WaitForSeconds(1f);
            }
            freeTime.Value = "00:00:00";
            btnCatCoin.SetNotice(presenter.IsSecretShopFree());
            presenter.SecretShopInit();
        }
    }
}