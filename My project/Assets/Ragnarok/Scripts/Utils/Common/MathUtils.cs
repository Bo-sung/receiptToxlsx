using UnityEngine;

namespace Ragnarok
{
    public static class MathUtils
    {
        private const float CellSize = 3.6f;
        private const float MapScale = 0.65f;
        private const float POSITION_Y = 2.47f;

        /// <summary>
        /// float 타입을 반올림 해줍니다.
        /// </summary>
        public static int RoundToInt(float input)
        {
            return (int)(input + (input > 0 ? 0.5f : -0.5f));
        }

        /// <summary>
        /// float 타입을 반올림 해줍니다.
        /// </summary>
        public static long RoundToLong(double input)
        {
            return (long)(input + (input > 0 ? 0.5f : -0.5f));
        }

        /// <summary>
        /// 절대값을 반환합니다.
        /// </summary>
        public static int Abs(int input)
        {
            return input < 0 ? -input : input;
        }

        /// <summary>
        /// 절대값을 반환합니다.
        /// </summary>
        public static float Abs(float input)
        {
            return input < 0 ? -input : input;
        }

        /// <summary>
        /// 최대 최소 값을 반한합니다.
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        /// <summary>
        /// 최대 최소 값을 반한합니다.
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        /// <summary>
        /// 진행도 값을 반환합니다.
        /// </summary>
        public static float GetProgress(float currentValue, float maxValue)
        {
            // max진행도가 0일 경우 진행도는 무조건 최대치
            if (maxValue == 0f)
                return 1f;

            return Clamp(currentValue, 0f, maxValue) / maxValue;
        }

        /// <summary>
        /// 진행도 값을 반환합니다.
        /// </summary>
        public static float GetProgress(int currentValue, int maxValue)
        {
            // max진행도가 0일 경우 진행도는 무조건 최대치
            if (maxValue == 0)
                return 1f;

            return Clamp(currentValue, 0f, maxValue) / maxValue;
        }

        /// <summary>
        /// 진행도 값을 반환합니다.
        /// </summary>
        public static float GetProgress(long currentValue, long maxValue)
        {
            // max진행도가 0일 경우 진행도는 무조건 최대치
            if (maxValue == 0)
                return 1f;

            return Clamp(currentValue, 0f, maxValue) / maxValue;
        }

        /// <summary>
        /// 배율 반환 (a / b)
        /// </summary>
        public static float GetRate(int a, int b)
        {
            if (b == 0)
                return float.MaxValue;

            return (Mathf.Round(((float)a / b) * 100) / 100.0f); // 서버 부동소수점 버그로 변경
        }

        /// <summary>
        /// 확률 계산 (만분율)
        /// </summary>
        public static bool IsCheckPermyriad(int value)
        {
            return Random.Range(0, 10000) < value;
        }

        /// <summary>
        /// value -> BitField
        /// </summary>
        public static int GetBitFieldValue(int value, int pos, int length = 31)
        {
            int ret = value;
            ret = ret >> pos;
            ret = ret & length;
            return ret;
        }

        /// <summary>
        /// BitField => IntValue
        /// </summary>
        public static int GetValueFromBitField(int pos, int length, params int[] values)
        {
            if (values == null)
                return 0;

            int maxValue = (1 << pos) - 1;
            if (length > maxValue)
                throw new System.ArgumentException($"유효하지 않은 처리: {nameof(pos)} = {pos}, {nameof(length)} = {length}");

            int checkValue = Mathf.Min(maxValue, length);
            int ret = 0;
            for (int i = values.Length - 1; i >= 0; i--)
            {
                if (values[i] > checkValue)
                    throw new System.ArgumentException($"유효하지 않은 처리: [{i}] = {values[i]}");

                if (i < values.Length - 1)
                    ret <<= pos;

                ret += values[i];
            }

            return ret;
        }

