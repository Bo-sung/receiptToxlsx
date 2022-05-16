using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CupetDataManager : Singleton<CupetDataManager>, IDataManger, IEqualityComparer<CupetDataManager.Key>
    {
        private struct Key
        {
            private readonly ObscuredInt id;
            private readonly ObscuredInt rate;

            public Key(ObscuredInt id, ObscuredInt rate)
            {
                this.id = id;
                this.rate = rate;
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

                hash = hash * 29 + id.GetHashCode();
                hash = hash * 29 + rate.GetHashCode();

                return hash;
            }

            public bool Equals(Key obj)
            {
                return id == obj.id && rate == obj.rate;
            }
        }

        private readonly HashSet<ObscuredInt> cupetIdHashSet;
        private readonly Dictionary<Key, CupetData> dataDic;

        public ResourceType DataType => ResourceType.CupetDataDB;

        public CupetDataManager()
        {
            cupetIdHashSet = new HashSet<ObscuredInt>(ObscuredIntEqualityComparer.Default);
            dataDic = new Dictionary<Key, CupetData>(this);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            cupetIdHashSet.Clear();
            dataDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    CupetData data = new CupetData(mpo.AsList());
                    Key key = CreateKey(data.id, data.cupet_rating);

                    if (dataDic.ContainsKey(key))
                        throw new System.ArgumentException($"중복된 데이터 입니다: {data.GetDump()}");

                    cupetIdHashSet.Add(data.id);
                    dataDic.Add(key, data);
                }
            }
        }

        /// <summary>
        /// 큐펫 정보
        /// </summary>
        /// <param name="id"></param>
        /// <param name="rating">등급 1~5</param>
        public CupetData Get(int id, int rating = 1)
        {
            if (rating == 0)
                return null;

            Key key = CreateKey(id, rating);

            if (!dataDic.ContainsKey(key))
            {
                Debug.LogError($"큐펫 데이터가 존재하지 않습니다: id = {id}, rating = {rating}");
                return null;
            }

            return dataDic[key];
        }

        /// <summary>
        /// 큐펫 아이디 목록
        /// </summary>
        public IEnumerable<ObscuredInt> GetCupetIDs()
        {
            return cupetIdHashSet;
        }

        private Key CreateKey(int id, int rating)
        {
            return new Key(id, rating);
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