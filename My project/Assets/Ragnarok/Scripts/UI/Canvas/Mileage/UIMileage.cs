using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIMileage : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButton background;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labTitle;
        [SerializeField] UILabelHelper LabelRewardDesc;
        [SerializeField] UILabelHelper labalAccrueMileage;
        [SerializeField] UILabelHelper labelMileagePoint;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UILabelHelper labelCoolingOff;

        MileagePresenter presenter;

        private MileageSlot.IInput[] arrData;

        protected override void OnInit()
        {
            presenter = new MileagePresenter();

            presenter.OnUpdateMileageReward += Refresh;

            presenter.AddEvent();

            EventDelegate.Add(background.onClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);
        }

        protected override void OnClose()
        {
            presenter.OnUpdateMileageReward -= Refresh;

            presenter.RemoveEvent();

            EventDelegate.Remove(background.onClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labTitle.LocalKey = LocalizeKey._4800; // 누적 결제 보상
            LabelRewardDesc.LocalKey = LocalizeKey._4804; // 누적된 상품 결제 금액에 따라 추가보상 받아가세요!
            labalAccrueMileage.LocalKey = LocalizeKey._4805; // 누적 마일리지
            btnConfirm.LocalKey = LocalizeKey._4806; // 확인
            labelCoolingOff.LocalKey = LocalizeKey._4808; // 설명
        }

        void CloseUI()
        {
            UI.Close<UIMileage>();
        }

        void Refresh()
        {
            arrData = presenter.GetInputs();
            wrapper.Resize(arrData.Length);

            labelMileagePoint.Text = presenter.GetMileageText();
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            MileageSlot ui = go.GetComponent<MileageSlot>();
            ui.Set(index, arrData[index], presenter);
        }
    }
}