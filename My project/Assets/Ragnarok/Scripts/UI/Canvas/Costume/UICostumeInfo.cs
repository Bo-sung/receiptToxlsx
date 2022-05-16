using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICostumeInfo : UICanvas
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper background;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] CostumeInfoView costumeInfoView;
        [SerializeField] UIButtonHelper btnEquip;
        [SerializeField] UIButtonHelper btnPreview;

        CostumeInfoPresenter presenter;

        protected override void OnInit()
        {
            presenter = new CostumeInfoPresenter();

            EventDelegate.Add(background.OnClick, CloseUI);
            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnEquip.OnClick, presenter.OnClickedBtnOk);
            EventDelegate.Add(btnPreview.OnClick, ShowPreview);

            costumeInfoView.OnSelectSource += OnSelectSource;
            presenter.OnUpdateItem += UpdateItem;
            presenter.OnUpdateCostume += CloseUI;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(background.OnClick, CloseUI);
            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnEquip.OnClick, presenter.OnClickedBtnOk);
            EventDelegate.Remove(btnPreview.OnClick, ShowPreview);

            costumeInfoView.OnSelectSource -= OnSelectSource;
            presenter.OnUpdateItem -= UpdateItem;
            presenter.OnUpdateCostume -= CloseUI;

            presenter.RemoveEvent();
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._16000; // 아이템 정보
        }

        void CloseUI()
        {
            UI.Close<UICostumeInfo>();
        }

        public void Set(long costumeNo)
        {
            presenter.SetCostumeNo(costumeNo);
            UpdateItem();
        }

        public void Set(ItemInfo costumeInfo)
        {
            presenter.SetCostumeInfo(costumeInfo);
            UpdateItem();
        }

        void UpdateItem()
        {
            UpdateCostumeView();
            UpdateButtonInfo();
        }

        void UpdateCostumeView()
        {
            ItemInfo info = presenter.GetCostume();
            if (info == null)
                return;

            costumeInfoView.SetData(info);
        }

        void UpdateButtonInfo()
        {
            ItemInfo info = presenter.GetCostume();
            if (info == null)
                return;

            btnEquip.Text = presenter.IsOwningCostume ? (info.IsEquipped ? LocalizeKey._4301.ToText() : LocalizeKey._4300.ToText()) : LocalizeKey._2902.ToText(); // 장착, 해제, 확인
            bool isPreview = info.SlotType != ItemEquipmentSlotType.CostumeTitle;

            // 몸 코스튬 성별 다름 체크
            if (info.CostumeType == CostumeType.Body)
            {
                if (!info.CostumeBodyType.IsInvisible(presenter.GetGender()))
                    isPreview = false;
            }

            btnPreview.SetActive(isPreview);
        }

        void OnSelectSource(UIItemSource.Mode mode)
        {
            ItemInfo info = presenter.GetCostume();
            if (info == null)
                return;

            UI.Show<UIItemSource>(new UIItemSource.Input(mode, info));
        }

        void ShowPreview()
        {
            UI.Show<UICostumePreview>(new UICostumePreview.Input(presenter.GetCostume()));
        }
    }
}