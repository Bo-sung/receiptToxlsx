using UnityEngine;

namespace Ragnarok
{
    public class UIItemSourceDetail : UICanvas<ItemSourceDetailPresenter>, ItemSourceDetailPresenter.IView, IAutoInspectorFinder
    {
        protected override UIType uiType => UIType.Hide | UIType.Back;

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnConfirm;

        [SerializeField] SuperScrollListWrapper wrapper_text;
        [SerializeField] GameObject prefab_text;
        [SerializeField] SuperScrollListWrapper wrapper_icon;
        [SerializeField] GameObject prefab_icon;


        public class Input : IUIData
        {
            public ItemSourceCategoryType categoryType;
            public ItemInfo itemInfo;

            public Input(ItemSourceCategoryType categoryType, ItemInfo itemInfo)
            {
                this.categoryType = categoryType;
                this.itemInfo = itemInfo;
            }

        }

        Input input;
        ItemSourceDetailData[] slotsData;

        protected override void OnInit()
        {
            presenter = new ItemSourceDetailPresenter(this);
            presenter.AddEvent();

            wrapper_text.SpawnNewList(prefab_text, 0, 0);
            wrapper_text.SetRefreshCallback(OnRefreshElement);

            wrapper_icon.SpawnNewList(prefab_icon, 0, 0);
            wrapper_icon.SetRefreshCallback(OnRefreshElement);

            EventDelegate.Add(btnExit.OnClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.OnClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            btnConfirm.LocalKey = LocalizeKey._46100; // 확 인
            Refresh();
        }

        protected override void OnShow(IUIData data = null)
        {
            this.input = data as Input;

            if (this.input == null)
                return;

            slotsData = presenter.GetDetailSlotData(input);
            Refresh();

            // 스테이지일 경우, 내가 갈 수 있는 최대 스테이지가 화면 최상위에 노출되도록 자동 스크롤.
            if (input.categoryType == ItemSourceCategoryType.StageDrop && slotsData.Length > 8)
            {
                float highestProgress = presenter.GetProgressHighestStage(slotsData);
                wrapper_text.SetProgress(highestProgress);
            }
            else
            {
                // 그 외의 경우 스크롤 초기화.
                bool isTextView = presenter.IsTextView(input.categoryType);
                if (isTextView)
                {
                    wrapper_text.SetProgress(0f);
                }
                else
                {
                    wrapper_icon.SetProgress(0f);
                }
            }
        }

        void Refresh()
        {
            if (this.input == null)
                return;

            labelTitle.Text = input.categoryType.GetName();

            bool isTextView = presenter.IsTextView(input.categoryType);
            if (isTextView)
            {
                wrapper_icon.gameObject.SetActive(false);
                wrapper_text.gameObject.SetActive(true);
                wrapper_text.Resize(slotsData.Length);
            }
            else
            {
                wrapper_text.gameObject.SetActive(false);
                wrapper_icon.gameObject.SetActive(true);
                wrapper_icon.Resize(slotsData.Length);
            }
        }

        void CloseUI()
        {
            UI.Close<UIItemSourceDetail>();
        }

        void OnRefreshElement(GameObject go, int index)
        {
            ItemSourceDetailSlot detailSlot = go.GetComponent<ItemSourceDetailSlot>();
            detailSlot.SetData(presenter, slotsData[index]);
        }
    }
}