        /// <summary>
        /// 부동소수점 처리하여 int로 처리, 
        /// 소수 3번쨰 자리에서 반올림
        /// </summary>
        public static int ToInt(float floatValue)
        {
            const int MAX_VALUE = int.MaxValue / 100;

            // 100 을 곱하면서 intMax 를 넘을 수 있으므로 Long 으로 변환
            if (floatValue > MAX_VALUE)
            {
                long longVlaue = RoundToLong(floatValue * 100) / 100; // 서버 부동소수점 버그로 변경

                // 계산 값이 intMax 를 넘을 경우
                if (longVlaue > int.MaxValue)
                    return int.MaxValue;

                return (int)longVlaue;
            }

            return RoundToInt(floatValue * 100) / 100; // 서버 부동소수점 버그로 변경
        }

        /// <summary>
        /// 부동소수점 처리하여 int로 처리, 
        /// 소수 digits번쨰 자리에서 반올림
        public static int ToInt(float floatValue, int digits)
        {
            digits = (int)Mathf.Pow(10, digits - 1);
            return RoundToInt(floatValue * digits) / digits; // 서버 부동소수점 버그로 변경
        }

        /// <summary>
        /// 부동소수점 처리하여 Long로 처리, 
        /// 소수 3번쨰 자리에서 반올림
        /// </summary>
        public static long ToLong(double doubleValue)
        {
            return RoundToLong(doubleValue * 100) / 100L; // 서버 부동소수점 버그로 변경
        }

        /// <summary>
        /// 부동소수점 처리하여 Long로 처리, 
        /// 소수 digits번쨰 자리에서 반올림
        /// </summary>
        public static long ToLong(double doubleValue, int digits)
        {
            digits = (int)Mathf.Pow(10, digits - 1);
            return RoundToLong(doubleValue * digits) / digits; // 서버 부동소수점 버그로 변경
        }

        /// <summary>
        /// 백분율 실제 값으로 변환 (1% => 0.01) or (100% => 1)
        /// </summary>
        public static float ToPercentValue(int intValue)
        {
            return intValue * 0.01f;
        }

        /// <summary>
        /// 0~1 값 퍼센트 텍스트료 변환
        /// </summary>
        public static string GetPercentText(float floatValue)
        {
            return StringBuilderPool.Get()
                .Append(Mathf.RoundToInt(floatValue * 100f))
                .Append("%")
                .Release();
        }

        /// <summary>
        /// 천분율 실제 값으로 변환 (1‰ => 0.001) or (100.0% => 1)
        /// </summary>
        public static float ToPermilleValue(int intValue)
        {
            return intValue * 0.001f;
        }

        /// <summary>
        /// 만분율 실제 값으로 변환 (1‱ => 0.0001) or (100.00% => 1)
        /// </summary>
        public static float ToPermyriadValue(int intValue)
        {
            return intValue * 0.0001f;
        }

        /// <summary>
        /// 만분율로 변환 (0.0001 => 1) or (1 => 10000)
        /// </summary>
        public static int ToPermyriad(float floatValue)
        {
            return ToInt(floatValue * 10000);
        }

        /// <summary>
        /// 만분율로 변경 (0.0001 => 1‱)
        /// </summary>
        public static string ToPermyriadText(int intValue)
        {
            return ToPermyriadValue(intValue).ToString("0.##%");
        }

        /// <summary>
        /// (int)밀리초를 (float)초로 변경
        /// </summary>
        /// <param name="millisecond">(단위: ms)</param>
        public static float MillisecToSec(int millisecond)
        {
            return millisecond / 1000f;
        }

        /// <summary>
        /// 좌표를 인덱스로 변환
        /// </summary>
        public static int PositionToIndex(int x, int y, int sizeY)
        {
            return (sizeY * x) + y;
        }

        /// <summary>
        /// 인덱스를 좌표로 변환
        /// </summary>
        public static (int x, int y) IndexToPosition(int index, int sizeY)
        {
            return (index / sizeY, index % sizeY);
        }

