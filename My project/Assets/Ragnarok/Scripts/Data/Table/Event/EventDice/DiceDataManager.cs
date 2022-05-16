using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class DiceDataManager : Singleton<DiceDataManager>, IDataManger
    {
        private readonly Dictionary<int, DiceData> dataDic;

        public ResourceType DataType => ResourceType.DiceDataDB;

        public DiceDataManager()
        {
            dataDic = new Dictionary<int, DiceData>(IntEqualityComparer.Default);
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
                    DiceData data = new DiceData(mpo.AsList());
                    if (dataDic.ContainsKey(data.EventId))
                        continue;

                    dataDic.Add(data.EventId, data);
                }
            }
        }

        /// <summary>
        /// 정보 반환
        /// </summary>
        public DiceData Get(int eventId)
        {
            return dataDic.ContainsKey(eventId) ? dataDic[eventId] : null;
        }

        /// <summary>
        /// 이미지 정보 모두 반환
        /// </summary>
        public string[] GetImageNames()
        {
            HashSet<string> hashSet = new HashSet<string>(System.StringComparer.Ordinal);
            BetterList<string> list = new BetterList<string>();
            foreach (var item in dataDic.Values)
            {
                if (hashSet.Contains(item.ImageName))
                    continue;

                hashSet.Add(item.ImageName);
                list.Add(item.ImageName);
            }

            return list.ToArray();
        }

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