using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Ragnarok
{
    public sealed class RewardGroupDataManager : Singleton<RewardGroupDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, RewardGroupData> dataDic;

        public ResourceType DataType => ResourceType.RewardGroupDataDB;

        public RewardGroupDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, RewardGroupData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    RewardGroupData data = new RewardGroupData(mpo.AsList());
                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public RewardGroupData Get(int groupId, int conditionValue)
        {
            RewardGroupData data = dataDic.Values.FirstOrDefault(x => x.group_id == groupId && x.condition_value == conditionValue);

            if (data == null)
                Debug.LogError($"보상그룹 데이터가 존재하지 않습니다: [{nameof(groupId)}] {nameof(groupId)} = {groupId}, [{nameof(conditionValue)}] {nameof(conditionValue)} = {conditionValue}");
            return data;
        }

        public int GetRewardGroupId(ShopConditionType type, bool isFree)
        {
            switch (type)
            {
                case ShopConditionType.JobLevel:
                    return isFree ? BasisType.SHOP_REWARD_GROUP_ID.GetInt(3) : BasisType.SHOP_REWARD_GROUP_ID.GetInt(1);

                case ShopConditionType.Scenario:
                    return isFree ? BasisType.SHOP_REWARD_GROUP_ID.GetInt(4) : BasisType.SHOP_REWARD_GROUP_ID.GetInt(2);
            }
            return default;
        }

        public RewardGroupData[] Gets(int groupId)
        {
            RewardGroupData[] data = dataDic.Values.Where(x => x.group_id == groupId).ToArray();

            if (data == null)
                Debug.LogError($"보상그룹 데이터가 존재하지 않습니다: [{nameof(groupId)}] {nameof(groupId)} = {groupId}");
            return data;
        }

        public List<RewardGroupData> GetList(int groupId)
        {
            List<RewardGroupData> data = dataDic.Values.Where(x => x.group_id == groupId).ToList();

            if (data == null)
                Debug.LogError($"보상그룹 데이터가 존재하지 않습니다: [{nameof(groupId)}] {nameof(groupId)} = {groupId}");
            return data;
        }

        public RewardGroupData GetByGroupIndex(int groupId, int groupIndex)
        {
            return dataDic.Values.FirstOrDefault(x => x.group_id == groupId && x.group_index == groupIndex);
        }

        public RewardGroupData[] Gets(int groupId, int conditionValue)
        {
            RewardGroupData[] data = dataDic.Values.Where(x => x.group_id == groupId && x.condition_value == conditionValue).ToArray();

            if (data == null)
                Debug.LogError($"보상그룹 데이터가 존재하지 않습니다: [{nameof(groupId)}] {nameof(groupId)} = {groupId}");
            return data;
        }        

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                if (item.rewardData.RewardType == RewardType.Item)
                {
                    if (ItemDataManager.Instance.Get(item.rewardData.ItemId) == null)
                    {
                        throw new System.Exception($"26.보상그룹 테이블 오류 ID={item.id}, 없는 아이템={item.rewardData.ItemId}");
                    }
                }
            }
#endif
        }
    }
}
