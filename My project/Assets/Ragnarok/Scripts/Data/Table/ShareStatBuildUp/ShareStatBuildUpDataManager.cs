using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class ShareStatBuildUpDataManager : Singleton<ShareStatBuildUpDataManager>, IDataManger
    {
        private readonly Dictionary<int, DataGroup<ShareStatBuildUpData>> dataDic;
        private readonly Buffer<DataGroup<ShareStatBuildUpData>> buffer;

        public ResourceType DataType => ResourceType.ShareStatBuildUpDataDB;

        public ShareStatBuildUpDataManager()
        {
            dataDic = new Dictionary<int, DataGroup<ShareStatBuildUpData>>(IntEqualityComparer.Default);
            buffer = new Buffer<DataGroup<ShareStatBuildUpData>>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            foreach (var item in dataDic.Values)
            {
                item.Release();
            }

            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    ShareStatBuildUpData data = new ShareStatBuildUpData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.group))
                        dataDic.Add(data.group, new DataGroup<ShareStatBuildUpData>());

                    dataDic[data.group].Add(data);
                }
            }
        }

        public DataGroup<ShareStatBuildUpData>[] GetData()
        {
            foreach (var item in dataDic.Values)
            {
                buffer.Add(item);
            }

            return buffer.GetBuffer(isAutoRelease: true);
        }

        public DataGroup<ShareStatBuildUpData> Get(int group)
        {
            return dataDic.ContainsKey(group) ? dataDic[group] : null;
        }

        public ShareStatBuildUpData Get(int group, int level)
        {
            DataGroup<ShareStatBuildUpData> dataGroup = Get(group);
            if (dataGroup == null)
                return null;

            foreach (var item in dataGroup)
            {
                if (item.stat_lv == level)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            foreach (var item in dataDic.Values)
            {
                item.Initialize();
            }
        }

        /// <summary>
        /// 데이터 검증
        /// </summary>
        public void VerifyData()
        {
        }
    }
}