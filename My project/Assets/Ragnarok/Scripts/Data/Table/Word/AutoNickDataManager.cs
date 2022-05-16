using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class AutoNickDataManager : Singleton<AutoNickDataManager>, IDataManger
    {
        private readonly Dictionary<int, BetterList<string>> listDic;

        public ResourceType DataType => ResourceType.AutoNickDataDB;

        public AutoNickDataManager()
        {
            listDic = new Dictionary<int, BetterList<string>>();
        }

        protected override void OnTitle()
        {
            if (IntroScene.IsBackToTitle)
                return;

            ClearData();
        }

        public void ClearData()
        {
            listDic.Clear();
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    AutoNickData data = new AutoNickData(mpo.AsList());
                    int languageType = data.language_type;
                    if (!listDic.ContainsKey(languageType))
                        listDic.Add(languageType, new BetterList<string>());

                    listDic[languageType].Add(data.word);
                }
            }
        }

        /// <summary>
        /// 랜덤 닉네임
        /// </summary>
        public string RandomPop()
        {
            return RandomPop(Language.Current);
        }

        public string RandomPop(LanguageType languageType)
        {
            int languageTypeValue = (int)languageType;
        
            if (listDic.ContainsKey(languageTypeValue))
            {
                int size = listDic[languageTypeValue].size;
                return size > 0 ? listDic[languageTypeValue][Random.Range(0, size)] : string.Empty;
            }

            return string.Empty;
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
            var sb = new System.Text.StringBuilder();

            FilterNickDataManager filterNickDataRepo = FilterNickDataManager.Instance;
            foreach (var item in listDic.Values)
            {
                for (int i = 0; i < item.size; i++)
                {
                    if (!filterNickDataRepo.Contains(item[i]))
                        continue;

                    sb.AppendLine();
                    sb.Append("(").Append(i + 1).Append(") ").Append(item[i]);
                }
            }

            if (sb.Length > 0)
            {
                int resourceIndex = (int)DataType + 1;
                throw new System.Exception($"{resourceIndex}.{DataType} Error ({"금지 이름 존재"}) {sb.ToString()}");
            }
#endif
        }
    }
}