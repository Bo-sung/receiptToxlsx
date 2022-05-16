using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class RandomTableDataManager : Singleton<RandomTableDataManager>, IDataManger, IRandomDamage
    {
        private readonly List<ObscuredInt> dataList;

        public ResourceType DataType => ResourceType.RandomTableDataDB;

        public RandomTableDataManager()
        {
            dataList = new List<ObscuredInt>();
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
                    dataList.Add(mpo.AsInt32());
                }
            }
        }

        // 랜덤 인덱스 값을 읽어 온다.
        public int GetRawValue(int iRandomIndex)
        {
            if (dataList.Count > iRandomIndex)
            {
                return dataList[iRandomIndex];
            }
            else
            {
                return dataList[iRandomIndex % dataList.Count];
            }
        }

        public int Get(int iRandomIndex, int begin, int end)
        {
            if (begin < end)
            {
                return (int)(GetRawValue(iRandomIndex) % (end - begin + 1)) + begin;
            }
            else
            {
                return begin;
            }
        }

        int IRandomDamage.GetRandomSeq()
        {
            return UnityEngine.Random.Range(0, dataList.Count);
        }

        int IRandomDamage.GetRandomRange(int seq, int min, int max)
        {
            return Get(seq, min, max);
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