using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class CentralLabMonsterDataManager : Singleton<CentralLabMonsterDataManager>, IDataManger
    {
        private class MonsterGroup : BetterList<BetterList<CentralLabMonsterData>>
        {
            public void Add(CentralLabMonsterData data)
            {
                int wave = data.stage_no;
                while (size < wave)
                {
                    Add(new BetterList<CentralLabMonsterData>());
                }

                base[wave - 1].Add(data);
            }
        }

        private readonly Dictionary<ObscuredInt, MonsterGroup> dataDic;
        private readonly Buffer<MonsterType> buffer;

        public ResourceType DataType => ResourceType.CLabMonsterDataDB;

        public CentralLabMonsterDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, MonsterGroup>(ObscuredIntEqualityComparer.Default);
            buffer = new Buffer<MonsterType>();
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
                    CentralLabMonsterData data = new CentralLabMonsterData(mpo.AsList());

                    int labId = data.lab_id;
                    if (!dataDic.ContainsKey(labId))
                        dataDic.Add(labId, new MonsterGroup());

                    dataDic[labId].Add(data);
                }
            }
        }

        public int GetMaxStage(int labId)
        {
            return dataDic.ContainsKey(labId) ? dataDic[labId].size : 0;
        }

        public MonsterType[] GetWaveInfo(int labId)
        {
            if (dataDic.ContainsKey(labId))
            {
                foreach (var item in dataDic[labId])
                {
                    buffer.Add(GetMonsterType(item));
                }
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        public CentralLabMonsterData[] GetArray(int labId, int waveIndex)
        {
            return waveIndex < GetMaxStage(labId) ? dataDic[labId][waveIndex].ToArray() : null;
        }

        private MonsterType GetMonsterType(BetterList<CentralLabMonsterData> list)
        {
            const int BOSS_MONSTER_TYPE = (int)MonsterType.Boss;
            foreach (var item in list)
            {
                if (item.monster_type == BOSS_MONSTER_TYPE)
                    return MonsterType.Boss;
            }

            return MonsterType.Normal;
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