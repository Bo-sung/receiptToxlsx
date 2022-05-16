using UnityEngine;

namespace Ragnarok
{
    public sealed class MakeInfo : DataInfo<MakeData>
    {
        private readonly IInventoryModel inventoryModel; // 인벤토리 모델
        private readonly ItemDataManager itemDataRepo;

        public MakeInfo(IInventoryModel inventoryModel)
        {
            this.inventoryModel = inventoryModel;
            itemDataRepo = ItemDataManager.Instance;
        }

        private bool isMaterials;
        private bool isZeny;

        public int ID => data.id;
        /// <summary>
        /// 제작 아이템
        /// </summary>
        public RewardData Reward { get; private set; }
        /// <summary>
        /// 제작 아이템 이름
        /// </summary>
        public string ItemName => Reward.ItemName;
        /// <summary>
        /// 제작 아이템 수량
        /// </summary>
        public int Count => Reward.Count;
        /// <summary>
        /// 제작에 필요한 제니
        /// </summary>
        public int Zeny => data.cost;
        public string Description => data.des_id.ToText();
        /// <summary>
        /// 메인 탭
        /// </summary>
        public int MainTab => data.display_type;
        /// <summary>
        /// 서브 탭
        /// </summary>
        public int SubTab => data.details_tab;
        /// <summary>
        /// 제작 가능 여부
        /// </summary>
        public bool IsMake => isMaterials && isZeny;
        /// <summary>
        /// 제작에 필요한 재료 정보
        /// </summary>
        public MaterialInfo[] MaterialInfos;
        /// <summary>
        /// 제작 성공 확률
        /// </summary>
        public int Rate => data.rate;
        /// <summary>
        /// 동시에 여러개 제작 가능 여부
        /// </summary>
        public bool IsStackable { get; private set; }
        /// <summary>
        /// 정렬 인덱스
        /// </summary>
        public int Sort => data.sort_index;
        /// <summary>
        /// 제작 가능한 횟수
        /// </summary>
        public int MaxMakeCount { get; private set; }
        /// <summary>
        /// 제작 결과 아이템
        /// </summary>
        public int ResultID => data.result;
        /// <summary>
        /// 장비 재료를 현재 갖고 있는가
        /// </summary>
        public bool IsEquipmentMaterialReady { get; private set; } = false;
        /// <summary>
        /// 표시 타입
        /// </summary>
        public EnableType EnableType => data.enable_type.ToEnum<EnableType>();

        public override void SetData(MakeData data)
        {
            base.SetData(data);

            if (data.enable_type == 0)
                return;

            Reward = new RewardData((byte)RewardType.Item, data.result, data.result_count);

            if (data.enable_type == 2)
                SetMaterial();
        }

        void SetMaterial()
        {
            IsStackable = true;
            MaterialInfos = new MaterialInfo[data.needItems.Count];

            for (int i = 0; i < MaterialInfos.Length; i++)
            {
                MaterialInfos[i] = new MaterialInfo(data.needItems[i].Item2, data.needItems[i].Item3);
                MaterialInfos[i].SetData(itemDataRepo.Get(data.needItems[i].Item1));
                MaterialInfos[i].SlotIndex = i;

                if (IsStackable)
                    IsStackable = MaterialInfos[i].IsStackable;
            }
        }

        public void SetItemCount()
        {
            isMaterials = true;
            int makeCount = 0;
            bool isInit = false;
            bool checkOwingEquipmentMat = true;
            IsEquipmentMaterialReady = false;

            for (int i = 0; i < MaterialInfos.Length; i++)
            {
                // 재료 개수
                int count = inventoryModel.GetItemCount(MaterialInfos[i].ItemId);
                if (isMaterials)
                {
                    // 재료 필요 수량 있는지 체크
                    if (count < MaterialInfos[i].Count)
                    {
                        makeCount = 0;
                        isMaterials = false;
                    }
                    else
                    {
                        // 제작 가능 횟수 체크
                        int maxCount = count / MaterialInfos[i].Count;
                        if (maxCount < makeCount || !isInit)
                        {
                            isInit = true;
                            makeCount = maxCount;
                        }
                    }
                }

                if (checkOwingEquipmentMat && MaterialInfos[i].ItemGroupType == ItemGroupType.Equipment && count > 0)
                {
                    checkOwingEquipmentMat = false;
                    IsEquipmentMaterialReady = true;
                }
            }

            isZeny = CoinType.Zeny.GetCoin() >= Zeny;
            MaxMakeCount = Mathf.Min(makeCount, (int)(CoinType.Zeny.GetCoin() / Zeny));
            InvokeEvent();
        }

        public void CheckZeny(long zeny = 0)
        {
            isZeny = CoinType.Zeny.GetCoin() >= Zeny;
            InvokeEvent();
        }

        public void Refresh()
        {
            InvokeEvent();
        }

        /// <summary>
        /// 제작 경고 메시지
        /// </summary>
        public string GetMakeWarningMessage(bool isPopupMessage)
        {
            int needLevel = data.job_lv;
            if (needLevel == 0)
                return string.Empty;

            if (inventoryModel == null)
                return string.Empty;

            int jobLevel = inventoryModel.GetJobLevel();

            // JobLevel 제한
            if (jobLevel < needLevel)
            {
                if (isPopupMessage)
                {
                    // JOB Lv이 부족하여 해당 장비를 제작할 수 없습니다.\n\n[c][4497F4](JOB Lv {LEVEL} 필요)[-][/c]
                    return LocalizeKey._28070.ToText().Replace(ReplaceKey.LEVEL, needLevel);
                }

                // JOB Lv\n{LEVEL}
                return LocalizeKey._28069.ToText().Replace(ReplaceKey.LEVEL, needLevel);
            }

            return string.Empty;
        }

        /// <summary>
        /// 제작 아이템 무게
        /// </summary>
        public bool IsWeight()
        {
            // 교환 탭만 무게 체크
            if (MainTab != 4)
                return false;

            return Reward.Weight > 0;
        }

        public bool HasMaterial(int materialId)
        {
            if (MaterialInfos == null)
                return false;

            foreach (var item in MaterialInfos)
            {
                if (item.ItemId == materialId)
                    return true;
            }
            return false;
        }
    }
}