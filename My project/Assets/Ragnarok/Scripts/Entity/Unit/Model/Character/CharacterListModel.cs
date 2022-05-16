using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 캐릭터 목록 정보
    /// </summary>
    public class CharacterListModel : CharacterEntityModel
    {
        public enum ReconnectionType
        {
            None,
            ServerChange,
        }

        private readonly SkillDataManager.ISkillDataRepoImpl skillDataRepoImpl;
        private readonly ProfileDataManager.IProfileDataRepoImpl profileDataRepoImpl;
        private readonly LoginManager loginManager;
        private readonly IGamePotImpl gamePot;
        private readonly BetterList<SimpleCharacterPacket> simpleCharacterList; // 보유중인 캐릭터 목록        

        /// <summary>
        /// 캐릭터 생성 시 호출
        /// </summary>
        public event Action OnCreateCharacter;

        /// <summary>
        /// 캐릭터 초기화 시 호출
        /// </summary>
        public event Action OnCharacterInit;

        /// <summary>
        /// 캐릭터 삭제 대기 요청 이벤트
        /// </summary>
        public event Action<int> OnCharacterDeleteWaiting;

        /// <summary>
        /// 캐릭터 삭제 요청 이벤트
        /// </summary>
        public event Action OnCharacterDelete;

        /// <summary>
        /// 캐릭터 삭제 취소 이벤트
        /// </summary>
        public event Action<int> OnCharacterDeleteCancel;

        /// <summary>
        /// 캐릭터 정보 업데이트
        /// </summary>
        public event Action OnUpdateCharacter;

        /// <summary>
        /// 네이버 배너 띄우기 여부
        /// </summary>
        public bool isShowNaverBanner;

        /// <summary>
        /// 최초 한 번 입장 체크
        /// </summary>
        public bool isFirstJoinGameMap = true;

        /// <summary>
        /// 선택한 캐릭터 CID
        /// </summary>
        public int SelectedCharacterCid { get; private set; }

        public CharacterListModel()
        {
            skillDataRepoImpl = SkillDataManager.Instance;
            profileDataRepoImpl = ProfileDataManager.Instance;
            loginManager = LoginManager.Instance;
            gamePot = GamePotSystem.Instance;
            simpleCharacterList = new BetterList<SimpleCharacterPacket>();
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                loginManager.OnLogout += OnLogout;
                BattleManager.OnReady += OnBattleReady;
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                loginManager.OnLogout -= OnLogout;
                BattleManager.OnReady -= OnBattleReady;
            }
        }

        public override void ResetData()
        {
            ClearCharacters();
        }

        private void OnLogout()
        {
            ResetSelectCharacterCid();
        }

        public void ResetSelectCharacterCid()
        {
            SelectedCharacterCid = 0;
        }

        private void OnBattleReady()
        {
            if (isShowNaverBanner)
            {
                isShowNaverBanner = false;

                // 네이버 배너 띄우기
                if (GameServerConfig.IsKorea())
                {
                    NaverGame.HomeBanner();
                }
                else
                {
                    gamePot.ShowNotice(true);
                }
            }
        }

        /// <summary>
        /// 기존 캐릭터 목록 제거
        /// </summary>
        private void ClearCharacters()
        {
            simpleCharacterList.Clear();
        }

        public SimpleCharacterPacket FindSimpleCharacter(int cid)
        {
            foreach (var item in simpleCharacterList)
            {
                if (item.Cid == cid)
                    return item;
            }

            return null;
        }

        public SimpleCharacterPacket[] GetCharacters()
        {
            return simpleCharacterList.ToArray();
        }

        public bool HasCharacter()
        {
            return simpleCharacterList.size != 0;
        }

        /// <summary>
        /// 캐릭터정보 리스트 요청
        /// </summary>
        public async Task<long> RequestCharacterList(bool isInitialize = false)
        {
            var response = await Protocol.CHARACTER_LIST.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return 0;
            }

            ClearCharacters(); // 기존 캐릭터 목록 제거

            if (response.ContainsKey("1"))
            {
                SimpleCharacterPacket[] packets = response.GetPacketArray<SimpleCharacterPacket>("1");
                foreach (var item in packets)
                {
                    // 인게임 안에서 요청 할때만 세팅
                    if (isInitialize)
                    {
                        item.Initialize(skillDataRepoImpl); // 스킬 세팅
                        item.Initialize(profileDataRepoImpl); // 프로필 세팅
                    }
                    simpleCharacterList.Add(item);
                }

                if (simpleCharacterList.size != 0)
                {
                    // 가장 최근에 접속한 캐릭터 선택
                    if (SelectedCharacterCid == 0)
                    {
                        Array.Sort(packets, SortByOrder);
                        SelectedCharacterCid = packets[0].Cid;
                    }
                }
            }
            OnUpdateCharacter?.Invoke();

            // 값이 0 이상 일 경우 탈퇴신청한 계정(탈퇴까지 남은시간)
            if (response.ContainsKey("2"))
                return response.GetLong("2");

            return 0;
        }

        private int SortByOrder(SimpleCharacterPacket a, SimpleCharacterPacket b)
        {
            return b.DisconnectTime.CompareTo(a.DisconnectTime);
        }

        /// <summary>
        /// 캐릭터 생성 요청
        /// </summary>
        public async Task<bool> RequestCreateCharacter(string charName, byte gender)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutUtfString("1", charName);
            sfs.PutByte("2", gender);
            // TODO 나중에 머리 모양, 머리 색상 제거
            sfs.PutByte("3", 1);
            sfs.PutByte("4", 1);

            var response = await Protocol.CREATE_CHARACTER.SendAsync(sfs);
            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return false;
            }

            SimpleCharacterPacket packet = response.GetPacket<SimpleCharacterPacket>("1");
            packet.Initialize(skillDataRepoImpl); // 스킬 세팅
            packet.Initialize(profileDataRepoImpl); // 프로필 세팅
            simpleCharacterList.Add(packet);

            int characterCount = simpleCharacterList.size;
            switch (characterCount)
            {
                case 1:
                    Analytics.TrackEvent(TrackType.FirstCharacterOpen, isUnique: true);
                    break;

                case 2:
                    Analytics.TrackEvent(TrackType.SecondCharacterOpen);
                    break;
            }

            Analytics.TrackEvent(TrackType.CharacterCreate);
            Analytics.TrackEvent(TrackType.Baselevel1);
            Analytics.TrackEvent(TrackType.Joblevel1);

            SelectedCharacterCid = packet.Cid;
            OnCreateCharacter?.Invoke();
            return true;
        }

        /// <summary>
        /// 캐릭터 삭제대기 요청
        /// </summary>
        public async Task RequestDeleteCharacterWaiting(int cid, string name)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cid);
            sfs.PutUtfString("2", name);

            var response = await Protocol.DELETE_CHARACTER_PRE.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            ClearCharacters(); // 기존 캐릭터 목록 제거

            if (response.ContainsKey("1"))
            {
                SimpleCharacterPacket[] packets = response.GetPacketArray<SimpleCharacterPacket>("1");
                foreach (var item in packets)
                {
                    item.Initialize(skillDataRepoImpl); // 스킬 세팅
                    item.Initialize(profileDataRepoImpl); // 프로필 세팅

                    simpleCharacterList.Add(item);
                }
            }
            OnCharacterDeleteWaiting?.Invoke(cid);
        }

        /// <summary>
        /// 캐릭터 삭제완료 요청
        /// </summary>
        public async Task RequestDeleteCharacterComplete(int cid, string name)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cid);
            sfs.PutUtfString("2", name);

            var response = await Protocol.DELETE_CHARACTER_COMPLETE.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            ClearCharacters(); // 기존 캐릭터 목록 제거

            if (response.ContainsKey("1"))
            {
                SimpleCharacterPacket[] packets = response.GetPacketArray<SimpleCharacterPacket>("1");
                foreach (var item in packets)
                {
                    item.Initialize(skillDataRepoImpl); // 스킬 세팅
                    item.Initialize(profileDataRepoImpl); // 프로필 세팅

                    simpleCharacterList.Add(item);
                }
            }
            OnCharacterDelete?.Invoke();
        }

        /// <summary>
        /// 캐릭터 삭제 요청 취소
        /// </summary>
        public async Task RequestDeleteCharacterCancel(int cid, string name)
        {
            var sfs = Protocol.NewInstance();
            sfs.PutInt("1", cid);
            sfs.PutUtfString("2", name);

            var response = await Protocol.DELETE_CHARACTER_CANCEL.SendAsync(sfs);

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            foreach (var item in simpleCharacterList)
            {
                if (item.Cid == cid)
                {
                    item.SetIsDeleteWaiting(false);
                    break;
                }
            }
            OnCharacterDeleteCancel?.Invoke(cid);
        }

        /// <summary>
        /// 게임에 입장
        /// </summary>
        public async Task<ResultCode> RequestJoinGame(bool isReconnection = false)
        {
            // 게임 첫 접속시 선택한 캐릭터가 없을때
            if (SelectedCharacterCid == 0 && !isReconnection)
                return ResultCode.FAIL;

            UI.ShowIndicator();

            var sfs = Protocol.NewInstance();
            if (isReconnection)
            {
                sfs.PutInt("1", Entity.Character.Cid);
            }
            else
            {
                sfs.PutInt("1", SelectedCharacterCid);
            }

            var response = await Protocol.JOIN_GAME_MAP.SendAsync(sfs);

            if (response.isSuccess)
            {
                // 1. 캐릭터 정보
                CharacterPacket characterPacket = response.GetPacket<CharacterPacket>("1");
                Notify(characterPacket);

                // 2. 캐릭터 스킬 정보
                CharacterSkillData[] skills = response.GetPacketArray<CharacterSkillData>("2");
                Notify(skills);

                // 3. 캐릭터 스킬 슬롯 정보
                CharacterSkillSlotData[] skillSlots = response.GetPacketArray<CharacterSkillSlotData>("3");
                Notify(skillSlots);

                // 4. 인벤토리 정보
                CharacterItemData[] items = response.ContainsKey("4") ? response.GetPacketArray<CharacterItemData>("4") : null;
                Notify(items);

                // 6. 버프 정보
                BuffPacket[] buffs = response.ContainsKey("6") ? response.GetPacketArray<BuffPacket>("6") : null;
                Notify(buffs);

                // 7. 알람 타입(New 표시)
                int alarmValue = response.ContainsKey("7") ? response.GetInt("7") : 0;
                NotifyAlarm(alarmValue);

                // 8. 상점 정보
                ShopPacket shopPacket = response.ContainsKey("8") ? response.GetPacket<ShopPacket>("8") : null;
                Notify(shopPacket);

                // q. 퀘스트 목록/진행도 정보
                QuestPacket questPacket = response.ContainsKey("q") ? response.GetPacket<QuestPacket>("q") : null;
                Notify(questPacket);

                // eb. 이벤트 버프 정보
                EventBuffPacket eventBuffPacket = response.ContainsKey("eb") ? response.GetPacket<EventBuffPacket>("eb") : null;
                Notify(eventBuffPacket);

                // wdr. 월드 듀얼 보상 정보
                EventDuelBuffPacket duelBuffPacket = response.ContainsKey("wdr") ? response.GetPacket<EventDuelBuffPacket>("wdr") : null;
                Notify(duelBuffPacket);

                // gs. 길드 스킬 정보
                GuildSkillPacket[] guildSkills = response.ContainsKey("gs") ? response.GetPacketArray<GuildSkillPacket>("gs") : null;
                Notify(guildSkills);

                // tr. 거래소 등록 품목 정보
                AuctionItemPacket tradeShopTradeItemPacket = response.ContainsKey("tr") ? response.GetPacket<AuctionItemPacket>("tr") : null;
                Notify(tradeShopTradeItemPacket);

                // pt. 개인상점 최근 등록 품목 정보
                PrivateStoreItemPacket[] tradeShopPrivateItems = response.ContainsKey("pt") ? response.GetPacketArray<PrivateStoreItemPacket>("pt") : null;
                Notify(tradeShopPrivateItems);

                // wba. 월드보스 알람 정보
                WorldBossAlarmPacket[] worldBossAlarmPacket = response.ContainsKey("wba") ? response.GetPacketArray<WorldBossAlarmPacket>("wba") : null;
                Notify(worldBossAlarmPacket);

                // noti. 공지사항 메시지
                // noti_version. 공지사항 버전
                string noti = response.ContainsKey("noti") ? response.GetUtfString("noti") : null;
                int noti_version = response.ContainsKey("noti_version") ? response.GetInt("noti_version") : default;
                ChatNoticePacket chatNotificationPacket = new ChatNoticePacket(noti, noti_version);
                Notify(chatNotificationPacket);

                // ei. 이벤트 배너 정보
                EventBannerPacket[] eventBannerPackets = response.ContainsKey("ei") ? response.GetPacketArray<EventBannerPacket>("ei") : null;
                Notify(eventBannerPackets);

                // 13. 동료 정보
                CharAgentPacket[] charAgentPackets = response.ContainsKey("13") ? response.GetPacketArray<CharAgentPacket>("13") : null;
                CharAgentBookPacket[] agentBookIDs;

                if (response.ContainsKey("18"))
                {
                    string agentBookIdsText = response.GetUtfString("18");
                    string[] splt = agentBookIdsText.Split(',');
                    Buffer<CharAgentBookPacket> buffer = new Buffer<CharAgentBookPacket>();
                    for (int i = 0; i < splt.Length; ++i)
                    {
                        if (int.TryParse(splt[i], out int id))
                        {
                            buffer.Add(new CharAgentBookPacket(id));
                        }
                    }

                    agentBookIDs = buffer.GetBuffer(isAutoRelease: true);
                }
                else
                {
                    agentBookIDs = Array.Empty<CharAgentBookPacket>();
                }

                Notify(charAgentPackets, agentBookIDs);

                AgentSlotInfoPacket[] agentSlotInfoPackets = response.ContainsKey("15") ? response.GetPacketArray<AgentSlotInfoPacket>("15") : null;
                Notify(agentSlotInfoPackets);

                AgentExploreCountInfo[] agentExploreCountInfos = response.ContainsKey("16") ? response.GetPacketArray<AgentExploreCountInfo>("16") : null;
                Notify(agentExploreCountInfos);

                float remainTimeToMidnight = response.ContainsKey("17") ? response.GetLong("17") / 1000f : (float)(TimeSpan.FromDays(1) - DateTime.Now.TimeOfDay).TotalSeconds;
                NotifyRemainTimeToMidnight(remainTimeToMidnight);

                CharacterSharingPacket characterSharingPacket = response.ContainsKey("19") ? response.GetPacket<CharacterSharingPacket>("19") : null;
                Notify(characterSharingPacket);

                int subChannelId = response.ContainsKey("20") ? response.GetInt("20") : 0; // gameStartMapType 1이 아닐때 0 : 로비 채널 ID, 2 : 멀티미로 ID     
                NotifySubChannelId(subChannelId);

                // 21. 내 정보 공개 여부
                bool isMyInfoOpen = response.GetByte("21") == 1;
                NotifyInfoOpen(isMyInfoOpen);

                // 22. 종료 시 자동 셰어 등록 여부
                bool isShareExitAutoSetting = response.GetByte("22") == 1;
                NotifyShareExitAutoSetting(isShareExitAutoSetting);

                // 23. 이벤트 룰렛 활성화 여부
                if (response.ContainsKey("23"))
                {
                    Notify(response.GetPacket<EventRoulettePacket>("23"));
                }

                // 24. 셰어 푸시 옵션 정보
                if (response.ContainsKey("24"))
                {
                    bool isSharePush = response.GetByte("24") == 1;
                    NotifySharePushSetting(isSharePush);
                }

                // 25. 채널 인덱스
                if (response.ContainsKey("25"))
                {
                    int channelIndex = response.GetInt("25");
                    NotifyChannelIndex(channelIndex);
                }

                // 26. 테이밍 던전 정보
                if (response.ContainsKey("26"))
                {
                    Notify(response.GetPacket<TamingDungeonInfoPacket>("26"));
                }

                // 27. 상점 구매 불가 상품 (이미 구매후 우편함에 존제)
                if (response.ContainsKey("27"))
                {
                    Notify(response.GetIntArray("27"));
                }

                // 28. 테이밍 던전 오픈 시간 정보
                if (response.ContainsKey("28"))
                {
                    Notify(response.GetPacket<TamingDungeonTimePacket>("28"));
                }

                int maxChannel = response.GetInt("29");
                Entity.ChatModel.Initialize(maxChannel);

                // 30. 스세셜 룰렛 정보
                if (response.ContainsKey("30"))
                {
                    Notify(response.GetPacket<SpecialRoulettePacket>("30"));
                }

                // 31. 이벤트 퀘스트 정보
                if (response.ContainsKey("31"))
                {
                    Notify(response.GetPacket<EventQuizPacket>("31"));
                }
                else
                {
                    Notify(EventQuizPacket.EMPTY);
                }

                // 32. 오버스탯 정보
                if (response.ContainsKey("32"))
                {
                    Notify(response.GetPacket<OverStatusPacket>("32"));
                }

                // 33. 고객감사 정보
                if (response.ContainsKey("33"))
                {
                    Notify(response.GetPacket<CustomerRewardPacket>("33"));
                }

                // 34. 가위바위보 정보
                if (response.ContainsKey("34"))
                {
                    Notify(response.GetPacket<EventRpsPacket>("34"));
                }

                // 35. 냥다래 받기 이벤트
                if (response.ContainsKey("35"))
                {
                    Notify(response.GetPacket<CatCoinGiftPacket>("35"));
                }

                if (response.ContainsKey("cd"))
                {
                    Notify(response.GetPacket<BookPacket>("cd"));
                }

                BingoSeasonPacket curSeasonPacket = null;
                BingoSeasonPacket nextSeasonPacket = null;

                if (response.ContainsKey("evb"))
                    curSeasonPacket = response.GetPacket<BingoSeasonPacket>("evb");

                if (response.ContainsKey("nevb"))
                    nextSeasonPacket = response.GetPacket<BingoSeasonPacket>("nevb");

                Notify(curSeasonPacket, nextSeasonPacket);

                if (response.ContainsKey("bg"))
                    Notify(response.GetPacket<BingoStatePacket>("bg"));

                string maxStageCoupon = response.GetUtfString("cp");
                Entity.Dungeon.SetMaxStageCoupon(maxStageCoupon);

                if (response.ContainsKey("36"))
                {
                    Notify(response.GetPacket<EventDicePacket>("36"));
                }
                else
                {
                    Notify(EventDicePacket.EMPTY);
                }

                // 37. 페이스북 친구초대 정보
                if (response.ContainsKey("37"))
                {
                    Notify(response.GetPacket<FriendInvitePacket>("37"));
                }

                // 38. 스테이지 이벤트 & 챌린지 모드 정보
                if (response.ContainsKey("38"))
                    Notify(response.GetPacket<EventChallengePacket>("38"));

                // 39. 어둠의나무 정보
                Notify(response.GetPacket<DarkTreePacket>("39"));

                // 40. 타임패트롤 도달한 레벨
                if (response.ContainsKey("40"))
                    NotifyFinalTimePatrolLevel(response.GetInt("40"));

                // 41. 마지막으로 입장한 타임패트롤 ID
                if (response.ContainsKey("41"))
                    NotifyLastTimePatrolId(response.GetInt("41"));

                // 42. 타임슈트 정보
                if (response.ContainsKey("42"))
                    Notify(response.GetPacketArray<TimeSuitPacket>("42"));

                // 44. 카프라 운송 상태
                if (response.ContainsKey("44"))
                    Notify(response.GetByte("44").ToEnum<KafraCompleteType>());

                // 45. 카프라 운송 목록
                if (response.ContainsKey("45"))
                    Notify(response.GetPacketArray<KafraDeliveryPacket>("45"));

                // 46. 쉐어포스스텟 정보
                string shareForceStatusInfo = response.ContainsKey("46") ? response.GetUtfString("46") : string.Empty;
                Entity.Character.InitializeShareForceStatus(shareForceStatusInfo);

                // 47. 출석 체크 이벤트 (14일)
                if (response.ContainsKey("47"))
                {
                    Notify(response.GetPacket<AttendEventPacket>("47"));
                }
                else
                {
                    Notify(AttendEventPacket.EMPTY);
                }

                // 48. 패스 정보
                if (response.ContainsKey("48"))
                {
                    Notify(response.GetPacket<PassPacket>("48"));
                }
                else
                {
                    Debug.Log("패스 정보 없음");
                    Notify(PassPacket.EMPTY);
                }

                // 49. 패스 시즌 정보
                if (response.ContainsKey("49"))
                {
                    Notify(response.GetPacket<PassSeasonPacket>("49"));
                }
                else
                {
                    Debug.Log("패스 시즌 정보 없음");
                    Notify(PassSeasonPacket.EMPTY);
                }

                // 50. 나비호 의뢰 정보
                if (response.ContainsKey("50"))
                    Notify(response.GetPacketArray<NabihoPacket>("50"));

                // 51. 나비호 경험치 정보
                int nabihoExp = response.GetInt("51");
                Entity.Inventory?.SetNabihoExp(nabihoExp);

                // 52. 단어수집 이벤트 정보
                if (response.ContainsKey("52"))
                {
                    Notify(response.GetPacket<WordCollectionPacket>("52"));
                }
                else
                {
                    Notify(WordCollectionPacket.EMPTY);
                }

                // 53. 온버프패스 정보
                if (response.ContainsKey("53"))
                {
                    Notify(response.GetPacket<OnBuffPassPacket>("53"));
                }
                else
                {
                    Debug.Log("패스 정보 없음");
                    Notify(OnBuffPassPacket.EMPTY);
                }

                // 54. 온버프패스 시즌 정보
                if (response.ContainsKey("54"))
                {
                    Notify(response.GetPacket<OnBuffPassSeasonPacket>("54"));
                }
                else
                {
                    Debug.Log("패스 시즌 정보 없음");
                    Notify(OnBuffPassSeasonPacket.EMPTY);
                }

                if (isReconnection)
                {
                    // 재접속 성공 이벤트
                }
                else
                {
                    UI.HideIndicator();

                    UI.HideLoudSpeaker();
                    UI.ShowLoudSpeaker();

                    // 캐릭터 초기화 완료 이벤트
                    OnCharacterInit?.Invoke();

                    BattleEntry.ResetNextAction(); // 캐릭터 변경 시, Entry 이벤트도 지워주기

                    // 최초 입장 한 번만 네이버 배너 보여주기
                    if (isFirstJoinGameMap)
                    {
                        isFirstJoinGameMap = false;
                        isShowNaverBanner = true;
                    }
                }
            }
            else
            {
                UI.HideIndicator();
                response.ShowResultCode();
            }

            return response.resultCode;
        }

        /// <summary>
        /// 캐릭터 선택화면으로 이동하기 전 서버로 호출할 것
        /// </summary>
        public async Task RequestGotoCharList(int cid = 0)
        {
            if (cid != 0)
                SelectedCharacterCid = cid;

            Response response = await Protocol.REQUEST_GOTO_CHAR_LIST.SendAsync();

            if (!response.isSuccess)
            {
                response.ShowResultCode();
                return;
            }

            IntroScene.IsBackToTitle = true;
            SceneLoader.LoadIntro(); // 타이틀 화면으로 이동
            UI.HideIndicator();
        }
    }
}