using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class MvpRewardUIDataManager : Singleton<MvpRewardUIDataManager>, IDataManger
    {
        public ResourceType DataType => ResourceType.MvpRewardUIDataDB;

        private Dictionary<ObscuredInt, MvpRewardUIData> dataDic;
        private Dictionary<ObscuredInt, List<MvpRewardUIData>> chapterToDataDic;

        public MvpRewardUIDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, MvpRewardUIData>(ObscuredIntEqualityComparer.Default);
            chapterToDataDic = new Dictionary<ObscuredInt, List<MvpRewardUIData>>(ObscuredIntEqualityComparer.Default);
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
            chapterToDataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    MvpRewardUIData data = new MvpRewardUIData(mpo.AsList());
                    dataDic.Add(data.id, data);
                }
            }
        }

        public IEnumerable<MvpRewardUIData> Get(int chapterID)
        {
            List<MvpRewardUIData> ret = null;
            chapterToDataDic.TryGetValue(chapterID, out ret);
            return ret;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            foreach (var each in dataDic.Values)
            {
                if (!chapterToDataDic.ContainsKey(each.chapter))
                    chapterToDataDic.Add(each.chapter, new List<MvpRewardUIData>());

                var list = chapterToDataDic[each.chapter];
                list.Add(each);
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
