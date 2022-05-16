using System;

namespace SheetViewer
{
    enum Format_Type
    {
        format,
        //format_Onbuff
    }

    public class MailConvertModel : IModel
    {
        const string ONBUFF_SERVER = "Test";
        const string OOBUFF_LOCALE = "NFT";
        static String input_appName;

        VersionSheet.Layout latest_Layout;
        public string GetConntentAddBoxText(String[] data, int count)
        {
            String edited = "";
            //폰트, 글자 크기 등등.
            String formatSettings = '\u0022' + "font-family: 맑은 고딕; font-size: 16px; line-height: 1.6; margin-top: 0px; margin-bottom: 0px;" + '\u0022';
            //내용 포멧
            String format = $"<li style=" + formatSettings + " data-mce-style=" + formatSettings + ">@Context</li>";

            //글머리 기호
            String versionTextHead_format = "<ul style=" + '\u0022' + "list - style - type: disc;" + '\u0022' + " data-mce-style=" + '\u0022' + "list - style - type: disc;" + '\u0022' + ">";
            String textHead_format = "<ul style=" + '\u0022' + "list - style - type: disc;" + '\u0022' + " data-mce-style=" + '\u0022' + "list - style - type: circle;" + '\u0022' + ">";

            bool IsversionEnd = true;
            bool isPlainTextEnd = true;
            edited += versionTextHead_format + "\n";
            bool isPrevPlain = false;
            foreach (string i in data)
            {
                string edit = i;
                if (edit.Contains("!버전"))
                {
                    if (latest_Layout.BundleVer != null || latest_Layout.BundleVer != "")
                    {
                        string bundleVerWithoutIndex = latest_Layout.BundleVer;
                        bundleVerWithoutIndex = bundleVerWithoutIndex.Remove(bundleVerWithoutIndex.LastIndexOf(".")) + ".";
                        edit = edit.Replace("!버전", bundleVerWithoutIndex);
                    }
                }


                if (edit.StartsWith(" "))
                {
                    if (isPlainTextEnd)
                    {
                        edited += "    " + textHead_format + "\n";
                        isPlainTextEnd = false;
                    }
                    edited += "    " + format.Replace("@Context", edit) + "\n";
                    isPrevPlain = true;
                }
                else
                {
                    if (!IsversionEnd)
                    {
                        if (isPrevPlain)
                            edited += "</ul>\n";
                        IsversionEnd = true;
                        isPlainTextEnd = true;
                    }
                    edited += format.Replace("@Context", $"<b>{edit}</b>") + "\n";
                    IsversionEnd = false;
                    isPrevPlain = false;
                }
            }
            return edited;
        }

