using AudienceNetwork;
using System;
using UnityEngine;

namespace Ragnarok
{
    public class IronSourceManager : GameObjectSingleton<IronSourceManager>
    {
        public enum PlacementNameType
        {
            // 누적결제 금액으로 체크
            None,

            // 초보자(직업등급 기준)나 첫결제내역으로 광고제거
            ShopFreeRewardedVideo,
            DungeonFreeRewardedVideo,
        }

        [SerializeField] string iosAppKey;
        [SerializeField] string androidAppKey;

        bool isInitPlugin;

        private Action onComplete;

        protected override void Awake()
        {
            base.Awake();

            // 개인화 동의팝업 표시한적이 있을 때, 바로 플러그인 초기화
            if (GameAdSettings.Instance.GameAdAppCustomType != AdAppCustomType.None)
            {
                isInitPlugin = true;
                InitPlugin();
            }
            else
            {
                // 첫 번째 광고 요청할 때, 초기화
                isInitPlugin = false;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 플러그인 제거
            DestroyPlugin();
        }

        protected override void OnTitle()
        {
        }

        /// <summary>
        /// 각 광고사에 트래킹 값 셋팅.
        /// </summary>
        /// <param name="activeTracking"></param>
        public void SetTracking(bool activeTracking)
        {
#if UNITY_EDITOR
            return;
#endif

            // 아이언소스 GDPR
            IronSource.Agent.setConsent(activeTracking);

            // 아이언소스 CCPA
            IronSource.Agent.setMetaData("do_not_sell", activeTracking ? "false" : "true");

            // 페이스북 트래킹 셋팅
            SetFBTracking(activeTracking);
        }

        private void SetFBTracking(bool activeTracking)
        {
#if UNITY_EDITOR
            return;
#endif
            // 페이스북 GDPR
            FacebookManager.Instance.SetTracking(activeTracking);

            // 오디언스 네트워크 CCPA 지원 관련
            AdSettings.SetDataProcessingOptions(new string[] {"LDU"}, 0, 0);

#if UNITY_IOS
            // 오디언스 네트워크 광고 추적 활성화(IOS)
            AdSettings.SetAdvertiserTrackingEnabled(activeTracking);
#endif
        }

        private void InitPlugin()
        {
#if UNITY_EDITOR
            return;
#endif

            // 트래킹 셋팅
            var activeTracking = GameAdSettings.Instance.GameAdAppCustomType == AdAppCustomType.On;
            SetTracking(activeTracking);

#if UNITY_IOS
            // 아이언소스 초기화
            IronSource.Agent.init(iosAppKey, IronSourceAdUnits.REWARDED_VIDEO);
#elif UNITY_ANDROID
            // 아이언소스 초기화
            IronSource.Agent.init(androidAppKey, IronSourceAdUnits.REWARDED_VIDEO);
#endif

#if DEBUG
            // 아이언소스 미디에이션 통합 확인
            IronSource.Agent.validateIntegration();
            // 어댑터 오류로그 확인, 테스트 후 지울것.
            IronSource.Agent.setAdaptersDebug(true);
#endif

            // 이벤트 등록
            IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
        }

        private void DestroyPlugin()
        {
#if UNITY_EDITOR
            return;
#endif

            // 이벤트 제거
            IronSourceEvents.onRewardedVideoAdOpenedEvent -= RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent -= RewardedVideoAdClickedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent -= RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent -= RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent -= RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent -= RewardedVideoAdShowFailedEvent;
        }

        /// <summary>애플리케이션 상태</summary>
        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }

#region event
        private void RewardedVideoAdOpenedEvent()
        {
            // 광고 시작
            Debug.LogWarning("open___________________");
        }

        private void RewardedVideoAdClickedEvent(IronSourcePlacement obj)
        {
            Debug.LogWarning("click___________________" + obj.getPlacementName());
        }

        private void RewardedVideoAdClosedEvent()
        {
            // 광고 종료
            Debug.LogWarning("close___________________");
        }

        // 광고를 볼 수 있는경우
        private void RewardedVideoAvailabilityChangedEvent(bool available)
        {
            //Change the in-app 'Traffic Driver' state according to availability.
            bool rewardedVideoAvailability = available;

            Debug.LogWarning("광고 볼수 있나?___________________" + available);
        }

        private void RewardedVideoAdStartedEvent()
        {
            Debug.LogWarning("start___________________");
        }

        private void RewardedVideoAdEndedEvent()
        {
            Debug.LogWarning("end___________________");
        }

        private void RewardedVideoAdRewardedEvent(IronSourcePlacement obj)
        {
            Debug.LogWarning("reward___________________" + obj.getPlacementName());

            onComplete?.Invoke();
            onComplete = null;
        }

        private void RewardedVideoAdShowFailedEvent(IronSourceError obj)
        {
            // 광고 오류로 중단된듯..
            Debug.LogWarning("fail code___________________" + obj.getCode());
            Debug.LogWarning("fail description___________________" + obj.getDescription());

            UI.ConfirmPopup(obj.getCode().ToString(), obj.getDescription());
        }
#endregion

