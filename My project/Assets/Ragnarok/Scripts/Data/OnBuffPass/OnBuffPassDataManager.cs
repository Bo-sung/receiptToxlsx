using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class OnBuffPassDataManager : Singleton<OnBuffPassDataManager>, IDataManger, IPassDataRepoImpl
    {
        private struct Output : IPassLevel
        {
            private readonly int level;
            private readonly int curExp;
            private readonly int maxExp;

            public int Level => level;
            public int CurExp => curExp;
            public int MaxExp => maxExp;

            public Output(int level, int curExp, int maxExp)
            {
                this.level = level;
                this.curExp = curExp;
                this.maxExp = maxExp;
            }
        }

        private readonly Dictionary<int, OnBuffPassData> dataDic;
        private int lastPassLevel;
        private int lastPassExp;

        public ResourceType DataType => ResourceType.OnBuffPassDataDB;

        public OnBuffPassDataManager()
        {
            dataDic = new Dictionary<int, OnBuffPassData>(IntEqualityComparer.Default);
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
                    OnBuffPassData data = new OnBuffPassData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.Id))
                        dataDic.Add(data.PassLevel, data);
                }
            }
        }

        public IPassData Get(int level)
        {
            if (!dataDic.ContainsKey(level))
            {
                Debug.LogError($"온버프 패스 데이터가 존재하지 않습니다: level = {level}");
                return null;
            }

            return dataDic[level];
        }

        public IEnumerable<IPassData> GetEnumerable()
        {
            return dataDic.Values;
        }

        public IPassLevel GetLevel(int totalExp)
        {
            int level = 0;
            int curExp = 0;
            int maxExp = 0;

            int preNeedExp = 0;
            foreach (OnBuffPassData data in dataDic.Values)
            {
                level = data.PassLevel;
                curExp = totalExp - preNeedExp;
                maxExp = data.NeedExp - preNeedExp;

                if (totalExp < data.NeedExp)
                    return new Output(level, curExp, maxExp);

                preNeedExp = data.NeedExp;
            }

            return new Output(level, curExp, maxExp);
        }

        public int GetLastPassLevel()
        {
            return lastPassLevel;
        }

        public int GetLastPassExp()
        {
            return lastPassExp;
        }

        public void Initialize()
        {
            foreach (var item in dataDic.Values)
            {
                if (item.PassLevel > lastPassLevel)
                    lastPassLevel = item.PassLevel;
            }
            lastPassExp = dataDic[lastPassLevel].NeedExp - dataDic[lastPassLevel - 1].NeedExp;
        }

        public void VerifyData()
        {
        }
    }
}