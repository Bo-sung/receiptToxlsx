using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class GateDataManager : Singleton<GateDataManager>, IDataManger
    {
        public ResourceType DataType => ResourceType.GateDataDB;

        private readonly Dictionary<int, GateData> dataDic;
        public GateData First { get; private set; }

        public GateDataManager()
        {
            dataDic = new Dictionary<int, GateData>(IntEqualityComparer.Default);
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
            First = null;
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    GateData data = new GateData(mpo.AsList());

                    if (First == null)
                        First = data;

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public GateData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"게이트 데이터가 존재하지 않습니다: id = {id}");
                return null;
            }

            return dataDic[id];
        }

        public void Initialize()
        {
            ForestMonDataManager forestMonDataRepo = ForestMonDataManager.Instance;

            foreach (GateData item in dataDic.Values)
            {
                item.SetBoss(forestMonDataRepo.GetBossMonster(item.monster_group)); // 보스 세팅
            }
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
#endif
        }
    }
}