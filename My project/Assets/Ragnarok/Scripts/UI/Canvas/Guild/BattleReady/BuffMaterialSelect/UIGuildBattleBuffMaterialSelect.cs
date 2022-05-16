using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIGuildBattleBuffMaterialSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] GuildBattleBuffMaterialView materialView;

        GuildBattleBuffMaterialSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new GuildBattleBuffMaterialSelectPresenter();

            selectPopupView.OnExit += CloseUI;
            selectPopupView.OnCancel += CloseUI;
            selectPopupView.OnConfirm += presenter.RequestSelectMatereial;

            presenter.OnUpdateMaterialSelect += RefreshProgress;
            presenter.OnFinished += CloseUI;
            presenter.OnUpdateGuildBattleBuff += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateMaterialSelect -= RefreshProgress;
            presenter.OnFinished -= CloseUI;
            presenter.OnUpdateGuildBattleBuff -= CloseUI;

            selectPopupView.OnExit -= CloseUI;
            selectPopupView.OnCancel -= CloseUI;
            selectPopupView.OnConfirm -= presenter.RequestSelectMatereial;
        }

        protected override void OnShow(IUIData data = null)
        {
            materialView.SetData(presenter.GetArrayData());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            selectPopupView.MainTitleLocalKey = LocalizeKey._33900; // 재료 선택
            selectPopupView.ConfirmLocalKey = LocalizeKey._33901; // 기부하기
            selectPopupView.CancelLocalKey = LocalizeKey._2; // 취소
        }

        public void SetData(int skillId)
        {
            presenter.SelectSkill(skillId);
            materialView.SetSkill(presenter.GetCurrentSkill(), presenter.GetCurPoint(), presenter.needLevelUpExp, presenter.maxLevel);
        }

        private void RefreshProgress()
        {
            materialView.UpdatePoint(presenter.GetCurPoint());
        }

        private void CloseUI()
        {
            UI.Close<UIGuildBattleBuffMaterialSelect>();
        }
    }
}