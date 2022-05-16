using MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class NabihoDataManager : Singleton<NabihoDataManager>, IDataManger
    {
        private readonly Dictionary<int, NabihoData> dataDic;
        private readonly BetterList<NabihoData> equipmentDataList;
        private readonly BetterList<NabihoData> boxDataList;
        private readonly BetterList<NabihoData> specialDataList;
        private int equipmentNeedLevel;
        private int boxNeedLevel;
        private int specialNeedLevel;

        public ResourceType DataType => ResourceType.NabihoDataDB;

        public NabihoDataManager()
        {
            dataDic = new Dictionary<int, NabihoData>(IntEqualityComparer.Default);
            equipmentDataList = new BetterList<NabihoData>();
            boxDataList = new BetterList<NabihoData>();
            specialDataList = new BetterList<NabihoData>();
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
            equipmentDataList.Clear();
            boxDataList.Clear();
            specialDataList.Clear();
            equipmentNeedLevel = -1;
            boxNeedLevel = -1;
            specialNeedLevel = -1;
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    NabihoData data = new NabihoData(mpo.AsList());
                    dataDic.Add(data.Id, data);

                    // SortNum 이 -1일 경우에는 포함시키지 않음
                    if (data.sort == -1)
                        continue;

                    switch (data.groupType)
                    {
                        case NabihoData.GROUP_EQUIPMENT:
                            equipmentDataList.Add(data);
                            break;

                        case NabihoData.GROUP_BOX:
                            boxDataList.Add(data);
                            break;

                        case NabihoData.GROUP_SPECIAL:
                            specialDataList.Add(data);
                            break;
                    }
                }
            }

            equipmentDataList.Sort(SortBySortNum);
            boxDataList.Sort(SortBySortNum);
            specialDataList.Sort(SortBySortNum);
        }

        public NabihoData Get(int id)
        {
            if (!dataDic.ContainsKey(id))
            {
                Debug.LogError($"91.나비호 데이터가 존재하지 않습니다: {nameof(id)} = {id}");
                return null;
            }

            return dataDic[id];
        }

        public int GetEquipmentNeedLevel()
        {
            return equipmentNeedLevel;
        }

        public int GetBoxNeedLevel()
        {
            return boxNeedLevel;
        }

        public int GetSpecialNeedLevel()
        {
            return specialNeedLevel;
        }

        public NabihoData[] GetEquipments()
        {
            return equipmentDataList.ToArray();
        }

        public NabihoData[] GetBoxes()
        {
            return boxDataList.ToArray();
        }

        public NabihoData[] GetSpecials()
        {
            return specialDataList.ToArray();
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            // Equipment
            if (equipmentDataList.size > 0)
            {
                int minLevel = int.MaxValue;
                for (int i = 0; i < equipmentDataList.size; i++)
                {
                    minLevel = Mathf.Min(minLevel, equipmentDataList[i].IntimacyCondition);
                }
                equipmentNeedLevel = minLevel;
            }

            // Box
            if (boxDataList.size > 0)
            {
                int minLevel = int.MaxValue;
                for (int i = 0; i < boxDataList.size; i++)
                {
                    minLevel = Mathf.Min(minLevel, boxDataList[i].IntimacyCondition);
                }
                boxNeedLevel = minLevel;
            }

            // Special
            if (boxDataList.size > 0)
            {
                int minLevel = int.MaxValue;
                for (int i = 0; i < specialDataList.size; i++)
                {
                    minLevel = Mathf.Min(minLevel, specialDataList[i].IntimacyCondition);
                }
                specialNeedLevel = minLevel;
            }
        }

        public void VerifyData()
        {
        }

        private int SortBySortNum(NabihoData a, NabihoData b)
        {
            return a.sort.CompareTo(b.sort);
        }
    }
}