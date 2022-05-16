using Ragnarok.View.Main;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMain : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_Chatting;

        public enum MenuContent
        {
            Menu = 1,
            CharacterInfo,
            Inven,
            Make,
            Shop,
            Chat,
        }

        [SerializeField] UIMainMenuView mainMenuView;

        MainPresenter presenter;

        public UIWidget InvenWidget { get { return mainMenuView.InvenWidget; } }
        public UIWidget MenuWidget { get { return mainMenuView.MenuWidget; } }
        public UIWidget MakeBtn { get { return mainMenuView.MakeBtn; } }

        protected override void OnInit()
        {
            presenter = new MainPresenter();

            mainMenuView.OnSelect += OnSelectContent;
            mainMenuView.OnUnselect += OnUnselectMainMenu;

            presenter.OnUpdateGoodsZeny += UpdateMakeNotice;
            presenter.OnUpdateInvenItem += UpdateInvenNotice;
            presenter.OnUpdateInvenItem += UpdateMakeNotice;
            presenter.OnUpdateDungeonTicket += UpdateMenuNotice;
            presenter.OnUpdateHasNewSkillPoint += UpdateMenuNotice;
            presenter.OnPurchaseSuccess += UpdateShopNotice;
            presenter.OnPurchaseSuccess += UpdateMenuNotice;
            presenter.OnUpdateShopSecretFree += UpdateShopNotice;
            presenter.OnUpdateJobLevel += UpdateShopNotice;
            presenter.OnUpdateMileageReward += UpdateShopNotice;
            presenter.OnRewardPackageAchieve += UpdateShopNotice;
            presenter.OnUpdateShopMail += UpdateShopNotice;
            presenter.OnUpdateClearedStage += UpdateShopNotice;
            presenter.OnResetFreeItemBuyCount += UpdateShopNotice;
            presenter.OnResetFreeItemBuyCount += UpdateMenuNotice;
            presenter.OnUpdateEveryDayGoods += UpdateShopNotice;
            presenter.OnUpdateNewOpenContent += UpdateNewIcon;
            presenter.OnEquipmentAttackPowerTableUpdate += UpdateCharacterInfoNotice;
            presenter.OnUpdateCostume += UpdateCharacterInfoNotice;
            presenter.OnUpdateAlarm += UpdateCharacterInfoNotice;
            presenter.OnTamingMazeOpen += UpdateMenuNotice;
            presenter.OnUpdateAllChatNotice += OnUpdateAllChatNotice;
            presenter.OnUpdateEndlessTowerFreeTicket += UpdateMenuNotice;
            presenter.OnStandByReward += UpdateShopNotice;
            presenter.OnUpdatePassExp += UpdateShopNotice;
            presenter.OnUpdatePassReward += UpdateShopNotice;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            mainMenuView.OnSelect -= OnSelectContent;
            mainMenuView.OnUnselect -= OnUnselectMainMenu;

            presenter.OnUpdateGoodsZeny -= UpdateMakeNotice;
            presenter.OnUpdateInvenItem -= UpdateInvenNotice;
            presenter.OnUpdateInvenItem -= UpdateMakeNotice;
            presenter.OnUpdateDungeonTicket -= UpdateMenuNotice;
            presenter.OnUpdateHasNewSkillPoint -= UpdateMenuNotice;
            presenter.OnPurchaseSuccess -= UpdateShopNotice;
            presenter.OnPurchaseSuccess -= UpdateMenuNotice;
            presenter.OnUpdateShopSecretFree -= UpdateShopNotice;
            presenter.OnUpdateJobLevel -= UpdateShopNotice;
            presenter.OnUpdateMileageReward -= UpdateShopNotice;
            presenter.OnRewardPackageAchieve -= UpdateShopNotice;
            presenter.OnUpdateShopMail -= UpdateShopNotice;
            presenter.OnUpdateClearedStage -= UpdateShopNotice;
            presenter.OnResetFreeItemBuyCount -= UpdateShopNotice;
            presenter.OnResetFreeItemBuyCount -= UpdateMenuNotice;
            presenter.OnUpdateEveryDayGoods -= UpdateShopNotice;
            presenter.OnUpdateNewOpenContent -= UpdateNewIcon;
            presenter.OnEquipmentAttackPowerTableUpdate -= UpdateCharacterInfoNotice;
            presenter.OnUpdateCostume -= UpdateCharacterInfoNotice;
            presenter.OnUpdateAlarm -= UpdateCharacterInfoNotice;
            presenter.OnTamingMazeOpen -= UpdateMenuNotice;
            presenter.OnUpdateAllChatNotice -= OnUpdateAllChatNotice;
            presenter.OnUpdateEndlessTowerFreeTicket -= UpdateMenuNotice;
            presenter.OnStandByReward -= UpdateShopNotice;
            presenter.OnUpdatePassExp -= UpdateShopNotice;
            presenter.OnUpdatePassReward -= UpdateShopNotice;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RequestItemShopLimitList(); // 상점 빨콩을 위해 필요

            UpdateNotice();
            UpdateNewIcon();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnSelectContent(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Menu:
                    UI.ShortCut<UIHome>();
                    break;

                case MenuContent.CharacterInfo:
                    UI.ShortCut<UICharacterInfo>();
                    break;

                case MenuContent.Inven:
                    UI.ShortCut<UIInven>();
                    break;

                case MenuContent.Make:
                    UI.ShortCut<UIMake>();
                    break;

                case MenuContent.Shop:
                    UI.ShortCut<UIShop>().Set(UIShop.ViewType.Default);
                    break;

                case MenuContent.Chat:
                    UI.ShortCut<UIChat>();
                    break;
            }
        }

        void OnUnselectMainMenu(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.Menu:
                    UI.Close<UIHome>();
                    break;

                case MenuContent.CharacterInfo:
                    UI.Close<UICharacterInfo>();
                    break;

                case MenuContent.Inven:
                    UI.Close<UIInven>();
                    break;

                case MenuContent.Make:
                    UI.Close<UIMake>();
                    break;

                case MenuContent.Shop:
                    UI.Close<UIShop>();
                    break;

                case MenuContent.Chat:
                    UI.Close<UIChat>();
                    break;
            }
        }

        private void UpdateMenuNotice()
        {
            UpdateNotice(MenuContent.Menu);
        }

        private void UpdateInvenNotice()
        {
            UpdateNotice(MenuContent.Inven);
        }

        private void UpdateMakeNotice()
        {
            UpdateNotice(MenuContent.Make);
        }

        private void UpdateShopNotice()
        {
            UpdateNotice(MenuContent.Shop);
        }

        private void UpdateCharacterInfoNotice()
        {
            UpdateNotice(MenuContent.CharacterInfo);
        }

        private void OnUpdateAllChatNotice(bool _)
        {
            UpdateNotice(MenuContent.Chat);
        }

        private void UpdateNotice()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UpdateNotice(item);
            }
        }

        private void UpdateNotice(MenuContent content)
        {
            mainMenuView.SetNotice(content, presenter.GetHasNotice(content));
        }

        private void UpdateNewIcon()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                mainMenuView.SetNewIcon(item, presenter.GetHasNewIcon(item));
            }
        }

        public Vector3 GetPosition(MenuContent content)
        {
            return mainMenuView.GetPosition(content);
        }

        public void PlayTweenEffect(MenuContent content)
        {
            mainMenuView.PlayTweenEffect(content);
        }

        public UISprite GetMenuIcon(MenuContent content)
        {
            return mainMenuView.GetMenuIcon(content);
        }
    }
}