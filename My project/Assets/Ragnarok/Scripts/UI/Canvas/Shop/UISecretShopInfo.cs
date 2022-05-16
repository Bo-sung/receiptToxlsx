using UnityEngine;

namespace Ragnarok
{
    public sealed class UISecretShopInfo : UIInfo<ShopPresenter, SecretShopInfo>
    {
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIRewardHelper itemProfile;
        [SerializeField] UIGridHelper rate;
        [SerializeField] UILabelHelper labelCount;
        [SerializeField] UIButtonWithIconHelper btnPurchase;
        [SerializeField] GameObject soldOutCover;
        [SerializeField] GameObject lockBase;
        [SerializeField] UILabelHelper labelChapter;

        [SerializeField] Animation anim;
        [SerializeField] GameObject item_FX;
        GameObject animObject;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnPurchase.OnClick, OnClickedBtnPurchase);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventDelegate.Remove(btnPurchase.OnClick, OnClickedBtnPurchase);
        }

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            // 슬롯 애니메이션은 기본적으로 꺼줌.
            PlaySlotAnimation(false);

            SetActive(true);

            labelName.Text = info.reward.ItemName;
            itemProfile.SetData(info.reward);
            btnPurchase.SetIconName(info.CoinType.IconName());
            btnPurchase.Text = info.Cost.ToString("N0");
            soldOutCover.SetActive(info.IsSoldOut);
            lockBase.SetActive(!presenter.IsOpenSecetShopItem(info.OpenChapter));
            labelChapter.Text = LocalizeKey._8032.ToText() // {NAME}\n도달 시 해금
                .Replace(ReplaceKey.NAME, BasisType.STAGE_TBLAE_LANGUAGE_ID.GetInt(info.OpenChapter).ToText());
        }

        public void PlaySlotAnimation(bool isActive = true)
        {
            if (animObject == null) animObject = anim.gameObject;

            if(isActive)
            {
                animObject.SetActive(false);
                item_FX.SetActive(false);

                anim.enabled = true;

                animObject.SetActive(true);
                item_FX.SetActive(true);
            }
            else
            {
                anim.enabled = false;
                item_FX.SetActive(false);
            }
        }

        /// <summary>
        /// 상품 구매
        /// </summary>
        void OnClickedBtnPurchase()
        {
            presenter.RequestSecretShopPurchase(info).WrapNetworkErrors();
        }
    }
}