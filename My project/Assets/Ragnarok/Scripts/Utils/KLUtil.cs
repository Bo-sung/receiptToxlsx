using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public static class KLUtil
    {
        // [출처] C# 한글 분리 및 합치기|작성자 킹닥터
        // http://blog.naver.com/PostView.nhn?blogId=sunho0371&logNo=220948604603

        public static string chosung = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        public static string jungsung = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";
        public static string jongsung = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ"; // 받침이 없을 수 있으므로 제일 첫부분은 띄워줘야 합니다.

        public static ushort UnicodeHangeulBase = 0xAC00;
        public static ushort UnicodeHangeulLast = 0xD79F;

        public static char Merge(char cho, char jung, char jong)
        {
            int ChosungCode = chosung.IndexOf(cho); // 초성 인덱스
            int JungsungCode = jungsung.IndexOf(jung); // 중성 인덱스
            int JongsungCode = jongsung.IndexOf(jong); // 종성 인덱스

            int Code = UnicodeHangeulBase + (ChosungCode * 21 + JungsungCode) * 28 + JongsungCode;

            return Convert.ToChar(Code);
        }

        public static void Divide(char c, out char cho, out char jung, out char jong)
        {
            ushort check = Convert.ToUInt16(c);
            cho = '\0';
            jung = '\0';
            jong = '\0';

            if (check > UnicodeHangeulLast || check < UnicodeHangeulBase)
                return;

            /*
            * 합칠 때 제일 처음에 UnicodeHangeulBase 를 더해 줬으므로
            * 제일 먼저 빼줘야 합니다.
            */

            int Code = check - UnicodeHangeulBase;

            int JongsungCode = Code % 28; // 종성 코드 분리
            Code = (Code - JongsungCode) / 28;

            int JungsungCode = Code % 21; // 중성 코드 분리
            Code = (Code - JungsungCode) / 21;

            int ChosungCode = Code; // 남는 게 자동으로 초성이 됩니다.

            char Chosung = chosung[ChosungCode]; // Chosung 목록 중에서 ChosungCode 번째 있는 글자
            char Jungsung = jungsung[JungsungCode];
            char Jongsung = jongsung[JongsungCode];

            cho = Chosung;
            jung = Jungsung;
            jong = Jongsung;
        }
    }
}
