using Ragnarok.View;
using System.Threading.Tasks;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIGuildTaming"/>
    /// </summary>
    public class GuildTamingPresenter : ViewPresenter
    {
        // <!-- Models --!>
        private readonly GuildModel guildModel;
        private readonly CharacterModel characterModel;

        // <!-- Repositories --!>
        private readonly TamingDataManager tamingDataRepo;

        // <!-- Managers --!>
        private readonly BattleManager battleManager;

        // <!-- Event --!>
        public event System.Action OnUpdateTime;

        private readonly GuildTamingInfo info;

        public GuildTamingPresenter()
        {
            guildModel = Entity.player.Guild;
            characterModel = Entity.player.Character;

            tamingDataRepo = TamingDataManager.Instance;

            battleManager = BattleManager.Instance;

            info = new GuildTamingInfo();
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        /// <summary>
        /// 테이밍 미로 Notice 끄기
        /// </summary>
        public void TurnOffNotice()
        {
            guildModel.HasTamingMazeNotice = false;
        }

        /// <summary>
        /// 오늘 요일 반환
        /// </summary>
        public int GetTodayIndex()
        {
            TamingData data = tamingDataRepo.Get(guildModel.TamingId);
            if (data == null)
                return 0;

            return data.day_type;
        }

        /// <summary>
        /// 현재 진행중 여부
        /// </summary>
        public bool GetIsInProgress()
        {
            return guildModel.IsTamingMazeInProgress;
        }

        /// <summary>
        /// 입장까지 남은 시간
        /// </summary>
        public RemainTime GetRemainTime()
        {
            return guildModel.TamingMazeRemainTime;
        }

        /// <summary>
        /// 날짜에 해당하는 데이터 반환
        /// </summary>
        public GuildTamingView.IInput GetData(int dayIndex)
        {
            TamingData data = tamingDataRepo.Get(guildModel.TamingMazeSeason, dayIndex);
            info.SetData(data);
            return info;
        }

        public void RequestTamingMazeInfo()
        {
            AsyncRequestTamingMazeInfo().WrapNetworkErrors();
        }

        public void ShowHelpPopup()
        {
            string title = LocalizeKey._33112.ToText(); // 테이밍 미로
            string description = GetHelpDescription(); // [476FAA]입장 가능 시간[-]\n\n[76C5FE]10 : 00 ~ 11 : 00\n16 : 00 ~ 17 : 00\n21 : 00 ~ 22 : 00[-]\n\n큐펫 조각을 획득할 수 있는 공간입니다.\n몬스터에게 먹이를 주어 테이밍 해보세요.
            UI.ConfirmPopup(title, description);
        }

        public void StartBattle()
        {
            int jobLevel = BasisType.TAMING_DUNGEON_LIMIT_JOB_LEVEL.GetInt();
            if (!characterModel.IsCheckJobLevel(jobLevel, true))
                return;

            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            if (BattleManager.Instance.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                return;
            }

            if (!guildModel.HaveGuild)
                return;

            if (!GetIsInProgress())
                return;

            if (GetRemainTime().ToRemainTime() <= 0f)
                return;

            battleManager.StartBattle(BattleMode.TamingMaze, guildModel.TamingId);
        }

        private async Task AsyncRequestTamingMazeInfo()
        {
            if (await guildModel.RequestTamingMazeInfo())
                OnUpdateTime?.Invoke();
        }

        private string GetHelpDescription()
        {
            var sb = StringBuilderPool.Get();
            sb.Append("[c]");
            {
                sb.Append("[476FAA]");
                {
                    sb.Append(LocalizeKey._33135.ToText()); // 입장 가능 시간
                }
                sb.Append("[-]");
                
                sb.AppendLine();

                sb.Append("[76C5FE]");
                {
                    int progressTime = BasisType.TAMING_DUNGEON_OPEN_DURATION.GetInt();
                    foreach (var item in guildModel.GetTamingOpenTimes())
                    {
                        sb.AppendLine();
                        sb.Append(item.ToString("HH : mm")).Append(" ~ ").Append(item.AddMilliseconds(progressTime).ToString("HH : mm"));
                    }
                }
                sb.Append("[-]");
            }
            sb.Append("[/c]");

            return sb.Release();
        }

        private class GuildTamingInfo : GuildTamingView.IInput
        {
            private readonly InventoryModel invenModel;
            private readonly MonsterDataManager monsterDataRepo;
            private readonly Buffer<MonsterInfo> monsterBuffer;

            private TamingData data;

            public GuildTamingInfo()
            {
                invenModel = Entity.player.Inventory;
                monsterDataRepo = MonsterDataManager.Instance;
                monsterBuffer = new Buffer<MonsterInfo>();
            }

            public void SetData(TamingData data)
            {
                this.data = data;
            }

            /// <summary>
            /// 먹이 아이템 정보 반환
            /// </summary>
            public RewardData GetFeedItem()
            {
                if (data == null)
                    return null;

                return new RewardData(RewardType.Item, data.use_item_id, 1);
            }

            /// <summary>
            /// 먹이 아이템 수량 반환
            /// </summary>
            public int GetFeedItemCount()
            {
                if (data == null)
                    return 0;

                return invenModel.GetItemCount(data.use_item_id);
            }

            /// <summary>
            /// 등장 몬스터 정보 반환
            /// </summary>
            public MonsterInfo[] GetMonsterInfos()
            {
                if (data != null)
                {
                    for (int i = 0; i < TamingData.MAX_SPAWN_MONSTER_INDEX; i++)
                    {
                        int monsterId = data.GetSpawnMonsterId(i);
                        if (monsterId == 0)
                            continue;

                        MonsterInfo info = new MonsterInfo(isBoss: false);
                        info.SetData(monsterDataRepo.Get(monsterId));
                        info.SetLevel(data.monster_level);
                        monsterBuffer.Add(info);
                    }
                }

                return monsterBuffer.GetBuffer(isAutoRelease: true);
            }
        }
    }
}