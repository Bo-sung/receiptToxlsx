using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="TPCostumeLevelDataManager"/>
    /// </summary>
    public class TPCostumeLevelData : IData
    {
        public ObscuredInt id;
        public ObscuredInt costume_type;
        public ObscuredInt smelt_level;
        public ObscuredInt resource_type_1;
        public ObscuredInt resource_value_1;
        public ObscuredInt resource_count_1;
        public ObscuredInt resource_type_2;
        public ObscuredInt resource_value_2;
        public ObscuredInt resource_count_2;
        public ObscuredInt resource_type_3;
        public ObscuredInt resource_value_3;
        public ObscuredInt resource_count_3;
        public ObscuredInt job_level;

        public TPCostumeLevelData(IList<MessagePackObject> data)
        {
            int index = 0;
            id               = data[index++].AsInt32();
            costume_type     = data[index++].AsInt32();
            smelt_level      = data[index++].AsInt32();
            resource_type_1  = data[index++].AsInt32();
            resource_value_1 = data[index++].AsInt32();
            resource_count_1 = data[index++].AsInt32();
            resource_type_2  = data[index++].AsInt32();
            resource_value_2 = data[index++].AsInt32();
            resource_count_2 = data[index++].AsInt32();
            resource_type_3  = data[index++].AsInt32();
            resource_value_3 = data[index++].AsInt32();
            resource_count_3 = data[index++].AsInt32();
            job_level        = data[index++].AsInt32();
        }

        public RewardData GetZeny()
        {
            return new RewardData(resource_type_1, resource_value_1, resource_count_1); // 무조건 제니
        }

        public RewardData[] GetMaterials()
        {
            return new RewardData[2]
            {
                   new RewardData(resource_type_2, resource_value_2, resource_count_2),
                   new RewardData(resource_type_3, resource_value_3, resource_count_3),
            };
        }
    }
}