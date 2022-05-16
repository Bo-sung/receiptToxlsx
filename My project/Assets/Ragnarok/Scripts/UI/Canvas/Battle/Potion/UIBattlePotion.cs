using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattlePotion : UICanvas
    {
        public enum MenuContent
        {
            /// <summary>
            /// Mp 포션
            /// </summary>
            MpPotion = 1,

            /// <summary>
            /// 오토가드
            /// </summary>
            AutoGuard = 2,
        }

        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIGrid grid;
        [SerializeField] UIBattlePotionButton btnMpPotion, btnAutoGuard;

        public event System.Action<MenuContent> OnSelect;

        BattlePotionPresenter presenter;

        protected override void OnInit()
        {
            presenter = new BattlePotionPresenter();

            EventDelegate.Add(btnMpPotion.OnClick, OnClickedBtnMpPotion);
            EventDelegate.Add(btnAutoGuard.OnClick, OnClickedBtnAutoGuard);

            presenter.OnUpdateZeny += Refresh;
            presenter.OnUpdateCatCoin += Refresh;
            presenter.OnUseMpPotion += StartCooldownMpPotion;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnMpPotion.OnClick, OnClickedBtnMpPotion);
            EventDelegate.Remove(btnAutoGuard.OnClick, OnClickedBtnAutoGuard);

            presenter.OnUpdateZeny -= Refresh;
            presenter.OnUpdateCatCoin -= Refresh;
            presenter.OnUseMpPotion -= StartCooldownMpPotion;
        }

        protected override void OnShow(IUIData data = null)
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UIBattlePotionButton button = GetButton(item);
                if (button == null)
                    continue;

                button.SetCostCount(presenter.GetNeedCost(item));
                button.InitCooldownTime(presenter.GetCooldownTime(item));
            }

            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UIBattlePotionButton button = GetButton(item);
                if (button == null)
                    continue;

                button.Text = presenter.GetLocalKey(item);
            }
        }

        void OnClickedBtnMpPotion()
        {
            Select(MenuContent.MpPotion);
        }

        void OnClickedBtnAutoGuard()
        {
            if (!Select(MenuContent.AutoGuard))
                return;

            StartCooldownAutoGuard();
        }

        public void SetMode(params MenuContent[] args)
        {
            HideButtons(); // 모든 버튼 Hide

            // 특정 버튼 Show
            int size = args == null ? 0 : args.Length;
            for (int i = 0; i < size; i++)
            {
                MenuContent content = args[i];

                UIButtonHelper button = GetButton(content);
                if (button == null)
                    continue;

                button.SetActive(true);
                button.SetAsLastSibling(); // 가장 마지막 Child로 세팅
            }

            grid.Reposition();

            Refresh();
        }

        public void ResetCooldownTime()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UIBattlePotionButton button = GetButton(item);
                if (button == null)
                    continue;

                button.ResetCooldown();
            }
        }

        private void HideButtons()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UIButtonHelper button = GetButton(item);
                if (button == null)
                    continue;

                button.SetActive(false);
            }
        }

        private UIBattlePotionButton GetButton(MenuContent content)
        {
            switch (content)
            {
                case MenuContent.MpPotion:
                    return btnMpPotion;

                case MenuContent.AutoGuard:
                    return btnAutoGuard;
            }

            return null;
        }

        private void Refresh()
        {
            foreach (MenuContent item in System.Enum.GetValues(typeof(MenuContent)))
            {
                UIBattlePotionButton button = GetButton(item);
                if (button == null)
                    continue;

                bool isEnable = presenter.CanUse(item);
                button.IsEnabled = isEnable;
                button.SetCostColor(isEnable);
            }
        }

        private void StartCooldownMpPotion()
        {
            StartCooldown(MenuContent.MpPotion);
        }

        private void StartCooldownAutoGuard()
        {
            StartCooldown(MenuContent.AutoGuard);
        }

        private void StartCooldown(MenuContent content)
        {
            UIBattlePotionButton button = GetButton(content);
            if (button == null)
                return;

            button.StartCooldown();
        }

        private bool Select(MenuContent content)
        {
            UIBattlePotionButton button = GetButton(content);
            if (button == null)
                return false;

            // 쿨타임 진행중
            if (button.HasCooldownTime)
                return false;

            OnSelect?.Invoke(content);
            return true;
        }
    }
}