        private bool TryGetFormatFileToString(Format_Type type, out String returnS)
        {
            returnS = "";
            //Format.html이 존재하는지 확인
            if (!System.IO.File.Exists($"Formats\\{type.ToString()}.html"))
            {
                returnS = $"{type.ToString()}.html is Unavalable";
                return false;
            }
            //포멧 형식 불러오기 
            foreach (var i in System.IO.File.ReadLines($"Formats\\{type.ToString()}.html"))
                returnS += i.Trim();
            if (returnS == "")   // 포맷이 빌 경우            
            {
                returnS = $"{type.ToString()}.html is Empty. Check it";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 다운로드 링크 생성
        /// </summary>
        public String CreateDownloadLink(String appName)
        {
            if (String.IsNullOrEmpty(appName))
                return "";
            MailConvertModel.input_appName = appName.Trim();
            //다운로드 링크 생성
            return "http://cdn.convtf.com/ragcube/build/" + appName;
        }

        public bool IsOnBuff(VersionSheet.Layout layout)
        {
            if (layout.Locale != OOBUFF_LOCALE)
                return false;
            return true;
        }
        public String InsertAtFormat(String Editor, VersionSheet.Layout layout)
        {
            latest_Layout = layout;
            //포맷 로드
            string edited = "";
            Format_Type format_type = Format_Type.format;

            if (!TryGetFormatFileToString(format_type, out edited))
                return "Failed To Get Format";

            //포멧에 데이터 삽입

            InsertVercode(edited, layout.Vercode, out edited);                         //버전코드
            InsertLocale(edited, layout.Locale, out edited);                           //지역
            InsertBundleVer(edited, layout.BundleVer, out edited);                     //번들 버전
            InsertServer(edited, layout.Server, out edited);                           //서버
            InsertPlatform(edited, layout.Platform, out edited);                       //플랫폼
            InsertAppName(edited, layout.AppName, out edited);                     //앱 이름
            InsertDownloadLink(edited, layout.DownloadLink, out edited);           //다운로드 링크
            InsertState(edited, layout.State, out edited);                             //상태
            InsertName(edited, Editor, out edited);                                    //이메일 작성자 이름

            return edited;
        }

        /// <summary>
        /// 버전 코드 삽입
        /// </summary>
        protected virtual bool InsertVercode(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";

            if (Format.Contains("@VerCode"))    //버전코드
            {
                outPut = Format.Replace("@VerCode", data);
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 지역 삽입
        /// </summary>
        protected virtual bool InsertLocale(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";

            if (Format.Contains("@Locale"))     //지역
            {
                if (data != "한국")
                    outPut = Format.Replace("@Locale", data);
                else
                    outPut = Format.Replace("@Locale", "글로벌");
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 번들버전 삽입
        /// </summary>
        protected virtual bool InsertBundleVer(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";
            if (Format.Contains("@BundleVer"))  //번들 버전
            {
                outPut = Format.Replace("@BundleVer", data);
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 서버 삽입
        /// </summary>
        protected virtual bool InsertServer(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";

            if (Format.Contains("@Server"))     //서버
            {
                switch (data)
                {
                    case "Test":
                        outPut = Format.Replace("@Server", "테스트");
                        break;
                    case "Stage":
                    case "Real":
                        outPut = Format.Replace("@Server", "");
                        break;
                    default:
                        outPut = Format.Replace("@Server", data);
                        break;
                }
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 플랫폼 삽입
        /// </summary>
        protected virtual bool InsertPlatform(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";

            if (Format.Contains("@Platform"))   //플랫폼
            {
                outPut = Format.Replace("@Platform", data);
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 온버프 전용 변경 삽입
        /// </summary>
        protected virtual bool InsertOnBuff(String Format, String appName, String downloadLink, out String outPut)
        {
            if (appName == null)
            {
                if (input_appName == null)
                    appName = "";
                else
                    appName = input_appName;
            }

            if (downloadLink == null)
                downloadLink = "";

            //파일명 aab가 추가로 필요함.[ \u0022 = " ]임 
            //만약 이미 apk나 aab 이름을 가지고 있을경우 제거.
            if (appName.Contains(".apk") || appName.Contains(".apk"))
            {
                appName = appName.Replace(".apk", "");
                appName = appName.Replace(".aab", "");
            }

            if (downloadLink.Contains(".apk") || downloadLink.Contains(".apk"))
            {
                downloadLink = downloadLink.Replace(".apk", "");
                downloadLink = downloadLink.Replace(".aab", "");
            }

            outPut = null;
            //만약 두 결과 모두 실패시 false 리턴과 함꼐 ountput = null
            if (!InsertAppName(Format, appName, out Format))
                return false;
            if (!InsertDownloadLink(Format, downloadLink, out outPut))
                return false;
            return true;
        }

        /// <summary>
        /// 앱 이름 삽입
        /// </summary>
        protected virtual bool InsertAppName(String Format, String data, out String outPut)
        {
            if (data == null)
            {
                if (input_appName == null)
                    data = "";
                else
                    data = input_appName;
            }

            if (Format.Contains("@AppName"))    //앱 이름
            {
                outPut = Format.Replace("@AppName", data);
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 다운로드 링크 삽입
        /// </summary>
        protected virtual bool InsertDownloadLink(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";

            if (Format.Contains("@DLink"))      //다운로드 링크
            {
                if (input_appName != "" && data == "")
                    data = CreateDownloadLink(input_appName);
                outPut = Format.Replace("@DLink", data);
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 상태 삽입
        /// </summary>
        protected virtual bool InsertState(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";

            if (Format.Contains("@State"))      //상태
            {
                outPut = Format.Replace("@State", data);
                if (data == "최신")
                    outPut = outPut.Replace("용", "");
                return true;
            }
            outPut = null;
            return false;
        }

        /// <summary>
        /// 이름 삽입
        /// </summary>
        protected virtual bool InsertName(String Format, String data, out String outPut)
        {
            if (data == null)
                data = "";

            if (Format.Contains("@Name"))       //이메일 작성자 이름
            {
                outPut = Format.Replace("@Name", data);
                return true;
            }
            outPut = null;
            return false;
        }
    }

}