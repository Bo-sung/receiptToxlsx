namespace Ragnarok
{
    public abstract class CharacterEntityModel : UnitModel<CharacterEntity>
    {
        protected void Notify(UserInfoData packet)
        {
            Entity.User?.Initialize(packet);
            Entity.Goods?.SetCatCoin(packet.totalCatCoin);
            Entity.ShopModel?.Initialize(packet);
        }

        protected void Notify(CharacterPacket packet)
        {
            Entity.Character?.Initialize(packet);
            Entity.Tutorial?.Initialize(packet.tutorialFinish);
            Entity.Goods?.SetZeny(packet.zeny);
            Entity.Goods?.SetRoPoint(packet.roPoint);
            Entity.Goods?.SetGuildCoin(packet.guildCoin);
            Entity.Goods?.SetNormalQuestCoin(packet.normalQuestCoin);
            Entity.Status?.Initialize(packet);
            Entity.Status?.Initialize(packet.autoStat);
            Entity.Skill?.SetSkillPoint(packet.remainSkillPoint);
            Entity.Skill?.SetIsFreeSkillReset(packet.isFreeSkillReset);
            Entity.Dungeon?.Initialize(packet);
            Entity.Inventory?.SetInvenWeight(packet.invenWeight, packet.invenWeightBuyCount);
            Entity.Guild?.Initialize(packet);
            Entity.League?.Initialize(packet);
            Entity.Sharing?.Initialize(packet);
            Entity.Duel?.Initialize(packet);
            Entity.ShopModel?.Initialize(packet);
            Entity.User?.ConnnectInfo();
            Entity.ChatModel?.JoinChannelChat();
            Entity.Event?.Initialize(packet);
            Entity.CupetList.Initialize(null);
        }

        protected void Notify(CharacterSkillData[] arrPacket)
        {
            Entity?.Skill.Initialize(arrPacket);
        }

        protected void Notify(CharacterSkillSlotData[] arrPacket)
        {
            Entity?.Skill.Initialize(arrPacket);
        }

        protected void Notify(CharacterItemData[] arrPacket)
        {
            Entity?.Inventory.Initialize(arrPacket);
            Entity?.Make.Initialize();
        }

        protected void Notify(BuffPacket[] arrPacket)
        {
            Entity.BuffItemList?.Initialize(arrPacket);
        }

        protected void Notify(QuestPacket packet)
        {
            Entity.Quest?.Initialize(packet);
        }

        protected void Notify(EventBuffPacket packet)
        {
            Entity.EventBuff?.Initialize(packet);
        }

        protected void Notify(EventDuelBuffPacket packet)
        {
            Entity.EventBuff?.Initialize(packet);
        }

        protected void Notify(GuildSkillPacket[] arrPacket)
        {
            Entity.Guild?.Initialize(arrPacket);
        }

        protected void Notify(ShopPacket packet)
        {
            Entity.ShopModel?.Initialize(packet);
        }

        protected void Notify(AuctionItemPacket packet)
        {
            Entity.Trade?.Initialize(packet);
        }

        protected void Notify(PrivateStoreItemPacket[] arrPacket)
        {
            Entity.Trade?.Initialize(arrPacket);
        }

        protected void Notify(WorldBossAlarmPacket[] arrPacket)
        {
            Entity.Dungeon?.Initialize(arrPacket);
        }

        protected void Notify(CharUpdateData packet)
        {
            if (packet is null)
                return;

            Entity.Goods?.Update(packet.zeny, packet.catCoin, packet.changedZeny, packet.guildCoin, packet.normalQuestCoin, packet.roPoint, packet.onBuffPoint);
            // 레벨업 전에 스탯포인트 갱신을 해야 한다.
            Entity.Status?.Update(packet.statData, packet.statPoint);
            Entity.Character?.UpdateData(packet.level, packet.levelExp, packet.jobLevel, packet.jobExp, packet.accrueStatPoint);
            Entity.Skill?.UpdateData(packet.skillPoint);

            Entity.Dungeon?.UpdateFreeTicket(packet.dungeonTicket, packet.dungeonCount
                , packet.worldBossTicket, packet.worldBossCount
                , packet.defenceDungeonTicket, packet.defenceDungeonCount
                , packet.stageBossTicket);

            Entity.Dungeon?.UpdateWorldBossRemainTime(packet.worldBossRemainTime);

            Entity.Inventory?.UpdateData(packet.items);
            Entity.BuffItemList?.UpdateData(packet.buffs);

            Entity.League?.UpdateData(packet.pveTicket, packet.pveCount);
            Entity.UpdateBattleScore(packet.battleScore);

            Entity.Dungeon?.UpdateDayMultiMazeTicket(packet.dayMultiMazeTicket, packet.dayMultiMazeCount);
            Entity.Dungeon?.UpdateSummonMvpTicket(packet.summonMvpTicket);
            Entity.Dungeon?.UpdateDayZenyDungeonTicket(packet.dayZenyDungeonTicket, packet.dayZenyDungeonCount);
            Entity.Dungeon?.UpdateDayExpDungeonTicket(packet.dayExpDungeonTicket, packet.dayExpDungeonCount);
            Entity.Dungeon?.UpdateCentralLabTicket(packet.centralLabFreeTicket, packet.centralLabTryCount);
            Entity.Dungeon?.UpdateEventMultiMazeTicket(packet.eventMultiMazeFreeTicket, packet.eventMultiMazeEntryCount);
            Entity.Dungeon?.UpdateEndlessTowerTicket(packet.endlessTowerFreeTicket, packet.endlessTowerCooldownTime);
            Entity.Dungeon?.UpdateForestMazeTicket(packet.forestMazeFreeTicket, packet.forestMazeEntryCount);

            Entity.Agent?.UpdateAgentData(packet.charAgents);
            Entity.Sharing?.Update(packet.share_char_use_ticket_10m, packet.share_char_use_ticket_30m, packet.share_char_use_ticket_60m);
            Entity.Sharing?.SetShareviceExperience(packet.shareLevel, packet.shareExp);
            Entity.ShopModel?.UpdateJobLevelInfo(packet.jobLevelPackageShopId, packet.jobLevelPackageReaminTime);
            Entity.ShopModel?.UpdatePassExp(PassType.Labyrinth, packet.passExp);
            Entity.ShopModel?.UpdatePassExp(PassType.OnBuff, packet.onBuffPassExp);
            Entity.ShopModel?.UpdateOnBuffMvpPoint(packet.onBuffMvpPoint);
        }

        protected void Notify(DailyInitPacket packet)
        {
            Entity.Dungeon?.UpdateFreeTicket(packet.day_field_dungeon_ticket, packet.day_field_dungeon_count
                , packet.day_world_boss_ticket, packet.day_world_boss_count
                , packet.day_def_dungeon_ticket, packet.day_def_dungeon_count, null);
            Entity.Dungeon?.UpdateDayMultiMazeTicket(packet.day_multi_maze_ticket, packet.day_multi_maze_count);
            Entity.Dungeon?.UpdateDayZenyDungeonTicket(packet.day_zeny_dungeon_ticket, packet.day_zeny_dungeon_count);
            Entity.Dungeon?.UpdateDayExpDungeonTicket(packet.day_exp_dungeon_ticket, packet.day_exp_dungeon_count);
            Entity.Dungeon?.UpdateCentralLabTicket(packet.day_central_lab_free_ticket, packet.day_central_lab_count);
            Entity.Dungeon?.UpdateDungeonFreeReward(packet.dayDungeonFreeReward);
            Entity.Dungeon?.UpdateEventMultiMazeTicket(packet.eventMultiMazeFreeTicket, packet.eventMultiMazeEntryCount);
            Entity.Dungeon?.ResetChallengClearCount();
            Entity.Dungeon?.ResetFreeTicketCount();
            Entity.Dungeon?.ResetEventDarkMazeEntryFlag();
            Entity.League?.UpdateData(packet.day_pve_ticket, packet.day_pve_count);
            Entity.Sharing?.SetDailyFreeShareTicket(packet.share_char_use_daily_ticket);
            Entity.Duel?.SetDuelPointBuyCount(packet.duel_point_buy_count);
            Entity.ShopModel?.UpdateOnBuffMvpPoint(0); // 초기화
            Entity.ShopModel?.UpdateReceivedOnBuffPoint(false); // 초기화
        }

        protected void Notify(EventBannerPacket[] arrPacket)
        {
            Entity.Event?.Initialize(arrPacket);
        }

        protected void Notify(CharAgentPacket[] arrPacket, CharAgentBookPacket[] agentBookIDs)
        {
            Entity.Agent?.Initialize(arrPacket, agentBookIDs);
        }

        protected void NotifyAlarm(int aralmValue)
        {
            AlarmType alarmType = aralmValue.ToEnum<AlarmType>();
            Entity.AlarmModel?.Initialize(alarmType);
        }

        protected void Notify(AgentSlotInfoPacket[] packets)
        {
            Entity.Agent?.UpdateSlotState(packets);
        }

        protected void Notify(AgentExploreCountInfo[] countInfos)
        {
            Entity.Agent?.InitializeExploreCountInfo(countInfos);
        }

        protected void NotifyRemainTimeToMidnight(float sec)
        {
            Entity.Daily?.SetRemainTimeToMidnight(sec);
        }

        protected void Notify(CharacterSharingPacket packet)
        {
            if (packet == null)
            {
                Entity.Sharing?.SetSharingState(SharingModel.SharingState.None);
                Entity.Sharing?.SetSharingReward(null);
                Entity.Sharing?.SetSharingEmployer(null);
                Entity.Sharing?.SetSharingFilter(new int[0]);
                return;
            }

            Entity.Sharing?.SetSharingState(packet.shareFlag.ToEnum<SharingModel.SharingState>());
            Entity.Sharing?.SetSharingReward(packet.sharingRewardPacket);
            Entity.Sharing?.SetSharingEmployer(packet.sharingEmployerPacket);
            Entity.Sharing?.SetSharingFilter(packet.jobFilterAry);
        }

        protected void Notify(EventRoulettePacket packet)
        {
            Entity.Event.Initialize(packet);
        }

        protected void Notify(ChatNoticePacket packet)
        {
            Entity.ChatModel?.SetNotice(packet.noti_message, packet.noti_version);
        }

        protected void NotifySubChannelId(int subChannelId)
        {
            Entity.Dungeon?.SetSubChannelId(subChannelId);
        }

        protected void NotifyInfoOpen(bool isInfoOpen)
        {
            if (Entity.User != null)
                Entity.User.IsOpenInfo = isInfoOpen;
        }

        protected void NotifyShareExitAutoSetting(bool isShareExitAutoSetting)
        {
            if (Entity.Sharing != null)
                Entity.Sharing.SetShareExitAutoSetting(isShareExitAutoSetting);
        }

        protected void NotifyAddAlarm(AlarmType type)
        {
            if (Entity.AlarmModel != null)
                Entity.AlarmModel.AddAlarm(type);
        }

        protected void NotifySharePushSetting(bool isSharePush)
        {
            if (Entity.User != null)
                Entity.User.SetSharePushSetting(isSharePush);
        }

        protected void NotifyChannelIndex(int channelIndex)
        {
            if (Entity.User != null)
                Entity.User.SetChannelIndex(channelIndex);

            Entity.ShopModel?.SetChannelIndex(channelIndex);
        }

        protected void Notify(TamingDungeonInfoPacket packet)
        {
            Entity.Guild?.InitializeTamingMazeInfo(packet);
        }

        protected void NotifyReceiveShopMail(int shopId)
        {
            Entity.ShopModel?.ReceiveShopMail(shopId);
        }

        protected void NotifyUpdateTreeRemainTime(long remainTime)
        {
            Entity.ShopModel?.UpdateTreeRemainTime(remainTime);
        }

        protected void Notify(TamingDungeonTimePacket packet)
        {
            ServerTime.Initialize(packet.currentServerTime);
            Entity.Guild?.Initialize(packet);
        }

        /// <summary>
        /// 가방 무게 증가
        /// </summary>
        protected void NotifyInvenWeight()
        {
            Entity.Inventory?.AddInvenWeight();
        }

        /// <summary>
        /// 상점 구매 불가 상품 (이미 구매후 우편함에 존제)
        /// </summary>
        /// <param name="shopIds"></param>
        protected void Notify(int[] shopIds)
        {
            Entity.ShopModel?.Initialize(shopIds);
        }

        protected void Notify(BookPacket packet)
        {
            Entity.Book?.Initialize(packet);
        }

        protected void Notify(BingoSeasonPacket curSeasonPacket, BingoSeasonPacket nextSeasonPacket)
        {
            Entity.Bingo?.Initialize(curSeasonPacket, nextSeasonPacket);
        }

        protected void Notify(BingoStatePacket packet)
        {
            Entity.Bingo?.Initialize(packet);
        }

        protected void NotyfyBookRecord(BookTabType bookTabType, int bookIndex)
        {
            Entity.Book?.Record(bookTabType, bookIndex);
        }

        protected void Notify(SpecialRoulettePacket packet)
        {
            Entity.Event?.Initialize(packet);
        }

        protected void Notify(EventQuizPacket packet)
        {
            Entity.Event?.Initialize(packet);
        }

        protected void NotifyDuelPoint(int point)
        {
            Entity.Duel?.SetDualPoint(point);
        }

        protected void Notify(OverStatusPacket packet)
        {
            Entity.Status?.Initialize(packet);
        }

        protected void Notify(CustomerRewardPacket packet)
        {
            Entity.ShopModel?.Initialize(packet);
        }

        protected void Notify(EventRpsPacket packet)
        {
            Entity.Event?.Initialize(packet);
        }

        protected void Notify(CatCoinGiftPacket packet)
        {
            Entity.Event?.Initialize(packet);
        }

        protected void Notify(EventDicePacket packet)
        {
            Entity.Event?.Initialize(packet);
        }

        protected void Notify(EventChallengePacket packet)
        {
            Entity.Dungeon?.Initialize(packet);
        }

        protected void Notify(DarkTreePacket packet)
        {
            Entity.Inventory?.Initialize(packet);
        }

        protected void NotifyFinalTimePatrolLevel(int level)
        {
            Entity.Dungeon?.InitializeFinalTimePatrolLevel(level);
        }

        protected void NotifyLastTimePatrolId(int id)
        {
            Entity.Dungeon?.InitializeLastTimePatrolId(id);
        }

        protected void Notify(TimeSuitPacket[] packets)
        {
            Entity.Character?.Initialize(packets);
        }

        protected void NotifyTimeSuit(ShareForceType type, int level)
        {
            Entity.Character.SetShareForceLevel(type, level);
        }

        protected void Notify(KafraCompleteType kafraCompleteType)
        {
            Entity.Quest?.Initialize(kafraCompleteType);
        }

        protected void Notify(KafraDeliveryPacket[] packets)
        {
            Entity.Quest?.Initialize(packets);
        }

        protected void Notify(FriendInvitePacket packet)
        {
            Entity.Friend?.Initialize(packet);
        }

        protected void Notify(AttendEventPacket packet)
        {
            Entity.Event?.Initialize(packet);
        }

        protected void Notify(PassPacket packet)
        {
            Entity.ShopModel?.Initialize(packet);
        }

        protected void Notify(PassSeasonPacket packet)
        {
            Entity.ShopModel?.Initialize(packet);
        }

        protected void Notify(NabihoPacket[] packets)
        {
            Entity.Inventory?.Initialize(packets);
        }

        protected void Notify(WordCollectionPacket packet)
        {
            Entity.Event?.Initialize(packet);
        }

        protected void NotifyOnBuffPoint(int value)
        {
            Entity.Goods?.UpdateOnBuffPoint(value);
        }

        protected void Notify(OnBuffPassPacket packet)
        {
            Entity.ShopModel?.Initialize(packet);
        }

        protected void Notify(OnBuffPassSeasonPacket packet)
        {
            Entity.ShopModel?.Initialize(packet);
        }
    }
}