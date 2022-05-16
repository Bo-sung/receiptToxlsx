using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public sealed class MakeDataManager : Singleton<MakeDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, MakeData> dataDic;

        public ResourceType DataType => ResourceType.MakeDataDB;

        public MakeDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, MakeData>(ObscuredIntEqualityComparer.Default);
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
                    MakeData data = new MakeData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public MakeData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
                throw new System.ArgumentException($"제작 데이터가 존재하지 않습니다: id = {id}");

            return dataDic[id];
        }

        public List<KeyValuePair<ObscuredInt, MakeData>> GetList()
        {
            return dataDic.OrderBy(x => x.Value.sort_index).ToList();
        }

        /// <summary>
        /// 특정 아이템을 결과로 내놓는 제작데이터 반환
        /// </summary>
        public MakeData GetMakeResultThisItem(int itemId)
        {
            foreach (MakeData makeData in dataDic.Values)
            {
                if (makeData.result == itemId)
                {
                    return makeData;
                }
            }
            return null;
        }

        /// <summary>
        /// 특정 아이템을 재료로 사용하는 제작 데이터 리스트 반환
        /// </summary>
        public MakeData[] GetMakesNeedThisItem(int itemId)
        {
            List<MakeData> makeDatas = new List<MakeData>();

            foreach (MakeData makeData in dataDic.Values)
            {
                if (IsMakeNeedThisItem(makeData, itemId))
                {
                    makeDatas.Add(makeData);
                }
            }

            return makeDatas.ToArray();
        }

        /// <summary>
        /// 해당 제작데이터가 특정 아이템을 재료로 사용하는지.
        /// </summary>
        private bool IsMakeNeedThisItem(MakeData makeData, int itemId)
        {
            if (makeData == null)
            {
                return false;
            }

            foreach (var item in makeData.needItems)
            {
                if (item.Item1 == itemId) // Item1 -> value_1
                {
                    return true;
                }
            }

            return false;
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
                if (item.enable_type != 0)
                {
                    if (ItemDataManager.Instance.Get(item.result) == null)
                    {
                        throw new System.Exception($"20.제작 테이블 오류 ID={item.id}, 없는 결과 아이템({nameof(item.result)})={item.result}");
                    }

                    if (item.enable_type == 2) // 실제 제작용
                    {
                        foreach (var needItem in item.needItems)
                        {
                            if (ItemDataManager.Instance.Get(needItem.Item1) == null)
                            {
                                throw new System.Exception($"20.제작 테이블 오류 ID={item.id}, 없는 재료 아이템={needItem.Item1}");
                            }
                        }
                    }
                }
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 아이템 재료 반환
        /// </summary>
        [System.Obsolete]
        public IEnumerable<int> GetItemMaterialId()
        {
            foreach (var item in dataDic.Values)
            {
                foreach (var item2 in item.needItems)
                {
                    yield return item2.Item1;
                }
            }
        }
#endif
    }
}