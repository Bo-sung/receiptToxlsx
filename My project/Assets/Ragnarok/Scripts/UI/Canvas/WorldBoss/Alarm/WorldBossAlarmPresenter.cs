using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UIWorldBossAlarm"/>
    /// </summary>
    public class WorldBossAlarmPresenter : ViewPresenter
    {
        private const string TAG = nameof(WorldBossAlarmPresenter);
        private const float Duration = 20f;

        IWorldBossAlarmCanvas canvas;
        private readonly DungeonModel dungeonModel;
        private readonly QuestModel questModel;
        private readonly WorldBossDataManager worldBossDataRepo;
        private readonly MonsterDataManager monsterDataRepo;

        private int curWorldBossId;

        public WorldBossAlarmPresenter(IWorldBossAlarmCanvas canvas)
        {
            this.canvas = canvas;
            dungeonModel = Entity.player.Dungeon;
            questModel = Entity.player.Quest;
            worldBossDataRepo = WorldBossDataManager.Instance;
            monsterDataRepo = MonsterDataManager.Instance;
        }

        public override void AddEvent()
        {
            BattleManager.OnStart += OnStart;
            dungeonModel.OnUpdateWorldBossOpen += OnUpdateWorldBossOpen;
        }

        public override void RemoveEvent()
        {
            BattleManager.OnStart -= OnStart;
            dungeonModel.OnUpdateWorldBossOpen -= OnUpdateWorldBossOpen;
        }

        private void OnStart(BattleMode mode)
        {
            if (mode == BattleMode.Stage)
            {
                if (!canvas.IsActive())
                {
                    CheckAlarm();
                }
            }
            else
            {
                Timing.KillCoroutines(TAG);
                canvas.SetActive(false);
            }
        }

        public void SetView()
        {
            CheckAlarm();
        }

        private void CheckAlarm()
        {
            if (dungeonModel.worldbossOpenIds.Count == 0)
            {
                curWorldBossId = 0;
                Timing.KillCoroutines(TAG);
                canvas.SetActive(false);
                return;
            }

            curWorldBossId = dungeonModel.worldbossOpenIds[0];
            dungeonModel.worldbossOpenIds.RemoveAt(0);

            // 입장이 불가능한 월드보스의 경우 다음 알람 체크
            if (!dungeonModel.CanEnter(DungeonType.WorldBoss, curWorldBossId, isShowPopup: true))
            {
                CheckAlarm();
                return;
            }

            IBossMonsterSpawnData boss = worldBossDataRepo.Get(curWorldBossId);
            MonsterInfo monster = new WorldBossMonsterInfo();
            monster.SetData(monsterDataRepo.Get(boss.BossMonsterId));
            monster.SetLevel(boss.Level);
            canvas.SetMonster(monster);
            canvas.SetActive(true);
            Timing.KillCoroutines(TAG);
            Timing.RunCoroutine(YieldTimer(), TAG);
        }

        public void OnClickedBtnMove()
        {
            dungeonModel.SetSelectWorldBoss(curWorldBossId);
            CheckAlarm();
            UI.Show<UIWorldBoss>();
        }

        public void OnClickedClose()
        {
            CheckAlarm();
        }

        void OnUpdateWorldBossOpen()
        {
            if (!questModel.IsOpenContent(ContentType.Dungeon, false))
                return;

            if (!canvas.IsActive())
            {
                CheckAlarm();
            }
        }

        IEnumerator<float> YieldTimer()
        {
            float time = 0;
            while (time <= Duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / Duration);
                canvas.SetProgress(1f - t);
                yield return Timing.WaitForOneFrame;
            }
            CheckAlarm();
        }
    }
}
