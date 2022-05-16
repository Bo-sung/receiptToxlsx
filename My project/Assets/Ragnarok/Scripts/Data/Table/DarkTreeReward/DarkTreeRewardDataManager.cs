using MsgPack;
using UnityEngine;

namespace Ragnarok
{
    public class DarkTreeRewardDataManager : Singleton<DarkTreeRewardDataManager>, IDataManger
    {
        private readonly BetterList<DarkTreeRewardData> dataList;

        public ResourceType DataType => ResourceType.DarkTreeRewardDB;

        public DarkTreeRewardDataManager()
        {
            dataList = new BetterList<DarkTreeRewardData>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            dataList.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    DarkTreeRewardData data = new DarkTreeRewardData(mpo.AsList());
                    dataList.Add(data);
                }
            }
        }

        public DarkTreeRewardData[] GetArrayData()
        {
            return dataList.ToArray();
        }

        public DarkTreeRewardData Get(int id)
        {
            if (id == 0)
                return null;

            for (int i = 0; i < dataList.size; i++)
            {
                if (dataList[i].Id == id)
                    return dataList[i];
            }

            Debug.LogError($"76.어둠의 나무 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
            return null;
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