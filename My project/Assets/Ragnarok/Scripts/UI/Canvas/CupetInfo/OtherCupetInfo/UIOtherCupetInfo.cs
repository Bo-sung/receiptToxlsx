using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIOtherCupetInfo : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] OtherCupetWindow otherCupetWindow;

        protected override void OnInit()
        {
            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;
        }

        protected override void OnClose()
        {
            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._19000; // 큐펫 정보
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
        }

        public void SetEntity(int cupetId, int rank, int level)
        {
            SetEntity(CupetEntity.Factory.CreateDummyCupet(cupetId, rank, level));
        }

        public void SetEntity(CupetEntity entity)
        {
            otherCupetWindow.Initialize(entity);
        }
    }
}