using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public class ExpData : IData
    {
        public readonly ObscuredInt level;
        public readonly ObscuredInt next_base_lv_need_exp;
        public readonly ObscuredLong next_job_lv_need_exp;
        public readonly ObscuredInt cupet_lv_need_exp;

        public ExpData(IList<MessagePackObject> data)
        {
            level                 = data[0].AsInt32();
            next_base_lv_need_exp = data[1].AsInt32();
            next_job_lv_need_exp  = data[2].AsInt64();
            cupet_lv_need_exp     = data[3].AsInt32();
        }

        public long GetNeedExp(ExpDataManager.ExpType expType)
        {
            switch (expType)
            {
                case ExpDataManager.ExpType.CharacterBase:
                    return next_base_lv_need_exp;

                case ExpDataManager.ExpType.CharacterJob:
                    return next_job_lv_need_exp;

                case ExpDataManager.ExpType.Cupet:
                    return cupet_lv_need_exp;
            }

            return 0;
        }
    }
}