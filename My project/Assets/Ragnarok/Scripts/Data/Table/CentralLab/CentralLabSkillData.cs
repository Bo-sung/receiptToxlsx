using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CentralLabSkillDataManager"/>
    /// </summary>
    public sealed class CentralLabSkillData : IData
    {
        public readonly int id;
        public readonly int job_id;
        public readonly int skill_type;
        public readonly int skill_id;
        public readonly int prob;

        public CentralLabSkillData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            job_id = data[index++].AsInt32();
            skill_type = data[index++].AsInt32();
            skill_id = data[index++].AsInt32();
            prob = data[index++].AsInt32();
        }
    }
}