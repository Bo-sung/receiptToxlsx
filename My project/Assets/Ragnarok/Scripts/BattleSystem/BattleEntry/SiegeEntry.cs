using MEC;
using Ragnarok.SceneComposition;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class SiegeEntry : BattleEntry
    {
        private const string SCENE_NAME = "TempSiege";
        private const string BGM = "Raid_01";

        private const int INIT_CARD_SKILL_COUNT = 5;
        private const int CARD_SKILL_CHARGE_COST = 100;

        /******************** UIs ********************/
        private UIMainTop uiMainTop;
        private UIBattleInfo uiBattleInfo;
        private UIController uiController;
        private UIBattleExit uiBattleExit;
        private UITempNotice uITempNotice;

        /******************** Temp Data ********************/
        private Map map;

        public SiegeEntry() : base(BattleMode.Siege)
        {
        }

        protected override void Dispose()
        {
            base.Dispose();

            map = null;
        }

        protected override void InitCanvas()
        {
            uiMainTop = UI.Show<UIMainTop>();
            uiBattleInfo = UI.Show<UIBattleInfo>();
            uiController = UI.Show<UIController>();
            uiBattleExit = UI.Show<UIBattleExit>();
            uITempNotice = UI.Show<UITempNotice>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;
            uiBattleExit.OnExit += ExitEntry;

            // Initialize
            uiMainTop.Initialize(showCatCoin: true, showRoPoint: false);
            uiMainTop.SetEnableButton(false);

            cameraController.SetView(CameraController.View.SiegeView); // SiegeView 로 전환

            uiBattleInfo.SetMode(UIBattleInfo.Mode.TYPE_3);
            uiBattleExit.SetMode(UIBattleExit.Mode.TYPE_2);
        }

        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;
            uiBattleExit.OnExit -= ExitEntry;

            UI.Close<UIMainTop>();
            UI.Close<UIBattleInfo>();
            UI.Close<UIController>();
            UI.Close<UIBattleExit>();
            UI.Close<UITempNotice>();

            uiMainTop = null;
            uiBattleInfo = null;
            uiController = null;
            uiBattleExit = null;
            uITempNotice = null;
        }

        protected override void OnLocalize()
        {
            uiBattleInfo.Set(LocalizeKey._2508.ToText()); // 공성전
        }

        void OnStartController()
        {
            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.Movement.Stop();
            actor.AI.SetInputMove(isControl: true);
        }

        void OnDragController(Vector2 position)
        {
            if (position == Vector2.zero)
                return;

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            if (player.IsDie)
                return;

            if (player.battleCrowdControlInfo.GetCannotMove())
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            actor.Movement.Move(motion);
        }

        void OnResetController()
        {
            if (player.IsDie)
                return;

            UnitActor actor = player.GetActor();
            if (actor == null)
                return;

            actor.AI.SetInputMove(isControl: false);
            actor.Movement.Stop();
        }

        public override IEnumerator<float> YieldEnterBattle()
        {
            IsSuccessEnter = true;
            yield break;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = SCENE_NAME;
            string bgmName = BGM;
            bool isChangeScene = true;
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                map = item.GetComponent<Map>();

                if (map)
                    break;
            }
        }

        protected override void OnReady()
        {
            base.OnReady();

            //var cardDatas = ObsoleteCentralLabCardSkillDataManager.Instance.GetArray();
            //var cardSkillInfos = GetRandomArray(cardDatas, INIT_CARD_SKILL_COUNT);
            //uiBattleCentralLab.SetPoint(0, CARD_SKILL_CHARGE_COST);
            //uiBattleCentralLab.SetCardSkillInfos(cardSkillInfos);
        }

        protected override IEnumerator<float> YieldComposeScene()
        {
            if (map == null)
            {
                Debug.LogError($"맵(Map)이 존재하지 않습니다: {nameof(sceneName)} = {sceneName}");
                yield break;
            }

            yield return Timing.WaitUntilDone(YieldSpawnCharacter(player), TAG);
        }

        private IEnumerator<float> YieldSpawnCharacter(CharacterEntity character)
        {
            UnitActor characterActor;
            if (impl.Add(character, isEnemy: false))
            {
                characterActor = character.SpawnActor(); // 유닛 소환
                cameraController.SetPlayer(characterActor.CachedTransform); // 카메라 타겟 세팅

                Vector3 playerPosition = map.GetPlayerPosition();
                characterActor.AI.SetHomePosition(playerPosition, isWarp: true); // 위치 세팅
                yield return Timing.WaitForOneFrame;
            }
        }

        protected override void OnAllReady()
        {
            base.OnAllReady();

            UnitActor characterActor = player.GetActor();
            if (characterActor)
                characterActor.Movement.SetDefaultSpeed(Constants.Battle.SIEGE_MOVE_SPEED);

            player.GetActor().EffectPlayer.ShowUnitCircle();

            UI.Show<UISlotMachine>();
        }

        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }
    }
}