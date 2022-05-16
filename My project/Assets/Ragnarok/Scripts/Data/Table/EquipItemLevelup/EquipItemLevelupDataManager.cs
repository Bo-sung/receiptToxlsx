using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;

namespace Ragnarok
{
    public class EquipItemLevelupDataManager : Singleton<EquipItemLevelupDataManager>, IDataManger
    {
        private readonly Dictionary<ObscuredInt, EquipItemLevelupData> dataDic;

        public ResourceType DataType => ResourceType.EquipItemLevelupDataDB;

        public EquipItemLevelupDataManager()
        {
            dataDic = new Dictionary<ObscuredInt, EquipItemLevelupData>(ObscuredIntEqualityComparer.Default);
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
            using (ByteArrayUnpacker unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    EquipItemLevelupData data = new EquipItemLevelupData(mpo.AsList());

                    if (!dataDic.ContainsKey(data.id))
                        dataDic.Add(data.id, data);
                }
            }
        }

        public EquipItemLevelupData Get(int type, int rating, int smeltLevel)
        {
            return dataDic.Values.FirstOrDefault(x => x.type == type && x.rating == rating && x.smelt_level == smeltLevel);
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

#if UNITY_EDITOR
        /// <summary>
        /// 아이템 재료 반환
        /// </summary>
        [System.Obsolete]
        public IEnumerable<int> GetItemMaterialId()
        {
            int typeValue = (int)RewardType.Item;
            foreach (var item in dataDic.Values)
            {
                if (item.resource_type != typeValue)
                    continue;

                yield return item.resource_value;
            }
        }
#endif
    }
}