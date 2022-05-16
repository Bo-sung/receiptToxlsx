using CodeStage.AntiCheat.ObscuredTypes;

namespace Ragnarok
{
    public static class ObscuredValueExtensions
    {
        public static bool Replace(this ref ObscuredBool oldValue, bool? newValue)
        {
            if (newValue == null)
                return false;

            bool value = newValue.Value;
            if (oldValue == value)
                return false;

            oldValue = value;
            return true;
        }

        public static bool Replace(this ref ObscuredByte oldValue, byte? newValue)
        {
            if (newValue == null)
                return false;

            byte value = newValue.Value;
            if (oldValue == value)
                return false;

            oldValue = value;
            return true;
        }

        public static bool Replace(this ref ObscuredShort oldValue, short? newValue)
        {
            if (newValue == null)
                return false;

            short value = newValue.Value;
            if (oldValue == value)
                return false;

            oldValue = value;
            return true;
        }

        public static bool Replace(this ref ObscuredInt oldValue, int? newValue)
        {
            if (newValue == null)
                return false;

            int value = newValue.Value;
            if (oldValue == newValue)
                return false;

            oldValue = value;
            return true;
        }

        public static bool Replace(this ref ObscuredLong oldValue, long? newValue)
        {
            if (newValue == null)
                return false;

            long value = newValue.Value;
            if (oldValue == newValue)
                return false;

            oldValue = value;
            return true;
        }
    }
}