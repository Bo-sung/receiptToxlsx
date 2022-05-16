using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;

namespace Ragnarok
{
    public class ShareDataManager : Singleton<ShareDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, ShareData> dataDic;

        public ResourceType DataType => ResourceType.ShareDataDB;

        public ShareDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, ShareData>(ObscuredIntEqualityComparer.Default);
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
                    ShareData data = new ShareData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.level))
                        dataDic.Add(data.level, data);
                }
            }
        }

        public ShareData Get(int level)
        {
            if (!dataDic.ContainsKey(level))
                throw new System.ArgumentException($"쉐어 데이터가 존재하지 않습니다: level = {level}");

            return dataDic[level];
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