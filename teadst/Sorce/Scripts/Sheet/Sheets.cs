using System;
using System.Collections.Generic;
using MyGoogleServices;


namespace SheetViewer
{
    /// <summary>
    /// 종류 상관없이 들고오는 시트. 20행까지 가능 
    /// </summary>
    public class GeneralSheet : GoogleSheetLayout
    {
        public struct Layout
        {
            public string Row_A { get; set; }
            public string Row_B { get; set; }
            public string Row_C { get; set; }
            public string Row_D { get; set; }
            public string Row_E { get; set; }
            public string Row_F { get; set; }
            public string Row_G { get; set; }
            public string Row_H { get; set; }
            public string Row_I { get; set; }
            public string Row_J { get; set; }
            public string Row_K { get; set; }
            public string Row_L { get; set; }
            public string Row_M { get; set; }
            public string Row_N { get; set; }
            public string Row_O { get; set; }
            public string Row_P { get; set; }
            public string Row_Q { get; set; }
            public string Row_R { get; set; }
            public string Row_S { get; set; }
            public string Row_T { get; set; }

            public string GetRowData(int index)
            {
                switch (index)
                {
                    case 0: return Row_A;
                    case 1: return Row_B;
                    case 2: return Row_C;
                    case 3: return Row_D;
                    case 4: return Row_E;
                    case 5: return Row_F;
                    case 6: return Row_G;
                    case 7: return Row_H;
                    case 8: return Row_I;
                    case 9: return Row_J;
                    case 10: return Row_K;
                    case 11: return Row_L;
                    case 12: return Row_M;
                    case 13: return Row_N;
                    case 14: return Row_O;
                    case 15: return Row_P;
                    case 16: return Row_Q;
                    case 17: return Row_R;
                    case 18: return Row_S;
                    case 19: return Row_T;
                }
                return "Error GeneralSheet RowRangeOver";
            }
        }

        public enum LayoutTypes
        {
            Row_A = 0,
            Row_B = 1,
            Row_C = 2,
            Row_D = 3,
            Row_E = 4,
            Row_F = 5,
            Row_G = 6,
            Row_H = 7,
            Row_I = 8,
            Row_J = 9,
            Row_K = 10,
            Row_L = 11,
            Row_M = 12,
            Row_N = 13,
            Row_O = 14,
            Row_P = 15,
            Row_Q = 16,
            Row_R = 17,
            Row_S = 18,
            Row_T = 19,
        }

        /// <summary>
        /// 단순 리스트
        /// </summary>
        public List<Layout> Layouts { get; set; }
        /// <summary>
        /// 시트 헤드
        /// </summary>
        public List<string> Heads { get; set; }
        public int MaxColumn { get; set; }
        public int MaxRow { get; set; }

        bool _ispreloaded;
        private List<string[]> _preloadedRows;
        private List<string[]> _preloadedColumm;

        private string[][] _sheetArrayArray;

        /// <summary>
        /// 헤드 데이터 기준 딕셔너리. 시트의 특정 값과 일치하는 행 찾기 등에 사용.
        /// </summary>
        public Dictionary<LayoutTypes, Dictionary<string, List<Layout>>> LayoutDic { get; set; }

        public List<Dictionary<LayoutTypes, string>> LayoutListWithDic { get; set; }

        public (List<Layout>, List<string>) GetLayouWithHeads()
        {
            return (Layouts, Heads);
        }
        public string[] GetRow(string head)
        {
            return GetRow(Heads.IndexOf(head));
        }
        public string[] GetRow(int index)
        {
            if (index == -1)
                return null;

            if (_ispreloaded)
                return _preloadedRows[index];

            string[] Rows = new string[_sheetArrayArray.Length - 1];
            for (int y = 0; y < _sheetArrayArray.Length - 1; y++)
            {
                if (_sheetArrayArray[y].Length <= index)
                {
                    Rows[y] = "";
                    continue;
                }
                Rows[y] = _sheetArrayArray[y][index];
            }

            return Rows;
        }
        public string[] GetCol(string head)
        {
            return GetCol(Heads.IndexOf(head));
        }
        public string[] GetCol(int index)
        {
            if (index == -1)
                return null;

            if (_ispreloaded)
                return _preloadedColumm[index];

            string[] col = new string[_sheetArrayArray.Length];
            for (int y = 0; y < _sheetArrayArray.Length; y++)
            {
                col = _sheetArrayArray[y];
            }
            return col;
        }

        /// <summary>
        /// 미리 로드.
        /// </summary>
        private void PreLoad()
        {
            _preloadedColumm = new List<string[]>();
            _preloadedRows = new List<string[]>();


            for (int y = 0; y < _sheetArrayArray.Length; y++)
            {
                _preloadedColumm.Add(_sheetArrayArray[y]);

                string[] Rows = new string[_sheetArrayArray.Length -1];

                for (int y2 = 0; y2 < _sheetArrayArray.Length -1; y2++)
                {
                    if (_sheetArrayArray[y2].Length <= y)
                    {
                        Rows[y2] = "";
                        continue;
                    }
                    Rows[y2] = _sheetArrayArray[y2][y];
                }
                _preloadedRows.Add(Rows);
            }
        }

