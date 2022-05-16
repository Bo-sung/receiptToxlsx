#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class TestEntry : BattleEntry
    {
        public enum TestFlag
        {
            UnitUndeadModeOn,
            UnitUndeadModeOff,
            Stun,
            Fall,

            ResetCamera,
            SetOpponentTarget,
        }

        private CharacterEntity opponent;

        private UnitActor playerActor;
        private UnitActor opponentActor;

        public TestEntry() : base(BattleMode.Test)
        {
        }

        /// <summary>
        /// 폐기
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            player.DespawnActor();
            playerActor = null;

            if (opponent != null)
            {
                opponent.DespawnActor();
                opponent = null;
            }
            opponentActor = null;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
        }        

        public override void Ready()
        {
            string sceneName = string.Empty; // 씬 이름
            string bgmName = string.Empty; // 배경음 이름
            bool isChangeScene = false; // 씬 변경 여부 - 음슴
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        protected override void OnReady()
        {
            // 플레이어 세팅
            if (playerActor == null)
            {
                if (impl.Add(player, isEnemy: false))
                {
                    playerActor = player.SpawnActor(); // 유닛 소환
                }
                else
                {
                    playerActor = player.GetActor();
                    playerActor.Show();
                }
            }

            playerActor.AI.SetHomePosition(Vector3.zero, isWarp: true);
            cameraController.SetPlayer(playerActor.CachedTransform); // 카메라 타겟 세팅

            // 적군 세팅
            if (opponent == null)
            {
                opponent = CharacterEntity.Factory.CreateMultiBattlePlayer();

                string name = "[TESTER]";
                int cid = -1;
                byte job = (byte)player.Character.Job;
                byte gender = (byte)player.Character.Gender;
                int jobLevel = player.Character.JobLevel;
                int str = player.Status.BasicStr;
                int agi = player.Status.BasicAgi;
                int vit = player.Status.BasicVit;
                int @int = player.Status.BasicInt;
                int dex = player.Status.BasicDex;
                int luk = player.Status.BasicLuk;

                DummyCharacterPacket packet = new DummyCharacterPacket
                {
                    name = name,
                    cid = cid,
                    job = job,
                    job_level = jobLevel,
                    gender = gender,
                };

                opponent.Character.Initialize(packet);
                opponent.Status.Initialize(packet);
                opponent.Guild.Initialize(packet);
            }

            if (opponentActor == null)
            {
                if (impl.Add(opponent, isEnemy: true))
                {
                    opponentActor = opponent.SpawnActor(); // 유닛 소환
                }
                else
                {
                    opponentActor = opponent.GetActor();
                    opponentActor.Show();
                }
            }

            opponentActor.AI.SetHomePosition(Vector3.forward, isWarp: true);
        }        

        /// <summary>
        /// 전투 준비
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            yield return 0f;
        }

        public void Test(TestFlag flag)
        {
            switch (flag)
            {
                case TestFlag.UnitUndeadModeOn:
                    player.SetForceStatus(ForceStatusType.MaxRegenOn);
                    opponent.SetForceStatus(ForceStatusType.MaxRegenOn);
                    break;

                case TestFlag.UnitUndeadModeOff:
                    player.SetForceStatus(ForceStatusType.MaxRegenOff);
                    opponent.SetForceStatus(ForceStatusType.MaxRegenOff);
                    break;

                case TestFlag.Stun:
                    opponent.SetForceStatus(ForceStatusType.Stun);
                    break;

                case TestFlag.Fall:
                    StartFallOpponent();
                    break;

                case TestFlag.ResetCamera:
                    ResetCamera();
                    break;

                case TestFlag.SetOpponentTarget:
                    SetOpponentTarget();
                    break;
            }
        }

        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        protected override void OnSceneLoaded(GameObject[] roots)
        {
        }

        private void StartFallOpponent()
        {
            if (opponentActor == null)
                return;

            Vector3 bossHomePosition = opponent.LastPosition; // 보스 위치 (바닥, 도착점)
            Vector3 bossSpawnPosition = bossHomePosition; // 보스 위치 (공중, 시작점)
            bossSpawnPosition.y = Constants.Map.BOSS_FALLING_POSITION_Y;
            float fallTime = Constants.Battle.FALL_TIME; // 보스 2초 동안 낙하 (보스 위치 이동, 그림자 크기 조절)

            opponentActor.Movement.Fall(bossSpawnPosition, bossHomePosition, fallTime);
        }

        private void ResetCamera()
        {
            if (playerActor == null || opponentActor == null)
                return;

            cameraController.RemoveMember(playerActor.CachedTransform);
            cameraController.RemoveMember(opponentActor.CachedTransform);

            cameraController.SetPlayer(playerActor.CachedTransform);
        }

        private void SetOpponentTarget()
        {
            if (playerActor == null || opponentActor == null)
                return;

            cameraController.SetBoss(playerActor.CachedTransform, opponentActor.CachedTransform);
            cameraController.SetTimeline(CameraController.Timeline.BossCut);
        }
    }
}
#endif