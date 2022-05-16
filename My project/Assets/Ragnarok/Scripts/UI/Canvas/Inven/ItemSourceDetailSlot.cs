using UnityEngine;

namespace Ragnarok
{
    public class ItemSourceDetailSlot : UIData<ItemSourceDetailPresenter, ItemSourceDetailData>, IAutoInspectorFinder
    {
        // 텍스트만 출력
        [SerializeField] UILabelHelper labelText;
        [SerializeField] GameObject goLockBase;

        // 아이콘과 제목 내용을 출력
        [SerializeField] UIRewardHelper rewardHelper;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelDesc;

        // 공통 버튼
        [SerializeField] UIButtonHelper btnEnter;

        protected override void Start()
        {
            base.Start();

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

            bool isTextView = presenter.IsTextView(data.categoryType);

            if (isTextView)
            {
                labelText.Text = data.text;

                // 스테이지의 경우 갈 수 없는 스테이지면 Lock을 건다. (+ 현재 스테이지도)
                if (data.categoryType == ItemSourceCategoryType.StageDrop)
                {
                    bool isOpenedStage = presenter.IsOpenedStage(data.value_1);
                    goLockBase.SetActive(!isOpenedStage);
                    btnEnter.IsEnabled = isOpenedStage;
                }
                else
                {
                    goLockBase.SetActive(false);
                    btnEnter.IsEnabled = true;
                }
            }
            else
            {
                RewardData rewardData = new RewardData(RewardType.Item, data.itemInfo.ItemId, 0);
                rewardHelper.SetData(rewardData);
                rewardHelper.UseDefaultButtonEvent = false;
                labelTitle.Text = data.itemInfo.Name;
                labelDesc.Text = data.itemInfo.Description;
            }

            btnEnter.Text = data.categoryType.GetDetailButtonType().GetText();
        }

        void OnClickedBtnEnter()
        {
            switch (data.categoryType)
            {
                case ItemSourceCategoryType.StageDrop:
                    if (UIBattleMatchReady.IsMatching)
                    {
                        string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                        UI.ShowToastPopup(message);
                        return;
                    }

                    if (Entity.player.Dungeon.LastEnterStageId == data.value_1)
                    {
                        UI.ShowToastPopup(LocalizeKey._90199.ToText());
                        return;
                    }

                    // 해당 스테이지로 이동
                    presenter.GoToStage(data.value_1);
                    break;

                case ItemSourceCategoryType.Box:
                    // 상자 아이템 정보
                    data.itemInfo.SetRemainCoolDown(1f);
                    presenter.ShowBoxItemInfo(data.itemInfo as BoxItemInfo);
                    break;

                case ItemSourceCategoryType.UseMake:
                    // 해당 아이템 제작으로 이동
                    presenter.GoToMake(data.itemInfo.ItemId, presenter.ItemId);
                    break;
            }
        }
    }
}