using UnityEngine;

namespace Ragnarok
{
    public class UIMakeSelectPart : UICanvas, MakeSelectPartPresenter.IView
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UIRewardHelper makeItem;
        [SerializeField] UILabelHelper labelName;
        [SerializeField] UIButtonHelper btnWhere;
        [SerializeField] SuperScrollListWrapper wrapper;
        [SerializeField] GameObject prefab;
        [SerializeField] UIButtonHelper btnInfo;
        [SerializeField] UIButtonHelper btnConfirm;
        [SerializeField] UILabelHelper labelEmpty;
        [SerializeField] UILabelHelper labelNoti;

        ItemInfo[] arrayInfo;
        MakeSelectPartPresenter presenter;

        protected override void OnInit()
        {
            presenter = new MakeSelectPartPresenter(this);
            presenter.AddEvent();

            wrapper.SetRefreshCallback(OnItemRefresh);
            wrapper.SpawnNewList(prefab, 0, 0);

            EventDelegate.Add(btnExit.OnClick, OnBack);
            EventDelegate.Add(btnConfirm.OnClick, OnBack);
            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Add(btnWhere.OnClick, OnClickedBtnWhere);
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            EventDelegate.Remove(btnExit.OnClick, OnBack);
            EventDelegate.Remove(btnConfirm.OnClick, OnBack);
            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
            EventDelegate.Remove(btnWhere.OnClick, OnClickedBtnWhere);
        }

        protected override void OnShow(IUIData data = null)
        {
            Refresh();
        }

        protected override void OnHide()
        {
            presenter.ResetInfoMode();
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._28200; // 장비 선택
            btnWhere.LocalKey = LocalizeKey._28203; // 획득 가능 장소
            labelEmpty.LocalKey = LocalizeKey._28202; // 현재 해당하는 장비가 없습니다.
            btnInfo.LocalKey = LocalizeKey._28204; // 세부 정보
            btnConfirm.LocalKey = LocalizeKey._28205; // 선택 완료
            labelNoti.LocalKey = LocalizeKey._28206; // 재료로 사용 시 인챈트된 카드도 사라집니다.
        }

        protected override void OnBack()
        {
            base.OnBack();
            presenter.UIMakeRefresh();
        }

        public void Refresh()
        {
            // 아이템 정보
            var info = presenter.Info;
            makeItem.SetData(info.GetRewardData());
            labelName.Text = info.ItemName;
            labelTitle.Text = LocalizeKey._28201.ToText() // 아래 조건에 해당하는 재료를선택해주세요({COUNT}/{NEED_COUNT})
                .Replace("{COUNT}", presenter.GetSelectItemCount(info.SlotIndex).ToString())
                .Replace("{NEED_COUNT}", info.Count.ToString());

            // 스크롤 정보
            arrayInfo = presenter.GetItemInfos();
            wrapper.Resize(arrayInfo.Length);
            labelEmpty.SetActive(arrayInfo.Length == 0);
        }

        private void OnItemRefresh(GameObject go, int index)
        {
            UIMakeSelectSlot ui = go.GetComponent<UIMakeSelectSlot>();
            ui.SetData(presenter, arrayInfo[index]);
        }

        /// <summary>
        /// 세부정보 버튼
        /// </summary>
        void OnClickedBtnInfo()
        {
            presenter.ToggelInfoMode();
        }

        /// <summary>
        /// 획득가능 장소 버튼
        /// </summary>
        void OnClickedBtnWhere()
        {
            ItemInfo info = new EquipmentItemInfo(Entity.player.Inventory);
            info.SetData(presenter.Info.ItemData);
            UI.Show<UIItemSource>(new UIItemSource.Input(UIItemSource.Mode.GetSource, info));
        }
    }
}