        public static Vector3 GetUnitPosition(int x, int y)
        {
            return new Vector3(x * (CellSize * MapScale), POSITION_Y, y * (CellSize * MapScale));
        }

        /// <summary>
        /// 귓속말할 때 필요한 HexCode <-> CID 변환 함수
        /// </summary>
        public static int[] mask = new int[]  {1,2,4,8,16,32,64,128,256,512,
            1024,2048,4096,8192,16384,32768,65536,131072,262144,524288,
            1048576,2097152,4194304,8388608,16777216,33554432,67108864,134217728,268435456,536870912,1073741824,-2147483648};

        public static int HexCodeToCid(string hex_cid)
        {
            int var;
            int len = 0;

            char[] buf = new char[8];
            for (int i = 0; i < 8; ++i)
                buf[i] = (char)48;

            char[] tmp = new char[16];
            for (int i = 0; i < 16; ++i)
                tmp[i] = (char)0;

            buf = hex_cid.ToCharArray();
            int uid = 0;
            for (int iCnt = 0; iCnt < 8; iCnt++)
            {
                if (buf[iCnt] <= 57)
                    uid += (int)((buf[iCnt] - 48) * (int)Mathf.Pow(16, iCnt));
                else uid += (int)((buf[iCnt] - 55) * (int)Mathf.Pow(16, iCnt));
            }

            int tmpCid = 0;

            if (1 == (uid & mask[30]) >> 30)
                tmpCid |= mask[1];
            if (1 == (uid & mask[22]) >> 22)
                tmpCid |= mask[0];
            if (1 == (uid & mask[14]) >> 14)
                tmpCid |= mask[2];
            if (1 == (uid & mask[6]) >> 6)
                tmpCid |= mask[3];
            if (1 == (uid & mask[29]) >> 29)
                tmpCid |= mask[4];
            if (1 == (uid & mask[21]) >> 21)
                tmpCid |= mask[5];
            if (1 == (uid & mask[13]) >> 13)
                tmpCid |= mask[6];
            if (1 == (uid & mask[5]) >> 5)
                tmpCid |= mask[7];
            if (1 == (uid & mask[28]) >> 28)
                tmpCid |= mask[8];
            if (1 == (uid & mask[20]) >> 20)
                tmpCid |= mask[9];
            if (1 == (uid & mask[12]) >> 12)
                tmpCid |= mask[10];
            if (1 == (uid & mask[4]) >> 4)
                tmpCid |= mask[11];
            if (1 == (uid & mask[27]) >> 27)
                tmpCid |= mask[12];
            if (1 == (uid & mask[19]) >> 19)
                tmpCid |= mask[13];
            if (1 == (uid & mask[11]) >> 11)
                tmpCid |= mask[14];
            if (1 == (uid & mask[3]) >> 3)
                tmpCid |= mask[15];
            if (1 == (uid & mask[26]) >> 26)
                tmpCid |= mask[16];
            if (1 == (uid & mask[18]) >> 18)
                tmpCid |= mask[17];
            if (1 == (uid & mask[10]) >> 10)
                tmpCid |= mask[18];
            if (1 == (uid & mask[2]) >> 2)
                tmpCid |= mask[19];
            if (1 == (uid & mask[25]) >> 25)
                tmpCid |= mask[20];
            if (1 == (uid & mask[17]) >> 17)
                tmpCid |= mask[21];
            if (1 == (uid & mask[9]) >> 9)
                tmpCid |= mask[22];
            if (1 == (uid & mask[1]) >> 1)
                tmpCid |= mask[23];
            if (1 == (uid & mask[24]) >> 24)
                tmpCid |= mask[24];
            if (1 == (uid & mask[16]) >> 16)
                tmpCid |= mask[25];
            if (1 == (uid & mask[8]) >> 8)
                tmpCid |= mask[26];
            if (1 == (uid & mask[0]) >> 0)
                tmpCid |= mask[27];
            if (1 == (uid & mask[23]) >> 23)
                tmpCid |= mask[28];
            if (1 == (uid & mask[15]) >> 15)
                tmpCid |= mask[29];
            if (1 == (uid & mask[7]) >> 7)
                tmpCid |= mask[30];

            tmpCid -= 97999999;


            return tmpCid;
        }

