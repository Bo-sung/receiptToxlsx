using MEC;
using Ragnarok.View;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIRpsEvent : UICanvas, IInspectorFinder
    {
        protected override UIType uiType => UIType.Back | UIType.Hide;

        [SerializeField] UIButtonHelper btnExit;
        [SerializeField] UIButtonHelper btnReset;
        [SerializeField] UIButtonHelper btnRanking;

        [SerializeField] UIButtonHelper btnRock;
        [SerializeField] UIButtonHelper btnScissors;
        [SerializeField] UIButtonHelper btnPaper;
        [SerializeField] UITexture iconRock;
        [SerializeField] UITexture iconScissors;
        [SerializeField] UITexture iconPaper;

        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UILabelHelper labelIntroduce;

        [SerializeField] UILabelHelper labelRound;
        [SerializeField] UITextureHelper iconPoring;
        [SerializeField] UILabelValue gameDesc;
        [SerializeField] TypewriterEffect writeEffect;

        [SerializeField] GameObject balloon;
        [SerializeField] Transform bossRps;
        [SerializeField] GameObject[] bossRpsAry;

        [SerializeField] GameObject gamePad;
        [SerializeField] UITextureHelper missile;

        [SerializeField] UIButtonWithIcon btnCost;
        [SerializeField] UITextureHelper iconItem;
        [SerializeField] UILabelHelper labelNeedItemCount;

        [SerializeField] UIGrid rewardGrid;
        [SerializeField] UIGrid selectGrid;
        [SerializeField] UIGrid completeGrid;

        [SerializeField] UIRewardHelper[] rewardAry;
        [SerializeField] UISprite[] selectIconAry;
        [SerializeField] UISprite[] completeIconAry;

        [SerializeField] RpsResetPopupView rpsResetPopupView;
        [SerializeField] RpsResultView rpsResultView;

        [SerializeField] Transform posCenter;
        [SerializeField] Transform posLeft;
        [SerializeField] Transform posRight;
        [SerializeField] TweenRotation tweenUserRps;
        [SerializeField] TweenRotation tweenBossRps;
        [SerializeField] GameObject fx;
        [SerializeField] UILabelHelper labelNotice;

        RpsEventPresenter presenter;
        RewardData[] rewardDatas;

        CoroutineHandle poringRpsHandle;

        EventRpsData rpsData;
        RpsType selectRpsType;

        RpsRoundType roundType;
        RpsResultType resultType;

        RemainTime remainTime;

        Color32 drawColor = new Color32(0xB7, 0xEC, 0xFF, 0xFF);
        Color32 defeatColor = new Color32(0xF7, 0x7C, 0x50, 0xFF);
        string rockImageName = "rpsevent_rock";
        string scissorsImageName = "rpsevent_scissors";
        string paperImageName = "rpsevent_paper";

        float waitTime = 0.25f;
        float tweenTime = 0.5f;
        bool canPlay;
        bool isWait;
        bool isInit;

        bool isSetTime = false;

        protected override void OnInit()
        {
            presenter = new RpsEventPresenter();

            rpsResetPopupView.OnReset += OnConfirmReset;

            presenter.OnUpdateRpsInfo += OnUpdateRpsInfo;
            presenter.AddEvent();

            EventDelegate.Add(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Add(btnReset.OnClick, OnClickedBtnReset);
            EventDelegate.Add(btnRanking.OnClick, OnClickedBtnRanking);
            EventDelegate.Add(btnRock.OnClick, OnClickedBtnRock);
            EventDelegate.Add(btnScissors.OnClick, OnClickedBtnScissors);
            EventDelegate.Add(btnPaper.OnClick, OnClickedBtnPaper);
            EventDelegate.Add(writeEffect.onFinished, StartRps);
            EventDelegate.Add(btnCost.OnClick, presenter.ShowCostItemData);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(btnExit.OnClick, OnClickedBtnExit);
            EventDelegate.Remove(btnReset.OnClick, OnClickedBtnReset);
            EventDelegate.Remove(btnRanking.OnClick, OnClickedBtnRanking);
            EventDelegate.Remove(btnRock.OnClick, OnClickedBtnRock);
            EventDelegate.Remove(btnScissors.OnClick, OnClickedBtnScissors);
            EventDelegate.Remove(btnPaper.OnClick, OnClickedBtnPaper);
            EventDelegate.Remove(writeEffect.onFinished, StartRps);
            EventDelegate.Remove(btnCost.OnClick, presenter.ShowCostItemData);

            presenter.RemoveEvent();
            presenter.OnUpdateRpsInfo -= OnUpdateRpsInfo;

            rpsResetPopupView.OnReset -= OnConfirmReset;
        }

        protected override void OnShow(IUIData data = null)
        {
            rpsResetPopupView.Hide();
            rpsResultView.Hide();

            // 보상리스트 정보 셋팅
            rewardDatas = presenter.GetRewardDatas();
            for (int i = 0; i < rewardAry.Length; i++)
            {
                rewardAry[i].SetData(rewardDatas[i]);
            }

            var costImage = presenter.GetCostItemImage();
            iconItem.SetItem(costImage, isAsync: false);

            // 라운드 정보 갱신
            SetRpsType();
            SetRound();
            SetRewardInfo();
        }

        public void SetRemainTime(RemainTime remainTime)
        {
            this.remainTime = remainTime;

            // 남은시간 체크
            isSetTime = true;
            CheckRemainTime();
        }

        void CheckRemainTime()
        {
            // 남은시간 셋팅이 된 후
            if (!isSetTime)
            {
                return;
            }

            // 이벤트 중
            if (remainTime.ToRemainTime() > 0)
            {
                return;
            }

            // 이벤트 종료
            CloseUI();
        }

        void SetRpsType()
        {
            fx.SetActive(false);

            roundType = presenter.GetRoundType();
            resultType = presenter.GetResultType();
        }

        void SetRewardInfo()
        {
            // 보상아이템 선택, 완료 표시
            for (int i = 0; i < selectIconAry.Length; i++)
            {
                selectIconAry[i].alpha = i == (int)roundType ? 1 : 0;
                completeIconAry[i].alpha = i < (int)roundType ? 1 : 0;
            }
        }

        void SetRound()
        {
            // 매 라운드 정보를 갱신할 때, 남은시간을 체크 함.
            CheckRemainTime();

            rpsData = presenter.GetData(roundType);

            var myCoinCount = presenter.GetCostItemCount();
            var needCoinCount = 0;
            switch (resultType)
            {
                case RpsResultType.Draw:
                    needCoinCount = 0;
                    break;

                case RpsResultType.Defeat:
                    needCoinCount = rpsData.retry_cost;
                    break;

                default:
                    needCoinCount = rpsData.try_cost;
                    break;
            }

            // 가위바위보 할 수 있는 조건인지 체크
            canPlay = myCoinCount >= needCoinCount;

            labelRound.Text = LocalizeKey._11202.ToText().Replace(ReplaceKey.VALUE, (int)roundType + 1);

            iconPoring.SetEvent(rpsData.provoke_image_name, isAsync: false);
            var poringName = rpsData.monster_name.ToText();
            gameDesc.Title = $"[{poringName}]";
            gameDesc.Value = rpsData.provoke_text_id.ToText();

            // 대사 진행동안은 게임패드 비활성화
            if (canPlay)
            {
                writeEffect.ResetToBeginning();
                ActiveGamePad(false);
            }
            else
            {
                gameDesc.Value = ""; // 조건이 안되면 메세지 안보여 줌.
                ActiveGamePad(true);
            }

            btnCost.Text = myCoinCount.ToString();
            labelNeedItemCount.Text = LocalizeKey._11203.ToText() // Required Coin : {VALUE}
                .Replace(ReplaceKey.VALUE, needCoinCount);
            labelNeedItemCount.Color = GetColor(resultType);

            btnReset.SetActive(roundType > RpsRoundType.Round1 && resultType != RpsResultType.Draw);
            btnCost.SetActiveIcon(myCoinCount >= presenter.eventCoinMaxCount);
        }

        Color GetColor(RpsResultType resultType)
        {
            switch (resultType)
            {
                case RpsResultType.Draw:
                    return drawColor;

                case RpsResultType.Defeat:
                    return defeatColor;

                //case RpsResultType.Ready:
                default:
                    return Color.white;
            }
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._11200; // 가위, 바위, 보 한 판 승부!
            labelIntroduce.LocalKey = LocalizeKey._11201; // 포링 군단의 역습

            btnReset.LocalKey = LocalizeKey._11207; // 초기화
            btnRanking.LocalKey = LocalizeKey._32000; // 랭킹

            labelNotice.Text = LocalizeKey._11210.ToText() // 이벤트 주화는 최대 {COUNT}개까지 획득할 수 있습니다.
                .Replace(ReplaceKey.COUNT, presenter.eventCoinMaxCount);
        }

        private void OnClickedBtnExit()
        {
            CloseUI();
        }

        private void CloseUI()
        {
            UI.Close<UIRpsEvent>();
        }

        private void OnClickedBtnReset()
        {
            rpsResetPopupView.Show(rpsData.reward_data, presenter.GetCostItemImage(), presenter.GetCostItemCount());
        }

        void OnConfirmReset()
        {
            isInit = true;

            Timing.KillCoroutines(poringRpsHandle); // 연출 중단
            presenter.RequestEventRpsInit();
        }

        private void OnClickedBtnRanking()
        {
            UI.Show<UIEventRank>()
                .SetRankType(RankType.RockPaperScissors);
        }

        private void OnClickedBtnRock()
        {
            if (canPlay)
            {
                selectRpsType = RpsType.Rock;
                RequestPlayRps();
            }
        }

        private void OnClickedBtnScissors()
        {
            if (canPlay)
            {
                selectRpsType = RpsType.Scissors;
                RequestPlayRps();
            }
        }

        private void OnClickedBtnPaper()
        {
            if (canPlay)
            {
                selectRpsType = RpsType.Paper;
                RequestPlayRps();
            }
        }

        private void RequestPlayRps()
        {
            switch (selectRpsType)
            {
                case RpsType.Rock:
                    missile.SetEvent(rockImageName, isAsync: false);
                    missile.cachedTransform.position = btnRock.transform.position;
                    break;
                case RpsType.Scissors:
                    missile.SetEvent(scissorsImageName, isAsync: false);
                    missile.cachedTransform.position = btnScissors.transform.position;
                    break;
                case RpsType.Paper:
                    missile.SetEvent(paperImageName, isAsync: false);
                    missile.cachedTransform.position = btnPaper.transform.position;
                    break;
            }
            missile.cachedTransform.rotation = Quaternion.Euler(Vector3.zero);

            // 게임패드 꺼주고 미사일만 표시
            gamePad.SetActive(false);
            missile.SetActive(true);

            presenter.RequestEventRpsStart();
        }

        private void OnUpdateRpsInfo()
        {
            // 결과 정보 갱신
            SetRpsType();

            if (isInit)
            {
                SetRound();
                SetRewardInfo();
            }
            else
            {
                isWait = false; // 연출시작
            }
        }

        private void StartRps()
        {
            ActiveGamePad(true);
        }

        private void ActiveGamePad(bool isActive)
        {
            if (isActive) // 게임 가능한 상태
            {
                btnRock.IsEnabled = canPlay;
                btnPaper.IsEnabled = canPlay;
                btnScissors.IsEnabled = canPlay;

                iconRock.color = canPlay ? Color.white : Color.gray;
                iconPaper.color = canPlay ? Color.white : Color.gray;
                iconScissors.color = canPlay ? Color.white : Color.gray;

                gamePad.SetActive(true);

                // 포링 말풍선은 게임 재화가 충분할 때
                if (canPlay)
                {
                    bossRps.localPosition = Vector3.zero;
                    bossRps.transform.localRotation = Quaternion.Euler(Vector3.zero);

                    balloon.SetActive(true);
                    poringRpsHandle = Timing.RunCoroutineSingleton(YieldStartRps().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
                }
                else
                {
                    balloon.SetActive(false);
                }
            }
            else // 대사치는 동안 게임패드 비활성화
            {
                gamePad.SetActive(false);
                balloon.SetActive(false);
            }
        }

        IEnumerator<float> YieldStartRps()
        {
            isWait = true; // 연출 대기
            isInit = false;

            int idx = 0;
            while (true)
            {
                idx++;
                if (idx >= 3) idx = 0;

                for (int i = 0; i < bossRpsAry.Length; i++)
                {
                    bossRpsAry[i].SetActive(i == idx);
                }

                // 연출 시작가능.
                if (!isWait && idx == GetBossRpsType())
                {
                    break;
                }

                yield return Timing.WaitForSeconds(waitTime);
            }

            // 보스 선택 후 잠깐 대기
            //TweenPosition.Begin(missile.cachedGameObject, waitTime, gamePad.transform.position, true); // 가운데로 이동하지 않음.
            yield return Timing.WaitForSeconds(tweenTime);

            // 연출이 진행되는 부분
            TweenPosition.Begin(missile.cachedGameObject, tweenTime, posCenter.position, true);
            TweenPosition.Begin(bossRps.gameObject, tweenTime, posCenter.position, true);
            yield return Timing.WaitForSeconds(tweenTime);

            // 이펙트
            fx.SetActive(true);

            // 이펙트 후 연출
            switch (resultType)
            {
                case RpsResultType.Ready:
                    RpsEffect(bossRps.gameObject, true, tweenTime);
                    tweenBossRps.enabled = true;
                    break;
                case RpsResultType.Draw:
                    RpsEffect(bossRps.gameObject, true, tweenTime);
                    RpsEffect(missile.cachedGameObject, false, tweenTime);
                    tweenBossRps.enabled = true;
                    tweenUserRps.enabled = true;
                    break;
                case RpsResultType.Defeat:
                    RpsEffect(missile.cachedGameObject, false, tweenTime);
                    tweenUserRps.enabled = true;
                    break;
                default:
                    break;
            }
            yield return Timing.WaitForSeconds(tweenTime);

            // Shake
            switch (resultType)
            {
                case RpsResultType.Ready:
                    ShakeObject(iconPoring.cachedGameObject);
                    break;
                case RpsResultType.Defeat:
                    ShakeObject(UICamera.currentCamera.gameObject);
                    break;
                //case RpsResultType.Draw:
                default:
                    break;
            }

            // 트윈 비활성화
            tweenBossRps.enabled = false;
            tweenUserRps.enabled = false;

            yield return Timing.WaitForSeconds(tweenTime);

            missile.SetActive(false);


            // 창이 열리고, 닫힐 때까지 대기
            rpsResultView.Show(rpsData, resultType);

            while (rpsResultView.IsShow)
            {
                yield return Timing.WaitForOneFrame;
            }

            // 1. 연출이 끝나고, 2. 팝업창이 닫히면 라운드 갱신
            SetRound();
            SetRewardInfo();
        }

        int GetBossRpsType()
        {
            int bossType;
            switch (resultType)
            {
                case RpsResultType.Ready:
                    bossType = (int)selectRpsType + 1;
                    bossType %= 3;
                    break;

                case RpsResultType.Defeat:
                    bossType = (int)selectRpsType - 1;
                    if (bossType < 0)
                    {
                        bossType = 2;
                    }
                    break;

                //case RpsResultType.Draw:
                default:
                    bossType = (int)selectRpsType;
                    break;
            }

            return bossType;
        }

        void RpsEffect(GameObject go, bool isBoss, float tweenTime)
        {
            if (isBoss)
            {
                // 우측 방향으로 날라감
                TweenPosition.Begin(go, tweenTime, posRight.position, true);
            }
            else
            {
                // 좌측 방향으로 날라감
                TweenPosition.Begin(go, tweenTime, posLeft.position, true);
            }
        }

        void ShakeObject(GameObject go)
        {
            var tweenShake = go.GetComponent<TweenShake>();
            if (tweenShake != null)
            {
                Destroy(tweenShake);
                tweenShake = null;
            }

            tweenShake = go.AddComponent<TweenShake>();
            tweenShake.duration = tweenTime;
            tweenShake.power = 20;
        }

        protected override void OnBack()
        {
            if (rpsResetPopupView.IsShow)
            {
                rpsResetPopupView.Hide();
                return;
            }

            if (rpsResultView.IsShow)
                return;

            base.OnBack();
        }

        bool IInspectorFinder.Find()
        {
            if (rewardGrid != null)
            {
                rewardAry = rewardGrid.GetComponentsInChildren<UIRewardHelper>();
            }

            if (selectGrid != null)
            {
                selectIconAry = selectGrid.GetComponentsInChildren<UISprite>();
            }

            if (completeGrid != null)
            {
                completeIconAry = completeGrid.GetComponentsInChildren<UISprite>();
            }

            if (bossRps != null)
            {
                var tempAry = bossRps.GetComponentsInChildren<UITexture>();
                bossRpsAry = new GameObject[tempAry.Length];
                for (int i = 0; i < tempAry.Length; i++)
                {
                    bossRpsAry[i] = tempAry[i].gameObject;
                }

                tweenBossRps = bossRps.GetComponent<TweenRotation>();
            }

            if (missile != null)
            {
                tweenUserRps = missile.GetComponent<TweenRotation>();
            }

            if (gameDesc != null)
            {
                writeEffect = gameDesc.GetComponentInChildren<TypewriterEffect>();
            }

            return true;
        }
    }
}