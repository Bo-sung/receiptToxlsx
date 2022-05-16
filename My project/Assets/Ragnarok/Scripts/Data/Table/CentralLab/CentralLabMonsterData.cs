using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="CentralLabMonsterDataManager"/>
    /// </summary>
    public sealed class CentralLabMonsterData : IData
    {
        public readonly ObscuredInt id; // 몬스터 고유 아이디
        public readonly ObscuredInt lab_id; // 실험실 고유 아이디 (참조)
        public readonly ObscuredInt stage_no; // 스테이지 index
        public readonly ObscuredInt monster_id; // 몬스터 id
        public readonly ObscuredInt monster_level; // 몬스터 레벨
        public readonly ObscuredInt monster_type; // 몬스터 type 1: 일반, 2: 보스
        public readonly ObscuredVector3 position;
        public readonly ObscuredInt monster_scale;

        public CentralLabMonsterData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id = data[index++].AsInt32();
            lab_id = data[index++].AsInt32();
            stage_no = data[index++].AsInt32();
            monster_id = data[index++].AsInt32();
            monster_level = data[index++].AsInt32();
            monster_type = data[index++].AsInt32();
            int pos_x = data[index++].AsInt32();
            int pos_z = data[index++].AsInt32();
            monster_scale = data[index++].AsInt32();
            position = new ObscuredVector3(pos_x * 0.001f, 0f, pos_z * 0.001f);
        }

        public float GetMonsterScale()
        {
            // 스케일 값이 존재하지 않을 경우에는 1로 고정
            if (monster_scale == 0)
                return 1f;

            return MathUtils.ToPercentValue(monster_scale);
        }
    }
}