using UnityEngine;

namespace Ragnarok
{
    public sealed class GuildAttackMonsterPacket : IPacket<Response>, IMonsterBotInput
    {
        public int Index { get; private set; }
        public int Id { get; private set; }
        public byte State { get; private set; }
        public float PosX { get; private set; }
        public float PosY => 0f;
        public float PosZ { get; private set; }
        public bool HasTargetPos => true;
        public float SavedTargetingTime { get; private set; }
        public float TargetPosX { get; private set; }
        public float TargetPosY => 0f;
        public float TargetPosZ { get; private set; }
        public bool HasMaxHp => true;
        public bool HasCurHp => true;
        public int CurHp { get; private set; }
        public int MaxHp { get; private set; }

        public MonsterType Type => MonsterType.Normal;
        public int Level { get; private set; }
        public float Scale { get; private set; }
        public CrowdControlType CrowdControl => default;
        public float? MoveSpeed => null;

        void IInitializable<Response>.Initialize(Response response)
        {
            Index = response.GetByte("1");
            Id = response.GetInt("2");
            State = response.GetByte("3");
            PosX = response.GetFloat("4");
            PosZ = response.GetFloat("5");
            TargetPosX = response.GetFloat("6");
            TargetPosZ = response.GetFloat("7");
            CurHp = response.GetInt("8");
            MaxHp = response.GetInt("9");
            Scale = MathUtils.ToPercentValue(response.GetInt("10"));
            Level = response.GetInt("11");

            SavedTargetingTime = Time.realtimeSinceStartup;
        }
    }
}