using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class MakeData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt display_type;
        public readonly ObscuredInt sort_index;
        public readonly ObscuredInt des_id;
        public readonly ObscuredInt cost;
        public readonly ObscuredInt result;
        public readonly ObscuredInt result_count;
        public readonly List<Tuple<ObscuredInt, ObscuredInt, ObscuredInt>> needItems;
        public readonly ObscuredShort rate;
        public readonly ObscuredInt details_tab;
        public readonly ObscuredByte enable_type;
        public readonly ObscuredInt job_lv;

        public MakeData(IList<MessagePackObject> data)
        {
            id           = data[0].AsInt32();
            display_type = data[1].AsInt32();
            sort_index   = data[2].AsInt32();
            des_id       = data[3].AsInt32();
            cost         = data[4].AsInt32();
            result       = data[5].AsInt32();
            result_count = data[6].AsInt32();

            int value_1  = data[7].AsInt32();
            int count_1  = data[8].AsInt32();
            int level_1  = data[9].AsInt32();

            int value_2  = data[10].AsInt32();
            int count_2  = data[11].AsInt32();
            int level_2  = data[12].AsInt32();

            int value_3  = data[13].AsInt32();
            int count_3  = data[14].AsInt32();
            int level_3  = data[15].AsInt32();

            int value_4  = data[16].AsInt32();
            int count_4  = data[17].AsInt32();
            int level_4  = data[18].AsInt32();

            int value_5  = data[19].AsInt32();
            int count_5  = data[20].AsInt32();
            int level_5  = data[21].AsInt32();

            int value_6  = data[22].AsInt32();
            int count_6  = data[23].AsInt32();
            int level_6  = data[24].AsInt32();

            int value_7  = data[25].AsInt32();
            int count_7  = data[26].AsInt32();
            int level_7  = data[27].AsInt32();

            int value_8  = data[28].AsInt32();
            int count_8  = data[29].AsInt32();
            int level_8  = data[30].AsInt32();

            rate         = data[31].AsInt16();
            details_tab  = data[32].AsByte();
            enable_type  = data[33].AsByte();
            job_lv       = data[34].AsInt32();

            needItems = new List<Tuple<ObscuredInt, ObscuredInt, ObscuredInt>>();
            if (value_1 != 0 && count_1 != 0)
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_1, count_1, level_1));
            if (value_2 != 0 && count_2 != 0)
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_2, count_2, level_2));
            if (value_3 != 0 && count_3 != 0)
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_3, count_3, level_3));
            if (value_4 != 0 && count_4 != 0)
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_4, count_4, level_4));
            if (value_5 != 0 && count_5 != 0)
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_5, count_5, level_5));
            if (value_6 != 0 && count_6 != 0)
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_6, count_6, level_6));
            if (value_7 != 0 && count_7 != 0)
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_7, count_7, level_7));
            if (value_8 != 0 && count_8 != 0)
            {
                needItems.Add(new Tuple<ObscuredInt, ObscuredInt, ObscuredInt>(value_8, count_8, level_8));
            }

#if UNITY_EDITOR

            // 필요 재료는 있는데 수량이 0인 경우가 있으면 오류

            if (value_1 != 0 && count_1 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_1={value_1}, count_1={count_1}");

            if (value_2 != 0 && count_2 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_2={value_2}, count_2={count_2}");

            if (value_3 != 0 && count_3 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_3={value_3}, count_3={count_3}");

            if (value_4 != 0 && count_4 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_4={value_4}, count_4={count_4}");

            if (value_5 != 0 && count_5 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_5={value_5}, count_5={count_5}");

            if (value_6 != 0 && count_6 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_6={value_6}, count_6={count_6}");

            if (value_7 != 0 && count_7 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_7={value_7}, count_7={count_7}");

            if (value_8 != 0 && count_8 == 0)
                Debug.LogError($"020.제작테이블 오류 : ID = {id}, value_8={value_8}, count_8={count_8}");

#endif
        }

    }
}
