using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBuffInfo : UICanvas, BuffInfoPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide | UIType.Single | UIType.Reactivation;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] SuperScrollTableWrapper wrapper;
        [SerializeField] GameObject prefab, empty;
        [SerializeField] UILabelHelper labelEmpty;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UIButtonHelper btnInven;
        [SerializeField] UILabelHelper labelItemTitle;
        [SerializeField] BuffInfoListView buffInfoListView;

        BuffInfoPresenter presenter;
        UIApplyBuffContent.IBuffInfo[] infos;

        protected override void OnInit()
        {
            presenter = new BuffInfoPresenter(this);

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            presenter.AddEvent();

            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
            EventDelegate.Add(btnInven.OnClick, OnClickedBtnInven);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
            EventDelegate.Remove(btnInven.OnClick, OnClickedBtnInven);

            if (presenter != null)
                presenter = null;
        }

        protected override void OnShow(IUIData data = null)
        {
            presenter.RemoveNewOpenContent_Buff(); // 신규 컨텐츠 플래그 제거
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._65000; // 버프 정보
            labelEmpty.LocalKey = LocalizeKey._65001; // 현재 적용중인 버프 정보가 없습니다.
            btnConfirm.LocalKey = LocalizeKey._65007; // 확 인
            btnInven.LocalKey = LocalizeKey._65008; // 상 점
            labelItemTitle.LocalKey = LocalizeKey._65006; // 보유 소비 아이템
        }

        void OnItemRefresh(GameObject go, int dataIndex)
        {
            UIApplyBuffContent ui = go.GetComponent<UIApplyBuffContent>();
            ui.SetData(infos[dataIndex]);
        }

        public void Refresh()
        {
            infos = presenter.GetBuffInfos();

            int length = infos == null ? 0 : infos.Length;
            wrapper.Resize(length, 0);
            empty.SetActive(length == 0);

            buffInfoListView.Set(presenter.GetConsumableSlotInfos());
        }

        void OnClickedBtnInven()
        {
            UI.Show<UIShop>().Set(UIShop.ViewType.Default, ShopTabType.Consumable);
        }

        private void CloseUI()
        {
            UI.Close<UIBuffInfo>();
        }
    }
}