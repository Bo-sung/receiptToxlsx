using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIDarkTreeMaterialSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] SelectPopupView selectPopupView;
        [SerializeField] DarkTreeMaterialView darkTreeMaterialView;

        DarkTreeMaterialSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new DarkTreeMaterialSelectPresenter();

            selectPopupView.OnExit += CloseUI;
            selectPopupView.OnCancel += CloseUI;
            selectPopupView.OnConfirm += presenter.RequestSelectMatereial;

            presenter.OnUpdateMaterialSelect += RefreshProgress;
            presenter.OnFinished += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateMaterialSelect -= RefreshProgress;
            presenter.OnFinished -= CloseUI;

            selectPopupView.OnExit -= CloseUI;
            selectPopupView.OnCancel -= CloseUI;
            selectPopupView.OnConfirm -= presenter.RequestSelectMatereial;
        }

        protected override void OnShow(IUIData data = null)
        {
            darkTreeMaterialView.SetReward(presenter.GetCurrentReward(), presenter.GetCurPoint(), presenter.GetMaxPoint());
            darkTreeMaterialView.SetData(presenter.GetArrayData());
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            selectPopupView.MainTitleLocalKey = LocalizeKey._9034; // 재료 선택
            selectPopupView.ConfirmLocalKey = LocalizeKey._9035; // 재료 넣기
            selectPopupView.CancelLocalKey = LocalizeKey._2; // 취소
        }

        private void RefreshProgress()
        {
            darkTreeMaterialView.UpdatePoint(presenter.GetCurPoint());
        }

        private void CloseUI()
        {
            UI.Close<UIDarkTreeMaterialSelect>();
        }
    }
}