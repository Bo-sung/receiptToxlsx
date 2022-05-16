#if UNITY_EDITOR
namespace Ragnarok
{
    public class DebugDamageTuple
    {
        public readonly DamagePacket.DamageUnitType attackerType;
        public readonly int attackerId;
        public readonly int attackerLevel;

        public readonly DamagePacket.DamageUnitType targetType;
        public readonly int targetId;
        public readonly int targetLevel;

        public readonly IDamageTuple client;
        public IDamageTuple server;

        public DebugDamageTuple(DamagePacket packet)
        {
            attackerType = packet.attackerType.ToEnum<DamagePacket.DamageUnitType>();
            attackerId = packet.attackerId;
            attackerLevel = packet.attackerLevel;

            targetType = packet.targetType.ToEnum<DamagePacket.DamageUnitType>();
            targetId = packet.targetId;
            targetLevel = packet.targetLevel;

            client = packet;

            if (DebugUtils.IsLogDamagePacket)
            {
                packet.ShowLog();
            }
        }

        public void SetServerResult(DamageCheckPacket result)
        {
            server = result;
        }
    }
}
#endif