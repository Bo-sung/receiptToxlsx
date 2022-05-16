using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 언어 데이터
    /// </summary>
    public sealed class LanguageDataManager : Singleton<LanguageDataManager>, IDataManger
    {
        private const string LOCAL_FILE_NAME = "LanguageData";

        private readonly Dictionary<ObscuredInt, LanguageData> dataDic;
        private readonly Dictionary<ObscuredInt, LanguageData> dataLocalDic;

        public ResourceType DataType => ResourceType.LanguageDataDB;

        bool IsUseEditorLocalize
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(nameof(IsUseEditorLocalize), defaultValue: false);
#else
                return false;
#endif
            }
        }

        public LanguageDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, LanguageData>(ObscuredIntEqualityComparer.Default);
            dataLocalDic = new Dictionary<ObscuredInt, LanguageData>(ObscuredIntEqualityComparer.Default);
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();

            LocalReadData();
        }

        public void ClearData()
        {
            dataDic.Clear();
        }

        /// <summary>
        /// 로컬에 저장된 언어파일을 읽는다. 
        /// </summary>
        private void LocalReadData()
        {
            TextAsset asset = null;
            if (IsUseEditorLocalize)
            {
#if UNITY_EDITOR
                asset = (TextAsset)UnityEditor.EditorGUIUtility.Load($"{LOCAL_FILE_NAME}.bytes");
#endif
            }
            else
            {
                asset = Resources.Load<TextAsset>(LOCAL_FILE_NAME);
            }

            using (var unpack = Unpacker.Create(asset.bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    LanguageData data = new LanguageData(mpo.AsList());

                    if (!dataLocalDic.ContainsKey(data.id))
                        dataLocalDic.Add(data.id, data);
                }
            }
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    LanguageData data = new LanguageData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        [System.Obsolete("Use id.ToText()")]
        public string GetString(int id)
        {
            return GetString(id, Language.Current);
        }

        [System.Obsolete("Use id.ToText(LanguageType)")]
        public string GetString(int id, LanguageType languageType)
        {
            if (id == 0)
                return string.Empty;

            string text = null;
            bool hasKey = false;

            // 로컬 데이터
            if (dataLocalDic.ContainsKey(id))
            {
                hasKey = true;
                text = dataLocalDic[id].GetString(languageType);
            }

            if (IsUseEditorLocalize)
            {
                // Do Nothing
            }
            else
            {
                // 데이터 덮어쓰기
                if (dataDic.ContainsKey(id))
                {
                    hasKey = true;
                    text = dataDic[id].GetString(languageType);
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                if (hasKey)
                {
                    // 데이터 자체의 공백일 경우에는 공백 return
                    return text;
                }

                Debug.LogError($"언어 데이터가 존재하지 않습니다: id = {id}");
                return string.Concat("#Local:", id);
            }

            return languageType == LanguageType.THAILAND ? ThaiFontAdjuster.Adjust(text) : text;
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