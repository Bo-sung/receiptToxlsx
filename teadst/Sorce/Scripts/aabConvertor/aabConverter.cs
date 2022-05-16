using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Web;
using Path = System.IO.Path;
using System.ComponentModel;
using System.Windows.Threading;

namespace SheetViewer
{
    /// <summary>
    /// aab 파일 변환기
    /// </summary>
    public class AabConvertor
    {
        public EventHandler EventHandler_ConsolePrint;
        private static AabConvertor instance;
        public static AabConvertor GetInstance()
        {
            if (instance == null)
                instance = new AabConvertor();
            return instance;
        }
        protected AabConvertor()
        {
            instance = this;
        }
        /// <summary>
        /// 배치파일, BundleTool, 포맷, 변환할 파일 모드 포함된 디렉터리 경로
        /// </summary>
        private String _folder_Path;
        /// <summary>
        /// 변환할 파일의 경로
        /// </summary>
        private String _path_Of_File_To_Convert;
        /// <summary>
        /// 변환할 파일의 이름
        /// </summary>
        private String _Name_Of_File_To_Convert;
        /// <summary>
        /// 변환할 파일의 확장자를 포함한 이름
        /// </summary>
        private String _FullName_Of_File_to_Convert;
        /// <summary>
        /// 파일 변환시 bat 파일 실행할 프로세스
        /// </summary>
        readonly System.Diagnostics.Process process = new System.Diagnostics.Process();
        /// <summary>
        /// 변환중인지 여부
        /// </summary>
        bool _IsConverting = false;
        /// <summary>
        /// batFormat.txt 파일
        /// </summary>
        const String BAT_FORMAT = "batFormat.txt";
        /// <summary>
        /// batch파일 이름
        /// </summary>
        const String BAT_NAME = "testBatch.bat";
        /// <summary>
        /// 생성된 batch 파일 경로
        /// </summary>
        private String _batFile_Path;
        /// <summary>
        /// aabConvertor 초기화. 
        /// </summary>
        /// <param name="folderPath">Bundletools 파일과 bat 파일 포맷 포함된 위치</param>
        public void Initiallize(String folderPath)
        {
            //디렉토리 확인
            if (!Directory.Exists(folderPath))
                throw new Exception("Directory is Missing");
            _folder_Path = folderPath;
            //번들툴 확인
            if (!File.Exists(Path.Combine(folderPath, "bundletool-all-1.3.0.jar")))
                throw new Exception("bundleTool is not Exist");
            //포맷 확인
            if (!File.Exists(Path.Combine(folderPath, "batFormat.txt")))
                throw new Exception("FormatFile Dose not Exist");
        }
        /// <summary>
        /// aabConvertor 사용 완료 후 호출할것.
        /// </summary>
        public void DeInitiallize()
        {
            //생성된 BAT 파일 삭제
            File.Delete(_batFile_Path);
            //설정된 다른 값 전부 초기화
            _folder_Path = null;
            _FullName_Of_File_to_Convert = null;
            _Name_Of_File_To_Convert = null;
            _path_Of_File_To_Convert = null;
        }
        /// <summary>
        /// 변환할 파일 세팅
        /// </summary>
        /// <param name="filePath"> 변환할 파일 경로</param>
        /// <exception cref="Exception"></exception>
        public void SetConvertFile(String filePath)
        {
            //aab파일인지 검사
            if (Path.GetExtension(filePath) == ".aab")
            {
                _FullName_Of_File_to_Convert = Path.GetFileName(filePath);
                _Name_Of_File_To_Convert = Path.GetFileNameWithoutExtension(_path_Of_File_To_Convert);
                if (!File.Exists(Path.Combine(_folder_Path, _Name_Of_File_To_Convert)))
                    File.Copy(filePath, Path.Combine(_folder_Path, _Name_Of_File_To_Convert));
                _path_Of_File_To_Convert = Path.Combine(_folder_Path, _Name_Of_File_To_Convert);
            }
            else
                throw new Exception($"{filePath} is Not .aab File!!");
        }
        /// <summary>
        /// 변환 작업 시작.
        /// </summary>
        /// <param name="ConsolePrinted"></param>
        /// <param name="ProcessHasExited"></param>
        public void ConvertProcessStart(System.Diagnostics.DataReceivedEventHandler ConsolePrinted, EventHandler ProcessHasExited)
        {
            //예외처리
            if (_IsConverting)
                return;
            if (_path_Of_File_To_Convert == null)
                return;
            if (!File.Exists(_path_Of_File_To_Convert))
                return;

            //batch파일 생성
            string batformat = "";
            string batFormat_Path = Path.Combine(_folder_Path, BAT_FORMAT);
            _Name_Of_File_To_Convert = Path.GetFileNameWithoutExtension(_path_Of_File_To_Convert);
            foreach (var i in File.ReadAllLines(batFormat_Path))
            {
                string edited = i.Replace("%FOLDER_PATH%", _folder_Path);
                batformat += edited.Replace("%FILE_NAME%", _Name_Of_File_To_Convert) + "\n";
            }
            _batFile_Path = Path.Combine(_folder_Path, BAT_NAME);
            System.IO.File.WriteAllText(_batFile_Path, batformat);

            //프로세스 준비
            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            process.StartInfo = processStartInfo;
            //프로세스 종료시 호출될 이벤트
            process.Exited += ProcessHasExited;

            //프로세스 시작
            process.Start();
            //프로세스 입력
            process.StandardInput.WriteLine(_batFile_Path);
            //비동기로 출력읽기
            process.BeginOutputReadLine();
            //에러 혹은 출력발생시 출력할 이벤트 추가
            process.ErrorDataReceived += ConsolePrinted;
            process.OutputDataReceived += ConsolePrinted += new System.Diagnostics.DataReceivedEventHandler(colsoleOutput);
            _IsConverting = true;
        }
        private void colsoleOutput(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            ResultAnalize(e.Data);
        }
        private void StopProcess()
        {
            process.CancelOutputRead();
            process.Close();
            DeInitiallize();
        }
        public void ResultAnalize(string data)
        {
            if (data == null)
                return;
            if (data == "Finished")
            {
                StopProcess();
                if (File.Exists(Path.Combine(_folder_Path, _FullName_Of_File_to_Convert.Replace("aab", "apk"))))
                {
                    File.Delete(Path.Combine("BundleTool", _FullName_Of_File_to_Convert));
                    //시작위치 폴더 열기
                    System.Diagnostics.Process.Start(System.Windows.Forms.Application.StartupPath);
                }
                return;
            }
            if (data.Contains("toc.pb을(를) 찾을 수 없습니다."))
            {
                throw new Exception("toc.pb을(를) 찾을 수 없습니다. 재시도 하십시오");
            }
            else if (data.Contains("명령 구문이 올바르지 않습니다."))
            {
                throw new Exception("apk name이 올바르지 않습니다.\n파일 이름은 universial.apk 입니다");
            }
            else if (data.Contains("Error: Unable to access jarfile bundletool-all-1.3.0.jar"))
            {
                throw new Exception("Unable to Access Jarfile \nBundleTool 파일에 접근할수 없습니다. 경로 혹은 파일 재확인 필요합니다.");
            }
            else if (data.Contains(".apks을(를) 찾을 수 없습니다."))
            {
                throw new Exception(".apks을(를) 찾을 수 없습니다. \n bundletool이 실패했거나 .keystore 파일이 없을수 있습니다. 재시도 하십시오");
            }
        }

    }
}