        private void InitSheet(IList<IList<object>> Sheet)
        {
            initSheetArr(Sheet);
            InitSheetList(Sheet);
        }

        /// <summary>
        /// 리스트 형식 시트 데이터 소요시간 큼
        /// </summary>
        private void InitSheetList(IList<IList<object>> Sheet)
        {
            //시트는 2차원 배열.

            Layouts = new List<Layout>();
            Heads = new List<string>();
            LayoutListWithDic = new List<Dictionary<LayoutTypes, string>>();
            LayoutDic = new Dictionary<LayoutTypes, Dictionary<string, List<Layout>>>();

            int index = 0;
            while (true)
            {
                Heads.Add(Sheet[0][index++].ToString());
                if (index == Sheet[0].Count)
                    break;
            }

            foreach (var value in Sheet)
            {
                //시트 데이터는 빈칸이 있는 경우 빈칸을 할당하지 않음.

                //시트 첫줄은 목차.
                if (value.Count == 0)
                    continue;
                index = 0;
                Layout _layout = new Layout();

                _layout.Row_A = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_B = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_C = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_D = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_E = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_F = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_G = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_H = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_I = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_J = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_K = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_L = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_M = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_N = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_O = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_P = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_Q = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_R = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_S = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                _layout.Row_T = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); FillDic(_layout); continue; }
                Layouts.Add(_layout);
                FillDic(_layout);
            }
            Layouts.RemoveAt(0);
        }

        /// <summary>
        /// 배열 형식 시트 데이터 소요시간 큼
        /// </summary>
        private void initSheetArr(IList<IList<object>> Sheet)
        {
            int x, y;
            _sheetArrayArray = new string[Sheet.Count][];
            for (y = 0; y < Sheet.Count - 1; y++)
            {
                _sheetArrayArray[y] = new string[Sheet[y + 1].Count];
                for (x = 0; x < Sheet[y + 1].Count; x++)
                {
                    //시트 데이터는 빈칸이 있는 경우 빈칸을 할당하지 않음.
                    _sheetArrayArray[y][x] = Sheet[y + 1][x].ToString();
                }
            }
        }

        private void FillDic(Layout _layout)
        {
            Dictionary<LayoutTypes, string> dic = new Dictionary<LayoutTypes, string>();
            AddToDic(LayoutTypes.Row_A, _layout.Row_A, _layout);        dic.Add(LayoutTypes.Row_A, _layout.Row_A);
            AddToDic(LayoutTypes.Row_B, _layout.Row_B, _layout);        dic.Add(LayoutTypes.Row_B, _layout.Row_B);
            AddToDic(LayoutTypes.Row_C, _layout.Row_C, _layout);        dic.Add(LayoutTypes.Row_C, _layout.Row_C);
            AddToDic(LayoutTypes.Row_D, _layout.Row_D, _layout);        dic.Add(LayoutTypes.Row_D, _layout.Row_D);
            AddToDic(LayoutTypes.Row_E, _layout.Row_E, _layout);        dic.Add(LayoutTypes.Row_E, _layout.Row_E);
            AddToDic(LayoutTypes.Row_F, _layout.Row_F, _layout);        dic.Add(LayoutTypes.Row_F, _layout.Row_F);
            AddToDic(LayoutTypes.Row_G, _layout.Row_G, _layout);        dic.Add(LayoutTypes.Row_G, _layout.Row_G);
            AddToDic(LayoutTypes.Row_H, _layout.Row_H, _layout);        dic.Add(LayoutTypes.Row_H, _layout.Row_H);
            AddToDic(LayoutTypes.Row_I, _layout.Row_I, _layout);        dic.Add(LayoutTypes.Row_I, _layout.Row_I);
            AddToDic(LayoutTypes.Row_J, _layout.Row_J, _layout);        dic.Add(LayoutTypes.Row_J, _layout.Row_J);
            AddToDic(LayoutTypes.Row_K, _layout.Row_K, _layout);        dic.Add(LayoutTypes.Row_K, _layout.Row_K);
            AddToDic(LayoutTypes.Row_L, _layout.Row_L, _layout);        dic.Add(LayoutTypes.Row_L, _layout.Row_L);
            AddToDic(LayoutTypes.Row_M, _layout.Row_M, _layout);        dic.Add(LayoutTypes.Row_M, _layout.Row_M);
            AddToDic(LayoutTypes.Row_N, _layout.Row_N, _layout);        dic.Add(LayoutTypes.Row_N, _layout.Row_N);
            AddToDic(LayoutTypes.Row_O, _layout.Row_O, _layout);        dic.Add(LayoutTypes.Row_O, _layout.Row_O);
            AddToDic(LayoutTypes.Row_P, _layout.Row_P, _layout);        dic.Add(LayoutTypes.Row_P, _layout.Row_P);
            AddToDic(LayoutTypes.Row_Q, _layout.Row_Q, _layout);        dic.Add(LayoutTypes.Row_Q, _layout.Row_Q);
            AddToDic(LayoutTypes.Row_R, _layout.Row_R, _layout);        dic.Add(LayoutTypes.Row_R, _layout.Row_R);
            AddToDic(LayoutTypes.Row_S, _layout.Row_S, _layout);        dic.Add(LayoutTypes.Row_S, _layout.Row_S);
            AddToDic(LayoutTypes.Row_T, _layout.Row_T, _layout);        dic.Add(LayoutTypes.Row_T, _layout.Row_T);
            LayoutListWithDic.Add(dic);
        }

        private void AddToDic(LayoutTypes types, string key, Layout layout)
        {
            if (LayoutDic == null)
            {
                LayoutDic = new Dictionary<LayoutTypes, Dictionary<string, List<Layout>>>();
            }
            if (key == null)
                return;

            if (LayoutDic.TryGetValue(types, out Dictionary<string, List<Layout>> data) == false)
            {
                LayoutDic.Add(types, new Dictionary<string, List<Layout>>());
                data = LayoutDic[types];
            }

            if (data.TryGetValue(key, out List<Layout> valuelayout) == false)
            {
                data.Add(key, new List<Layout>());
                valuelayout = data[key];
            }
            valuelayout.Add(layout);
        }


        /// <summary>
        /// 단일 파라미터로 검색.
        /// </summary>
        public bool Find(LayoutTypes type, string param, out List<Layout> whereList)
        {
            whereList = new List<Layout>();
            //딕셔너리에 있으면 값 꺼내고 참 리턴
            if (LayoutDic.TryGetValue(type, out Dictionary<string, List<Layout>> dic))
                if (dic.TryGetValue(param, out whereList))
                    return true;
            //없으면 빈값과 거짓 리턴
            return false;
        }

        //다중 파라미터로 검색
        public bool Find(Dictionary<LayoutTypes, string> SearchData, out List<Layout> whereList)
        {
            whereList = Layouts;

            //열거형 갯수 만큼 반복
            for (int i = 0; i < System.Enum.GetValues(typeof(LayoutTypes)).Length; ++i)
            {
                //현재 열거형이 검색 데이터에 존재 하는지 확인
                if (!SearchData.TryGetValue((LayoutTypes)i, out string value))
                {
                    continue;
                }
                switch (i)
                {
                    case (int)LayoutTypes.Row_A: { whereList = whereList.FindAll(element => element.Row_A == value); } break;
                    case (int)LayoutTypes.Row_B: { whereList = whereList.FindAll(element => element.Row_B == value); } break;
                    case (int)LayoutTypes.Row_C: { whereList = whereList.FindAll(element => element.Row_C == value); } break;
                    case (int)LayoutTypes.Row_D: { whereList = whereList.FindAll(element => element.Row_D == value); } break;
                    case (int)LayoutTypes.Row_E: { whereList = whereList.FindAll(element => element.Row_E == value); } break;
                    case (int)LayoutTypes.Row_F: { whereList = whereList.FindAll(element => element.Row_F == value); } break;
                    case (int)LayoutTypes.Row_G: { whereList = whereList.FindAll(element => element.Row_G == value); } break;
                    case (int)LayoutTypes.Row_H: { whereList = whereList.FindAll(element => element.Row_H == value); } break;
                    case (int)LayoutTypes.Row_I: { whereList = whereList.FindAll(element => element.Row_I == value); } break;
                    case (int)LayoutTypes.Row_J: { whereList = whereList.FindAll(element => element.Row_J == value); } break;
                    case (int)LayoutTypes.Row_K: { whereList = whereList.FindAll(element => element.Row_K == value); } break;
                    case (int)LayoutTypes.Row_L: { whereList = whereList.FindAll(element => element.Row_L == value); } break;
                    case (int)LayoutTypes.Row_M: { whereList = whereList.FindAll(element => element.Row_M == value); } break;
                    case (int)LayoutTypes.Row_N: { whereList = whereList.FindAll(element => element.Row_N == value); } break;
                    case (int)LayoutTypes.Row_O: { whereList = whereList.FindAll(element => element.Row_O == value); } break;
                    case (int)LayoutTypes.Row_P: { whereList = whereList.FindAll(element => element.Row_P == value); } break;
                    case (int)LayoutTypes.Row_Q: { whereList = whereList.FindAll(element => element.Row_Q == value); } break;
                    case (int)LayoutTypes.Row_R: { whereList = whereList.FindAll(element => element.Row_R == value); } break;
                    case (int)LayoutTypes.Row_S: { whereList = whereList.FindAll(element => element.Row_S == value); } break;
                    case (int)LayoutTypes.Row_T: { whereList = whereList.FindAll(element => element.Row_T == value); } break;
                }                         
            }

            if (whereList != null)
                return true;

            //없으면 빈값과 거짓 리턴
            return false;
        }


    public GeneralSheet(IList<IList<object>> Sheet) : base(Sheet)
        {
            InitSheet(Sheet);
        }

        public GeneralSheet(IList<IList<object>> Sheet, bool isPreLoad) : base(Sheet)
        {
            InitSheet(Sheet);
            if (!isPreLoad)
                return;
            PreLoad();
            _ispreloaded = true;
        }
    }

}
