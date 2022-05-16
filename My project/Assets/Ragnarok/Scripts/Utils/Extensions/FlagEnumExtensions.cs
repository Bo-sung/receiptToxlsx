using System;
using System.Collections.Generic;

namespace Ragnarok
{
    public static class FlagEnumExtensions
    {
        /// <summary>
        /// Byte Value 로 변환
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte ToByteValue<T>(this T value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return Convert.ToByte(value);
        }

        public static byte ToByteValue(this bool value)
        {
            return Convert.ToByte(value);
        }

        /// <summary>
        /// Int Value 로 변환
        /// </summary>
        public static int ToIntValue<T>(this T value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Flag Enum 체크
        /// </summary>
        public static bool HasFlagEnum<T>(this T value, T flag)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            int intValue = ToIntValue(value);
            int flagValue = ToIntValue(flag);
            return (intValue & flagValue) == flagValue;
        }

        /// <summary>
        /// Flag Enum 추가
        /// </summary>
        public static void AddFlagEnum<T>(this ref T value, T flag)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            int intValue = ToIntValue(value);
            int flagValue = ToIntValue(flag);
            value = (intValue | flagValue).ToEnum<T>();
        }

        /// <summary>
        /// Flag Enum 제거
        /// </summary>
        public static void RemoveFlagEnum<T>(this ref T value, T flag)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            int intValue = ToIntValue(value);
            int flagValue = ToIntValue(flag);
            value = (intValue & ~flagValue).ToEnum<T>();
        }

        /// <summary>
        /// Flag Enum으로 변환
        /// </summary>
        public static T ToFlagEnum<T>(this byte value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (1 << value).ToEnum<T>();
        }

        /// <summary>
        /// Flag Enum으로 변환
        /// </summary>
        public static T ToFlagEnum<T>(this short value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (1 << value).ToEnum<T>();
        }

        /// <summary>
        /// Flag Enum으로 변환
        /// </summary>
        public static T ToFlagEnum<T>(this int value)
            where T : struct, IConvertible, IFormattable, IComparable
        {
            return (1 << value).ToEnum<T>();
        }

        /// <summary>
        /// FlagsAttribute enum을 Array로 변환한다.
        /// </summary>
        public static T[] FlagsToArray<T>(this T eFlagEnumValue)
            where T : Enum
        {
            List<T> enumList = new List<T>();

            T[] enumValues = (T[])Enum.GetValues(typeof(T));
            foreach (var value in enumValues)
            {
                if (eFlagEnumValue.HasFlag(value))
                {
                    enumList.Add(value);
                }
            }

            return enumList.ToArray();
        }
    }
}