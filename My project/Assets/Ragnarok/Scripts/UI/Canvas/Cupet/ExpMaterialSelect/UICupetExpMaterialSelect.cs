using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICupetExpMaterialSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] CupetExpMaterialView materialView;

        CupetExpMaterialSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CupetExpMaterialSelectPresenter();

            selectPopupView.OnExit += CloseUI;
            selectPopupView.OnCancel += CloseUI;
            selectPopupView.OnConfirm += presenter.RequestSelectMatereial;

            presenter.OnUpdateMaterialSelect += RefreshProgress;
            presenter.OnFinished += CloseUI;
            presenter.OnUpdateCupetList += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateMaterialSelect -= RefreshProgress;
            presenter.OnFinished -= CloseUI;
            presenter.OnUpdateCupetList -= CloseUI;

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
            selectPopupView.MainTitleLocalKey = LocalizeKey._9100; // 재료 선택
            selectPopupView.ConfirmLocalKey = LocalizeKey._9101; // 적용하기
            selectPopupView.CancelLocalKey = LocalizeKey._2; // 취소
        }

        public void SetData(int cupetId)
        {
            presenter.SelectCupet(cupetId);
            materialView.SetCupet(presenter.GetCurrentCupet(), presenter.GetCurPoint(), presenter.GetMaxPoints());
        }

        private void RefreshProgress()
        {
            materialView.UpdatePoint(presenter.GetCurPoint());
        }

        private void CloseUI()
        {
            UI.Close<UICupetExpMaterialSelect>();
        }
    }
}