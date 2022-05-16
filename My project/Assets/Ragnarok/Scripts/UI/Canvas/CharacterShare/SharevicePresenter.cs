namespace Ragnarok
{
    public class SharevicePresenter : ViewPresenter, IShareviceItemImpl
    {
        /******************** Models ********************/
        private readonly InventoryModel inventoryModel;
        private readonly SharingModel sharingModel;
        private readonly CharacterModel characterModel;

        /******************** Repositories ********************/

        /******************** Event ********************/
        public event System.Action OnLevelUpSharevice
        {
            add { sharingModel.OnUpdateShareviceExperience += value; }
            remove { sharingModel.OnUpdateShareviceExperience -= value; }
        }

        /// <summary>
        /// 보유중인 쉐어바이스 아이템들
        /// </summary>
        private ShareviceItem[] shareviceItems;

        public SharevicePresenter()
        {
            inventoryModel = Entity.player.Inventory;
            sharingModel = Entity.player.Sharing;
            characterModel = Entity.player.Character;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 쉐어바이스 레벨 반환
        /// </summary>
        public int GetShareviceLevel()
        {
            return sharingModel.GetShareviceLevel();
        }

        /// <summary>
        /// 쉐어바이스 레벨 반환
        /// </summary>
        public int GetShareviceLevel(int selectedExp)
        {
            return sharingModel.GetShareviceLevel(selectedExp, characterModel.JobLevel);
        }

        /// <summary>
        /// 쉐어바이스의 현재 경험치 반환
        /// </summary>
        public int GetShareviceExp()
        {
            return sharingModel.GetShareviceExp();
        }

        /// <summary>
        /// 쉐어바이스 레벨업까지 남은 경험치 반환
        /// </summary>
        public int GetShareviceNeedExp()
        {
            return sharingModel.GetShareviceNeedExp();
        }

        /// <summary>
        /// 쉐어바이스의 최대 전투력 반환
        /// </summary>
        public int GetShareviceMaxBP()
        {
            return sharingModel.GetShareviceMaxBP();
        }

        /// <summary>
        /// 쉐어바이스의 현재 상태 반환
        /// </summary>
        public ShareviceState GetShareviceState()
        {
            return sharingModel.GetShareviceState();
        }

        /// <summary>
        /// 쉐어바이스 레벨업에 사용된 경험치 반환
        /// </summary>
        public int GetShareviceTempExp()
        {
            return sharingModel.GetShareviceTempExp();
        }

        /// <summary>
        /// 쉐어바이스 레벨업까지 남은 시간 반환
        /// </summary>
        public RemainTime GetShareviceLevelUpRemainTime()
        {
            return sharingModel.GetShareviceLevelUpRemainTime();
        }

        /// <summary>
        /// 쉐어바이스 레벨에 따른 레벨업시간 반환
        /// </summary>
        public long GetLevelUpTotalRemainTime(int targetLevel)
        {
            return sharingModel.GetLevelUpTotalRemainTime(targetLevel);
        }

        /// <summary>
        /// 쉐어바이스 레벨업이 가능한지 여부 검사.
        /// 셰어바이스 레벨은 잡레벨까지만 올릴 수 있음.
        /// </summary>
        public bool IsPossibleViceLevelUp()
        {
            return GetShareviceLevel() < characterModel.JobLevel;
        }

        /// <summary>
        /// 최대로 올릴수 있는 쉐어바이스 경험치 반환
        /// </summary>
        public int GetMaxShareviceExp()
        {
            return sharingModel.GetMaxShareviceExp(characterModel.JobLevel);
        }

        /// <summary>
        /// 쉐어바이스 레벨업 요청
        /// </summary>
        public void LevelUpSharevice()
        {
            sharingModel.RequestShareviceLevelUp(shareviceItems, GetTotalSelectedExp()).WrapNetworkErrors();
        }

        /// <summary>
        /// 쉐어바이스 레벨업 즉시완료 요청
        /// </summary>
        public void LevelUpCompleteSharevice()
        {
            sharingModel.RequestShareviceLevelUpComplete().WrapNetworkErrors();
        }

        /// <summary>
        /// 쉐어바이스 레벨 아이템 전체 사용
        /// </summary>
        public bool SetAutoMaxSelect()
        {
            if (IsImpossibleViceLevelUp())
            {
                UI.ShowToastPopup(LocalizeKey._10256.ToText());
                return false;
            }
            int remainExp = GetShareviceNeedExp() - (GetShareviceExp() + GetTotalSelectedExp());
            if (remainExp <= 0)
            {
                UI.ShowToastPopup(LocalizeKey._10257.ToText());
                return false;
            }
            bool isDirty = false;
            foreach (ShareviceItem item in shareviceItems)
            {
                while (item.SelectedCount < item.itemCount)
                {
                    isDirty = true;
                    item.PlusSelectCount(1);
                    remainExp -= item.itemExp;

                    if (remainExp <= 0)
                        return isDirty;
                }
            }
            return isDirty;
        }

        /// <summary>
        /// 셰어바이스 경험치 아이템 목록 반환
        /// </summary>
        private ItemInfo[] GetViceExperienceItemInfos()
        {
            var viceItems = inventoryModel.itemList.FindAll(a => a is PartsItemInfo && a.ItemType == ItemType.ShareVice);
            viceItems.Sort(delegate (ItemInfo a, ItemInfo b) // 경험치량이 작은 아이템 순으로 정렬
            {
                if (a.ExpValue > b.ExpValue) return 1;
                else if (a.ExpValue < b.ExpValue) return -1;
                return 0;
            });

            return viceItems.ToArray();
        }

        /// <summary>
        /// 셰어바이스 경험치 아이템 목록 반환
        /// </summary>
        public ShareviceItem[] GetShareviceItems()
        {
            ItemInfo[] itemInfos = GetViceExperienceItemInfos(); // 셰어바이스 경험치 재료 가져오기
            shareviceItems = new ShareviceItem[itemInfos.Length];
            for (int i = 0; i < shareviceItems.Length; i++)
            {
                shareviceItems[i] = new ShareviceItem(this, itemInfos[i]);
            }

            return shareviceItems;
        }

        /// <summary>
        /// 셰어바이스 경험치 음슴
        /// </summary>
        public bool IsEmptyShareViceItem()
        {
            return shareviceItems.Length == 0;
        }

        /// <summary>
        /// 레벨업 불가능 여부
        /// </summary>
        public bool IsImpossibleViceLevelUp()
        {
            return GetShareviceLevel() >= characterModel.JobLevel;
        }

        /// <summary>
        /// 선택한 경험치 최대 여부
        /// </summary>
        public bool IsMaxExp()
        {
            return GetShareviceExp() + GetTotalSelectedExp() >= GetShareviceNeedExp();
        }

        /// <summary>
        /// 선택한 총 경험치
        /// </summary>
        public int GetTotalSelectedExp()
        {
            int selectedExp = 0;
            for (int i = 0; i < shareviceItems.Length; i++)
            {
                selectedExp += shareviceItems[i].GetSelectedExp();
            }
            return selectedExp;
        }

        /// <summary>
        /// 경험치 선택 여부
        /// </summary>
        public bool IsSelectedExp()
        {
            return GetTotalSelectedExp() > 0;
        }
    }
}