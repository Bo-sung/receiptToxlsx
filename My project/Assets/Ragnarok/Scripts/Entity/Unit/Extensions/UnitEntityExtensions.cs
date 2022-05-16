namespace Ragnarok
{
    public static class UnitEntityExtensions
    {
        /// <summary>
        /// 유효성 여부
        /// </summary>
        public static bool IsValid(this UnitEntity unitEntity)
        {
            return !unitEntity.IsInvalid();
        }

        /// <summary>
        /// 유효성 여부
        /// </summary>
        public static bool IsInvalid(this UnitEntity unitEntity)
        {
            return unitEntity == null || unitEntity.IsDie || unitEntity.GetActor() == null;
        }
    }
}