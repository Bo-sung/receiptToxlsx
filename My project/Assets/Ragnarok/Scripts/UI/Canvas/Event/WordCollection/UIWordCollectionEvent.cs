using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIWordCollectionEvent : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] PopupView popupView;
        [SerializeField] UIBannerElement banner;
        [SerializeField] UIEventMaterialRewardHelper[] materialElements;
        [SerializeField] GameObject goClear;
        [SerializeField] UILabelHelper labelCompleteTitle;
        [SerializeField] UILabelHelper labelComplete;
        [SerializeField] UIDiceCompleteElement[] completeElements;
        [SerializeField] UILabelHelper labelNotice;

        WordCollectionEventPresenter presenter;

        protected override void OnInit()
        {
            presenter = new WordCollectionEventPresenter();

            popupView.OnExit += OnBack;
            popupView.OnConfirm += OnBack;

            foreach (var item in completeElements)
            {
                item.OnSelectReceive += presenter.RequestWordCollectionReward;
            }
            presenter.OnUpdateWordCollectionItemCount += UpdateView;

            presenter.AddEvent();

            Initialize();
        }

        protected override void OnClose()
        {
            popupView.OnExit -= OnBack;
            popupView.OnConfirm -= OnBack;

            foreach (var item in completeElements)
            {
                item.OnSelectReceive -= presenter.RequestWordCollectionReward;
            }
            presenter.OnUpdateWordCollectionItemCount -= UpdateView;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
            //if (!presenter.HasEventData())
            //{
            //    UI.Close<UIWordCollectionEvent>();
            //    return;
            //}
            UpdateView();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            popupView.MainTitleLocalKey = LocalizeKey._7400; // 글자 완성
            popupView.ConfirmLocalKey = LocalizeKey._1; // 확인
            labelCompleteTitle.LocalKey = LocalizeKey._7401; // 완성보상
            labelNotice.LocalKey = LocalizeKey._7403; // 라비린스 카드는 직업 사냥을 통해 획득할 수 있습니다.
        }

        private void Initialize()
        {
            // 배너 초기화
            banner.SetData(presenter.GetData());

            // 재료 초기화
            RewardData[] materialData = presenter.GetMaterials();
            for (int i = 0, dataMax = materialData == null ? 0 : materialData.Length; i < materialElements.Length; i++)
            {
                materialElements[i].SetData(i < dataMax ? materialData[i] : null);
            }

            // 보상 초기화
            UIDiceCompleteElement.IInput[] inputs = presenter.GetRewards();
            for (int i = 0, dataMax = inputs == null ? 0 : inputs.Length; i < completeElements.Length; i++)
            {
                completeElements[i].Initialize(i);
                completeElements[i].SetData(i < dataMax ? inputs[i] : null);
            }
        }

        private void UpdateView()
        {
            goClear.SetActive(presenter.IsAllCompleteReward());

            int completeCount = presenter.GetCompleteCount();
            int completeRewardStep = presenter.GetCompleteRewardStep();
            foreach (var item in completeElements)
            {
                item.SetRewardStep(completeCount, completeRewardStep);
            }

            labelComplete.Text = LocalizeKey._7402.ToText().Replace(ReplaceKey.COUNT, completeRewardStep); // 완성한 횟수 : {COUNT}
        }        

        public override bool Find()
        {
            UIDiceCompleteElement[] elements = GetComponentsInChildren<UIDiceCompleteElement>();
            completeElements = new UIDiceCompleteElement[elements.Length];
            for (int i = 0, index = elements.Length - 1; i < completeElements.Length; i++, index--)  // 역순으로 넣기
            {
                completeElements[i] = elements[index];
            }

            return base.Find();
        }
    }
}