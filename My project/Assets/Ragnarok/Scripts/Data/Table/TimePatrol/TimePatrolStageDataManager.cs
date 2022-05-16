using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TimePatrolStageDataManager : Singleton<TimePatrolStageDataManager>, IDataManger
    {
        private readonly Dictionary<int, TimePatrolStageData> dataDic;

        public ResourceType DataType => ResourceType.TimePatrolStageDataDB;

        public TimePatrolStageDataManager()
        {
            dataDic = new Dictionary<int, TimePatrolStageData>(IntEqualityComparer.Default);
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
                    TimePatrolStageData data = new TimePatrolStageData(mpo.AsList());

                    if (dataDic.ContainsKey(data.id))
                        continue;

                    dataDic.Add(data.id, data);
                }
            }
        }

        public TimePatrolStageData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"80.TP스테이지 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
        }

        public TimePatrolStageData Get(int level, int zoneId)
        {
            foreach (var item in dataDic.Values)
            {
                if (item.level == level && item.zone_id == zoneId)
                    return item;
            }
            return null;
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

                    if (item.normal_drop_1 > 0)
                    {
                        if (!list.Contains(item.normal_drop_1))
                        {
                            list.Add(item.normal_drop_1);
                        }
                    }

                    if (item.normal_drop_2 > 0)
                    {
                        if (!list.Contains(item.normal_drop_2))
                        {
                            list.Add(item.normal_drop_2);
                        }
                    }

                    if (item.normal_drop_3 > 0)
                    {
                        if (!list.Contains(item.normal_drop_3))
                        {
                            list.Add(item.normal_drop_3);
                        }
                    }

                    if (item.normal_drop_4 > 0)
                    {
                        if (!list.Contains(item.normal_drop_4))
                        {
                            list.Add(item.normal_drop_4);
                        }
                    }
                }

                if (count == 6)
                    break;
            }
            return list.GetBuffer(isAutoRelease: true);
        }

        public bool IsExists(int id)
        {
            return dataDic.ContainsKey(id);
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