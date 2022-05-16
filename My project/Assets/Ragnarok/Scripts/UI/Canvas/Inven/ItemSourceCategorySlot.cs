using UnityEngine;

namespace Ragnarok
{
    public class ItemSourceCategorySlot : UIData<ItemSourcePresenter, ItemSourceCategoryData>, IAutoInspectorFinder
    {
        [SerializeField] UITextureHelper icon; // 아이콘
        [SerializeField] UILabelHelper labelTitle; // 제목
        [SerializeField] UILabelHelper labelDesc; // 설명
        [SerializeField] UIButtonHelper btnEnter; // 목록 버튼

        protected void Awake()
        {
            EventDelegate.Add(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnEnter.OnClick, OnClickedBtnEnter);
        }

        protected override void Refresh()
        {
            if (data == null)
                return;

            icon.Set(data.categoryType.GetIconName(presenter.GetItemInfo()));

            labelTitle.Text = data.categoryType.GetName();
            labelDesc.Text = data.categoryType.GetDescription();

            // 버튼 설정
            ItemSourceButtonType buttonType = data.categoryType.GetButtonType();
            if (buttonType != ItemSourceButtonType.None)
            {
                btnEnter.SetActive(true);
                btnEnter.Text = data.categoryType.GetButtonType().GetText();
            }
            else
            {
                btnEnter.SetActive(false);
            }
        }

        void OnClickedBtnEnter()
        {
            if (data == null)
                return;

            // 잠긴 콘텐츠인지 확인
            if (!presenter.IsOpenedContent(data.categoryType))
                return;

            ItemSourceButtonType buttonType = data.categoryType.GetButtonType();
            switch (buttonType)
            {
                case ItemSourceButtonType.ListPopup:
                    UI.Show<UIItemSourceDetail>(new UIItemSourceDetail.Input(data.categoryType, data.itemInfo));
                    break;

                case ItemSourceButtonType.Move:
                    presenter.Move(data.categoryType, data.itemInfo);
                    break;
            }
        }
    }
}