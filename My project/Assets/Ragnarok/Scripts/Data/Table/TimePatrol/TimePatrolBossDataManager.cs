using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TimePatrolBossDataManager : Singleton<TimePatrolBossDataManager>, IDataManger
    {
        private readonly Dictionary<int, TimePatrolBossData> dataDic;

        public ResourceType DataType => ResourceType.TimePatrolBossDataDB;

        public TimePatrolBossDataManager()
        {
            dataDic = new Dictionary<int, TimePatrolBossData>(IntEqualityComparer.Default);
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
                    TimePatrolBossData data = new TimePatrolBossData(mpo.AsList());

                    if (dataDic.ContainsKey(data.id))
                        continue;

                    dataDic.Add(data.id, data);
                }
            }
        }

        public TimePatrolBossData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"81.TP보스테이블 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
        }

        public int[] GetRewrads(int level)
        {
            int count = 0;
            Buffer<int> list = new Buffer<int>();
            foreach (var item in dataDic.Values)
            {
                if (item.level == level)
                {
                    count++;

                    if (item.boss_drop > 0)
                    {
                        if (!list.Contains(item.boss_drop))
                        {
                            list.Add(item.boss_drop);
                        }
                    }
                }

                if (count == 6)
                    break;
            }
            return list.GetBuffer(isAutoRelease: true);
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