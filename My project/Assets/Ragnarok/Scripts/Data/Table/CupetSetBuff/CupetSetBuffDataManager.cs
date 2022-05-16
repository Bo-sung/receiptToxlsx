using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using System.Linq;

namespace Ragnarok
{
    public sealed class CupetSetBuffDataManager : Singleton<CupetSetBuffDataManager>, IDataManger, IEqualityComparer<CupetSetBuffDataManager.Key>
    {
        private struct Key
        {
            private readonly ObscuredInt cupetId1;
            private readonly ObscuredInt cupetId2;
            private readonly ObscuredInt cupetId3;
            private readonly ObscuredInt cupetId4;

            public Key(ObscuredInt cupetId1, ObscuredInt cupetId2, ObscuredInt cupetId3, ObscuredInt cupetId4)
            {
                this.cupetId1 = cupetId1;
                this.cupetId2 = cupetId2;
                this.cupetId3 = cupetId3;
                this.cupetId4 = cupetId4;
            }

            public override bool Equals(object obj)
            {
                if (obj is Key)
                    return Equals((Key)obj);

                return false;
            }

            public override int GetHashCode()
            {
                int hash = 17;

                hash = hash * 29 + cupetId1.GetHashCode();
                hash = hash * 29 + cupetId2.GetHashCode();
                hash = hash * 29 + cupetId3.GetHashCode();
                hash = hash * 29 + cupetId4.GetHashCode();

                return hash;
            }

            public bool Equals(Key obj)
            {
                return cupetId1 == obj.cupetId1 && cupetId2 == obj.cupetId2 && cupetId3 == obj.cupetId3 && cupetId4 == obj.cupetId4;
            }
        }

        private readonly Dictionary<Key, CupetSetBuffData> dataDic;

        public ResourceType DataType => ResourceType.CupetSetBuffDataDB;

        public CupetSetBuffDataManager()
        {
            dataDic = new Dictionary<Key, CupetSetBuffData>();
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
                    CupetSetBuffData data = new CupetSetBuffData(mpo.AsList());
                    Key key = CreateKey(data.cupet_id_1, data.cupet_id_2, data.cupet_id_3, data.cupet_id_4);

                    if (dataDic.ContainsKey(key))
                        throw new System.ArgumentException($"중복된 데이터 입니다: {data.GetDump()}");

                    dataDic.Add(key, data);
                }
            }
        }

        public CupetSetBuffData Get(int[] cupetIds)
        {
            Key key = CreateKey(cupetIds);
            return dataDic.ContainsKey(key) ? dataDic[key] : null;
        }

        public CupetSetBuffData[] GetArray()
        {
            return dataDic.Values.ToArray();
        }

        private Key CreateKey(params int[] cupetIDs)
        {
            System.Array.Sort(cupetIDs, SortByCupetID);
            return new Key(cupetIDs[0], cupetIDs[1], cupetIDs[2], cupetIDs[3]);
        }

        private int SortByCupetID(int a, int b)
        {
            return a.CompareTo(b);
        }

        bool IEqualityComparer<Key>.Equals(Key x, Key y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Key>.GetHashCode(Key obj)
        {
            return obj.GetHashCode();
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