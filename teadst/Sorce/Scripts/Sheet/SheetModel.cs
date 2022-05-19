using System;
using System.Collections.Generic;
using System.Windows;
using MyGoogleServices;

namespace SheetViewer
{
    public class SheetModel : IModel
    {
        public SheetModel(IGoogleManager manager, String SpreadSheetID, String Range)
        {
            _manager = manager;
            Initialize(SpreadSheetID, Range);
        }
        public bool IsSheetAccessible()
        {
            return GetSheet() != null;
        }

        private IGoogleManager _manager;
        private String _spreadsheetId = "1jLZ3UmfGTPzjSUSpCtJu1Y6jPLqHQtZc1cWOgVXf91U";
        private String _range = "[NFT]WB.HACK!F2";
        /// <summary>
        /// 시트 모델 초기화.
        /// </summary>
        /// <param name="SpreadSheetID">시트의 키값</param>
        /// <param name="Range">시트 내에서 받아올 구역</param>
        public void Initialize(String SpreadSheetID, String Range)
        {
            _spreadsheetId = SpreadSheetID;
            _range = Range;
        }
        /// <summary>
        /// 시트 받아옴
        /// </summary>
        /// <returns></returns>
        public IList<IList<object>> GetSheet()
        {
            if (_manager == null)
            {
                MessageBox.Show("시트 접근 불가!!!. 계정 확인후 재시도 token.json 폴더안 파일 삭제 후 재시도", "시트 접근 불가!!!");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return null;
            }
            var sheetManager = (GoogleSheetManager)_manager;
            return sheetManager.GetSheet(_spreadsheetId, _range);
        }
        /// <summary>
        /// 버전 시트 규격으로 시트 받아옴
        /// </summary>
        /// <returns></returns>
        public VersionSheet GetVerSheet()
        {
            IList<IList<object>> sheet = GetSheet();
            return new VersionSheet(sheet);
        }

        public List<VersionSheet.Layout> GetVerSheetList()
        {
            IList<IList<object>> sheet = GetSheet();
            return new VersionSheet(sheet).Data;
        }

        public (List<GeneralSheet.Layout>, List<string>) GetGeneralSheetLayoutWithHead()
        {
            IList<IList<object>> sheet = GetSheet();
            return new GeneralSheet(sheet).GetLayouWithHeads();
        }

        public GeneralSheet GetGeneralSheet()
        {
            IList<IList<object>> sheet = GetSheet();
            return new GeneralSheet(sheet, true);
        }

        /// <summary>
        /// 직전에 사용한 버전
        /// </summary>
        int _prev_version;
        /// <summary>
        /// 직전에 사용한 플랫폼
        /// </summary>
        String _prev_Platform;
        /// <summary>
        /// 직전에 사용한 레이아웃
        /// </summary>
        VersionSheet.Layout _prev_layout;
        /// <summary>
        /// 직전에 사용한 플랫폼 리스트
        /// </summary>
        List<VersionSheet.Layout> _prev_PlatformList;
        public bool Is_prev_serched_sheet(int version, String Platform, out VersionSheet.Layout layout)
        {
            if (Platform == _prev_Platform &&
                version == _prev_version &&
                _prev_layout.Date != null
                )
            {
                layout = _prev_layout;
                return true;
            }
            layout = new VersionSheet.Layout();
            return false;
        }
        public void Set_Prev_Serched_sheet(int version, String Platform, List<VersionSheet.Layout> PlatformList)
        {
            _prev_version = version;
            _prev_Platform = Platform;
            _prev_PlatformList = PlatformList;
            _prev_layout = PlatformList[version];
            //GetVerSheet().Find(VersionSheet.Layout_Types.Platform, Platform, out _prev_PlatformList);
        }
        public bool Is_PrevSerchedPlatform(String PlatForm, out List<VersionSheet.Layout> PlatformList)
        {
            if (PlatForm == _prev_Platform)
            {
                PlatformList = _prev_PlatformList;
                return true;
            }
            GetVerSheet().Find(VersionSheet.Layout_Types.Platform, PlatForm, out PlatformList);
            PlatformList.RemoveAll(IsBundleVerNullOrEmpty);
            //VersionSheet.Layout layout = PlatformList[0];
            //for(int i)
            //{
            //    PlatformList.Remove(layout);
            //}
            return false;
        }
        private bool IsBundleVerNullOrEmpty(VersionSheet.Layout layout)
        {
            return layout.BundleVer == null;
        }
        /// <summary>
        /// 시트 데이터 추출. 실패시 에러 메세지 출력 및 false 리턴
        /// </summary>
        /// <param name="layout"></param>
        /// <returns></returns>
        public bool TryGetLayoutLine(String _cur_version, String _cur_Platform, out VersionSheet.Layout layout)
        {
            layout = new VersionSheet.Layout();
            int Version;
            //입력값 확인
            //Latest 인경우 0 저장.
            if (_cur_version == "Latest" || _cur_version == "SheetVersion")
                Version = 0;
            else if (!int.TryParse(_cur_version, out Version)) //만약 정수가 아닐경우 출력
            {
                MessageBox.Show("Version is Only Input Latest or Numbers", "Error");
                return false;
            }
            //플랫폼 텍스트 입력
            String Platform = _cur_Platform;
            //대소문자 허용
            switch (Platform.ToLower())
            {
                case "ios":
                    {
                        Platform = "iOS";
                    }
                    break;
                case "android":
                    {
                        Platform = "Android";
                    }
                    break;
            }
            //만약 직전에 불러온 시트와 동일하다면 다시 보내줌.
            if (Is_prev_serched_sheet(Version, Platform, out layout))
                return true;
            Is_PrevSerchedPlatform(Platform, out List<VersionSheet.Layout> PlatformList);//시트에서 플랫폼으로 검색 후 True시 기존 데이터를 리턴. False시 새로 찾아서 리턴.

            if (Version > PlatformList.Count)   //시트 위치에서 벗어나는 값이 입력될 경우
            {
                MessageBox.Show("SheetRow Is Overed. check SheetVersion", "Error");
                return false;
            }
            if (Version < 0)    //버전 값이 0보다 작을경우
            {
                MessageBox.Show("Version is More than 0", "Error");
                return false;
            }
            layout = PlatformList[Version];
            Set_Prev_Serched_sheet(Version, Platform, PlatformList);
            return true;
        }
    }
}
