using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="MvpDataManager"/>
    /// </summary>
    public sealed class MvpData : IData, UIBossComing.IInput
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt group_id;
        public readonly ObscuredInt monster_id;
        public readonly ObscuredInt rate;
        public readonly ObscuredInt rare_type;
        public readonly ObscuredInt monster_scale;
        private readonly int cameraType;

        public MvpData(IList<MessagePackObject> data)
        {
            byte index = 0;
            id            = data[index++].AsInt32();
            group_id      = data[index++].AsInt32();
            monster_id    = data[index++].AsInt32();
            rate          = data[index++].AsInt32();
            rare_type     = data[index++].AsInt32();
            monster_scale = data[index++].AsInt32();
            cameraType    = data[index++].AsInt32();
        }

        public float GetScale()
        {
            return MathUtils.ToPercentValue(monster_scale);
        }

        public int GetMonsterId()
        {
            return monster_id;
        }

        public string GetDescription()
        {
            return LocalizeKey._2800.ToText(); // 유니크 MVP 출현!
        }

        public string GetSpriteName()
        {
            switch (rare_type)
            {
                default:
                case 1: return "Ui_Common_Icon_MVP";
                case 2: return "Ui_Common_Icon_MVP_2";
                case 3: return "Ui_Common_Icon_MVP_3";
            }
        }

        public bool IsUniqueMvpMonsterBattleView()
        {
            return cameraType == 1;
        }
    }
}