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
        }

        public List<Layout> Layouts { get; set; }
        public List<string> Heads { get; set; }

        public (List<Layout>, List<string>) GetLayouWithHeads()
        {
            return (Layouts, Heads);
        }
        public GeneralSheet(IList<IList<object>> Sheet) : base(Sheet)
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
    }

}