        /// <summary>
        /// 광고 가용성 체크..(네트워크 환경이 원활하고, 광고가 로드 완료된 상태)
        /// </summary>
        private bool IsRewardedVideoAvailable()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        /// <summary>
        /// 광고 최대치 체크..(특정 시간동안 가능한 광고를 모두 본 상태)
        /// </summary>
        public bool IsRewardedVideoPlacementCapped(string placementName)
        {
            return IronSource.Agent.isRewardedVideoPlacementCapped(placementName);
        }

        /// <summary>
        /// 광고 시작
        /// </summary>
        /// <param name="placementName">플레이스먼트 그룹</param>
        /// <param name="hasPaymentHistory">첫 결제 했는지, None의 경우는 누적 결제내역</param>
        /// <param name="isBeginner">초보자(직업등급 1까지), 누적 결제는 체크하지 않음</param>
        /// <param name="onComplete">광고 완료</param>
        /// <param name="descriptionId">광고 선택팝업 문구 언어 key</param>
        public async void ShowRewardedVideo(PlacementNameType placementName, bool hasPaymentHistory, bool isBeginner = false, Action onComplete = null, int descriptionId = 8058)
        {
            this.onComplete = onComplete;

            #region 광고없이 보상 받을 수 있는지 체크
            if (placementName == PlacementNameType.None) // 누적 결제로 광고제거
            {
                if (hasPaymentHistory)
                {
                    // 누적결제 즉시보상
                    this.onComplete?.Invoke();
                    this.onComplete = null;
                    return;
                }
            }
            else // 첫 결제로 광고제거
            {
                // 첫결제 체크
                if (hasPaymentHistory)
                {
                    // 과금유저 즉시보상
                    this.onComplete?.Invoke();
                    this.onComplete = null;
                    return;
                }

                // 초보자 체크
                if (isBeginner)
                {
                    await UI.ConfirmPopupAsync(LocalizeKey._8063.ToText() // {GRADE}차전직 전에는 광고 없이 상품을 받을 수 있습니다.
                        .Replace(ReplaceKey.GRADE, BasisType.AD_NEED_JOB_GRADE.GetInt().ToString()));
                    this.onComplete?.Invoke();
                    this.onComplete = null;
                    return;
                }
            }
            #endregion

            #region 개인화 동의 체크
            switch (GameAdSettings.Instance.GameAdAppCustomType)
            {
                case AdAppCustomType.None:
                    UI.Show<UIAdConsent>();
                    return;

                case AdAppCustomType.Off:
                    //UI.ShowToastPopup(LocalizeKey._14403.ToText()); // 먼저 게임내 설정에서 개인화 동의를 활성화 해주세요.
                    //return;
                    // 일반 광고
                    break;

                case AdAppCustomType.On:
                default:
                    // 개인화 광고
                    break;
            }

            // 개인화 동의를 얻은 후에 플러그인 초기화
            if (!isInitPlugin)
            {
                isInitPlugin = true;
                InitPlugin();

                // 초기화 타이밍에 광고 요청하지 않게 추가함.
                UI.ConfirmPopup(LocalizeKey._8059.ToText()); // 광고를 준비하고 있습니다. 5분 후에 다시 시도하십시오.
                return;
            }
            #endregion

            #region 에디터 체크
#if UNITY_EDITOR
            // 1. 에디터 체크
            this.onComplete?.Invoke();
            this.onComplete = null;
            return;
#endif
            #endregion

            #region 광고보고 보상 받을지 선택
            // 광고 안내팝업
            if (!await UI.SelectPopup(descriptionId.ToText())) // 광고를 시청하고 상품을 받으시겠습니까?
                return;

            if (placementName == PlacementNameType.None) // 기본 광고
            {
                // 기본 광고는 최대치 체크 안함.
                ShowRewardedVideoResultState(placementName);
            }
            else // 광고없으면 바로 보상
            {
                // 광고 최대치 체크
                if (IsRewardedVideoPlacementCapped(placementName.ToString()))
                {
                    // 광고 쿨타임.. 보상 즉시지급
                    await UI.ConfirmPopupAsync(LocalizeKey._8060.ToText()); // 준비된 광고가 없습니다. 바로 상품을 받아가세요.
                    this.onComplete?.Invoke();
                    this.onComplete = null;
                    return;
                }
                else
                {
                    ShowRewardedVideoResultState(placementName);
                }
            }
            #endregion
        }        

        // 대시보드에 설정된 플레이스먼트의 보상값이 필요할때 인데.. 이건 필요 없을거같은데.. 
        public void GetPlacementInfo(string placementName)
        {
            IronSourcePlacement placement = IronSource.Agent.getPlacementInfo(placementName);
        }

        /// <summary>
        /// 광고 최대치를 먼저 체크해야 함.
        /// </summary>
        private void ShowRewardedVideoResultState(PlacementNameType placementName)
        {
            // 광고 가용성 확인
            if (!IsRewardedVideoAvailable())
            {
                UI.ConfirmPopup(LocalizeKey._8059.ToText()); // 광고를 준비하고 있습니다. 5분 후에 다시 시도하십시오.
                return;
            }

            // 광고 실행
            if (placementName == PlacementNameType.None)
            {
                IronSource.Agent.showRewardedVideo(); // 기본
            }
            else
            {
                IronSource.Agent.showRewardedVideo(placementName.ToString()); // 플레이스먼트 네임으로
            }            
        }
    }
}