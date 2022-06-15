using System;
using System.Collections.Generic;
using MyGoogleServices;
using System.Windows;


namespace SheetViewer
{
    /// <summary>
    /// 종류 상관없이 들고오는 시트. 20행까지 가능 
    /// </summary>
    public class GeneralSheet : GoogleSheetLayout
    {
        public struct LayoutElement
        {
            public string Value { get; set; }
            public Point position { get; set; }
        }

        public struct Layout
        {
            public int Length;
            public int Column;
            public string Row_A {get; set;}
            public string Row_B {get; set;}
            public string Row_C {get; set;}
            public string Row_D {get; set;}
            public string Row_E {get; set;}
            public string Row_F {get; set;}
            public string Row_G {get; set;}
            public string Row_H {get; set;}
            public string Row_I {get; set;}
            public string Row_J {get; set;}
            public string Row_K {get; set;}
            public string Row_L {get; set;}
            public string Row_M {get; set;}
            public string Row_N {get; set;}
            public string Row_O {get; set;}
            public string Row_P {get; set;}
            public string Row_Q {get; set;}
            public string Row_R {get; set;}
            public string Row_S {get; set;}
            public string Row_T {get; set;}

            public string GetRowData(int index)
            {
                switch (index)
                {
                    case 0:     return Row_A;
                    case 1:     return Row_B;
                    case 2:     return Row_C;
                    case 3:     return Row_D;
                    case 4:     return Row_E;
                    case 5:     return Row_F;
                    case 6:     return Row_G;
                    case 7:     return Row_H;
                    case 8:     return Row_I;
                    case 9:     return Row_J;
                    case 10:    return Row_K;
                    case 11:    return Row_L;
                    case 12:    return Row_M;
                    case 13:    return Row_N;
                    case 14:    return Row_O;
                    case 15:    return Row_P;
                    case 16:    return Row_Q;
                    case 17:    return Row_R;
                    case 18:    return Row_S;
                    case 19:    return Row_T;
                }
                return "Error GeneralSheet RowRangeOver";
            }

            public bool Contain(string param, out int index)
            {
                for(index = 0; index < Length; ++index)
                {
                    if (param == GetRowData(index))
                        return true;
                }
                index = -1;
                return false;
            }
        }

        public struct LayoutWithPos
        {
            public int Length;
            public int Column;
            public LayoutElement Row_A { get; set; }
            public LayoutElement Row_B { get; set; }
            public LayoutElement Row_C { get; set; }
            public LayoutElement Row_D { get; set; }
            public LayoutElement Row_E { get; set; }
            public LayoutElement Row_F { get; set; }
            public LayoutElement Row_G { get; set; }
            public LayoutElement Row_H { get; set; }
            public LayoutElement Row_I { get; set; }
            public LayoutElement Row_J { get; set; }
            public LayoutElement Row_K { get; set; }
            public LayoutElement Row_L { get; set; }
            public LayoutElement Row_M { get; set; }
            public LayoutElement Row_N { get; set; }
            public LayoutElement Row_O { get; set; }
            public LayoutElement Row_P { get; set; }
            public LayoutElement Row_Q { get; set; }
            public LayoutElement Row_R { get; set; }
            public LayoutElement Row_S { get; set; }
            public LayoutElement Row_T { get; set; }

            public string GetRowData(int index)
            {
                switch (index)
                {
                    case 0:  return Row_A.Value;
                    case 1:  return Row_B.Value;
                    case 2:  return Row_C.Value;
                    case 3:  return Row_D.Value;
                    case 4:  return Row_E.Value;
                    case 5:  return Row_F.Value;
                    case 6:  return Row_G.Value;
                    case 7:  return Row_H.Value;
                    case 8:  return Row_I.Value;
                    case 9:  return Row_J.Value;
                    case 10: return Row_K.Value;
                    case 11: return Row_L.Value;
                    case 12: return Row_M.Value;
                    case 13: return Row_N.Value;
                    case 14: return Row_O.Value;
                    case 15: return Row_P.Value;
                    case 16: return Row_Q.Value;
                    case 17: return Row_R.Value;
                    case 18: return Row_S.Value;
                    case 19: return Row_T.Value;
                }
                return "Error GeneralSheet RowRangeOver";
            }

