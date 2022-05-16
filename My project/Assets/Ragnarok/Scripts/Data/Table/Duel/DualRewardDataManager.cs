using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DuelRewardDataManager : Singleton<DuelRewardDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, DuelRewardData> dataDic;
        private readonly BetterList<DuelRewardData> eventDataList;
        private readonly BetterList<DuelRewardData> arenaDataList;

        public ResourceType DataType => ResourceType.DuelRewardDataDB;

        public DuelRewardDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, DuelRewardData>(ObscuredIntEqualityComparer.Default);
            eventDataList = new BetterList<DuelRewardData>();
            arenaDataList = new BetterList<DuelRewardData>();
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
            eventDataList.Clear();
            arenaDataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    DuelRewardData data = new DuelRewardData(mpo.AsList());

                    switch (data.check_type)
                    {
                        case DuelRewardData.STAGE_DUEL_TYPE:
                            dataDic.Add(data.id, data);
                            break;

                        case DuelRewardData.EVENT_DUEL_TYPE:
                            eventDataList.Add(data);
                            break;

                        case DuelRewardData.ARENA_DUEL_TYPE:
                            arenaDataList.Add(data);
                            break;
                    }
                }
            }
        }

        public DuelRewardData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"모험 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
        }

        public DuelRewardData[] GetDatas() { return dataDic.Values.ToArray(); }

        public DuelRewardData[] GetEventRewards()
        {
            return eventDataList.ToArray();
        }

        public DuelRewardData[] GetArenaRewards()
        {
            return arenaDataList.ToArray();
        }

        public void Initialize()
        {
        }

        public void VerifyData()
        {
            #region UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                if (item.reward_type.ToEnum<RewardType>() == RewardType.Item)
                {
                    if (ItemDataManager.Instance.Get(item.reward_value) == null)
                        throw new System.Exception($"47.듀얼보상 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value}");
                }
            }
            #endregion
        }
    }
}
