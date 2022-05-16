using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIAdventureGuide : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] AdventureGuideView adventureGuideView;

        AdventureGuidePresenter presenter;

        protected override void OnInit()
        {
            presenter = new AdventureGuidePresenter();

            popupView.OnConfirm += OnBack;
            popupView.OnExit += OnBack;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            popupView.OnConfirm -= OnBack;
            popupView.OnExit -= OnBack;
        }

        protected override void OnShow(IUIData data = null)
        {
            adventureGuideView.SetData(presenter.scoreRankData, presenter.killRankData);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._48268; // 가이드
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
        }
    }
}