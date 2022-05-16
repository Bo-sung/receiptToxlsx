using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICharacterShare2nd : UICanvas, TutorialShareVice2ndOpen.IOpenCharacterShare2ndImpl
    {
        private const int TAB_SHARE_FORCE = 0;
        private const int TAB_FORCE_LEVEL_UP = 1;

        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton btnMainExit;
        [SerializeField] UIButtonHelper btnToggle;
        [SerializeField] UITabHelper mainTab;
        [SerializeField] CharacterShare2nd characterShare2nd;
        [SerializeField] ShareForceUpgradeView shareForceUpgradeView;

        [Header("Bottom")]
        [SerializeField] UILabelValue labelStatPoint;

        CharacterShare2ndPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CharacterShare2ndPresenter();

            EventDelegate.Add(btnMainExit.onClick, HideUI);
            EventDelegate.Add(btnToggle.OnClick, SwitchSharevice);
            mainTab.OnSelect += OnSelectMainTab;
            characterShare2nd.OnLockErrorMessage += OnLockErrorMessage;
            shareForceUpgradeView.OnReset += presenter.RequestResetStatus;

            presenter.OnUpdateShareForce += Refresh;
            presenter.OnUpdateShareFreeTicket += UpdateNotice;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            
            EventDelegate.Remove(btnMainExit.onClick, HideUI);
            EventDelegate.Remove(btnToggle.OnClick, SwitchSharevice);
            mainTab.OnSelect -= OnSelectMainTab;
            characterShare2nd.OnLockErrorMessage -= OnLockErrorMessage;
            shareForceUpgradeView.OnReset -= presenter.RequestResetStatus;

            presenter.OnUpdateShareForce -= Refresh;
            presenter.OnUpdateShareFreeTicket -= UpdateNotice;
        }

        protected override void OnShow(IUIData data = null)
        {
            mainTab.Value = TAB_SHARE_FORCE;

            UpdateNotice();
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnToggle.LocalKey = LocalizeKey._10267; // 1세대
            mainTab[TAB_SHARE_FORCE].LocalKey = LocalizeKey._48249; // 쉐어 포스
            mainTab[TAB_FORCE_LEVEL_UP].LocalKey = LocalizeKey._48250; // 포스 강화
            labelStatPoint.TitleKey = LocalizeKey._10269; // 포스 강화 스탯 포인트
        }

        private void HideUI()
        {
            UI.Close<UICharacterShare2nd>();
        }

        void SwitchSharevice()
        {
            UI.Show<UICharacterShare>();
            HideUI();
        }

        void OnSelectMainTab(int index)
        {
            characterShare2nd.SetActive(index == TAB_SHARE_FORCE);
            shareForceUpgradeView.SetActive(index == TAB_FORCE_LEVEL_UP);
        }

        void OnLockErrorMessage(ShareForceType type)
        {
            QuestData questData = presenter.GetQuestData(type);
            if (questData == null)
                return;

            string description = LocalizeKey._48255.ToText() // 타임패트롤 퀘스트 [{NUMBER}.{NAME}] 클리어 해야합니다.
                .Replace(ReplaceKey.NUMBER, questData.daily_group)
                .Replace(ReplaceKey.NAME, questData.name_id.ToText());

            UI.ShowToastPopup(description);
        }

        private void Refresh()
        {
            mainTab[TAB_FORCE_LEVEL_UP].SetNotice(presenter.HasNoticeShareForceStat());

            characterShare2nd.SetData(presenter.GetSlots());
            shareForceUpgradeView.SetData(presenter.GetStatus());
            shareForceUpgradeView.SetCanReset(presenter.CanReset());
            labelStatPoint.Value = presenter.GetStatPoint().ToString("N0");
        }

        private void UpdateNotice()
        {
            btnToggle.SetNotice(presenter.HasCharacterShareNotice());
        }

        #region 튜토리얼

        [SerializeField] UIWidget shareForceWidget;
        [SerializeField] UIWidget shareForceSlotWidget;

        UIWidget TutorialShareVice2ndOpen.IOpenCharacterShare2ndImpl.GetShareForceWidget()
        {
            return shareForceWidget;
        }

        UIWidget TutorialShareVice2ndOpen.IOpenCharacterShare2ndImpl.GetShareForceSlotWidget()
        {
            return shareForceSlotWidget;
        }

        #endregion
    }
}