            public bool Contain(string param, out int index)
            {
                for (index = 0; index < Length; ++index)
                {
                    if (param == GetRowData(index))
                        return true;
                }
                index = -1;
                return false;
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

        public string[][] SheetDataArrayArray;

        /// <summary>
        /// 헤드 데이터 기준 딕셔너리. 시트의 특정 값과 일치하는 행 찾기 등에 사용.
        /// </summary>
        public Dictionary<LayoutTypes, Dictionary<LayoutElement, List<LayoutWithPos>>> LayoutDic { get; set; }

        public List<Dictionary<LayoutTypes, string>> LayoutListWithDic { get; set; }

        public (List<Layout>, List<string>) GetLayouWithHeads()
        {
            return (Layouts, Heads);
        }

        /// <summary>
        /// 미리 로드.
        /// </summary>
        private void PreLoad()
        {

        }

        private void InitSheet(IList<IList<object>> Sheet)
        {
            InitSheetData(Sheet);
        }

        /// <summary>
        /// 리스트 형식 시트 데이터 소요시간 큼
        /// </summary>
        private void InitSheetData(IList<IList<object>> Sheet)
        {
            //시트는 2차원 배열.

            Layouts = new List<Layout>();
            Heads = new List<string>();
            LayoutListWithDic = new List<Dictionary<LayoutTypes, string>>();
            LayoutDic = new Dictionary<LayoutTypes, Dictionary<LayoutElement, List<LayoutWithPos>>>();

            int row = 0;
            while (true)
            {
                Heads.Add(Sheet[0][row++].ToString());
                if (row == Sheet[0].Count)
                    break;
            }
            //행
            int column = 0;
            SheetDataArrayArray = new string[Sheet.Count][];
            foreach (var value in Sheet)
            {
                //시트 데이터는 빈칸이 있는 경우 빈칸을 할당하지 않음.

                //시트 첫줄은 목차.
                if (value.Count == 0)
                    continue;
                //열
                row = 0;
                Layout _layout = new Layout();
                LayoutWithPos _layoutWithPos = new LayoutWithPos();
                SheetDataArrayArray[column] = new string[value.Count];

                //배열에 Value값 등록, 리스트에 들어갈 layout 구조체값 등록 후 길이 저장, 길이가 밸류값의 크기와 같으면 구조체를 리스트에 추가 및 딕셔너리에 추가   
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_A = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_A = new LayoutElement { Value = _layout.Row_A, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_B = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_B = new LayoutElement { Value = _layout.Row_B, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_C = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_C = new LayoutElement { Value = _layout.Row_C, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_D = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_D = new LayoutElement { Value = _layout.Row_D, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_E = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_E = new LayoutElement { Value = _layout.Row_E, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_F = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_F = new LayoutElement { Value = _layout.Row_F, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_G = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_G = new LayoutElement { Value = _layout.Row_G, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_H = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_H = new LayoutElement { Value = _layout.Row_H, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_I = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_I = new LayoutElement { Value = _layout.Row_I, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_J = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_J = new LayoutElement { Value = _layout.Row_J, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_K = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_K = new LayoutElement { Value = _layout.Row_K, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_L = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_L = new LayoutElement { Value = _layout.Row_L, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_M = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_M = new LayoutElement { Value = _layout.Row_M, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_N = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_N = new LayoutElement { Value = _layout.Row_N, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_O = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_O = new LayoutElement { Value = _layout.Row_O, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_P = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_P = new LayoutElement { Value = _layout.Row_P, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_Q = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_Q = new LayoutElement { Value = _layout.Row_Q, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_R = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_R = new LayoutElement { Value = _layout.Row_R, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_S = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_S = new LayoutElement { Value = _layout.Row_S, position = new Point(_layout.Column, _layout.Length), }; if (row == value.Count) { column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos); continue; }
                SheetDataArrayArray[column][row] = (string)value[row]; _layout.Row_T = (string)value[row++]; _layout.Length = row; _layoutWithPos.Row_T = new LayoutElement { Value = _layout.Row_T, position = new Point(_layout.Column, _layout.Length), };                           column++; Layouts.Add(_layout); FillDic(_layout, _layoutWithPos);
            }
            Layouts.RemoveAt(0);
            MaxColumn = Layouts.Count -1;
        }

        private void FillDic(Layout _layout, LayoutWithPos _layoutWithPos)
        {
            int index = 0;
            Dictionary<LayoutTypes, string> dic = new Dictionary<LayoutTypes, string>();
            AddToDic(LayoutTypes.Row_A, new LayoutElement { Value = _layout.Row_A, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_A, _layout.Row_A);
            AddToDic(LayoutTypes.Row_B, new LayoutElement { Value = _layout.Row_B, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_B, _layout.Row_B);
            AddToDic(LayoutTypes.Row_C, new LayoutElement { Value = _layout.Row_C, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_C, _layout.Row_C);
            AddToDic(LayoutTypes.Row_D, new LayoutElement { Value = _layout.Row_D, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_D, _layout.Row_D);
            AddToDic(LayoutTypes.Row_E, new LayoutElement { Value = _layout.Row_E, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_E, _layout.Row_E);
            AddToDic(LayoutTypes.Row_F, new LayoutElement { Value = _layout.Row_F, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_F, _layout.Row_F);
            AddToDic(LayoutTypes.Row_G, new LayoutElement { Value = _layout.Row_G, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_G, _layout.Row_G);
            AddToDic(LayoutTypes.Row_H, new LayoutElement { Value = _layout.Row_H, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_H, _layout.Row_H);
            AddToDic(LayoutTypes.Row_I, new LayoutElement { Value = _layout.Row_I, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_I, _layout.Row_I);
            AddToDic(LayoutTypes.Row_J, new LayoutElement { Value = _layout.Row_J, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_J, _layout.Row_J);
            AddToDic(LayoutTypes.Row_K, new LayoutElement { Value = _layout.Row_K, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_K, _layout.Row_K);
            AddToDic(LayoutTypes.Row_L, new LayoutElement { Value = _layout.Row_L, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_L, _layout.Row_L);
            AddToDic(LayoutTypes.Row_M, new LayoutElement { Value = _layout.Row_M, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_M, _layout.Row_M);
            AddToDic(LayoutTypes.Row_N, new LayoutElement { Value = _layout.Row_N, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_N, _layout.Row_N);
            AddToDic(LayoutTypes.Row_O, new LayoutElement { Value = _layout.Row_O, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_O, _layout.Row_O);
            AddToDic(LayoutTypes.Row_P, new LayoutElement { Value = _layout.Row_P, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_P, _layout.Row_P);
            AddToDic(LayoutTypes.Row_Q, new LayoutElement { Value = _layout.Row_Q, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_Q, _layout.Row_Q);
            AddToDic(LayoutTypes.Row_R, new LayoutElement { Value = _layout.Row_R, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_R, _layout.Row_R);
            AddToDic(LayoutTypes.Row_S, new LayoutElement { Value = _layout.Row_S, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_S, _layout.Row_S);
            AddToDic(LayoutTypes.Row_T, new LayoutElement { Value = _layout.Row_T, position = new Point(_layout.Column, index++), }, _layoutWithPos); dic.Add(LayoutTypes.Row_T, _layout.Row_T);
            LayoutListWithDic.Add(dic);
        }

        private void AddToDic(LayoutTypes types, LayoutElement key, LayoutWithPos layout)
        {
            if (LayoutDic == null)
            {
                LayoutDic = new Dictionary<LayoutTypes, Dictionary<LayoutElement, List<LayoutWithPos>>>();
            }
            if (key.Equals(new LayoutElement()))
                return;

            if (LayoutDic.TryGetValue(types, out Dictionary<LayoutElement, List<LayoutWithPos>> data) == false)
            {
                LayoutDic.Add(types, new Dictionary<LayoutElement, List<LayoutWithPos>>());
                data = LayoutDic[types];
            }

            if (data.TryGetValue(key, out List<LayoutWithPos> valuelayout) == false)
            {
                data.Add(key, new List<LayoutWithPos>());
                valuelayout = data[key];
            }

            valuelayout.Add(layout);
        }

        public static bool Find()
        {
            return false;
        }

        /// <summary>
        /// 단일 파라미터로 검색.
        /// </summary>
        public static bool Find(Dictionary<LayoutTypes, Dictionary<string, List<Layout>>> LayoutDic, LayoutTypes type, string param, out List<Layout> whereList)
        {
            whereList = new List<Layout>();
            //딕셔너리에 있으면 값 꺼내고 참 리턴
            if (LayoutDic.TryGetValue(type, out Dictionary<string, List<Layout>> dic))
                if (dic.TryGetValue(param, out whereList))
                    return true;
            //없으면 빈값과 거짓 리턴
            return false;
        }



        /// <summary>
        /// 단일 파라미터로 검색.
        /// </summary>
        public bool Find(LayoutTypes type, LayoutElement param, out List<LayoutWithPos> searchResult)
        {
            searchResult = new List<LayoutWithPos>();
            //딕셔너리에 있으면 값 꺼내고 참 리턴
            if (LayoutDic.TryGetValue(type, out Dictionary<LayoutElement, List<LayoutWithPos>> dic))
                if (dic.TryGetValue(param, out searchResult))
                    return true;
            //없으면 빈값과 거짓 리턴
            return false;
        }

        /// <summary>
        /// 다중 파라미터로 검색
        /// </summary>
        public static bool Find(List<Layout> Layouts, Dictionary<LayoutTypes, string> SearchData, out List<Layout> searchResult)
        {
            searchResult = Layouts;

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
                    case (int)LayoutTypes.Row_A: { searchResult = searchResult.FindAll(element => element.Row_A == value); } break;
                    case (int)LayoutTypes.Row_B: { searchResult = searchResult.FindAll(element => element.Row_B == value); } break;
                    case (int)LayoutTypes.Row_C: { searchResult = searchResult.FindAll(element => element.Row_C == value); } break;
                    case (int)LayoutTypes.Row_D: { searchResult = searchResult.FindAll(element => element.Row_D == value); } break;
                    case (int)LayoutTypes.Row_E: { searchResult = searchResult.FindAll(element => element.Row_E == value); } break;
                    case (int)LayoutTypes.Row_F: { searchResult = searchResult.FindAll(element => element.Row_F == value); } break;
                    case (int)LayoutTypes.Row_G: { searchResult = searchResult.FindAll(element => element.Row_G == value); } break;
                    case (int)LayoutTypes.Row_H: { searchResult = searchResult.FindAll(element => element.Row_H == value); } break;
                    case (int)LayoutTypes.Row_I: { searchResult = searchResult.FindAll(element => element.Row_I == value); } break;
                    case (int)LayoutTypes.Row_J: { searchResult = searchResult.FindAll(element => element.Row_J == value); } break;
                    case (int)LayoutTypes.Row_K: { searchResult = searchResult.FindAll(element => element.Row_K == value); } break;
                    case (int)LayoutTypes.Row_L: { searchResult = searchResult.FindAll(element => element.Row_L == value); } break;
                    case (int)LayoutTypes.Row_M: { searchResult = searchResult.FindAll(element => element.Row_M == value); } break;
                    case (int)LayoutTypes.Row_N: { searchResult = searchResult.FindAll(element => element.Row_N == value); } break;
                    case (int)LayoutTypes.Row_O: { searchResult = searchResult.FindAll(element => element.Row_O == value); } break;
                    case (int)LayoutTypes.Row_P: { searchResult = searchResult.FindAll(element => element.Row_P == value); } break;
                    case (int)LayoutTypes.Row_Q: { searchResult = searchResult.FindAll(element => element.Row_Q == value); } break;
                    case (int)LayoutTypes.Row_R: { searchResult = searchResult.FindAll(element => element.Row_R == value); } break;
                    case (int)LayoutTypes.Row_S: { searchResult = searchResult.FindAll(element => element.Row_S == value); } break;
                    case (int)LayoutTypes.Row_T: { searchResult = searchResult.FindAll(element => element.Row_T == value); } break;
                }
            }

            if (searchResult != null)
                return true;

            //없으면 빈값과 거짓 리턴
            return false;
        }

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
    }

}
