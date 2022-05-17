using System;
using System.Collections.Generic;
using MyGoogleServices;


namespace SheetViewer
{
    public class TestSheet : GoogleSheetLayout
    {
        public struct Layout
        {
            public string Param0 { get; set; }
            public string Param1 { get; set; }
            public string Param2 { get; set; }
            public string Param3 { get; set; }
            public string Param4 { get; set; }
            public string Param5 { get; set; }
            public string Param6 { get; set; }
            public string Param7 { get; set; }
            public string Param8 { get; set; }
            public string Param9 { get; set; }
            public string Param10 { get; set; }
            public string Param11 { get; set; }
            public string Param12 { get; set; }
            public string Param13 { get; set; }
            public string Param14 { get; set; }
            public string Param15 { get; set; }
            public string Param16 { get; set; }
            public string Param17 { get; set; }
            public string Param18 { get; set; }
            public string Param19 { get; set; }
        }

        public List<Layout> Layouts { get; set; }
        public List<string> Heads { get; set; }

        public (List<Layout>, List<string>) GetLayouWithHeads()
        {
            return (Layouts, Heads);
        }
        public TestSheet(IList<IList<object>> Sheet) : base(Sheet)
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
                if (value.Count == 0)
                    continue;
                index = 0;
                Layout _layout = new Layout();

                _layout.Param0 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param1 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param2 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param3 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param4 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param5 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param6 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param7 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param8 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param9 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param10 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param11 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param12 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param13 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param14 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param15 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param16 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param17 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param18 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                _layout.Param19 = value[index++].ToString(); if (index == value.Count) { Layouts.Add(_layout); continue; }
                Layouts.Add(_layout);
            }
            Layouts.RemoveAt(0);
        }
    }

}
