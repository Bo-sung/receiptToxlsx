using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public sealed class DungeonRequestDrawer : CheatRequestDrawer
    {
        public override int OrderNum => 4;

        public override string Title => "던전";

        // <!-- Models --!>
        private GuildModel guildModel;

        // <!-- Repositories --!>
        private readonly EnumDrawer cupetEnumDrawer = new SortedEnumDrawer(isShowId: true);
        private int guildAttackEmperiumMaxLevel = 1;

        // <!-- Editor Fields --!>
        private int cupetPieceCount = 100_000;
        private int guildAttackLevel;
        private int guildAttackDonationCount;
        private int guildAttackStartMinutes;
        private int guildAttackStartDamage;
        private int guildAttackEmperiumLevel;
        private int arenaPoint;

        protected override void Awake()
        {
            guildModel = Entity.player.Guild;

            cupetEnumDrawer.Clear();
            CupetDataManager cupetDataRepo = CupetDataManager.Instance;
            foreach (int cupetId in cupetDataRepo.GetCupetIDs())
            {
                CupetData data = cupetDataRepo.Get(cupetId);
                string name = data.name_id.ToText(LanguageType.KOREAN);
                cupetEnumDrawer.Add(cupetId, name);
            }

            cupetEnumDrawer.Ready();

            guildAttackEmperiumMaxLevel = BasisType.GUILD_ATTACK_EMPERIUM_MAX_LEVEL.GetInt();
        }

        protected override void OnDestroy()
        {
            guildModel = null;
            cupetEnumDrawer.Clear();
        }

        protected override void OnDraw()
        {
            if (DrawMiniHeader("난전"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("난전 시작");
                    DrawRequest(SendFreeFightStart);

                    GUILayout.Label("난전이벤트 시작");
                    DrawRequest(SendFreeFightEventStart);
                }
            }

            if (DrawMiniHeader("테이밍 던전"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("바로 시작");
                    DrawRequest(SendTamingStart);

                    GUILayout.Label("종료");
                    DrawRequest(SendTamingEnd);
                }
            }

            if (DrawMiniHeader("길드 습격"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("시작시간 변경 (랜덤)");
                    DrawRequest(SendGuildAttackChangeRandomTime);

                    GUILayout.Label("시작시간 변경");
                    guildAttackStartMinutes = Mathf.Max(1, EditorGUILayout.IntField("minutes", guildAttackStartMinutes));
                    DrawRequest(RequestGuildAttackChangeTime);

                    GUILayout.Label("바로 시작");
                    guildAttackLevel = MathUtils.Clamp(EditorGUILayout.IntField("level", guildAttackLevel), 1, guildAttackEmperiumMaxLevel);
                    DrawRequest(RequestGuildAttackStart);

                    GUILayout.Label("승리");
                    DrawRequest(SendGuildAttackWin);

                    GUILayout.Label("패배");
                    DrawRequest(SendGuildAttackLose);
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("몬스터 대미지 설정");
                    guildAttackStartDamage = Mathf.Max(1, EditorGUILayout.IntField("damage", guildAttackStartDamage));
                    DrawRequest(RequestGuildAttackDamage);

                    GUILayout.Label("몬스터 대미지 초기화");
                    DrawRequest(RequestGuildAttackResetDamage);
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("엠펠리움 조각 기부");
                    guildAttackDonationCount = Mathf.Max(1, EditorGUILayout.IntField("count", guildAttackDonationCount));
                    DrawRequest(RequestGuildAttackDonation);

                    GUILayout.Label("엠펠리움 레벨 세팅");
                    guildAttackEmperiumLevel = MathUtils.Clamp(EditorGUILayout.IntField("level", guildAttackEmperiumLevel), 1, guildAttackEmperiumMaxLevel);
                    DrawRequest(RequestGuildAttackEmperiumLevel);
                }
            }

            if (DrawMiniHeader("엔들리스 타워"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("티켓 시간 초기화");
                    DrawRequest(SendEndlessTowerTicketResetTime);
                }
            }

            if (DrawMiniHeader("길드전"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("현재 상태 확인");
                    DrawRequest(RequestGuildBattleSeasonInfo);

                    GUILayout.Label("다음 상태로 변경");
                    DrawRequest(RequestNextGuildBattleSeasonInfo);
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("큐펫 조각");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            cupetEnumDrawer.DrawEnum();
                            cupetPieceCount = Mathf.Max(1, EditorGUILayout.IntField(nameof(cupetPieceCount), cupetPieceCount));
                            DrawRequest(RequestCupet);
                        }
                    }
                }
            }

            if (DrawMiniHeader("듀얼 아레나"))
            {
                using (ContentDrawer.Default)
                {
                    GUILayout.Label("듀얼 아레나 시작");
                    DrawRequest(SendArenaStart);

                    GUILayout.Label("듀얼 아레나 종료");
                    DrawRequest(SendArenaEnd);
                }

                using (ContentDrawer.Default)
                {
                    GUILayout.Label("듀얼 아레나 깃발");
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(20f);
                        using (new GUILayout.VerticalScope())
                        {
                            arenaPoint = Mathf.Max(1, EditorGUILayout.IntField(nameof(arenaPoint), arenaPoint));
                            DrawRequest(RequestArenaPoint);
                        }
                    }
                }
            }
        }

        private void RequestGuildAttackStart()
        {
            SendGuildAttackStart(guildAttackLevel);
        }

        private void RequestGuildAttackDonation()
        {
            SendGuildAttackDonation(guildAttackDonationCount);
        }

        private void RequestGuildAttackChangeTime()
        {
            SendGuildAttackChangeTime(guildAttackStartMinutes);
        }

        private void RequestGuildAttackDamage()
        {
            SendGuildAttackDamage(guildAttackStartDamage);
        }

        private void RequestGuildAttackResetDamage()
        {
            SendGuildAttackDamage(-1);
        }

        private void RequestGuildAttackEmperiumLevel()
        {
            SendGuildAttackDamage(guildAttackEmperiumLevel);
        }

        private void RequestGuildBattleSeasonInfo()
        {
            if (!guildModel.HaveGuild)
            {
                AddWarningMessage("길드 음슴");
                return;
            }

            AsyncRequestGuildBattleSeasonInfo(isNext: false).WrapNetworkErrors();
        }

        private void RequestNextGuildBattleSeasonInfo()
        {
            if (!guildModel.HaveGuild)
            {
                AddWarningMessage("길드 음슴");
                return;
            }

            AsyncRequestGuildBattleSeasonInfo(isNext: true).WrapNetworkErrors();
        }

        private void RequestCupet()
        {
            if (!guildModel.HaveGuild)
            {
                AddWarningMessage("길드 음슴");
                return;
            }

            int cupetId = cupetEnumDrawer.id;
            if (cupetId == 0)
            {
                AddWarningMessage("얻을 큐펫 선택 필요");
                return;
            }

            SendCupet(cupetId, cupetPieceCount);
        }

        private async Task AsyncRequestGuildBattleSeasonInfo(bool isNext)
        {
            GuildBattleSeasonType type;
            Response response = await Protocol.REQUEST_GUILD_BATTLE_SEASON_INFO.SendAsync();
            if (!response.isSuccess)
            {
                if (response.resultCode != ResultCode.NOT_REQUEST_GUILD_BATTLE)
                {
                    string errorMessage;
                    if (response.resultCode == ResultCode.RCODE_MSG && response.ContainsKey("1"))
                    {
                        errorMessage = response.GetUtfString("1");
                    }
                    else
                    {
                        errorMessage = response.resultCode.Value.ToText(LanguageType.KOREAN);
                    }

                    AddErrorMessage(errorMessage);
                    return;
                }

                type = GuildBattleSeasonType.InProgress;
            }
            else
            {
                type = response.GetByte("1").ToEnum<GuildBattleSeasonType>();
            }

            AddMessage($"현재 길드전 상태: {type}");

            if (!isNext)
                return;

            switch (type)
            {
                case GuildBattleSeasonType.Ready:
                    AddMessage("시작 상태로 변경");
                    SendGuildBattleStart();
                    break;

                case GuildBattleSeasonType.InProgress:
                    AddMessage("종료 상태로 변경");
                    SendGuildBattleEnd();
                    break;

                case GuildBattleSeasonType.Calculating:
                    AddMessage("준비 상태로 변경");
                    SendGuildBattleReady();
                    break;
            }
        }

        private void RequestArenaPoint()
        {           
            SendArenaPoint(arenaPoint);
        }
    }
}