        public static string CidToHexCode(int cid)
        {
            int var;
            int len = 0;

            char[] buf = new char[8];
            for (int i = 0; i < 8; ++i)
                buf[i] = (char)48;

            char[] tmp = new char[16];
            for (int i = 0; i < 16; ++i)
                tmp[i] = (char)0;

            cid += 97999999;
            //		uid *= 3;
            int tmpCid = 0;

            if (1 == (cid & mask[1]) >> 1)
                tmpCid |= mask[30];
            if (1 == (cid & mask[0]) >> 0)
                tmpCid |= mask[22];
            if (1 == (cid & mask[2]) >> 2)
                tmpCid |= mask[14];
            if (1 == (cid & mask[3]) >> 3)
                tmpCid |= mask[6];
            if (1 == (cid & mask[4]) >> 4)
                tmpCid |= mask[29];
            if (1 == (cid & mask[5]) >> 5)
                tmpCid |= mask[21];
            if (1 == (cid & mask[6]) >> 6)
                tmpCid |= mask[13];
            if (1 == (cid & mask[7]) >> 7)
                tmpCid |= mask[5];
            if (1 == (cid & mask[8]) >> 8)
                tmpCid |= mask[28];
            if (1 == (cid & mask[9]) >> 9)
                tmpCid |= mask[20];
            if (1 == (cid & mask[10]) >> 10)
                tmpCid |= mask[12];
            if (1 == (cid & mask[11]) >> 11)
                tmpCid |= mask[4];
            if (1 == (cid & mask[12]) >> 12)
                tmpCid |= mask[27];
            if (1 == (cid & mask[13]) >> 13)
                tmpCid |= mask[19];
            if (1 == (cid & mask[14]) >> 14)
                tmpCid |= mask[11];
            if (1 == (cid & mask[15]) >> 15)
                tmpCid |= mask[3];
            if (1 == (cid & mask[16]) >> 16)
                tmpCid |= mask[26];
            if (1 == (cid & mask[17]) >> 17)
                tmpCid |= mask[18];
            if (1 == (cid & mask[18]) >> 18)
                tmpCid |= mask[10];
            if (1 == (cid & mask[19]) >> 19)
                tmpCid |= mask[2];
            if (1 == (cid & mask[20]) >> 20)
                tmpCid |= mask[25];
            if (1 == (cid & mask[21]) >> 21)
                tmpCid |= mask[17];
            if (1 == (cid & mask[22]) >> 22)
                tmpCid |= mask[9];
            if (1 == (cid & mask[23]) >> 23)
                tmpCid |= mask[1];
            if (1 == (cid & mask[24]) >> 24)
                tmpCid |= mask[24];
            if (1 == (cid & mask[25]) >> 25)
                tmpCid |= mask[16];
            if (1 == (cid & mask[26]) >> 26)
                tmpCid |= mask[8];
            if (1 == (cid & mask[27]) >> 27)
                tmpCid |= mask[0];
            if (1 == (cid & mask[28]) >> 28)
                tmpCid |= mask[23];
            if (1 == (cid & mask[29]) >> 29)
                tmpCid |= mask[15];
            if (1 == (cid & mask[30]) >> 30)
                tmpCid |= mask[7];


            string hex = "";
            do
            {
                var = tmpCid % 16;
                tmpCid /= 16;
                /* try to convert ascii code */
                if (var < 10) buf[len] = (char)(var + 48);
                else buf[len] = (char)(var + 55);
                len++;
            }
            while (tmpCid > 0);

            hex += string.Join(string.Empty, buf);
            return hex;
        }
    }
}