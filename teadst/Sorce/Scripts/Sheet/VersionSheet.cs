using System;
using System.Collections.Generic;
using MyGoogleServices;

namespace SheetViewer
{
    /// <summary>
    /// 버전관리 문서 전용 시트 포맷.
    /// </summary>
    public sealed class VersionSheet : GoogleSheetLayout
    {
        public List<Layout> Data;
        public Layout[] Data_arr;
        /// <summary>
        /// 시트 레이아웃
        /// </summary>
        public struct Layout
        {
            /// <summary>
            /// 날짜
            /// </summary>
            public string Date;
            /// <summary>
            /// 플랫폼
            /// </summary>
            public string Platform;
            /// <summary>
            /// 번들 버전
            /// </summary>
            public string BundleVer;
            /// <summary>
            /// 버전 코드
            /// </summary>
            public string Vercode;
            /// <summary>
            /// 서버
            /// </summary>
            public string Server;
            /// <summary>
            /// 지역
            /// </summary>
            public string Locale;
            /// <summary>
            /// 비고
            /// </summary>
            public string State;
            /// <summary>
            /// 다운로드 링크
            /// </summary>
            public string DownloadLink;
            /// <summary>
            /// 앱 이름
            /// </summary>
            public string AppName;
            public String MakeString()
            {
                String str = Date + "   " +
                    Platform + "   " +
                    BundleVer + "   " +
                    Vercode + "   " +
                    Server + "   " +
                    Locale + "   " +
                    State + "   " +
                    DownloadLink + "   " +
                    AppName + "    ";
                return str;
            }
        }

        /// <summary>
        /// 레이아웃 타입
        /// </summary>
        public enum Layout_Types
        {
            /// <summary>
            /// 날짜
            /// </summary>
            Date,
            /// <summary>
            /// 플랫폼
            /// </summary>
            Platform,
            /// <summary>
            /// 번들 버전
            /// </summary>
            BundleVer,
            /// <summary>
            /// 버전 코드
            /// </summary>
            Vercode,
            /// <summary>
            /// 서버
            /// </summary>
            Server,
            /// <summary>
            /// 지역
            /// </summary>
            Locale,
            /// <summary>
            /// 비고
            /// </summary>
            State,
            /// <summary>
            /// 다운로드 링크
            /// </summary>
            DownloadLink,
            /// <summary>
            /// 앱 이름
            /// </summary>
            AppName
        }
        /// <summary>
        /// 버전 시트 데이터 정리.
        /// </summary>
        /// <param name="Sheet"></param>
        public VersionSheet(IList<IList<object>> Sheet) : base(Sheet)
        {
            //시트는 2차원 배열.

            Data = new List<Layout>();
            foreach (var value in Sheet)
            {
                //시트 데이터는 빈칸이 있는 경우 빈칸을 할당하지 않음.
                if (value.Count == 0)
                    continue;
                int index = 0;
                Layout _layout = new Layout();

                _layout.Date = value[index++].ToString(); if (index == value.Count) { Data.Add(_layout); continue; }
                _layout.Platform = value[index++].ToString(); if (index == value.Count) { Data.Add(_layout); continue; }
                _layout.BundleVer = value[index++].ToString(); if (index == value.Count) { Data.Add(_layout); continue; }
                _layout.Vercode = value[index++].ToString(); if (index == value.Count) { Data.Add(_layout); continue; }
                _layout.Server = value[index++].ToString(); if (index == value.Count) { Data.Add(_layout); continue; }
                _layout.Locale = value[index++].ToString(); if (index == value.Count) { Data.Add(_layout); continue; }
                _layout.State = value[index++].ToString(); if (index == value.Count) { Data.Add(_layout); continue; }
                _layout.DownloadLink = value[index++].ToString();
                _layout.AppName = _layout.DownloadLink.Replace("http://cdn.convtf.com/ragcube/build/", "");
                if (_layout.DownloadLink.IndexOf("http://cdn.convtf.com/ragcube/build/") == -1)
                {
                    _layout.AppName = _layout.DownloadLink.Replace("http://cdn.convtf.com/ragcube/Labyrinth/Build/", "");
                }
                if (index == value.Count) { Data.Add(_layout); continue; }
                Data.Add(_layout);
            }
            Data_arr = new Layout[Sheet.Count];
        }
        /// <summary>
        /// 시트 내 데이터 검색. 하드 카피라 오랜 시간 소요됨
        /// </summary>
        /// <param name="type">검색할 타입</param>
        /// <param name="param">검색할 데이터</param>
        /// <param name="whereList">결과 출력 리스트</param>
        /// <returns></returns>
        public bool Find(Layout_Types type, string param, out List<Layout> whereList)
        {
            whereList = new List<Layout>();
            switch (type)
            {
                case Layout_Types.Date:
                    {
                        foreach (var i in Data)
                            if (i.Date == param)
                                whereList.Add(i);
                        return true;
                    }
                case Layout_Types.DownloadLink:
                    {
                        foreach (var i in Data)
                            if (i.DownloadLink == param)
                                whereList.Add(i);
                        return true;
                    }
                case Layout_Types.BundleVer:
                    {
                        foreach (var i in Data)
                            if (i.BundleVer == param)
                                whereList.Add(i);
                        return true;
                    }
                case Layout_Types.Locale:
                    {
                        foreach (var i in Data)
                            if (i.Locale == param)
                                whereList.Add(i);
                        return true;
                    }
                case Layout_Types.Platform:
                    {
                        string _allowedParam = param;
                        foreach (var i in Data)
                            if (i.Platform == _allowedParam)
                                whereList.Add(i);
                        return true;
                    }
                case Layout_Types.Server:
                    {
                        foreach (var i in Data)
                            if (i.Server == param)
                                whereList.Add(i);
                        return true;
                    }
                case Layout_Types.State:
                    {
                        foreach (var i in Data)
                            if (i.State == param)
                                whereList.Add(i);
                        return true;
                    }
                case Layout_Types.Vercode:
                    {
                        foreach (var i in Data)
                            if (i.Vercode == param)
                                whereList.Add(i);
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}
