using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UINabiho : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single | UIType.Reactivation;

        [SerializeField] TitleView titleView;
        [SerializeField] NabihoMainView nabihoMainView;
        [SerializeField] NabihoGiftSelectView nabihoGiftSelectView;
        [SerializeField] NabihoRewardSelectView nabihoRewardSelectView;

        NabihoPresenter presenter;

        protected override void OnInit()
        {
            presenter = new NabihoPresenter();

            nabihoMainView.OnSelectGift += OnSelectGift;
            nabihoMainView.OnSelectEquipment += ShowSelectEquipment;
            nabihoMainView.OnSelectBox += ShowSelectBox;
            nabihoMainView.OnSelectSpecial += ShowSelectSpecial;
            nabihoMainView.OnTryShowAd += presenter.ShowAd;
            nabihoMainView.OnTryCancel += presenter.RequestCancel;
            nabihoMainView.OnTryReward += presenter.RequestReward;
            nabihoGiftSelectView.OnSelect += presenter.RequestSendPresent;
            nabihoRewardSelectView.OnSelect += presenter.RequestItemSelect;

            presenter.OnUpdateZeny += titleView.ShowZeny;
            presenter.OnUpdateCatCoin += titleView.ShowCatCoin;
            presenter.OnRefresh += Refresh;
            presenter.OnUpdateNabihoItem += RefreshMaterial;

            titleView.Initialize(TitleView.FirstCoinType.Zeny, TitleView.SecondCoinType.CatCoin);

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateZeny -= titleView.ShowZeny;
            presenter.OnUpdateCatCoin -= titleView.ShowCatCoin;
            presenter.OnRefresh -= Refresh;
            presenter.OnUpdateNabihoItem -= RefreshMaterial;

            nabihoMainView.OnSelectGift -= OnSelectGift;
            nabihoMainView.OnSelectEquipment -= ShowSelectEquipment;
            nabihoMainView.OnSelectBox -= ShowSelectBox;
            nabihoMainView.OnSelectSpecial -= ShowSelectSpecial;
            nabihoMainView.OnTryShowAd -= presenter.ShowAd;
            nabihoMainView.OnTryCancel -= presenter.RequestCancel;
            nabihoMainView.OnTryReward -= presenter.RequestReward;
            nabihoGiftSelectView.OnSelect -= presenter.RequestSendPresent;
            nabihoRewardSelectView.OnSelect -= presenter.RequestItemSelect;
        }

        protected override void OnShow(IUIData data = null)
        {
            nabihoMainView.Initialize(presenter.GetEquipmentNeedLevel(), presenter.GetBoxNeedLevel(), presenter.GetSpecialNeedLevel());
            nabihoGiftSelectView.Initialize(presenter.GetMaterialNameId());

            nabihoGiftSelectView.Hide();
            nabihoRewardSelectView.Hide();

            Refresh();
            RefreshMaterial();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleView.ShowTitle(LocalizeKey._10900.ToText()); // 나비호
        }

        void OnSelectGift()
        {
            if (presenter.IsMaxLevel())
            {
                string message = LocalizeKey._10924.ToText(); // 최대 친밀도에 도달하였습니다.
                UI.ShowToastPopup(message);
                return;
            }

            nabihoGiftSelectView.Show();
        }

        void ShowSelectEquipment()
        {
            nabihoRewardSelectView.Show(presenter.GetSelectEquipmentInfos());
        }

        void ShowSelectBox()
        {
            nabihoRewardSelectView.Show(presenter.GetSelectBoxInfos());
        }

        void ShowSelectSpecial()
        {
            nabihoRewardSelectView.Show(presenter.GetSelectSpecialInfos());
        }

        private void Refresh()
        {
            // Refresh Level
            int level = presenter.GetCurrentLevel();
            int curExp = presenter.GetCurrentExp();
            int maxExp = presenter.GetMaxExp();
            int reduceMinutes = presenter.GetReduceMinutes();
            nabihoMainView.UpdateLevel(level, curExp, maxExp, reduceMinutes);
            nabihoGiftSelectView.UpdateLevel(level, curExp, maxExp);
            nabihoRewardSelectView.UpdateLevel(level);

            // Refresh Info
            UINabihoSelectBar.IInput equipmentInfo = presenter.GetEquipmentInfo();
            UINabihoSelectBar.IInput boxInfo = presenter.GetBoxInfo();
            UINabihoSelectBar.IInput specialInfo = presenter.GetSpecialInfo();
            nabihoMainView.UpdateEquipment(equipmentInfo);
            nabihoMainView.UpdateBox(boxInfo);
            nabihoMainView.UpdateSpecial(specialInfo);
        }

        private void RefreshMaterial()
        {
            nabihoGiftSelectView.UpdateMaterial(presenter.GetMaterial());
        }

        protected override void OnBack()
        {
            if (nabihoGiftSelectView.IsShow)
            {
                nabihoGiftSelectView.Hide();
                return;
            }

            if (nabihoRewardSelectView.IsShow)
            {
                nabihoRewardSelectView.Hide();
                return;
            }

            base.OnBack();
        }
    }
}