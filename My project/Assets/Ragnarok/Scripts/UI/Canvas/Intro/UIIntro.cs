using CodeStage.AntiCheat.ObscuredTypes;
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIIntro : UICanvas
    {
        public enum State
        {
            /// <summary>
            /// 아무 상태 아님
            /// </summary>
            None,
            /// <summary>
            /// 계정 로그인
            /// </summary>
            ReqAccountLogin,
            /// <summary>
            /// 지역 코드 받아오기
            /// </summary>
            ReqContryCode,
            /// <summary>
            /// 인증서버 연결
            /// </summary>
            ReqAuthServerConnect,
            /// <summary>
            /// 인증서버 로그인
            /// </summary>
            ReqAuthServerLogin,
            /// <summary>
            /// 게임서버 연결
            /// </summary>
            ReqGameServerConnect,
            /// <summary>
            /// 게임서버 로그인
            /// </summary>
            ReqGameServerLogin,
            /// <summary>
            /// 로컬 데이터 로드
            /// </summary>
            LoadLocalData,
            /// <summary>
            /// 데이터 버전 체크
            /// </summary>
            CheckDataVersion,
            /// <summary>
            /// 어셋번들 패치리스트 다운로드
            /// </summary>
            DownloadAssetBundlePatchList,
            /// <summary>
            /// 데이터 다운로드
            /// </summary>
            DownloadData,
            /// <summary>
            /// 어셋번들 다운로드
            /// </summary>
            DownloadAssetBundle,
            /// <summary>
            /// 데이터 로드
            /// </summary>
            LoadData,
            /// <summary>
            /// 어셋번들 로드
            /// </summary>
            LoadAssetBundle,
            /// <summary>
            /// 유저 정보 조회
            /// </summary>
            ReqUserInfo,
            /// <summary>
            /// 캐릭터 목록 정보 조회
            /// </summary>
            ReqCharacterList,
        }

        protected override UIType uiType => UIType.Destroy;

        [SerializeField] UIProgressBar totalProgress;
        [SerializeField] UIProgressBar detailProgress;
        [SerializeField] UILabelHelper labelDesc;
        [SerializeField] UITweener tweenDescription;
        [SerializeField] UILabel labelBuildVersion;
        [SerializeField] UITextureHelper logoImage;
        [SerializeField] GameObject synopsisLoading;
        [SerializeField] GameObject baseView;
        [SerializeField] UILabelHelper labelDownloadSpeed;

        IntroPresenter presenter;

        private State state;
        private int cur;
        private int max;
        private string description;

        protected override void OnInit()
        {
            presenter = new IntroPresenter();

            presenter.OnUpdateState += OnUpdateState;
            presenter.OnUpdateTotalProgress += UpdateTotalProgress;
            presenter.OnUpdateDetailProgress += UpdateDetailProgress;
            presenter.OnUpdateDownloadCountProgress += UpdateCountProgress;
            presenter.OnAgreedTerm += NextCloseAgreeDialog;
            presenter.OnAllReady += OnAllReady;
            presenter.OnFinished += CloseUI;
            presenter.OnStartLoad += OnStartLoad;
            presenter.OnFinishedLoad += OnFinishedLoad;
            presenter.OnDownloadSpeed += UpdateDownloadSpeed;

            presenter.AddEvent();
        }

        protected override void OnClose()
        {
            presenter.RemoveEvent();

            presenter.OnUpdateState -= OnUpdateState;
            presenter.OnUpdateTotalProgress -= UpdateTotalProgress;
            presenter.OnUpdateDetailProgress -= UpdateDetailProgress;
            presenter.OnUpdateDownloadCountProgress -= UpdateCountProgress;
            presenter.OnAgreedTerm -= NextCloseAgreeDialog;
            presenter.OnAllReady -= OnAllReady;
            presenter.OnFinished -= CloseUI;
            presenter.OnStartLoad -= OnStartLoad;
            presenter.OnFinishedLoad -= OnFinishedLoad;
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            logoImage.Set(presenter.GetLogoTextureName(), isAsync: false);
            logoImage.MakePixelPerfect();
            RefreshDescription(); // 설명 변경
        }

        public void OnTitle()
        {
            labelBuildVersion.text = presenter.GetBuildVersion();
            labelDownloadSpeed.Text = string.Empty;

            ShowDescription(); // 설명 표시
            HideProgress(); // 진행도 숨김
            SetActiveSynopsis(false);

            //ShowAgreeDialog(); // 푸시알람동의 및 이용약관
            NextCloseAgreeDialog();

            Debug.Log($"Adjust IsEnabled={com.adjust.sdk.Adjust.isEnabled()}");
        }

        /// <summary>
        /// 푸시알람동의 및 이용약관
        /// </summary>
        private void ShowAgreeDialog()
        {
            // 약관동의 안했을 때..
            if (!ObscuredPrefs.GetBool(Config.CONFIRM_AGREE, false))
            {
                presenter.ShowAgreeDialog(); // 푸시알람동의 및 이용약관
            }
            else
            {
                NextCloseAgreeDialog();
            }
        }

        /// <summary>
        /// 푸시알람동의 후 호출
        /// </summary>
        private void NextCloseAgreeDialog()
        {
            if (Issue.TEST_ALWAYS_SHOW_LANGUAGE_POPUP)
            {
                UI.Show<UILanguagePopup>().OnFinish(NextCloseLanguagePopup); // 언어 선택 창
            }
            else
            {
                NextCloseLanguagePopup();
            }
        }

        /// <summary>
        /// 언어 선택 닫은 후 호출
        /// </summary>
        private void NextCloseLanguagePopup()
        {
            if (presenter.GetLastLoginType() == NCommon.LoginType.NONE)
            {
                ShowLoginUI(); // 로그인 선택 창
            }
            else
            {
                ConnectServer(isNeedLogin: true); // 서버 연결 (로그인 필요)
            }
        }

        /// <summary>
        /// 로그인 표시
        /// </summary>
        private void ShowLoginUI()
        {
            UI.Show<UILogin>().Set(NextCloseLoginUI);
        }

        /// <summary>
        /// 로그인 버튼 닫은 후 호출
        /// </summary>
        private void NextCloseLoginUI(bool isLogined)
        {
            if (isLogined)
            {
                ConnectServer(isNeedLogin: false); // 서버 연결 (이미 로그인하여 로그인 필요 음슴)
            }
            else
            {
                ShowLoginUI(); // 로그인 표시
            }
        }

        /// <summary>
        /// 서버 연결
        /// </summary>
        private void ConnectServer(bool isNeedLogin)
        {
            presenter.ConnectServer(isNeedLogin);
        }

        /// <summary>
        /// 스테이트 변경 시 호출
        /// </summary>
        private void OnUpdateState(State state)
        {
            this.state = state;
            description = GetDescription();
            RefreshDescription();
            ClearDownLoadSpeed();
        }

        /// <summary>
        /// 설명 업데이트
        /// </summary>
        private void RefreshDescription()
        {
            var sb = StringBuilderPool.Get().Append(description);

            if (max > 0)
                sb.Append(" (").Append(cur).Append("/").Append(max).Append(")");

            labelDesc.Text = sb.Release();
        }

        private string GetDescription()
        {
            switch (state)
            {
                case State.ReqAccountLogin:
                    return LocalizeKey._108.ToText(); // 계정 로그인 중입니다.

                case State.ReqContryCode:
                    return LocalizeKey._109.ToText(); // 서버 구성에 필요한 정보를 가져오는 중입니다.

                case State.ReqAuthServerConnect:
                    return LocalizeKey._110.ToText(); // 인증 요청을 시도합니다.

                case State.ReqAuthServerLogin:
                    return LocalizeKey._111.ToText(); // 인증 요청 중입니다.

                case State.ReqGameServerConnect:
                    return LocalizeKey._112.ToText(); // 게임 서버에 연결 중입니다.

                case State.ReqGameServerLogin:
                    return LocalizeKey._100.ToText(); // 게임에 접속 중입니다.

                case State.CheckDataVersion:
                    return LocalizeKey._113.ToText(); // 데이터 버전을 확인 중입니다.

                case State.DownloadAssetBundlePatchList:
                    return LocalizeKey._114.ToText(); // 리소스 버전을 확인 중입니다.

                case State.DownloadData:
                    return LocalizeKey._101.ToText(); // 데이터 파일 다운로드 중입니다.

                case State.DownloadAssetBundle:
                    return LocalizeKey._102.ToText(); // 리소스 파일을 다운로드 중입니다.

                case State.LoadLocalData:
                case State.LoadData:
                    return LocalizeKey._115.ToText(); // 데이터 파일 로딩 중입니다.

                case State.LoadAssetBundle:
                    return LocalizeKey._103.ToText(); // 리소스 파일 로딩 중입니다.

                case State.ReqUserInfo:
                    return LocalizeKey._116.ToText(); // 유저 정보를 조회합니다.

                case State.ReqCharacterList:
                    return LocalizeKey._117.ToText(); // 캐릭터 목록 정보를 조회합니다.
            }

            return string.Empty;
        }

        /// <summary>
        /// 진행도 업데이트 (전체)
        /// </summary>
        private void UpdateTotalProgress(float value)
        {
            totalProgress.value = value;
            //NGUITools.SetActive(poring.cachedGameObject, value < 1f);
        }

        /// <summary>
        /// 진행도 업데이트 (디테일)
        /// </summary>
        private void UpdateDetailProgress(float value)
        {
            detailProgress.value = value;
        }

        /// <summary>
        /// 진행도 개수 업데이트
        /// </summary>
        private void UpdateCountProgress(int cur, int max)
        {
            this.cur = cur;
            this.max = max;
            RefreshDescription();
        }

        /// <summary>
        /// 다운로드 속도 표시 갱신
        /// </summary>
        /// <param name="downloadSize"></param>
        private void UpdateDownloadSpeed(long downloadSize)
        {
            labelDownloadSpeed.Text = $"{downloadSize.FormatBytes()}/s";
        }

        /// <summary>
        /// 다운로드 속도 표시 제거
        /// </summary>
        private void ClearDownLoadSpeed()
        {
            labelDownloadSpeed.Text = string.Empty;
        }

        /// <summary>
        /// 게임 시작 준비 완료 시 호출
        /// </summary>
        private void OnAllReady()
        {
            HideDescription(); // 설명 숨김

            if (presenter.HasCharacter())
            {
                presenter.JoinGameMap(); // 보유 캐릭터가 존재할 경우 바로 시작
            }
        }

        /// <summary>
        /// UI 닫기
        /// </summary>
        private void CloseUI()
        {
            UI.Close<UIIntro>();
        }

        /// <summary>
        /// 다운로드 시작
        /// </summary>
        void OnStartLoad()
        {
            ShowProgress();
            SetActiveSynopsis(true);
            tweenDescription.Finish();
            //tweenDescription.ResetToBeginning();
            tweenDescription.Sample(0f, false);
        }

        /// <summary>
        /// 다운로드 종료
        /// </summary>
        void OnFinishedLoad()
        {
            tweenDescription.PlayForward();
            HideProgress();
        }

        /// <summary>
        /// 설명 표시
        /// </summary>
        private void ShowDescription()
        {
            labelDesc.SetActive(true);
            RefreshDescription();
        }

        /// <summary>
        /// 설명 숨김
        /// </summary>
        private void HideDescription()
        {
            labelDesc.SetActive(false);
        }

        /// <summary>
        /// 진행도 표시
        /// </summary>
        private void ShowProgress()
        {
            NGUITools.SetActive(totalProgress.gameObject, true);
            NGUITools.SetActive(detailProgress.gameObject, true);
        }

        /// <summary>
        /// 진행도 숨김
        /// </summary>
        private void HideProgress()
        {
            totalProgress.value = 0f;
            detailProgress.value = 0f;

            max = 0;
            NGUITools.SetActive(totalProgress.gameObject, false);
            NGUITools.SetActive(detailProgress.gameObject, false);
        }

        public void SetActiveSynopsis(bool isActive, bool onlyProgress = false, bool useBaseView = true)
        {
            if (onlyProgress)
            {
                baseView.SetActive(false);

                Timing.RunCoroutineSingleton(YieldLoadingTween(false).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
            else
            {
                if (!isActive) baseView.SetActive(true);
                if (!useBaseView) baseView.SetActive(false);

                Timing.RunCoroutineSingleton(YieldLoadingTween(isActive).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
            }
        }

        private IEnumerator<float> YieldLoadingTween(bool isActive, bool useHideTween = false)
        {
            var tweenTime = 0.5f;

            if (isActive)
            {
                TweenAlpha.Begin(synopsisLoading, 0f, 0f);
                yield return Timing.WaitForOneFrame;

                synopsisLoading.SetActive(true);

                TweenAlpha.Begin(synopsisLoading, tweenTime, 1f);
            }
            else
            {
                if (useHideTween)
                {
                    TweenAlpha.Begin(synopsisLoading, tweenTime, 0f);
                    yield return Timing.WaitForSeconds(tweenTime);
                }

                synopsisLoading.SetActive(false);
            }
        }

        public override bool Find()
        {
            base.Find();

            tweenDescription = labelDesc.GetComponent<UITweener>();
            return true;
        }
    }
}