namespace Ragnarok
{
    public sealed class DailyInitPacket : IPacket<Response>
    {
        public int day_field_dungeon_count;
        public int day_central_lab_count;
        public short day_field_dungeon_ticket;
        public short day_central_lab_free_ticket;

        [System.Obsolete]
        public byte is_used_hottime; // 1: 사용가능, 0: 불가능

        public short day_world_boss_ticket;
        public short day_def_dungeon_ticket;
        public int day_world_boss_count;
        public int day_def_dungeon_count;
        public int day_pve_ticket;
        public int day_pve_count;
        public int share_char_use_daily_ticket;
        public int day_multi_maze_ticket;
        public int day_multi_maze_count;
        public int duel_point_buy_count;
        public int day_exp_dungeon_ticket;
        public int day_exp_dungeon_count;
        public int day_zeny_dungeon_ticket;
        public int day_zeny_dungeon_count;
        public int dayDungeonFreeReward; // 던전3종 일일 무료보상 수령여부
        public int eventMultiMazeFreeTicket;
        public int eventMultiMazeEntryCount;

        void IInitializable<Response>.Initialize(Response response)
        {
            day_field_dungeon_count = response.GetInt("1");
            day_central_lab_count = response.GetInt("2");
            day_field_dungeon_ticket = response.GetShort("3");
            day_central_lab_free_ticket = response.GetShort("4");
            is_used_hottime = response.GetByte("5");
            day_world_boss_ticket = response.GetShort("6");
            day_def_dungeon_ticket = response.GetShort("7");
            day_world_boss_count = response.GetInt("8");
            day_def_dungeon_count = response.GetInt("9");
            day_pve_ticket = response.GetShort("10");
            day_pve_count = response.GetInt("11");
            share_char_use_daily_ticket = response.GetByte("12");
            day_multi_maze_ticket = response.GetInt("13");
            day_multi_maze_count = response.GetInt("14");
            duel_point_buy_count = response.GetInt("15");
            day_exp_dungeon_ticket = response.GetInt("16");
            day_exp_dungeon_count = response.GetInt("17");
            day_zeny_dungeon_ticket = response.GetInt("18");
            day_zeny_dungeon_count = response.GetInt("19");
            dayDungeonFreeReward = response.GetInt("20");
            eventMultiMazeFreeTicket = response.GetByte("21");
            eventMultiMazeEntryCount = response.GetShort("22");
            //byte forestTicketCount = response.GetByte("23");
            //short forestTicketBuyCount = response.GetShort("24");
            //int guildBattleCount = response.GetInt("25");
            //byte eventDarkMazeEntryFlag = response.GetByte("26");
            //int onbuffMvpPoint = response.GetInt("27");
        }
    }
}