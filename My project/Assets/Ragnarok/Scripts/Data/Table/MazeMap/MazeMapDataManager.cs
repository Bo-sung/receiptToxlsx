using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class MazeMapDataManager : Singleton<MazeMapDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, MazeMapData> dataDic;

        public ResourceType DataType => ResourceType.MazeMapDataDB;

        public MazeMapDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, MazeMapData>(ObscuredIntEqualityComparer.Default);
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
                    MazeMapData data = new MazeMapData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public MazeMapData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
                throw new System.ArgumentException($"미로맵 데이터가 존재하지 않습니다: id = {id}");

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

#endif
        }
    }
}
