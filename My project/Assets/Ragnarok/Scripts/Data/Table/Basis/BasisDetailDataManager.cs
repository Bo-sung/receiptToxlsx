using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BasisDetailDataManager : Singleton<BasisDetailDataManager>, IDataManger
    {
        private readonly Dictionary<int, Dictionary<int, ObscuredBool>> boolDic;
        private readonly Dictionary<int, Dictionary<int, ObscuredInt>> intDic;
        private readonly Dictionary<int, Dictionary<int, ObscuredFloat>> floatDic;
        private readonly Dictionary<int, Dictionary<int, ObscuredString>> stringDic;

        public ResourceType DataType => ResourceType.BasisDetailDataDB;

        public BasisDetailDataManager()
        {
            boolDic = new Dictionary<int, Dictionary<int, ObscuredBool>>(IntEqualityComparer.Default);
            intDic = new Dictionary<int, Dictionary<int, ObscuredInt>>(IntEqualityComparer.Default);
            floatDic = new Dictionary<int, Dictionary<int, ObscuredFloat>>(IntEqualityComparer.Default);
            stringDic = new Dictionary<int, Dictionary<int, ObscuredString>>(IntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            boolDic.Clear();
            intDic.Clear();
            floatDic.Clear();
            stringDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    var result = mpo.AsList();

                    BasisData basisData = new BasisData(result, true);

                    switch (basisData.unit)
                    {
                        case DataUnit.Boolean:
                            if (!boolDic.ContainsKey(basisData.id))
                                boolDic.Add(basisData.id, new Dictionary<int, ObscuredBool>(IntEqualityComparer.Default));

                            boolDic[basisData.id].Add(basisData.seq, basisData.data != "0");
                            break;

                        case DataUnit.Integer:
                            if (!intDic.ContainsKey(basisData.id))
                                intDic.Add(basisData.id, new Dictionary<int, ObscuredInt>(IntEqualityComparer.Default));

                            intDic[basisData.id].Add(basisData.seq, int.Parse(basisData.data));
                            break;

                        case DataUnit.Percent:
                            if (!floatDic.ContainsKey(basisData.id))
                                floatDic.Add(basisData.id, new Dictionary<int, ObscuredFloat>(IntEqualityComparer.Default));

                            floatDic[basisData.id].Add(basisData.seq, MathUtils.ToPercentValue(int.Parse(basisData.data)));
                            break;

                        case DataUnit.PerMille:
                            if (!floatDic.ContainsKey(basisData.id))
                                floatDic.Add(basisData.id, new Dictionary<int, ObscuredFloat>(IntEqualityComparer.Default));

                            floatDic[basisData.id].Add(basisData.seq, MathUtils.ToPermilleValue(int.Parse(basisData.data)));
                            break;

                        case DataUnit.PerTenThousand:
                            if (!floatDic.ContainsKey(basisData.id))
                                floatDic.Add(basisData.id, new Dictionary<int, ObscuredFloat>(IntEqualityComparer.Default));

                            floatDic[basisData.id].Add(basisData.seq, MathUtils.ToPermyriadValue(int.Parse(basisData.data)));
                            break;

                        case DataUnit.String:
                            if (!stringDic.ContainsKey(basisData.id))
                                stringDic.Add(basisData.id, new Dictionary<int, ObscuredString>(IntEqualityComparer.Default));

                            stringDic[basisData.id].Add(basisData.seq, basisData.data);
                            break;
                    }
                }
            }
        }

        [System.Obsolete("Use BasisType.GetBool")]
        public bool GetBool(int id, int seq)
        {
            if (boolDic.ContainsKey(id) && boolDic[id].ContainsKey(seq))
                return boolDic[id][seq];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}, seq = {seq}");
#endif
            return false;
        }

        [System.Obsolete("Use BasisType.GetInt")]
        public int GetInt(int id, int seq)
        {
            if (intDic.ContainsKey(id) && intDic[id].ContainsKey(seq))
                return intDic[id][seq];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}, seq = {seq}");
#endif
            return 0;
        }

        [System.Obsolete("Use BasisType.GetString")]
        public string GetString(int id, int seq)
        {
            if (stringDic.ContainsKey(id) && stringDic[id].ContainsKey(seq))
                return stringDic[id][seq];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}, seq = {seq}");
#endif
            return string.Empty;
        }

        [System.Obsolete("Use BasisType.GetFloat")]
        public float GetFloat(int id, int seq)
        {
            if (floatDic.ContainsKey(id) && floatDic[id].ContainsKey(seq))
                return floatDic[id][seq];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}, seq = {seq}");
#endif
            return 0F;
        }

        public List<int> GetKeyList(int id)
        {
            if (boolDic.ContainsKey(id))
                return new List<int>(boolDic[id].Keys);

            if (intDic.ContainsKey(id))
                return new List<int>(intDic[id].Keys);

            if (floatDic.ContainsKey(id))
                return new List<int>(floatDic[id].Keys);

            if (stringDic.ContainsKey(id))
                return new List<int>(stringDic[id].Keys);

            return new List<int>();
        }

        public BasisUrl GetItemDescUrl(int itemId)
        {
            int id = BasisType.REF_ITEM_DESC_URL.Key;
            if (intDic.ContainsKey(id) && intDic[id].ContainsKey(itemId))
                return intDic[id][itemId].ToEnum<BasisUrl>();

            return default;
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