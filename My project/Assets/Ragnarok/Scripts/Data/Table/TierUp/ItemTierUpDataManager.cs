using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ItemTierUpDataManager : Singleton<ItemTierUpDataManager>, IDataManger
    {
        private Dictionary<(ObscuredInt, ObscuredInt), TierUpData> dataDic;

        public ResourceType DataType => ResourceType.ItemTierUpDataDB;

        public ItemTierUpDataManager()
        {
            dataDic = new Dictionary<(ObscuredInt, ObscuredInt), TierUpData>();
        }

        public void Initialize()
        {
        }

        public void LoadData(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                while (unpack.ReadObject(out MessagePackObject mpo))
                {
                    TierUpData data = new TierUpData(mpo.AsList());
                    dataDic.Add((data.tier, data.class_bit_type), data);
                }
            }
        }

        public void VerifyData()
        {
#if UNITY_EDITOR
            foreach (var item in dataDic.Values)
            {
                int nextTier = item.tier + 1;
                int needJobLevel = BasisType.ITEM_TRANSCEND_JOB_LEVEL.GetInt(nextTier);
                if (needJobLevel == 0)
                {
                    throw new Exception($"34.장비티어업 테이블 오류 ID={item.tier}, 필요직업레벨 음슴");
                }
            }
#endif
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

        public TierUpData Get(int tier, int classBitType)
        {
            TierUpData ret = null;
            dataDic.TryGetValue((tier, classBitType), out ret);
            return ret;
        }
    }
}
