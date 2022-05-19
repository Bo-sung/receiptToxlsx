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
                return "";
            }
        }

        public List<Layout> Layouts { get; set; }
        public List<string> Heads { get; set; }
        bool _ispreloaded;
        private List<string[]> _preloadedRows;
        private List<string[]> _preloadedColumm;
        [Obsolete("교체중")]
        private string[,] _sheetMatrixArray;
        private string[][] _sheetArrayArray;
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

                _layout.Row_A = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_B = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_C = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_D = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_E = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_F = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_G = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_H = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_I = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_J = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_K = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_L = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_M = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_N = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_O = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_P = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_Q = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_R = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_S = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Row_T = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                Layouts.Add(_layout);
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
