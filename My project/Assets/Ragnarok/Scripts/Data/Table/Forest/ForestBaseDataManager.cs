using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ForestBaseDataManager : Singleton<ForestBaseDataManager>, IDataManger
    {
        private readonly BetterList<int> groupIdList;
        private readonly Dictionary<ObscuredInt, ForestBaseData[]> dataDic; // Key: groupId

        public ResourceType DataType => ResourceType.ForestBaseDataDB;

        public ForestBaseDataManager()
        {
            groupIdList = new BetterList<int>();
            dataDic = new Dictionary<ObscuredInt, ForestBaseData[]>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            groupIdList.Release();
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            List<ForestBaseData> tempDataList = new List<ForestBaseData>();
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ForestBaseData data = new ForestBaseData(mpo.AsList());
                    tempDataList.Add(data);

                    int groupId = data.group_id;
                    if (groupIdList.Contains(groupId))
                        continue;

                    groupIdList.Add(groupId);
                }
            }

            for (int i = 0; i < groupIdList.size; i++)
            {
                int groupId = groupIdList[i];
                dataDic.Add(groupId, tempDataList.FindAll(a => a.group_id == groupId).ToArray());
            }
        }

        public ForestBaseData GetFirstData()
        {
            int[] groupIds = GetGroupIds();
            int groupId = (groupIds == null || groupIds.Length == 0) ? 0 : groupIds[0];
            ForestBaseData[] arrData = Get(groupId);
            return (arrData == null || arrData.Length == 0) ? null : arrData[0];
        }

        public int[] GetGroupIds()
        {
            return groupIdList.ToArray();
        }

        public ForestBaseData[] Get(int groupId)
        {
            if (!dataDic.ContainsKey(groupId))
            {
                Debug.LogError($"77.미궁숲 데이터가 존재하지 않습니다: {nameof(groupId)} = {groupId}");
                return null;
            }

            return dataDic[groupId];
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

#endif
        }
    }
}