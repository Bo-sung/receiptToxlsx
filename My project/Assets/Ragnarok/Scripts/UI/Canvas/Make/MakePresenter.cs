namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIMake"/>
    /// </summary>
    public class MakePresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
            void SetMakeDetailView();
            void PlayMakeAnimation();
            void SetActiveMakeAnimation(bool isActive);
        }

        private readonly IView view;
        private readonly GoodsModel goodsModel;
        private readonly MakeModel makeModel;
        private readonly InventoryModel inventoryModel;
        private readonly QuestModel questModel;

        public MakePresenter(IView view)
        {
            this.view = view;
            goodsModel = Entity.player.Goods;
            makeModel = Entity.player.Make;
            inventoryModel = Entity.player.Inventory;
            questModel = Entity.player.Quest;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int MainTab => makeModel.MainTab;
        public int SubTab => makeModel.SubTab;
        /// <summary>
        /// 선택한 제작 아이템 정보
        /// </summary>
        public MakeInfo Info => makeModel.SelectMakeInfo;

        /// <summary>
        /// 제작할 아이템 수량
        /// </summary>
        public int MakeCount => makeModel.MakeCount;

        /// <summary>
        /// 보유 제니
        /// </summary>
        public long Zeny => goodsModel.Zeny;

        public MakeInfo[] GetMakeInfos(int mainTab, int subTab)
        {
            return makeModel.GetMakeInfos(mainTab, subTab);
        }

        /// <summary>
        /// 특정 아이템의 탭 정보를 반환
        /// </summary>
        public (int mainTab, int subTab) GetMakeTab(int itemId, int materialId)
        {
            return makeModel.GetMakeTab(itemId, materialId);
        }

        public MakeInfo GetMakeInfo(int itemId, int materialId)
        {
            return makeModel.GetMakeInfo(itemId, materialId);
        }

        public void ResetMakeInfo()
        {
            if (makeModel.SelectMakeInfo != null)
                makeModel.SetSelectMakeInfo(null);
        }

        public void SetSelectMakeInfo(MakeInfo info)
        {
            makeModel.SetSelectMakeInfo(info);
            makeModel.SetMakeCount(1);
            view.Refresh();
        }

        public void SetSelectMaterialInfo(MaterialInfo info)
        {
            makeModel.SetSelectMaterialInfo(info);
        }

        public int GetSelectItemCount(int slotIndex)
        {
            return makeModel.GetSelectItemCount(slotIndex);
        }

        /// <summary>
        /// 보유중인 아이템 수량
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public int GetItemCount(int itemId)
        {
            return inventoryModel.GetItemCount(itemId);
        }

        /// <summary>
        /// 제작 수량 세팅
        /// </summary>
        /// <param name="count"></param>
        /// <param name="isEvent"></param>
        public void SetMakeCount(int count, bool isEvent = true)
        {
            makeModel.SetMakeCount(count);
            if (isEvent)
                view.SetMakeDetailView();
        }
        /// <summary>
        /// 제작 수량 받기
        /// </summary>
        /// <param name="isEvent"></param>
        public int GetMakeCount()
        {
            return makeModel.MakeCount;
        }
        /// <summary>
        /// 메인탭기준 제작가능한 아이템 있는지 여부
        /// </summary>
        /// <param name="mainTab"></param>
        /// <returns></returns>
        public bool IsMake(int mainTab)
        {
            return makeModel.IsMakeTab(mainTab);
        }

        /// <summary>
        /// 서브탭중 제작가능한 아이템 있는지 여부
        /// </summary>
        /// <param name="mainTab"></param>
        /// <param name="subTab"></param>
        /// <returns></returns>
        public bool IsMake(int mainTab, int subTab)
        {
            return makeModel.IsMakeSubTab(mainTab, subTab);
        }

        /// <summary>
        /// 신규 컨텐츠 플래그 제거
        /// </summary>
        public void RemoveNewOpenContent_Make()
        {
            questModel.RemoveNewOpenContent(ContentType.Make); // 신규 컨텐츠 플래그 제거 (제작)
        }

        /// <summary>
        /// 제작 요청
        /// </summary>
        /// <param name="make"></param>
        public async void RequestMakeItem()
        {
            view.SetActiveMakeAnimation(false);

            var info = makeModel.SelectMakeInfo;
            var isSuccess = await makeModel.RequestMakeItem(info, makeModel.MakeCount);
            if (!isSuccess)
                return;

            view.Refresh();
        }

        public void MakeItem()
        {
            view.PlayMakeAnimation();
        }

        public ItemInfo CreateItemInfo(int itemId)
        {
            return inventoryModel.CreateItmeInfo(itemId);
        }

        public bool IsOpenContent()
        {
            return questModel.IsOpenContent(ContentType.Make, isShowPopup: false);
        }

        public string OpenContentText()
        {
            return questModel.OpenContentText(ContentType.Make);
        }
    }
}