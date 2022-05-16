using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CameraController : GameObjectSingleton<CameraController>, IAutoInspectorFinder
    {
        private const float DEFAULT_WEIGHT = 1f;
        private const float PLUS_WEIGHT = 2f;

        private const float SKILL_CAMERA_FADE_ALPHA = 0.6f;

        private readonly float CameraPinchZoomSpeed = 0.05f;

        public enum View
        {
            Top = 1,
            Quater40_2_5,
            Quater40_5,
            Quater40_10,
            Quater50_5,
            Quater50_10,
            Quater50_17,
            QuaterFree,
            Back,
            ZoomOutBack,
            Group,
            GroupHigh,
            SiegeView,
            LongMazeView,
            GuildWarView,
            ChallengeViewZoomOut,
            ChallengeViewZoomIn,
            WaitRoomView,
            TestTopView,
            GroupViewFront,
            GroupViewSkill,
            RightAngle, // 90도 회전뷰
            RightAngle_90, // 90도 회전뷰
            RightAngle_180, // 90도 회전뷰
            RightAngle_270, // 90도 회전뷰
            Front,
            Wide,
            Dialogue,
            Guide,
            MvpBattle,
            NaviQuater50,
            UniqueMvpMonsterBattle,
            EventMazeView,

            /**********/
            FIRST = Top,
            LAST = RightAngle,
        }

        public enum Clearshot
        {
            None = 1,
            Inven,
            Chat,
            MazeTopView,
            MazeBattleView,
        }

        public enum Timeline
        {
            None = 1,
            BossCut,
        }

        public enum CameraEffect
        {
            None = 0,
            UniqueMvpBoss = 1,
        }

        /******************** View - Priority 1 ********************/
        [SerializeField] CinemachineVirtualCamera topView;
        [SerializeField] CinemachineVirtualCamera quaterView40_2_5;
        [SerializeField] CinemachineVirtualCamera quaterView40_5;
        [SerializeField] CinemachineVirtualCamera quaterView40_10;
        [SerializeField] CinemachineVirtualCamera quaterViewFree;
        [SerializeField] CinemachineVirtualCamera backView;
        [SerializeField] CinemachineVirtualCamera zoomOutBackView;
        Transform groupViewTransform;
        Vector3 groupViewOrigAngle;
        [SerializeField] CinemachineVirtualCamera rightAngleView;
        [SerializeField] CinemachineVirtualCamera rightAngleView_90;
        [SerializeField] CinemachineVirtualCamera rightAngleView_180;
        [SerializeField] CinemachineVirtualCamera rightAngleView_270;
        int currentRightAngleViewIndex; // 직각회전뷰 각도 (0~3)
        [SerializeField] CinemachineVirtualCamera quaterView50_5;
        [SerializeField] CinemachineVirtualCamera quaterView50_10;
        [SerializeField] CinemachineVirtualCamera quaterView50_17;

        /******************** PlayMode - Priority 2 ********************/
        [SerializeField] CinemachineVirtualCamera naviQuaterView50;

        /******************** PlayMode - Priority 3 ********************/
        public BossCutViewTimeline bossCutView;

        /******************** PlayMode - Priority 5 ********************/
        // 커스텀 하나로는 Linear 하게 보여줄 수 없으므로 두 개의 CustomView 를 번갈아가면서 처리한다
        [SerializeField] CinemachineVirtualCamera customView1;
        [SerializeField] CinemachineVirtualCamera customView2;

        /******************** CamMode - Priority 6 ********************/
        [SerializeField] CinemachineVirtualCamera invenView;
        [SerializeField] CinemachineVirtualCamera chatView;
        [SerializeField] CinemachineVirtualCamera mazeTopView; // 미로 탑뷰
        [SerializeField] CinemachineVirtualCamera mazeBattleView; // 미로 전투 뷰

        /******************** CamMode - Priority 8 ********************/
        [SerializeField] CinemachineVirtualCamera uniqueMvpMonsterBattleView; // unique mvp 보스 뷰

        /******************** PlayMode - Priority 10 ********************/
        [SerializeField] CinemachineVirtualCamera waitRoomView;
        [SerializeField] CinemachineVirtualCamera testTopView;
        [SerializeField] CinemachineVirtualCamera frontView;
        [SerializeField] CinemachineVirtualCamera guideView;
        [SerializeField] CinemachineVirtualCamera wideView;
        [SerializeField] CinemachineVirtualCamera dialogueView;
        [SerializeField] CinemachineVirtualCamera longMazeView;
        [SerializeField] CinemachineVirtualCamera guildWarView;
        [SerializeField] CinemachineVirtualCamera challengeViewZoomOut;
        [SerializeField] CinemachineVirtualCamera challengeViewZoomIn;
        [SerializeField] CinemachineVirtualCamera siegeView;
        [SerializeField] CinemachineVirtualCamera groupView;
        [SerializeField] CinemachineVirtualCamera groupViewHigh;
        [SerializeField] CinemachineBlendListCamera groupViewFront;
        [SerializeField] CinemachineBlendListCamera groupViewSkill;
        [SerializeField] CinemachineVirtualCamera mvpBattleView;
        [SerializeField] CinemachineVirtualCamera eventMazeView;

        /******************** PlayMode - Priority 20 ********************/
        [SerializeField] CinemachineVirtualCamera testView;

        [SerializeField] CinemachineTargetGroup targetGroup;
        [SerializeField] CinemachineTargetGroup targetGroupFront;
        [SerializeField] CinemachineTargetGroup targetGroupSkill;
        [SerializeField] CinemachineTargetGroup targetGroupDialogue;

        [SerializeField] GameObject goSkillCamera;
        [SerializeField] SpriteRenderer sprSkillCameraFade;

        /******************** Settings ********************/
        [SerializeField] NoiseSettings uniqueMvpMonsterNoise;

        View view;
        Clearshot clearshot;
        Timeline timeline;
        Transform player;
        private bool isGroupViewRotate;
        [SerializeField] private float groupViewRotateSpeed;
        [SerializeField] float rotAngle;
        float groupViewAngleY = -150f;
        float groupViewAngleStartY = -150f;

        private CinemachineVirtualCamera[] allCamera; // 모든 카메라 목록
        private Dictionary<View, CinemachineVirtualCamera> cameraOfViewDic; // 각 뷰에 해당하는 카메라
        private Dictionary<Clearshot, CinemachineVirtualCamera> cameraOfClearshotDic; // 각 클리어샷에 해당하는 카메라
        Dictionary<CinemachineVirtualCamera, (CinemachineFramingTransposer transposer, float distance)> cameraDistanceDic; // 모든 Transposer 카메라의 초기 Distance
        Dictionary<CinemachineVirtualCamera, CinemachineBasicMultiChannelPerlin> cameraNoiseDic; // Noise가 있는 카메라들

        public delegate void CameraViewChangeEvent(View view);
        public delegate void CameraClearShotChangeEvent(Clearshot clearShot);
        public delegate void HideByWallEvent(bool isHide);

        public event CameraViewChangeEvent OnCameraViewChange;
        public event CameraClearShotChangeEvent OnCameraClearShotChange;

        private bool isInitializeUniqueMvpViewNoise;
        private bool isInitializeMvpBattleViewRotate;

        protected override void Awake()
        {
            base.Awake();

            allCamera = new CinemachineVirtualCamera[]
            {
                /******************** View - Priority 1 ********************/
                topView,
                quaterView40_2_5,
                quaterView40_5,
                quaterView40_10,
                quaterViewFree,
                backView,
                zoomOutBackView,
                groupView,
                groupViewHigh,
                siegeView,
                longMazeView,
                guildWarView,
                challengeViewZoomOut,
                challengeViewZoomIn,
                rightAngleView,
                rightAngleView_90,
                rightAngleView_180,
                rightAngleView_270,
                quaterView50_5,
                quaterView50_10,
                quaterView50_17,
                waitRoomView,
                testTopView,
                testView,
                frontView,
                wideView,
                customView1,
                customView2,
                guideView,
                mvpBattleView,
                naviQuaterView50,
                uniqueMvpMonsterBattleView,
                eventMazeView,

                /******************** CamMode - Priority 2 ********************/
                invenView,
                chatView,
                mazeTopView,
                mazeBattleView,
            };

            cameraOfViewDic = new Dictionary<View, CinemachineVirtualCamera>()
            {
                /******************** View - Priority 1 ********************/
                { View.Top, topView },
                { View.Quater40_2_5, quaterView40_2_5 },
                { View.Quater40_5, quaterView40_5 },
                { View.Quater40_10, quaterView40_10 },
                { View.QuaterFree, quaterViewFree },
                { View.Back, backView },
                { View.ZoomOutBack, zoomOutBackView },
                { View.Group, groupView },
                { View.GroupHigh, groupViewHigh },
                { View.SiegeView, siegeView },
                { View.LongMazeView, longMazeView },
                { View.GuildWarView, guildWarView },
                { View.ChallengeViewZoomOut, challengeViewZoomOut },
                { View.ChallengeViewZoomIn, challengeViewZoomIn },
                { View.RightAngle, rightAngleView },
                { View.RightAngle_90, rightAngleView_90 },
                { View.RightAngle_180, rightAngleView_180 },
                { View.RightAngle_270, rightAngleView_270 },
                { View.Quater50_5, quaterView50_5 },
                { View.Quater50_10, quaterView50_10 },
                { View.Quater50_17, quaterView50_17 },
                { View.WaitRoomView, waitRoomView },
                { View.TestTopView, testTopView },
                { View.Front, frontView },
                { View.Wide, wideView },
                { View.Dialogue, dialogueView },
                { View.Guide, guideView },
                { View.MvpBattle, mvpBattleView },
                { View.NaviQuater50, naviQuaterView50 },
                { View.UniqueMvpMonsterBattle, uniqueMvpMonsterBattleView },
                { View.EventMazeView, eventMazeView },
            };

            cameraOfClearshotDic = new Dictionary<Clearshot, CinemachineVirtualCamera>()
            {
                /******************** CamMode - Priority 2 ********************/
                { Clearshot.Inven, invenView },
                { Clearshot.Chat, chatView },
                { Clearshot.MazeTopView, mazeTopView },
                { Clearshot.MazeBattleView, mazeBattleView },
            };

            // Distance Dic 설정
            cameraDistanceDic = new Dictionary<CinemachineVirtualCamera, (CinemachineFramingTransposer transposer, float distance)>();
            foreach (var cam in allCamera)
            {
                if (cam == null)
                    continue;

                var transposer = cam.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (transposer is null)
                    continue;

                cameraDistanceDic[cam] = (transposer: transposer, distance: transposer.m_CameraDistance);
            }

            // Noise Dic 설정
            cameraNoiseDic = new Dictionary<CinemachineVirtualCamera, CinemachineBasicMultiChannelPerlin>();
            foreach (var cam in allCamera)
            {
                if (cam == null)
                    continue;

                var noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (noise is null)
                    continue;

                cameraNoiseDic[cam] = noise;
            }


            isGroupViewRotate = false;
            groupViewTransform = groupView.transform;
            groupViewOrigAngle = groupViewTransform.localEulerAngles;
        }

        protected override void OnTitle()
        {
            currentRightAngleViewIndex = 0;

            HideCustomView();
            SetView(View.Top);
            SetClearshot(Clearshot.None);
            SetTimeline(Timeline.None);
        }

        void LateUpdate()
        {
            // Group 뷰
            if (view == View.Group)
            {
                if (isGroupViewRotate)
                {
                    float newAngleY = Mathf.Lerp(groupViewAngleY, groupViewAngleStartY - rotAngle, Time.deltaTime * groupViewRotateSpeed);
                    groupViewAngleY = newAngleY;
                    Vector3 angle = groupViewTransform.localEulerAngles;
                    angle.y = newAngleY;
                    groupViewTransform.localEulerAngles = angle;
                }
            }

            if (goSkillCamera.activeSelf)
            {
                goSkillCamera.GetComponent<Camera>().fieldOfView = Camera.main.fieldOfView;
            }
        }

#if UNITY_EDITOR
        private void DrawCollider(Vector3 center, Vector3 extents)
        {
            // 위 사각형
            float x = extents.x;
            float y = extents.y;
            //Debug.DrawLine(GetVector3(cetner, ));
        }

        private Vector3 GetVector3(Vector3 center, float x, float y, float z)
        {
            return center + Vector3.right * x + Vector3.up * y + Vector3.forward * z;
        }
#endif

        /// <summary>
        /// 다음 뷰로 변경
        /// </summary>
        public void SetNextView()
        {
            View nextView = view + 1;

            if (nextView > View.LAST)
                nextView = View.FIRST;

            SetView(nextView);
        }

        /// <summary>
        /// 뷰 이름 반환
        /// </summary>
        public string GetViewName()
        {
            return view.ToString();
        }

        /// <summary>
        /// 뷰 반환
        /// </summary>
        public View GetView()
        {
            return view;
        }

        /// <summary>
        /// 뷰 세팅
        /// </summary>
        public void SetView(View view)
        {
            if (view == View.MvpBattle)
            {
                InitializeMvpBattleViewRotate();
            }

            bool isDirty = (this.view != view);

            // 이전 카메라 초기화
            SetCameraNoise();
            CameraUtils.Zoom(CameraZoomType.None);

            this.view = view;

            // 카메라 Active 설정
            foreach (var pair in cameraOfViewDic)
            {
                var thisView = pair.Key;
                var thisCam = pair.Value;

                thisCam.enabled = (thisView == this.view);
            }

            groupViewFront.enabled = view == View.GroupViewFront;
            groupViewSkill.enabled = view == View.GroupViewSkill;

            // 특수 처리
            backView.Priority = view == View.Back ? 2 : 1;
            zoomOutBackView.Priority = view == View.ZoomOutBack ? 2 : 1;
            if (view == View.Group)
                groupViewTransform.localEulerAngles = groupViewOrigAngle;

            // 새 카메라 초기화
            if (isDirty)
            {
                SetCameraNoise();
                CameraUtils.Zoom(CameraZoomType.None);
            }

            if (isDirty)
            {
                OnCameraViewChange?.Invoke(this.view);
            }
        }

        /// <summary>
        /// 직각회전뷰 다음 각도로 회전
        /// </summary>
        public void RotateRightAngleViewNext()
        {
            SetRightAngleViewIndex(currentRightAngleViewIndex + 1);
        }

        /// <summary>
        /// 직각회전뷰 이전 각도로 회전
        /// </summary>
        public void RotateRightAngleViewPrev()
        {
            SetRightAngleViewIndex(currentRightAngleViewIndex - 1);
        }

        /// <summary>
        /// 직각회전뷰 각도 세팅
        /// </summary>
        /// <param name="idx">각도 인덱스 (0~3)</param>
        void SetRightAngleViewIndex(int idx)
        {
            if (!IsRightAngleView(this.view))
                return;

            idx = (idx + 4) % 4; // idx Clamp
            currentRightAngleViewIndex = idx;

            this.view = View.RightAngle + currentRightAngleViewIndex;

            SetView(this.view);
            return;
        }

        private bool IsRightAngleView(View view)
        {
            if (view == View.RightAngle || view == View.RightAngle_90 || view == View.RightAngle_180 || view == View.RightAngle_270)
                return true;
            return false;
        }


        /// <summary>
        /// 현재 View 카메라 Distance 반환
        /// </summary>
        public float GetCameraDistance()
        {
            var thisCam = GetCameraOfView(this.view);
            if (thisCam is null || !cameraDistanceDic.ContainsKey(thisCam))
                return 0f;

            return cameraDistanceDic[thisCam].transposer.m_CameraDistance;
        }

        /// <summary>
        /// 카메라 Distance 강제 설정
        /// </summary>
        public void SetCameraDistanceForce(float dist)
        {
            var thisCam = GetCameraOfView(this.view);
            if (thisCam is null || !cameraDistanceDic.ContainsKey(thisCam))
                return;

            cameraDistanceDic[thisCam].transposer.m_CameraDistance = dist;
        }

        /// <summary>
        /// 자유쿼터뷰 핀치줌 적용
        /// </summary>
        [System.Obsolete]
        public void AddQuaterFreeViewPinchZoom(float zoomDistance)
        {
            if (zoomDistance == 0f)
                return;

            if (this.view != View.QuaterFree)
                return;

            var quaterFreeViewCam = GetCameraOfView(View.QuaterFree);
            var transposer = cameraDistanceDic[quaterFreeViewCam].transposer;
            transposer.m_CameraDistance += zoomDistance * CameraPinchZoomSpeed;
            transposer.m_CameraDistance = Mathf.Max(2f, transposer.m_CameraDistance);

            OnCameraViewChange?.Invoke(this.view);
        }

        /// <summary>
        /// 카메라 흑백필터
        /// </summary>
        public void SetCameraGrayScale(bool isActive)
        {
            var grayScaleEffect = Camera.main.gameObject.GetComponent<GrayscaleEffect>();
            if (grayScaleEffect is null)
            {
                Debug.LogError("GrayscaleEffect 없음");
                return;
            }
            grayScaleEffect.enabled = isActive;
        }

        /// <summary>
        /// 카메라 번쩍임
        /// </summary>
        public void FlashCamera()
        {
            var flash = Camera.main.gameObject.GetComponent<ContrastStretchEffect>();
            if (flash is null)
            {
                Debug.LogError("ContrastStretchEffect 없음");
                return;
            }
            flash.enabled = false;

            //flash.adaptationSpeed = 0.06f;
            //flash.limitMinimum = 0f;
            //flash.limitMaximum = 0.75f;

            flash.enabled = true;
        }

        public void SetCameraBlur(bool isActive)
        {
            var motionBlur = Camera.main.gameObject.GetComponent<MotionBlur>();
            if (motionBlur is null)
            {
                Debug.LogError("MotionBlur 없음");
                return;
            }
            motionBlur.enabled = isActive;
        }

        /// <summary>
        /// 현재 View 카메라 Noise 설정
        /// </summary>
        public void SetCameraNoise(float freq = 0f)
        {
            var thisCam = GetCameraOfView(this.view);
            if (thisCam is null || !cameraNoiseDic.ContainsKey(thisCam))
                return;

            cameraNoiseDic[thisCam].m_AmplitudeGain = freq;
        }

        /// <summary>
        /// View에 해당하는 Camera 반환
        /// </summary>
        private CinemachineVirtualCamera GetCameraOfView(View view)
        {
            if (!cameraOfViewDic.ContainsKey(view))
            {
                Debug.Log($"존재하지 않는 View : {view}");
                return null;
            }

            return cameraOfViewDic[view];
        }

        public void SetGroupViewRotate(bool isRotate)
        {
            groupViewAngleStartY = groupViewAngleY = groupViewTransform.localEulerAngles.y;

            isGroupViewRotate = isRotate;
        }

        /// <summary>
        /// 클리어샷 세팅
        /// </summary>
        public void SetClearshot(Clearshot clearshot)
        {
            if (this.clearshot == clearshot)
                return;

            this.clearshot = clearshot;

            invenView.enabled = clearshot == Clearshot.Inven;
            chatView.enabled = clearshot == Clearshot.Chat;
            mazeTopView.enabled = clearshot == Clearshot.MazeTopView;
            mazeBattleView.enabled = clearshot == Clearshot.MazeBattleView;

            OnCameraClearShotChange?.Invoke(this.clearshot);
        }

        /// <summary>
        /// 타임라인 세팅
        /// </summary>
        public void SetTimeline(Timeline timeline)
        {
            if (this.timeline == timeline)
                return;

            this.timeline = timeline;

            bossCutView.enabled = timeline == Timeline.BossCut;
        }

        /// <summary>
        /// 플레이어 세팅
        /// </summary>
        public void SetPlayer(Transform player)
        {
            this.player = player;

            var exceptCameras = new CinemachineVirtualCamera[] { GetCameraOfView(View.Group), GetCameraOfView(View.GroupHigh), GetCameraOfView(View.Front), GetCameraOfView(View.Wide), GetCameraOfView(View.Dialogue), GetCameraOfView(View.Guide), }; // Follow 세팅 제외 대상
            foreach (var cam in allCamera)
            {
                if (exceptCameras.Contains(cam))
                    continue;

                cam.Follow = player;
            }

            backView.LookAt = player;
            zoomOutBackView.LookAt = player;
            mazeBattleView.LookAt = player;
        }

        /// <summary>
        /// 플레이어 반환
        /// </summary>
        public Transform GetPlayer()
        {
            return player;
        }

        public void SetNpc(Transform npc)
        {
            frontView.Follow = npc;
            guideView.Follow = npc;
        }

        /// <summary>
        /// 카메라 멤버 추가
        /// </summary>
        public void AddMember(Transform member, float radius = 1f, float weight = 1f)
        {
            if (member == null)
                return;

            AddMember(targetGroup, member, weight, radius);
            AddMember(targetGroupFront, member, weight, radius);
            AddMember(targetGroupDialogue, member, weight, radius);
        }

        public void AddMemberSkillGroup(Transform member, float radius = 1f, float weight = 1f)
        {
            if (member == null)
                return;

            AddMember(targetGroupSkill, member, weight, radius);
        }

        /// <summary>
        /// 카메라 맴버 추가 (다이얼로그 전용)
        /// </summary>
        public void AddDialogueMember(Transform member)
        {
            AddMember(member, 12f, 1f);
        }

        /// <summary>
        /// 카메라 멤버 제거
        /// </summary>
        public void RemoveMember(Transform member)
        {
            RemoveMember(targetGroup, member);
            RemoveMember(targetGroupFront, member);
            RemoveMember(targetGroupDialogue, member);
        }

        public void RemoveMemberSkillGroup(Transform member)
        {
            RemoveMember(targetGroupSkill, member);
        }

        public void ClearMember()
        {
            ClearMember(targetGroup);
            ClearMember(targetGroupFront);
            ClearMember(targetGroupDialogue);
        }

        public void ClearMemberSkillGroup()
        {
            ClearMember(targetGroupSkill);
        }

        public CinemachineTargetGroup.Target[] GetMember()
        {
            return targetGroup.m_Targets;
        }

        public void PlusWeight(Transform member)
        {
            SetWeight(targetGroup, member, PLUS_WEIGHT);
            SetWeight(targetGroupFront, member, PLUS_WEIGHT);
            SetWeight(targetGroupDialogue, member, PLUS_WEIGHT);
        }

        public void ResetWeight(Transform member)
        {
            SetWeight(targetGroup, member, DEFAULT_WEIGHT);
            SetWeight(targetGroupFront, member, DEFAULT_WEIGHT);
            SetWeight(targetGroupDialogue, member, DEFAULT_WEIGHT);
        }

        private void AddMember(CinemachineTargetGroup tg, Transform member, float weight, float radius)
        {
            if (member == null)
                return;

            // 이미 존재할 경우
            if (tg.FindMember(member) != -1)
                return;

            tg.AddMember(member, weight, radius);
        }

        private void RemoveMember(CinemachineTargetGroup tg, Transform member)
        {
            if (member == null)
                return;

            tg.RemoveMember(member);
        }

        private void ClearMember(CinemachineTargetGroup tg)
        {
            if (tg.IsEmpty)
                return;

            foreach (var item in tg.m_Targets)
            {
                tg.RemoveMember(item.target);
            }
        }

        private void SetWeight(CinemachineTargetGroup tg, Transform member, float weight)
        {
            int index = tg.FindMember(member);
            if (index == -1)
                return;

            tg.m_Targets[index].weight = weight;
        }

        /// <summary>
        /// 보스 세팅
        /// </summary>
        public void SetBoss(Transform player, Transform boss)
        {
            bossCutView.SetTarget(player, boss);
        }

        /// <summary>
        /// 카메라 레이어 추가
        /// </summary>
        public void AddMask(params int[] layers)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            foreach (var layer in layers)
            {
                mainCamera.cullingMask |= 1 << layer;
            }
        }

        /// <summary>
        /// 카메라 레이어 제거
        /// </summary>
        public void RemoveMask(params int[] layers)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            foreach (var layer in layers)
            {
                mainCamera.cullingMask &= ~(1 << layer);
            }
        }

        public void SetTestViewRotateX(float rotateX)
        {
            Vector3 angle = testView.transform.localEulerAngles;
            angle.x = rotateX;
            testView.transform.localEulerAngles = angle;
        }

        public void SetTestViewRotateY(float rotateY)
        {
            Vector3 angle = testView.transform.localEulerAngles;
            angle.y = rotateY;
            testView.transform.localEulerAngles = angle;
        }

        public void SetTestViewDistance(float distance)
        {
            CinemachineFramingTransposer transposer = testView.GetCinemachineComponent<CinemachineFramingTransposer>();
            transposer.m_CameraDistance = distance;
        }

        public void SetCustomViewRotateX(float rotateX)
        {
            if (customView1.enabled)
            {
                Vector3 angle = customView1.transform.localEulerAngles;
                angle.x = rotateX;
                customView1.transform.localEulerAngles = angle;
            }
            else if (customView2.enabled)
            {
                Vector3 angle = customView2.transform.localEulerAngles;
                angle.x = rotateX;
                customView2.transform.localEulerAngles = angle;
            }
        }

        public void SetCustomViewRotateY(float rotateY)
        {
            if (customView1.enabled)
            {
                Vector3 angle = customView1.transform.localEulerAngles;
                angle.y = rotateY;
                customView1.transform.localEulerAngles = angle;
            }
            else if (customView2.enabled)
            {
                Vector3 angle = customView2.transform.localEulerAngles;
                angle.y = rotateY;
                customView2.transform.localEulerAngles = angle;
            }
        }

        public void SetCustomViewDistance(float distance)
        {
            if (customView1.enabled)
            {
                CinemachineFramingTransposer transposer = customView1.GetCinemachineComponent<CinemachineFramingTransposer>();
                transposer.m_CameraDistance = distance;
            }
            else if (customView2.enabled)
            {
                CinemachineFramingTransposer transposer = customView2.GetCinemachineComponent<CinemachineFramingTransposer>();
                transposer.m_CameraDistance = distance;
            }
        }

        public float GetDefaultAngleX()
        {
            CinemachineVirtualCamera camera = GetCameraOfView(view);
            if (camera == null)
                return 0f;

            return camera.transform.localEulerAngles.x;
        }

        public float GetDefaultAngleY()
        {
            CinemachineVirtualCamera camera = GetCameraOfView(view);
            if (camera == null)
                return 0f;

            return camera.transform.localEulerAngles.y;
        }

        public float GetDefaultDistance()
        {
            CinemachineVirtualCamera camera = GetCameraOfView(view);
            if (camera == null)
                return 0f;

            CinemachineFramingTransposer transposer = testView.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer == null)
                return 0f;

            return transposer.m_CameraDistance;
        }

        public void ShowTestView()
        {
            testView.enabled = true;
        }

        public void HideTestView()
        {
            testView.enabled = false;
        }

        public void ShowCustomView()
        {
            if (customView1.enabled)
            {
                customView1.enabled = false;
                customView2.enabled = true;
            }
            else
            {
                customView1.enabled = true;
                customView2.enabled = false;
            }
        }

        public void HideCustomView()
        {
            customView1.enabled = false;
            customView2.enabled = false;
        }

        public void ShowSkillCamera(bool isActive)
        {
            if (isActive == true) // Fade Alpha 0으로 초기화
            {
                TweenAlpha.Begin(sprSkillCameraFade.gameObject, 0f, 0f);

                RemoveMask(Layer.CUBE, Layer.DEFAULT);
                Camera.main.clearFlags = CameraClearFlags.Depth;
            }
            else
            {
                AddMask(Layer.CUBE, Layer.DEFAULT);
                Camera.main.clearFlags = CameraClearFlags.Skybox;
            }

            goSkillCamera.SetActive(isActive);
        }

        public void SkillCameraFadeOver(float duration)
        {
            TweenAlpha.Begin(sprSkillCameraFade.gameObject, duration, SKILL_CAMERA_FADE_ALPHA);
        }

        public void SkillCameraFadeOut(float duration)
        {
            TweenAlpha.Begin(sprSkillCameraFade.gameObject, duration, 0f);
        }

        public void SetConfine(View view, Collider boundingVolume)
        {
            CinemachineVirtualCamera vcam = GetCameraOfView(view);
            if (vcam == null)
                return;

            CinemachineConfiner confiner = vcam.GetComponent<CinemachineConfiner>();
            if (confiner == null)
                return;

            confiner.m_BoundingVolume = boundingVolume;
        }

        public void SetEffect(CameraEffect effect)
        {
            if (effect == CameraEffect.UniqueMvpBoss)
                InitializeUniqueMvpViewNoise();
        }

        private void InitializeUniqueMvpViewNoise()
        {
            if (isInitializeUniqueMvpViewNoise)
                return;

            isInitializeUniqueMvpViewNoise = true;

            CinemachineBasicMultiChannelPerlin channelPerlin = uniqueMvpMonsterBattleView.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            channelPerlin.m_NoiseProfile = uniqueMvpMonsterNoise;
        }

        private void InitializeMvpBattleViewRotate()
        {
            if (isInitializeMvpBattleViewRotate)
                return;

            isInitializeMvpBattleViewRotate = true;

            float rotateX = MathUtils.ToPercentValue(BasisType.MVP_VIEW_CAMERA_ANGLE.GetInt());
            Vector3 angle = mvpBattleView.transform.localEulerAngles;
            angle.x = rotateX;
            mvpBattleView.transform.localEulerAngles = angle;
        }
    }
}