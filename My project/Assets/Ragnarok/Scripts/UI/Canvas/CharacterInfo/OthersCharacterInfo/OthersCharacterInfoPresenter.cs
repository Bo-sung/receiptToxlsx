namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIOthersCharacterInfo"/>
    /// </summary>
    public class OthersCharacterInfoPresenter : ViewPresenter
    {
        public interface IView
        {

        }

        /******************** Models ********************/
        private CharacterEntity targetEntity;
        private CharacterModel characterModel;
        private StatusModel statusModel;
        private InventoryModel inventoryModel;
        private SkillModel skillModel;
        private GuildModel guildModel;
        private BattleCharacterPacket targetPlayerPacket;

        /******************** Repositories ********************/
        private readonly ItemDataManager.IItemDataRepoImpl itemDataRepo;

        /******************** Event ********************/

        private readonly IView view;
        private CharacterEntity dummyUIEntity;

        public OthersCharacterInfoPresenter(IView view)
        {
            this.view = view;


            itemDataRepo = ItemDataManager.Instance;
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

            if (targetEntity != null)
                targetEntity.SetForceStatus(ForceStatusType.BlssBuffOn);
        }

        public void SetPlayer(BattleCharacterPacket packet)
        {
            this.targetPlayerPacket = packet;
            IMultiPlayerInput input = packet;

            targetEntity = CharacterEntity.Factory.CreateMultiBattlePlayer(UnitEntity.UnitState.Stage);
            targetEntity.SetForceStatus(ForceStatusType.BlssBuffOff);
            characterModel = targetEntity.Character;
            statusModel = targetEntity.Status;
            inventoryModel = targetEntity.Inventory;
            skillModel = targetEntity.Skill;
            guildModel = targetEntity.Guild;

            // Model 초기화
            characterModel.Initialize(input);
            statusModel.Initialize(input);
            statusModel.Initialize(input.IsExceptEquippedItems, input.BattleOptions, input.GuildBattleOptions);
            inventoryModel.Initialize(input.ItemStatusValue, weaponItemId: 0, input.ArmorItemId, input.WeaponChangedElement, input.WeaponElementLevel, input.ArmorChangedElement, input.ArmorElementLevel, input.GetEquippedItems);
            skillModel.Initialize(input.IsExceptEquippedItems, input.Skills);
            skillModel.Initialize(input.Slots);
            guildModel.Initialize(input);

            // 상태 세팅
            InitializeAppearance(packet); // 외관 세팅

            // 장비아이템 Reload
            inventoryModel.ForceReload();
        }

        /// <summary>
        /// 장비 인포 팝업 Show
        /// </summary>
        public void ShowEquipmentInfo(ItemEquipmentSlotType slotType)
        {
            UI.Show<UIEquipmentInfo>().Set(FindEquipmentItemNo(slotType), inventoryModel);
            return;
        }

        /// <summary>
        /// 장착된 장비의 ItemNo 반환
        /// </summary>
        private long FindEquipmentItemNo(ItemEquipmentSlotType slotType)
        {
            ItemInfo[] equippedItems = inventoryModel.GetEquippedItems();
            for (int i = 0; i < equippedItems.Length; i++)
            {
                if (equippedItems[i].EquippedSlotType == slotType)
                    return equippedItems[i].ItemNo;
            }

            return 0L;
        }

        /// <summary>
        /// 장착중인 장비 반환
        /// </summary>
        public EquipmentItemInfo GetEquipment(ItemEquipmentSlotType findSlotType)
        {
            return inventoryModel.itemList.Find(e => e.EquippedSlotType == findSlotType) as EquipmentItemInfo;
        }


        /// <summary>
        /// 전투력 반환
        /// </summary>
        public int GetBattleScore()
        {
            return this.targetPlayerPacket.Power;
        }

        /// <summary>
        /// 인벤토리 모델 반환
        /// </summary>
        public InventoryModel GetInventoryModel()
        {
            return this.inventoryModel;
        }

        /// <summary>
        /// 외관 세팅 (직업, 성별 등)
        /// </summary>
        private void InitializeAppearance(BattleCharacterPacket packet)
        {
            characterModel.SetJob(packet.Job.ToEnum<Job>());
            characterModel.SetGender(packet.Gender.ToEnum<Gender>());
        }

        /// <summary>
        /// UnitViewer용 더미 플레이어 반환
        /// </summary>
        public CharacterEntity GetDummyUIPlayer()
        {
            if (dummyUIEntity is null)
                dummyUIEntity = CharacterEntity.Factory.CreateDummyUIPlayer(isBookPreview: false);
            DummyCharacterModel dummyCharacterModel = dummyUIEntity.Character as DummyCharacterModel;

            IMultiPlayerInput input = this.targetPlayerPacket;
            dummyCharacterModel.Set(this.targetPlayerPacket.Job.ToEnum<Job>(), this.targetPlayerPacket.Gender.ToEnum<Gender>());
            dummyCharacterModel.Set(this.targetPlayerPacket.WeaponItemId);
            dummyCharacterModel.SetCostume(input.GetEquippedItems);

            return dummyUIEntity;
        }


        public CharacterEntity GetPlayer() => targetEntity;
        public string GetNickname() => targetEntity.GetName();
    }
}