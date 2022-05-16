using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIWorldBoss"/>
    /// </summary>
    public class WorldBossPresenter : BaseDungeonPresenter, IDungeonWorldBossImpl
    {
        private const string TAG = nameof(WorldBossPresenter);

        public interface IView
        {
            void Refresh();
            void UpdateZeny(long zeny);
            void UpdateCatCoin(long catCoin);
            void CloseUI();
            void TryStartBattle();
        }

        private readonly IView view;
        private readonly WorldBossDataManager worldBossDataRepo;
        private readonly ConnectionManager connectionManager;
        private readonly GoodsModel goodsModel;
        private readonly CharacterModel characterModel;
        private readonly DungeonModel dungeonModel;

        WorldBossDungeonElement[] worldBossDungeonElements;
        WorldBossDungeonElement selectElement;

        bool isAllList = true;

        public WorldBossPresenter(IView view)
        {
            this.view = view;
            worldBossDataRepo = WorldBossDataManager.Instance;
            connectionManager = ConnectionManager.Instance;
            goodsModel = Entity.player.Goods;
            characterModel = Entity.player.Character;
            dungeonModel = Entity.player.Dungeon;
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += view.UpdateZeny;
            goodsModel.OnUpdateCatCoin += view.UpdateCatCoin;
            characterModel.OnUpdateJobLevel += OnUpdateJobLevel;
            dungeonModel.OnUpdateTicket += view.Refresh;

            OnUpdateJobLevel(characterModel.JobLevel);
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= view.UpdateZeny;
            goodsModel.OnUpdateCatCoin -= view.UpdateCatCoin;
            characterModel.OnUpdateJobLevel -= OnUpdateJobLevel;
            dungeonModel.OnUpdateTicket -= view.Refresh;
        }

        /// <summary>
        /// 월드보스 던전 정보 반환
        /// </summary>
        /// <returns>(선택된 월드보스 ID, 월드보스 정보 목록)</returns>
        public (WorldBossDungeonElement, WorldBossDungeonElement[]) GetWorldBossDungeonElements()
        {
            if (worldBossDungeonElements == null)
            {
                worldBossDungeonElements = CreateWorldBossDungeonElements();
            }
            return (GetSelectElement(), worldBossDungeonElements);
        }

        public WorldBossDungeonElement GetSelectElement()
        {
            if (worldBossDungeonElements == null)
            {
                worldBossDungeonElements = CreateWorldBossDungeonElements();
            }

            if (selectElement != null && selectElement.Id != dungeonModel.SelectWorldBossId)
            {
                selectElement = worldBossDungeonElements.FirstOrDefault(x => x.Id == dungeonModel.SelectWorldBossId);
            }
            return selectElement;
        }

        /// <summary>
        /// 월드보스 던전 정보 생성
        /// </summary>
        /// <returns></returns>
        private WorldBossDungeonElement[] CreateWorldBossDungeonElements()
        {
            IDungeonGroup[] dungeonGroups = worldBossDataRepo.GetList().ToArray();
            WorldBossDungeonElement[] output = new WorldBossDungeonElement[dungeonGroups.Length];
            for (int i = 0; i < dungeonGroups.Length; i++)
            {
                output[i] = new WorldBossDungeonElement(dungeonGroups[i], this);

                // 기본으로 첫번째 던전보스 선택
                if(dungeonModel.SelectWorldBossId == 0)
                {
                    dungeonModel.SetSelectWorldBoss(dungeonGroups[i].Id, output[i].MaxHp);
                    selectElement = output[i];
                }
                else if(dungeonModel.SelectWorldBossId == dungeonGroups[i].Id)
                {
                    dungeonModel.SetSelectWorldBoss(dungeonGroups[i].Id, output[i].MaxHp);
                    selectElement = output[i];
                }               
            }
            return output;
        }

        void OnUpdateJobLevel(int jobLevel)
        {
            if (worldBossDungeonElements != null)
            {
                foreach (var item in worldBossDungeonElements)
                {
                    if (item.ConditionType == DungeonOpenConditionType.JobLevel)
                        item.InvokeUpdateEvent();
                }
            }
        }

        public void OnClickedBtnZeny()
        {
            UI.ShowZenyShop();
        }

        public void OnClickedBtnCatCoin()
        {
            UI.ShowCashShop();
        }

        public void StartUpdateWorldBossInfos()
        {
            KillCoroutines();
            Timing.RunCoroutine(UpdateWorldBossInfos(), TAG);
        }

        public void KillCoroutines()
        {
            Timing.KillCoroutines(TAG);
        }

        IEnumerator<float> UpdateWorldBossInfos()
        {
            while (true)
            {
                var async = GetWorldBossInfos();
                while (!async.IsCompleted)
                {
                    yield return Timing.WaitForOneFrame;
                }
                yield return Timing.WaitForSeconds(3f);
            }
        }

        private async Task GetWorldBossInfos()
        {
            if (isAllList)
                isAllList = false;

            var response = await dungeonModel.RequestWorldBossList(isAllList);

            if (!response.isSuccess)
                return;

            foreach (var item in response.worldBossInfos.OrEmptyIfNull())
            {
                var element = worldBossDungeonElements.FirstOrDefault(x => x.Id == item.world_boss_id);
                if (element != null)
                {
                    element.SetData(item);
                    if (element.Id == dungeonModel.SelectWorldBossId)
                    {
                        dungeonModel.SetSelectWorldBoss(element.Id, element.MaxHp);
                    }
                }
            }
        }

        public void SetSelectWorldBoss(WorldBossDungeonElement element)
        {
            dungeonModel.SetSelectWorldBoss(element.Id, element.MaxHp);
        }

        public void SetAlarmWorldBoss(WorldBossDungeonElement element)
        {
            dungeonModel.SetWorldBossAlarm(element.Id, element.IsAlarm);
        }

        public async void StartWorldBoss()
        {
            if (UIBattleMatchReady.IsMatching)
            {
                string message = LocalizeKey._90231.ToText(); // 매칭 중에는 이용할 수 없습니다.\n매칭 취소 후 이용 가능합니다.
                UI.ShowToastPopup(message);
                return;
            }

            if (battleManager.Mode == BattleMode.MultiMazeLobby)
            {
                UI.ShowToastPopup(LocalizeKey._90226.ToText()); // 미로섬에서는 입장할 수 없습니다.\n사냥 필드로 이동해주세요.
                return;
            }

            var curSelected = worldBossDungeonElements.FirstOrDefault(v => v.Id == dungeonModel.SelectWorldBossId);

            if (curSelected != null)
            {
                if (!curSelected.IsOpen())
                {
                    UI.ShowToastPopup(LocalizeKey._453.ToText());
                    return;
                }
            }
            else
            {
                Debug.LogError("[WorldBossPresenter] 현재 선택된 월드 보스 데이터를 찾을 수 없습니다.");
            }

            if (!await dungeonModel.IsEnterDungeon(DungeonType.WorldBoss))
                return;

            view.TryStartBattle();
            battleManager.StartBattle(BattleMode.WorldBoss, dungeonModel.SelectWorldBossId);
        }

        float IDungeonWorldBossImpl.GetFreeTicketCoolTime()
        {
            return dungeonModel.WorldBossFreeTicketCoolTime;
        }

        bool IDungeonWorldBossImpl.IsAlarm(int worldBossId)
        {
            return dungeonModel.IsAlarmWorldBoss(worldBossId);
        }

        WorldBossMonsterInfo[] IDungeonWorldBossImpl.GetMonsterInfos((int monsterId, MonsterType type, int monsterLevel)[] monsterInfos)
        {
            if (monsterInfos is null)
                return null;

            WorldBossMonsterInfo[] result = new WorldBossMonsterInfo[monsterInfos.Length];
            for (int i = 0; i < result.Length; i++)
            {
                MonsterData data = monsterDataRepo.Get(monsterInfos[i].monsterId);
                if (data == null)
                    continue;

                WorldBossMonsterInfo info = new WorldBossMonsterInfo();
                info.SetData(data);
                info.SetLevel(monsterInfos[i].monsterLevel);
                result[i] = info;
            }

            return result;
        }

        /// <summary>
        /// 던전 입장 가능 여부
        /// </summary>
        bool IDungeonWorldBossImpl.CanEnter(DungeonType dungeonType, int id, bool isShowPopup)
        {
            return dungeonModel.CanEnter(dungeonType, id, isShowPopup);
        }

        public int GetMaxLevel()
        {
            return BasisType.MAX_JOB_LEVEL_STAGE_BY_SERVER.GetInt(connectionManager.GetSelectServerGroupId());
        }
    }
}