using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class EventLoginBonusDataManager : Singleton<EventLoginBonusDataManager>, IDataManger
    {
        private const int ATTEND_EVENT_GROUP_ID = 999; // 출석 체크 이벤트 그룹 ID

        public ResourceType DataType => ResourceType.EventLoginBonusDataDB;

        private readonly Dictionary<ObscuredInt, List<EventLoginBonusData>> dataDic;

        public int MaxAttendEventDay { get; private set; } // 14일 출석체크 출석 최대값

        public EventLoginBonusDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, List<EventLoginBonusData>>(ObscuredIntEqualityComparer.Default);
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

        public List<EventLoginBonusData> Get(int group)
        {
            if (dataDic.TryGetValue(group, out List<EventLoginBonusData> list))
                return list;
            return null;
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    EventLoginBonusData data = new EventLoginBonusData(mpo.AsList());

                    List<EventLoginBonusData> list = null;

                    if (!dataDic.TryGetValue(data.group_no, out list))
                    {
                        list = new List<EventLoginBonusData>();
                        dataDic.Add(data.group_no, list);
                    }

                    list.Add(data);
                }
            }

            foreach(var each in dataDic.Values)
                each.Sort((a, b) => a.day - b.day);
        }

        public void Initialize()
        {
            var data = Get(ATTEND_EVENT_GROUP_ID);
            if (data != null)
                MaxAttendEventDay = data.Count;
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var eventLoginBonusDataList in dataDic.Values)
            {
                foreach (var item in eventLoginBonusDataList)
                {
                    if (item.reward_type.ToEnum<RewardType>() == RewardType.Item)
                    {
                        if (ItemDataManager.Instance.Get(item.reward_value) == null)
                            throw new System.Exception($"50.특별로그인보너스 테이블 오류 ID={item.id}, 없는 아이템={item.reward_value}");
                    }
                }                
            }
#endif
        }
    }
}
