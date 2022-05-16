using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class BasisDataManager : Singleton<BasisDataManager>, IDataManger
    {
        private readonly Dictionary<int, ObscuredBool> boolDic;
        private readonly Dictionary<int, ObscuredInt> intDic;
        private readonly Dictionary<int, ObscuredFloat> floatDic;
        private readonly Dictionary<int, ObscuredString> stringDic;

        public ResourceType DataType => ResourceType.BasisDataDB;

        public BasisDataManager()
        {
            boolDic = new Dictionary<int, ObscuredBool>(IntEqualityComparer.Default);
            intDic = new Dictionary<int, ObscuredInt>(IntEqualityComparer.Default);
            floatDic = new Dictionary<int, ObscuredFloat>(IntEqualityComparer.Default);
            stringDic = new Dictionary<int, ObscuredString>(IntEqualityComparer.Default);
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
                    BasisData basisData = new BasisData(mpo.AsList());

                    switch (basisData.unit)
                    {
                        case DataUnit.Boolean:
                            boolDic.Add(basisData.id, basisData.data != "0");
                            break;

                        case DataUnit.Integer:
                            intDic.Add(basisData.id, int.Parse(basisData.data));
                            break;

                        case DataUnit.Percent:
                            floatDic.Add(basisData.id, MathUtils.ToPercentValue(int.Parse(basisData.data)));
                            break;

                        case DataUnit.PerMille:
                            int permilleValue = int.Parse(basisData.data);
                            floatDic.Add(basisData.id, MathUtils.ToPermilleValue(permilleValue));
                            break;

                        case DataUnit.PerTenThousand:
                            int permyriadValue = int.Parse(basisData.data);
                            floatDic.Add(basisData.id, MathUtils.ToPermyriadValue(permyriadValue));
                            break;

                        case DataUnit.String:
                            stringDic.Add(basisData.id, basisData.data);
                            break;
                    }
                }
            }
        }

        [System.Obsolete("Use BasisType.GetBool")]
        public bool GetBool(int id)
        {
            if (boolDic.ContainsKey(id))
                return boolDic[id];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}");
#endif
            return false;
        }

        [System.Obsolete("Use BasisType.GetInt")]
        public int GetInt(int id)
        {
            if (intDic.ContainsKey(id))
                return intDic[id];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}");
#endif
            return 0;
        }

        [System.Obsolete("Use BasisType.GetString")]
        public string GetString(int id)
        {
            if (stringDic.ContainsKey(id))
                return stringDic[id];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}");
#endif
            return string.Empty;
        }

        [System.Obsolete("Use BasisType.GetFloat")]
        public float GetFloat(int id)
        {
            if (floatDic.ContainsKey(id))
                return floatDic[id];

#if UNITY_EDITOR
            Debug.LogError($"기초 데이터 없거나 DataUnit이 다른 id = {id}");
#endif
            return 0F;
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            Constants.Battle.REGEN_MP_DELAY = Mathf.Max(0f, MathUtils.ToPermyriadValue(BasisType.REGEN_MP_DELAY.GetInt()));
        }

        public void VerifyData()
        {
#if UNITY_EDITOR

#endif
        }
    }
}