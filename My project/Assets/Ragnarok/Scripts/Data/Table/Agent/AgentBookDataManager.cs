using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class AgentBookDataManager : Singleton<AgentBookDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, AgentBookData> dataDic;

        public ResourceType DataType => ResourceType.AgentBookDataDB;

        public AgentBookDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, AgentBookData>(ObscuredIntEqualityComparer.Default);
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

        public IEnumerable<AgentBookData> GetWholeBookDatas() { return dataDic.Values; }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    AgentBookData data = new AgentBookData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public AgentBookData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"동료세트효과 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
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
                if (item.reward_type == 0)
                    continue;

                if (item.reward_type == 6) // 아이템
                {
                    if (ItemDataManager.Instance.Get(item.reward_value) == null)
                    {
                        throw new System.Exception($"044.동료세트효과테이블 오류 ID={item.id}, 없는 아이템={item.reward_value}");
                    }
                    if (item.reward_count == 0)
                    {
                        throw new System.Exception($"044.동료세트효과테이블 오류 ID={item.id}, 보상 수량={item.reward_count}");
                    }
                }
                else
                {
                    if (item.reward_value == 0)
                    {
                        throw new System.Exception($"044.동료세트효과테이블 오류 ID={item.id}, {nameof(item.reward_type)}={item.reward_type}, 보상 수량={item.reward_value}");
                    }
                }
            }
#endif
        }
    }
}