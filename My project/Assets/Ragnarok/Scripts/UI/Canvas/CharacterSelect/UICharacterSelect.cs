using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICharacterSelect : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single | UIType.Reactivation;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] CharacterSelectView characterSelectView;
        [SerializeField] UILabelHelper labelServerName;
        [SerializeField] CharacterSelectListView characterSelectListView;
        [SerializeField] UIButtonHelper btnCancelDelete, btnRequestDelete, btnDelete, btnEnter;

        CharacterSelectPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CharacterSelectPresenter();

            characterSelectListView.OnSelect += OnSelectCharacter;
            characterSelectListView.OnCreate += presenter.OnShowCharacterCreate;
            characterSelectListView.OnFinishRemainTime += UpdateView;

            presenter.OnUpdateCharacter += OnUpdateCharacter;
            presenter.OnUpdateView += UpdateView;

            presenter.AddEvent();

            EventDelegate.Add(btnCancelDelete.OnClick, presenter.RequestDeleteCharacterCancel);
            EventDelegate.Add(btnRequestDelete.OnClick, presenter.RequestDeleteCharacterWaiting);
            EventDelegate.Add(btnDelete.OnClick, presenter.RequestDeleteCharacterComplete);
            EventDelegate.Add(btnEnter.OnClick, presenter.RequestJoinGame);
        }

        protected override void OnClose()
        {
            presenter.Dispose();
            presenter.RemoveEvent();

            characterSelectListView.OnSelect -= OnSelectCharacter;
            characterSelectListView.OnCreate -= presenter.OnShowCharacterCreate;
            characterSelectListView.OnFinishRemainTime -= UpdateView;

            presenter.OnUpdateCharacter -= OnUpdateCharacter;
            presenter.OnUpdateView -= UpdateView;

            EventDelegate.Remove(btnCancelDelete.OnClick, presenter.RequestDeleteCharacterCancel);
            EventDelegate.Remove(btnRequestDelete.OnClick, presenter.RequestDeleteCharacterWaiting);
            EventDelegate.Remove(btnDelete.OnClick, presenter.RequestDeleteCharacterComplete);
            EventDelegate.Remove(btnEnter.OnClick, presenter.RequestJoinGame);
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RequestCharacterList();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._1005; // 캐릭터 선택
            labelServerName.LocalKey = presenter.GetCurrentServerNameKey();
            btnCancelDelete.LocalKey = LocalizeKey._1002; // 삭제 취소
            btnRequestDelete.LocalKey = LocalizeKey._1001; // 삭 제
            btnDelete.LocalKey = LocalizeKey._1001; // 삭 제
            btnEnter.LocalKey = LocalizeKey._1000; // 모험의 시작
        }

        private void OnUpdateCharacter()
        {
            UpdateView();
            characterSelectListView.MoveTo(presenter.GetSelectedIndex());
        }

        private void UpdateView()
        {
            UpdateChacterSelectView();
            UpdateCharacerSelectListView();
        }

        private void UpdateChacterSelectView()
        {
            characterSelectView.SetData(presenter.GetDummyUIPlayer());
        }

        private void UpdateCharacerSelectListView()
        {
            characterSelectListView.SetData(presenter.GetArrayData());
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (presenter.IsDeleteWaiting())
            {
                btnCancelDelete.SetActive(true);
                btnRequestDelete.SetActive(false);

                if (presenter.GetRemainTimeDeleteWaiting() > 0)
                {
                    // 삭제 대기중
                    btnDelete.SetActive(false);
                    btnEnter.SetActive(true);
                    btnEnter.IsEnabled = false;
                }
                else
                {
                    // 삭제 가능
                    btnDelete.SetActive(true);
                    btnEnter.SetActive(false);
                }
            }
            else
            {
                // 현재 플레이중인 캐릭터에 대한 처리
                bool isCurrentPlayingCharacter = presenter.IsCurrentPlayingCharacter();
                btnCancelDelete.SetActive(false);
                btnRequestDelete.SetActive(true);
                btnRequestDelete.IsEnabled = !isCurrentPlayingCharacter;
                btnDelete.SetActive(false);
                btnEnter.SetActive(true);
                btnEnter.IsEnabled = !isCurrentPlayingCharacter;
            }
        }

        private void OnSelectCharacter(int cid)
        {
            presenter.SetSelectCharacter(cid);
            UpdateView();
        }
    }
}