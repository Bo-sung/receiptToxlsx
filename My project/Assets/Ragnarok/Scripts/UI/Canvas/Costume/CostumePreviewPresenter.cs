namespace Ragnarok
{
    public class CostumePreviewPresenter : ViewPresenter
    {
        private class DummyEquippedInfo : ItemInfo.IEquippedItemValue
        {
            public int ItemId => info.ItemId;

            public int ItemLevel => info.Smelt;

            public int ItemTranscend => info.ItemTranscend;

            public int ItemChangedElement => info.ItemChangedElement;

            public int ElementLevel => info.ElementLevel;

            private ItemInfo info;

            public DummyEquippedInfo(ItemInfo info)
            {
                this.info = info;
            }

            public int? GetEquippedCardId(int index)
            {
                return null;
            }

            public int? GetEquippedCardLevel(int index)
            {
                return null;
            }
        }

        private CharacterEntity dummyUIEntity;

        /// <summary>
        /// UnitViewer용 더미 플레이어 반환
        /// </summary>
        public CharacterEntity GetDummyUIPlayer(ItemInfo costume)
        {
            if (dummyUIEntity is null)
                dummyUIEntity = CharacterEntity.Factory.CreateDummyUIPlayer(isBookPreview: true);
            DummyCharacterModel dummyCharacterModel = dummyUIEntity.Character as DummyCharacterModel;
            var charModel = Entity.player.Character;
            var weapon = Entity.player.Inventory.GetItemInfo(ItemEquipmentSlotType.Weapon);

            dummyCharacterModel.Set(charModel.Job, charModel.Gender);
            dummyCharacterModel.Set(weapon != null ? weapon.ItemId : 0);
            dummyCharacterModel.SetCostume(new ItemInfo.IEquippedItemValue[] { new DummyEquippedInfo(costume) });

            return dummyUIEntity;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public void Dispose()
        {
            if (dummyUIEntity != null)
            {
                dummyUIEntity.ResetData();
                dummyUIEntity.Dispose();
            }
        }
    }
}
