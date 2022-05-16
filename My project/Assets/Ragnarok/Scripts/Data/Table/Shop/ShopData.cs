using CodeStage.AntiCheat.ObscuredTypes;
using MsgPack;
using System.Collections.Generic;

namespace Ragnarok
{
    public sealed class ShopData : IData
    {
        public readonly ObscuredInt id;
        public readonly ObscuredInt name_id;
        public readonly ObscuredInt des_id;
        public readonly ObscuredString icon_name;
        public readonly ObscuredByte pay_type;
        public readonly ObscuredInt pay_value;
        public readonly ObscuredByte sell_type;
        public readonly ObscuredInt sell_value;        
        public readonly ObscuredByte goods_type;
        public readonly ObscuredInt goods_value;
        public readonly ObscuredShort goods_count;
        public readonly ObscuredByte condition_type;
        public readonly ObscuredByte add_goods_type;
        public readonly ObscuredInt add_goods_value;
        public readonly ObscuredShort add_goods_count;
        public readonly ObscuredByte gender; // 미사용
        public readonly ObscuredByte job;
        public readonly ObscuredInt shop_tab;
        public readonly ObscuredInt tab_order;
        public readonly ObscuredByte limit_day_type;
        public readonly ObscuredInt limit_day_count;
        public readonly ObscuredInt limit_id;
        public readonly ObscuredInt sale; // 세일 퍼센트 표시 (0~99) // 세일은 캐쉬구매로 사용 할 수 없다
        public readonly ObscuredInt sale_pay_value; // 세일 이전가격 표시용
        public readonly ObscuredInt state; // New, Best, Hot 상태 표시
        public readonly ObscuredLong timeout;
        public readonly ObscuredInt price;
        public readonly ObscuredInt local_price;
        public readonly ObscuredInt usd_price;
        public readonly ObscuredString dia_shop_code;
        public readonly ObscuredString google_product_id;
        public readonly ObscuredString apple_product_id;
        public readonly ObscuredString tstore_product_id;
        public readonly ObscuredByte visable_type;
        public readonly ObscuredInt visable_value;
        public readonly ObscuredInt visable_value_2;
        public readonly ObscuredByte is_log;
        public readonly ObscuredByte first_day_type;
        public readonly ObscuredInt first_pos;
        public readonly ObscuredInt first_rate;
        public readonly ObscuredInt mileage;
        public readonly ObscuredInt pay_back_event_value;
        public readonly ObscuredInt limit_type;
        public readonly ObscuredString mail_icon;
        public readonly ObscuredInt package_type;
        public readonly ObscuredInt productvalue_type; // 직업 레벨 패키지 UI 표시 타입 (0 : 표기 없음, 1 : productvalue_value배 가치, 2: productvalue_value% 할인
        public readonly ObscuredInt productvalue_value;
        public readonly ObscuredInt des_id_2; // 직업 레벨 패키지 포링 설명 ID

        public ShopData(IList<MessagePackObject> data)
        {            
            byte index           = 0;
            id                   = data[index++].AsInt32();
            name_id              = data[index++].AsInt32();
            des_id               = data[index++].AsInt32();
            icon_name            = data[index++].AsString();
            pay_type             = data[index++].AsByte();
            pay_value            = data[index++].AsInt32();
            sell_type            = data[index++].AsByte();
            sell_value           = data[index++].AsInt32();
            goods_type           = data[index++].AsByte();
            goods_value          = data[index++].AsInt32();
            goods_count          = data[index++].AsInt16();
            condition_type       = data[index++].AsByte();
            add_goods_type       = data[index++].AsByte();
            add_goods_value      = data[index++].AsInt32();
            add_goods_count      = data[index++].AsInt16();
            gender               = data[index++].AsByte();
            job                  = data[index++].AsByte();
            shop_tab             = data[index++].AsInt32();
            tab_order            = data[index++].AsInt32();
            limit_day_type       = data[index++].AsByte();
            limit_day_count      = data[index++].AsInt32();
            limit_id             = data[index++].AsInt32();
            sale                 = data[index++].AsInt32();
            sale_pay_value       = data[index++].AsInt32();
            state                = data[index++].AsInt32();
            timeout              = data[index++].AsInt64();
            price                = data[index++].AsInt32();
            local_price          = data[index++].AsInt32();
            usd_price            = data[index++].AsInt32();
            dia_shop_code        = data[index++].AsString();
            google_product_id    = data[index++].AsString();
            apple_product_id     = data[index++].AsString();
            tstore_product_id    = data[index++].AsString();
            visable_type         = data[index++].AsByte();
            visable_value        = data[index++].AsInt32();
            visable_value_2      = data[index++].AsInt32();
            is_log               = data[index++].AsByte();
            first_day_type       = data[index++].AsByte();
            first_pos            = data[index++].AsInt32();
            first_rate           = data[index++].AsInt32();
            mileage              = data[index++].AsInt32();
            pay_back_event_value = data[index++].AsInt32();
            limit_type           = data[index++].AsInt32();
            mail_icon            = data[index++].AsString();
            package_type         = data[index++].AsInt32();
            productvalue_type    = data[index++].AsInt32();
            productvalue_value   = data[index++].AsInt32();
            des_id_2             = data[index++].AsInt32();
        }
    }
}