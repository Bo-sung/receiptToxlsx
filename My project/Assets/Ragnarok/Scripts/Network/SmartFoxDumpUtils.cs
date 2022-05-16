using Sfs2X.Entities.Data;
using Sfs2X.Protocol.Serialization;
using Sfs2X.Util;
using System.Text;

namespace Ragnarok
{
    public static class SmartFoxDumpUtils
    {
        public static string GetDump(ISFSObject obj)
        {
#if UNITY_EDITOR
            if (obj is SFSObject sfsObject)
                return DefaultObjectDumpFormatter.PrettyPrintDump(GetDump(sfsObject));
#endif

            return obj.GetDump();
        }

#if UNITY_EDITOR
        private static string GetDump(bool[] input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);

            int size = input.Length;
            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append("[").Append(i).Append("] ");
                stringBuilder.Append(input[i].ToString());
                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(ByteArray input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);

            byte[] bytes = input.Bytes;
            int size = bytes.Length;
            stringBuilder.Append("Binary Size: ").Append(size).Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            if (size > DefaultObjectDumpFormatter.MAX_DUMP_LENGTH)
            {
                stringBuilder.Append("** Data larger than max dump size of ").Append(DefaultObjectDumpFormatter.MAX_DUMP_LENGTH).Append(". Data not displayed");
            }
            else
            {
                int num = 0;
                for (int i = 0; i < size; i++)
                {
                    stringBuilder.AppendFormat("{0:x2}", bytes[i]);
                    if (++num == DefaultObjectDumpFormatter.HEX_BYTES_PER_LINE)
                    {
                        num = 0;
                        stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
                    }
                    else
                    {
                        stringBuilder.Append(" ");
                    }
                }

                if (size > 0)
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(short[] input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);

            int size = input.Length;
            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append("[").Append(i).Append("] ");
                stringBuilder.Append(input[i].ToString());
                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(int[] input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);

            int size = input.Length;
            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append("[").Append(i).Append("] ");
                stringBuilder.Append(input[i].ToString());
                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(long[] input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);

            int size = input.Length;
            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append("[").Append(i).Append("] ");
                stringBuilder.Append(input[i].ToString());
                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(float[] input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);

            int size = input.Length;
            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append("[").Append(i).Append("] ");
                stringBuilder.Append(input[i].ToString());
                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(double[] input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);

            int size = input.Length;
            for (int i = 0; i < size; i++)
            {
                stringBuilder.Append("[").Append(i).Append("] ");
                stringBuilder.Append(input[i].ToString());
                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(SFSArray input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);
            int size = input.Size();
            for (int i = 0; i < size; i++)
            {
                SFSDataWrapper sfsDataWrapper = input.GetWrappedElementAt(i);
                SFSDataType type = (SFSDataType)sfsDataWrapper.Type;
                string result;
                switch (type)
                {
                    case SFSDataType.NULL:
                        result = "NULL";
                        break;

                    case SFSDataType.BOOL_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as bool[]);
                        break;

                    case SFSDataType.BYTE_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as ByteArray);
                        break;

                    case SFSDataType.SHORT_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as short[]);
                        break;

                    case SFSDataType.INT_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as int[]);
                        break;

                    case SFSDataType.LONG_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as long[]);
                        break;

                    case SFSDataType.FLOAT_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as float[]);
                        break;

                    case SFSDataType.DOUBLE_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as double[]);
                        break;

                    case SFSDataType.SFS_ARRAY:
                        result = GetDump(sfsDataWrapper.Data as SFSArray);
                        break;

                    case SFSDataType.SFS_OBJECT:
                        result = GetDump(sfsDataWrapper.Data as SFSObject);
                        break;

                    default:
                        result = sfsDataWrapper.Data.ToString();
                        break;
                }

                stringBuilder.Append("(" + type.ToString().ToLower() + ") ");
                stringBuilder.Append(result);
                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static string GetDump(SFSObject input)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_OPEN);
            string[] keys = input.GetKeys();
            System.Array.Sort(keys, SortByIntTryParse); // 숫자 Key 재정렬
            int size = keys.Length;
            for (int i = 0; i < size; i++)
            {
                string key = keys[i];
                SFSDataWrapper sfsDataWrapper = input.GetData(key);
                SFSDataType type = (SFSDataType)sfsDataWrapper.Type;
                stringBuilder.Append("(" + type.ToString().ToLower() + ")");
                stringBuilder.Append(" " + key + ": ");
                switch (type)
                {
                    case SFSDataType.BOOL_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as bool[]));
                        break;

                    case SFSDataType.BYTE_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as ByteArray));
                        break;

                    case SFSDataType.SHORT_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as short[]));
                        break;

                    case SFSDataType.INT_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as int[]));
                        break;

                    case SFSDataType.LONG_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as long[]));
                        break;

                    case SFSDataType.FLOAT_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as float[]));
                        break;

                    case SFSDataType.DOUBLE_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as double[]));
                        break;

                    case SFSDataType.SFS_ARRAY:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as SFSArray));
                        break;

                    case SFSDataType.SFS_OBJECT:
                        stringBuilder.Append(GetDump(sfsDataWrapper.Data as SFSObject));
                        break;

                    default:
                        stringBuilder.Append(sfsDataWrapper.Data);
                        break;
                }

                stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_DIVIDER);
            }

            if (size > 0)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.Append(DefaultObjectDumpFormatter.TOKEN_INDENT_CLOSE).ToString();
        }

        private static int SortByIntTryParse(string x, string y)
        {
            if (IsNumber(x) && IsNumber(y))
            {
                int numX = int.Parse(x);
                int numY = int.Parse(y);

                return numX.CompareTo(numY);
            }

            return x.CompareTo(y);
        }

        private static bool IsNumber(string text)
        {
            foreach (char ch in text)
            {
                if (!char.IsDigit(ch))
                    return false;
            }

            return true;
        }
#endif
    }
}