using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BingoDataManager : Singleton<BingoDataManager>, IDataManger
    {
        private Dictionary<ObscuredInt, List<BingoData>> groupToBingos;

        public ResourceType DataType => ResourceType.BingoDataDB;

        public BingoDataManager()
        {
            groupToBingos = new Dictionary<ObscuredInt, List<BingoData>>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            groupToBingos.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    BingoData data = new BingoData(mpo.AsList());

                    List<BingoData> list = null;
                    groupToBingos.TryGetValue(data.group_id, out list);

                    if (list == null)
                    {
                        list = new List<BingoData>(36);
                        groupToBingos.Add(data.group_id, list);
                    }

                    list.Add(data);
                }
            }
        }

        public IEnumerable<BingoData> Get(int group)
        {
            List<BingoData> list = null;
            groupToBingos.TryGetValue(group, out list);
            return list;
        }

        public IEnumerable<BingoData> ReadRespectToGroup()
        {
            foreach (var each in groupToBingos.Values)
                yield return each[0];
            yield break;
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
            foreach (var bingoDataList in groupToBingos.Values)
            {
                foreach (var item in bingoDataList.OrEmptyIfNull())
                {
                    if(item.reward_type.ToEnum<RewardType>() == RewardType.Item)
                    {
                        if(ItemDataManager.Instance.Get(item.reward_id) == null)
                            throw new System.Exception($"61.빙고 테이블 오류 ID={item.id}, 없는 아이템={item.reward_id}");
                    }
                }
            }
#endif
        }
    }
}
