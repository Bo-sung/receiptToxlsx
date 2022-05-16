using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public sealed class PrologueEntry : BattleEntry
    {
        private const string SCENE_NAME = "Scene_Prologue";
        private const string BGM_NAME = "Title";

        private const string PLAYER_M = "PlayerM";
        private const string PLAYER_F = "PlayerF";
        private const string Npc = "Npc";
        private const string Portal = "Portal";
        private const string Animation = "PrologueDirection";

        private const CameraController.View DEFAULT_VIEW = CameraController.View.Quater40_2_5;

        // <!-- Models --!>
        private readonly CharacterListModel characterListModel;
        private readonly CharacterModel characterModel;
        private readonly DungeonModel dungeonModel;

        // <!-- UIs --!>
        private UIController uiController;
        private UIPrologue uiPrologue;
        private UISkipPrologue uiSkipPrologue;
        private UIPrologueFX uiPrologueFX;
        private UIScenarioPrologue uiScenarioPrologue;

        // <!-- Temp Data --!>
        private Gender gender;
        private string dummyPlayerName; // 플레이어 Object 이름 (성별에 따라 바뀜)
        private PrologueCharacter dummyPlayer;
        private GameObject portalObject;
        private Transform npcTransform;
        private TargetingArrow targetingArrow;
        private GameObject aniObject;

        private bool isAssetReady;
        private bool isFinishedPrologue;
        private bool isActiveTutorial = false;
        private bool isRequestCreateCharacter;

        public PrologueEntry() : base(BattleMode.Prologue)
        {
            characterListModel = player.CharacterList;
            characterModel = player.Character;
            dungeonModel = player.Dungeon;
        }

        protected override void AddEvent()
        {
            base.AddEvent();

            AssetManager.OnAllAssetReady += OnAssetReady;
            characterListModel.OnCreateCharacter += OnCreateCharacter;
        }

        protected override void RemoveEvent()
        {
            base.RemoveEvent();

            AssetManager.OnAllAssetReady -= OnAssetReady;
            characterListModel.OnCreateCharacter -= OnCreateCharacter;
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        protected override void InitCanvas()
        {
            uiController = UI.Show<UIController>();
            uiPrologue = UI.Show<UIPrologue>();
            uiSkipPrologue = UI.Show<UISkipPrologue>();
            //uIPrologueFX = UI.Show<UIPrologueFX>(); // 표시되는 시점 변경
            uiScenarioPrologue = UI.Show<UIScenarioPrologue>();

            uiController.OnStart += OnStartController;
            uiController.OnDrag += OnDragController;
            uiController.OnReset += OnResetController;

            uiPrologue.OnHideUI += OnHidePrologueUI;
            uiPrologue.OnFinished += OnFinishedPrologue;
            uiPrologue.OnTutorial += OnTutorial;
            uiPrologue.OnShowPortal += OnShowPortal;

            uiSkipPrologue.OnSkip += OnSkipPrologue;

            cameraController.SetView(DEFAULT_VIEW);

            uiSkipPrologue.ActiveSkipButton(false);
        }

        /// <summary>
        /// UI 폐기
        /// </summary>
        protected override void DisposeCanvas()
        {
            uiController.OnStart -= OnStartController;
            uiController.OnDrag -= OnDragController;
            uiController.OnReset -= OnResetController;

            uiPrologue.OnHideUI -= OnHidePrologueUI;
            uiPrologue.OnFinished -= OnFinishedPrologue;
            uiPrologue.OnTutorial -= OnTutorial;
            uiPrologue.OnShowPortal -= OnShowPortal;

            uiSkipPrologue.OnSkip -= OnSkipPrologue;

            UI.Close<UIController>();
            UI.Close<UIPrologue>();
            UI.Close<UISkipPrologue>();
            UI.Close<UIPrologueFX>();
            UI.Close<UIScenarioPrologue>();

            uiController = null;
            uiPrologue = null;
            uiSkipPrologue = null;
            uiPrologueFX = null;
            uiScenarioPrologue = null;
        }

        /// <summary>
        /// 로컬라이즈 변경
        /// </summary>
        protected override void OnLocalize()
        {
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public override void EndBattle()
        {
            base.EndBattle();

            RemoveDummyPlayerEvent(); // 더미 플레이어 이벤트 연결 해제
        }

        /// <summary>
        /// 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle()
        {
            return YieldEnterBattle(0);
        }

        /// <summary>
        /// 특정 id로 진입 요청
        /// </summary>
        public override IEnumerator<float> YieldEnterBattle(int id)
        {
            base.YieldEnterBattle(id);

            bool isMale = Random.Range(0, 2) == 1; // 랜덤 성별 지정
            gender = isMale ? Gender.Male : Gender.Female;
            dummyPlayerName = isMale ? PLAYER_M : PLAYER_F; // 랜덤 성별에 따른 Object 이름 세팅

            IsSuccessEnter = true; // 무조건 성공

            yield break;
        }

        /// <summary>
        /// 전투 준비
        /// </summary>
        public override void Ready()
        {
            string sceneName = SCENE_NAME; // 씬 이름
            string bgmName = BGM_NAME; // 배경음 이름
            bool isChangeScene = true; // 씬 변경 여부
            SceneLoader.SceneType sceneType = SceneLoader.SceneType.Dungeon;

            Ready(sceneName, bgmName, isChangeScene, sceneType); // 전투 준비
        }

        /// <summary>
        /// 씬 로드 완료 후 호출
        /// </summary>
        protected override void OnSceneLoaded(GameObject[] roots)
        {
            foreach (var item in roots)
            {
                PrologueCharacter[] array = item.GetComponentsInChildren<PrologueCharacter>(includeInactive: true);
                foreach (var element in array)
                {
                    if (element.name.Equals(dummyPlayerName)) // 성별에 따른 이름 비교
                    {
                        dummyPlayer = element; // 해당 Gender 세팅
                    }
                    else
                    {
                        element.Hide(); // 나머지 숨김
                    }
                }

                Collider[] colliders = item.GetComponentsInChildren<Collider>(includeInactive: true);
                foreach (var col in colliders)
                {
                    if (col.name.Equals(Portal))
                    {
                        portalObject = col.gameObject;
                        portalObject.SetActive(false);
                    }
                    else if (col.name.Equals(Npc))
                    {
                        npcTransform = col.transform;
                    }
                }

                TargetingArrow[] arrows = item.GetComponentsInChildren<TargetingArrow>(includeInactive: true);
                foreach (var arrow in arrows)
                {
                    if (arrow.name.Equals(nameof(TargetingArrow)))
                    {
                        targetingArrow = arrow;
                        break;
                    }
                }

                Animation[] anis = item.GetComponentsInChildren<Animation>(includeInactive: true);
                foreach (var ani in anis)
                {
                    if (ani.name.Equals(Animation))
                    {
                        aniObject = ani.gameObject;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 전투 준비 완료
        /// </summary>
        protected override void OnReady()
        {
            base.OnReady();

            // 초기 시나리오 연출이 끝난후에 기존 연출 진행
            Timing.RunCoroutine(YieldWaitingDirection(), TAG);
        }

        /// <summary>
        /// 씬 구성
        /// </summary>
        protected override IEnumerator<float> YieldComposeScene()
        {
            if (dummyPlayer == null)
            {
                Debug.LogError($"더미 플레이어가 존재하지 않습니다: {nameof(sceneName)} = {sceneName}, {nameof(dummyPlayerName)} = {dummyPlayerName}");
                yield break;
            }

            cameraController.SetPlayer(dummyPlayer.CachedTransform); // 카메라 타겟 세팅
        }

        /// <summary>
        /// 모든 전투 준비 완료
        /// </summary>
        protected override void OnAllReady()
        {
            base.OnAllReady();

            AddDummyPlayerEvent(); // 더미 플레이어 이벤트 연결
        }

        /// <summary>
        /// 유닛 죽음 처리
        /// </summary>
        public override void OnDie(UnitEntity unitEntity, UnitEntity attacker)
        {
        }

        public override void OnBack()
        {
            if (uiSkipPrologue.GetSkipPrologueView().IsShow)
            {
                uiSkipPrologue.GetSkipPrologueView().Hide();
                return;
            }

            base.OnBack();
        }

        /// <summary>
        /// 초기 연출이 완료된 후에 UI 표시
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> YieldWaitingDirection()
        {
            yield return Timing.WaitForSeconds(1f); // ScenarioPrologue가 켜진후에 로딩배경 비활성화

            UIIntro uiIntro = UI.GetUI<UIIntro>();
            if (uiIntro)
                uiIntro.SetActiveSynopsis(false, true);

            Analytics.TrackEvent(TrackType.Tutorial01_Synopsis);

            yield return Timing.WaitUntilFalse(uiScenarioPrologue.IsDirection);

            // 애니메이션 연출 시작 ( 배 연출 )
            aniObject.SetActive(true);

            yield return Timing.WaitForOneFrame;

            // 시나리오 종료.
            //UI.Close<UIScenarioPrologue>(); // 또 사용함..

            // 인트로 UI 닫는 시기 조절..           

            dummyPlayer.Hide();
            targetingArrow.SetActive(false);

            // 별이 떨어지는 부분까지의 연출
            uiPrologueFX = UI.Show<UIPrologueFX>();
            yield return Timing.WaitUntilFalse(uiPrologueFX.IsFallingDirection);
            dummyPlayer.Show();

            // 전체 연출이 끝날때까지 대기
            yield return Timing.WaitUntilFalse(IsFinalParticle);           

            uiPrologue.InitPrologueData(gender == Gender.Male);
            uiPrologue.Show();

            yield return Timing.WaitForOneFrame;
            dummyPlayer.FirstIdle();

            targetingArrow.SetActive(true);

            uiPrologueFX.Hide();

            Analytics.TrackEvent(TrackType.Tutorial02_Move);
        }

        /// <summary>
        /// false 값 리턴 시, 시나리오 계속 진행. 오류의 경우 false 리턴!
        /// </summary>
        private bool IsFinalParticle()
        {
            if (uiPrologueFX == null)
                return false;

            ParticleSystem particle = uiPrologueFX.GetFinalParticle();
            if (particle == null)
                return false;

            GameObject particleObject = particle.gameObject;
            if (particleObject == null)
                return false;

            bool isFinalParticle = !particleObject.activeSelf || particle.isPlaying;
            return isFinalParticle;
        }

        private void AddDummyPlayerEvent()
        {
            if (dummyPlayer == null)
                return;

            dummyPlayer.OnPortal += OnPortal;
        }

        private void RemoveDummyPlayerEvent()
        {
            if (dummyPlayer == null)
                return;

            dummyPlayer.OnPortal -= OnPortal;
        }

        void OnPortal(GameObject other)
        {
            switch (other.name)
            {
                case Npc:
                    OnCollisionNpc();
                    break;

                case Portal:
                    OnCollisionPortal();
                    break;
            }
        }

        void OnCollisionNpc()
        {
            if (!uiPrologue.PossibleStep(ProloguePresenter.ProgressType.Flow4_table)) return;
            
            Analytics.TrackEvent(TrackType.Tutorial03_Stranger_start);

            uiPrologue.ShowNextView();

            uiController.Hide();
            dummyPlayer.MoveStop();
            targetingArrow.SetActive(false);
        }

        void OnCollisionPortal()
        {
            if (!uiPrologue.PossibleStep(ProloguePresenter.ProgressType.Flow5_effect)) return;

            uiPrologue.ShowNextView();

            uiController.Hide();
            dummyPlayer.MoveStop();
        }

        void OnStartController()
        {
            if (dummyPlayer == null)
                return;

            CameraUtils.InvokePlayerTrackingEffect();

            dummyPlayer.MoveStop();
        }

        void OnDragController(Vector2 position)
        {
            if (dummyPlayer == null)
                return;

            if (position == Vector2.zero)
                return;

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
                return;

            if (isActiveTutorial)
            {
                isActiveTutorial = false;
                uiSkipPrologue.ActiveTutorialView(isActiveTutorial);
            }

            Vector3 motion = mainCamera.transform.TransformDirection(position);
            motion.y = 0;
            motion.Normalize();
            dummyPlayer.Move(motion);
        }

        void OnResetController()
        {
            if (dummyPlayer == null)
                return;

            CameraUtils.Zoom(CameraZoomType.None);

            dummyPlayer.MoveStop();
        }

        void OnHidePrologueUI()
        {
            uiController.Show();
        }

        void OnAssetReady()
        {
            isAssetReady = true;

            if (isFinishedPrologue)
            {
                Finish();
            }
            else
            {
                uiSkipPrologue.ActiveSkipButton(true);
            }
        }

        /// <summary>
        /// 프롤로그 끝남
        /// </summary>
        void OnFinishedPrologue()
        {
            isFinishedPrologue = true;
            uiSkipPrologue.ActiveSkipButton(false);
            Finish();
            Analytics.TrackEvent(TrackType.Tutorial05_Stranger_end);
        }

        void OnTutorial()
        {
            isActiveTutorial = true;
            uiSkipPrologue.ActiveTutorialView(isActiveTutorial);

            dummyPlayer.ActiveTargetArrow(true, npcTransform);
        }

        void OnShowPortal()
        {
            portalObject.SetActive(true);

            dummyPlayer.ActiveTargetArrow(true, portalObject.transform);
        }

        /// <summary>
        /// 프롤로그 스킵
        /// </summary>
        void OnSkipPrologue()
        {
            UI.Close<UIPrologue>();
            UI.Close<UIScenarioPrologue>();
            UI.Show<UIIntro>().SetActiveSynopsis(true, useBaseView: false);
            Finish();
        }

        /// <summary>
        /// 프롤로그 종료
        /// </summary>
        private void Finish()
        {
            // 에세번들이 다운로드중이거나, 로드중이면 Tip을 보여준다.
            if (!isAssetReady)
            {
                uiSkipPrologue.ShowTipView();
                UI.Close<UIPrologue>();
                return;
            }

            // 게임 입장
            AsyncJoinGameMap();
        }

        /// <summary>
        /// 게임 맵 입장
        /// </summary>
        private async void AsyncJoinGameMap()
        {
            if (isRequestCreateCharacter)
                return;

            isRequestCreateCharacter = true;

            await Awaiters.Seconds(0.5f); // 로딩 트윈시간..

            string characterName = FilterUtils.GetAutoNickname();
            characterListModel.RequestCreateCharacter(characterName, (byte)gender).WrapNetworkErrors(); // 강제 캐릭터 생성            
        }

        private async void OnCreateCharacter()
        {            
            ResultCode resultCode = await characterListModel.RequestJoinGame(); // 게임 입장
            if (resultCode != ResultCode.SUCCESS)
                return;

            characterModel.SendLogCharacter();
            dungeonModel.StartBattle();
        }
    }
}