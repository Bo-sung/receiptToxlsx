using UnityEngine;

namespace Ragnarok
{
    public class UIItemSource : UICanvas<ItemSourcePresenter>, ItemSourcePresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper LabelTitle;
        [SerializeField] UIButtonHelper BtnConfirm;
        [SerializeField] UIButtonHelper BtnExit;

        [SerializeField] UIEquipmentProfile itemProfile;
        [SerializeField] UILabelHelper labelItemName;
        [SerializeField] UILabelHelper labelItemDesc;


        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;

        public enum Mode
        {
            GetSource, // 획득처
            Use, // 사용처
        }

        public class Input : IUIData
        {
            public Mode mode; // 현재 UI가 획득처UI인지 사용처UI인지.
            public ItemInfo itemInfo;

            public Input(Mode mode, ItemInfo itemInfo)
            {
                this.mode = mode;
                this.itemInfo = itemInfo;
            }
        }

        Input input;
        ItemSourceCategoryData[] categoryDatas;

        protected override void OnInit()
        {
            presenter = new ItemSourcePresenter(this);
            presenter.AddEvent();

            wrapper.SpawnNewList(prefab, 0, 0);
            wrapper.SetRefreshCallback(OnRefreshItem);

            EventDelegate.Add(BtnConfirm.OnClick, CloseUI);
            EventDelegate.Add(BtnExit.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();
            EventDelegate.Remove(BtnConfirm.OnClick, CloseUI);
            EventDelegate.Remove(BtnExit.OnClick, CloseUI);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            BtnConfirm.LocalKey = LocalizeKey._46002; // 확 인
            Refresh();
        }

        protected override void OnShow(IUIData data = null)
        {
            this.input = data as Input;

            if (input.mode == Mode.Use && input.itemInfo.Use_ClassBitType == ItemSourceCategoryType.None ||
                input.mode == Mode.GetSource && input.itemInfo.Get_ClassBitType == ItemSourceCategoryType.None)
            {
                UI.ShowToastPopup(LocalizeKey._46055.ToText()); // 정보가 없습니다
                CloseUI();
                return;
            }

            Refresh();
        }

        void Refresh()
        {
            if (input == null)
                return;


            switch (input.mode)
            {
                case Mode.GetSource:
                    LabelTitle.LocalKey = LocalizeKey._46000; // 아이템 획득처
                    break;
                case Mode.Use:
                    LabelTitle.LocalKey = LocalizeKey._46001; // 아이템 사용처
                    break;
            }

            itemProfile.SetData(input.itemInfo);

            labelItemName.Text = input.itemInfo.Name;
            labelItemDesc.Text = input.itemInfo.Description;

            categoryDatas = presenter.GetCategoryTypeArray(input.itemInfo, input.mode);
            wrapper.Resize(categoryDatas.Length);
            wrapper.SetProgress(0f);
        }

        void OnRefreshItem(GameObject go, int idx)
        {
            ItemSourceCategorySlot data = go.GetComponent<ItemSourceCategorySlot>();
            data.SetData(presenter, categoryDatas[idx]);
        }

        void CloseUI()
        {
            UI.Close<UIItemSource>();
        }

        ItemInfo ItemSourcePresenter.IView.GetItemInfo()
        {
            return input?.itemInfo;
        }


        public static string GetActivationColorString(bool isActive)
        {
            if (isActive)
                return "[64A2EE]";
            return "[B3B3B3]";
        }
    }
}