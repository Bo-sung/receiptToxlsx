using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Ragnarok
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this string value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this byte value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this sbyte value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this short value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this ushort value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this int value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this uint value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this long value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this ulong value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// Enum으로 반환
        /// </summary>
        public static T ToEnum<T>(this ObscuredString value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((string)value);
        }

        /// <summary>
        /// Enum으로 반환
        /// </summary>
        public static T ToEnum<T>(this ObscuredByte value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((byte)value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this ObscuredSByte value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((sbyte)value);
        }

        /// <summary>
        /// Enum으로 반환
        /// </summary>
        public static T ToEnum<T>(this ObscuredShort value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((short)value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this ObscuredUShort value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((ushort)value);
        }

        /// <summary>
        /// Enum으로 반환
        /// </summary>
        public static T ToEnum<T>(this ObscuredInt value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((int)value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this ObscuredUInt value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((uint)value);
        }

        /// <summary>
        /// Enum으로 반환
        /// </summary>
        public static T ToEnum<T>(this ObscuredLong value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((long)value);
        }

        /// <summary>
        /// Enum으로 변환
        /// </summary>
        public static T ToEnum<T>(this ObscuredULong value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return ToEnum<T>((ulong)value);
        }

        /// <summary>
        /// 다음 Enum Value 반환
        /// </summary>
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}