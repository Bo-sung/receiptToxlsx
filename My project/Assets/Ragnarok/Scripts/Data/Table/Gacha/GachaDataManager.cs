using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public sealed class GachaDataManager : Singleton<GachaDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, GachaData> dataDic;

        public ResourceType DataType => ResourceType.GachaDataDB;

        public GachaDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, GachaData>(ObscuredIntEqualityComparer.Default);
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
                    GachaData data = new GachaData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public GachaData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
                throw new System.ArgumentException($"가챠 데이터가 존재하지 않습니다: id = {id}");

            return dataDic[id];
        }

        public GachaData[] Gets(int groupId)
        {
            return dataDic.Values.Where(x => x.group_type == 1 && x.group_id == groupId).ToArray();
        }

        public GachaData[] Gets(GroupType type, int groupId)
        {
            return dataDic.Values.Where(x => x.group_type == (byte)type && x.group_id == groupId).ToArray();
        }

        /// <summary>
        /// 특정 아이템을 주는 가챠 데이터 리스트 반환
        /// </summary>
        public GachaData[] GetGachasDropItem(int itemId)
        {
            List<GachaData> gachaList = new List<GachaData>();

            foreach (var gacha in dataDic.Values)
            {
                if (gacha.reward_type.ToEnum<RewardType>() != RewardType.Item)
                    continue;

                if (gacha.reward_value == itemId)
                {
                    gachaList.Add(gacha);
                }
            }

            return gachaList.ToArray();
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
                if (item.group_type == 6 || item.group_type == 7 || item.group_type == 8) // 6:동료 가챠 - item id 대신 동료 id 참조, 7:전투 동료 합성 - item id 대신 동료 id 참조, 8: 파견 동료 합성
                {
                    if (AgentDataManager.Instance.Get(item.reward_value) == null)
                    {
                        throw new System.Exception($"24.가챠 테이블 오류 ID={item.id}, 없는 동료={item.reward_value}");
                    }
                }
                else
                {
                    if (item.group_type == 3 && item.reward_type.ToEnum<RewardType>() != RewardType.Item) // 3: 재료 나무
                    {
                        throw new System.Exception($"24.가챠 테이블 오류 ID={item.id}, 재료 나무에 아이템 타입이 아님={item.reward_value}");
                    }

                    if (item.reward_type.ToEnum<RewardType>() != RewardType.Item)
                        continue;

                    if (ItemDataManager.Instance.Get(item.reward_value) == null)
                    {
                        throw new System.Exception($"24.가챠 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value}");
                    }
                }
            }
#endif
        }
    }
}
