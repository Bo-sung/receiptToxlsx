using UnityEngine;

namespace Ragnarok
{
    public class ShareForceLevelUpPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly CharacterModel characterModel;
        private readonly InventoryModel inventoryModel;

        // <!-- Repositories --!>
        private readonly TPCostumeLevelDataManager tpCostumeLevelDataRepo;

        // <!-- Event --!>
        public event System.Action OnShareForceLevelUp
        {
            add { characterModel.OnShareForceLevelUp += value; }
            remove { characterModel.OnShareForceLevelUp -= value; }
        }

        public ShareForceLevelUpPresenter()
        {
            characterModel = Entity.player.Character;
            inventoryModel = Entity.player.Inventory;
            tpCostumeLevelDataRepo = TPCostumeLevelDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public int GetLevel(ShareForceType type)
        {
            return characterModel.GetShareForceLevel(type);
        }

        public RewardData GetZeny(ShareForceType type)
        {
            int level = GetLevel(type) + 1;
            TPCostumeLevelData data = tpCostumeLevelDataRepo.Get(type.ToIntValue(), level);

            if (data == null)
                return null;

            return data.GetZeny();
        }

        public RewardData[] GetMaterials(ShareForceType type)
        {
            int level = GetLevel(type) + 1;
            TPCostumeLevelData data = tpCostumeLevelDataRepo.Get(type.ToIntValue(), level);

            if (data == null)
                return null;

            return data.GetMaterials();
        }

        public int GetItemCount(int itemId)
        {
            return inventoryModel.GetItemCount(itemId);
        }

        public int GetShareForce(int level)
        {
            return BasisType.TIME_SUIT_LEVEL_INC.GetInt(level);
        }

        public int GetNeedJobLevel(ShareForceType type)
        {
            int level = GetLevel(type) + 1;
            TPCostumeLevelData data = tpCostumeLevelDataRepo.Get(type.ToIntValue(), level);

            if (data != null)
                return data.job_level;

            return -1; // 데이터가 없을 경우
        }

        public int GetMaxShareForceMaxLevel()
        {
            return BasisType.SHARE_FORCE_MAX_LEVEL.GetInt();
        }

        public int GetJobLevel()
        {
            return characterModel.JobLevel;
        }

        public bool IsShareForceMaxLevel(ShareForceType type)
        {
            int shareForceLevel = GetLevel(type);
            int shareForceMaxLevel = GetMaxShareForceMaxLevel();
            return shareForceLevel >= shareForceMaxLevel; // 강화 최대레벨
        }

        public void RequestShareForceLevelUp(ShareForceType type)
        {            
            Debug.Log($"CurShareForceLevel={GetLevel(type)}");
            Debug.Log($"MaxShareForceMaxLevel={GetMaxShareForceMaxLevel()}");
            Debug.Log($"JobLevel={GetJobLevel()}, NeedJobLevel={GetNeedJobLevel(type)}");

            // 재료 체크
            var item = GetZeny(type);
            if (item.RewardType == RewardType.Item)
            {
                var myCount = GetItemCount(item.ItemId);
                var needCount = item.Count;

                if (myCount < needCount)
                {
                    UI.ConfirmPopup(LocalizeKey._16018.ToText()); // 강화 재료가 부족합니다
                    return;
                }
            }
            else
            {
                // 재화 체크
                CoinType coinType = item.RewardType.ToCoinType();
                int needCoin = item.Count;
                if (!coinType.Check(needCoin))
                    return;
            }

            var materials = GetMaterials(type);
            if (materials != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].RewardType == RewardType.Item)
                    {
                        var myCount = GetItemCount(materials[i].ItemId);
                        var needCount = materials[i].Count;
                        if (myCount < needCount)
                        {
                            UI.ConfirmPopup(LocalizeKey._16018.ToText()); // 강화 재료가 부족합니다
                            return;
                        }
                    }
                }
            }

            characterModel.RequestShareForceLevelUp(type).WrapNetworkErrors();
        }
    }
}