using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UICardInfo"/>
    /// </summary>
    public sealed class CardInfoPresenter : ViewPresenter
    {
        public interface IView
        {
            void Refresh();
        }

        private readonly IView view;
        private readonly InventoryModel inventoryModel;

        public UICardInfo.Input input { get; private set; }

        public CardInfoPresenter(IView view)
        {
            this.view = view;
            inventoryModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {
            inventoryModel.OnUpdateItem += view.Refresh;
        }

        public override void RemoveEvent()
        {
            inventoryModel.OnUpdateItem -= view.Refresh;
            RemoveInfoEvent();
        }

        public void SelectInfo(UICardInfo.Input info)
        {
            RemoveInfoEvent();

            input = info;

            AddInfoEvent();
            view.Refresh();
        }

        private void AddInfoEvent()
        {
            if (input != null)
                input.itemInfo.OnUpdateEvent += view.Refresh;
        }

        private void RemoveInfoEvent()
        {
            if (input != null)
            {
                input.itemInfo.OnUpdateEvent -= view.Refresh;
                input = null;
            }
        }

        /// <summary>
        /// 장비에서 카드 인첸트 해제
        /// </summary>
        /// <returns></returns>
        public async Task RequestUnEnchantEquipment()
        {
            var sender = new CardEquipSender[1];
            sender[0] = new CardEquipSender(0, (byte)(input.index + 1));

            await inventoryModel.RequestMultiCardEquip(input.equipmentInfo.ItemNo, sender);
        }

        /// <summary>
        /// 장비에 카드 인첸트
        /// </summary>
        /// <returns></returns>
        public async Task RequestEnchantEquipment()
        {
            var sender = new CardEquipSender[1];
            sender[0] = new CardEquipSender(input.itemInfo.ItemNo, (byte)(input.index + 1));
            await inventoryModel.RequestMultiCardEquip(input.equipmentInfo.ItemNo, sender);
        }
    }
}
