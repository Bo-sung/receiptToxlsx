using System;

namespace Ragnarok.CIBuilder
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StoreTypeConfigAttribute : Attribute
    {
        public static readonly StoreTypeConfigAttribute DEFAULT = new StoreTypeConfigAttribute(default(StoreType));

        public readonly int storeType;

        /// <summary>
        /// 스토어 타입 설정
        /// </summary>
        /// <param name="storeType">스토어 타입</param>
        public StoreTypeConfigAttribute(StoreType storeType)
        {
            this.storeType = (int)storeType;
        }

        public bool Equals(int storeType)
        {
            if (!this.storeType.Equals(storeType))
                return false;

            return true;
        }
    }
}