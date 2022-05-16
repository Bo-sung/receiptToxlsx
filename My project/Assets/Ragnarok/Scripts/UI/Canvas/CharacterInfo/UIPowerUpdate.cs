using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIPowerUpdate : UICanvas, IInspectorFinder
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        const float PLAY_TIME = 0.8f;
        const float UPDATE_INTERVAL = 0.025f;
        const float REMAIN_TIME = 2.0f;
        const string INCREASE_ICON = "Ui_Common_Icon_PoringDaily_1";
        const string DECREASE_ICON = "Ui_Common_Icon_SkillLock";

        public class Input : IUIData
        {
            public BattleStatusData status;
            public bool showAmountOfChange; // true: 변화량만 보여줌. false: 최종 전투력의 변화를 보여줌

            /// <summary>
            /// ctor 
            /// </summary>
            /// <param name="showAmountOfChange">true: 변화량만 보여줌. false: 최종 전투력의 변화를 보여줌.</param>
            public Input(BattleStatusData battleStatusData, bool showAmountOfChange = false)
            {
                this.status = battleStatusData;
                this.showAmountOfChange = showAmountOfChange;
            }
        }

        [SerializeField] UILabelHelper labelPower;
        [SerializeField] UIPlayTween[] playTween;
        [SerializeField] TweenScale tweenScale_Power; // 전투력 라벨의 TweenScale. (전투력 하강할 경우 따로 꺼주기 위해)
        [SerializeField] StatusChangeLabelGroupView statusChangeGroup;
        [SerializeField] UIGridHelper gridBG;
        [SerializeField] UITexture background;
        [SerializeField] UISprite icon;

        Input input;

        static bool isIgnoreOnce = false;
        public static bool IsIgnoreOnce { get => isIgnoreOnce; set => isIgnoreOnce = value; } /// TODO: 버프 적용 전의 능력치를 받도록 수정해서, 불안정한 코드 제거.

        protected override void OnInit()
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnLocalize()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            this.input = data as Input;

            if (IsInvalid())
                return;

            Timing.KillCoroutines(gameObject);
            Timing.RunCoroutine(YieldPowerUpdate(), gameObject);
        }

        IEnumerator<float> YieldPowerUpdate()
        {
            statusChangeGroup.Initialize(input.status);

            int showCount = input.status.GetValidStatusCount();
            gridBG.SetValue(showCount);

            var bounds = NGUIMath.CalculateRelativeWidgetBounds(gridBG.transform);

            int size = (int)Mathf.Abs(bounds.min.y - bounds.max.y);
            background.SetDimensions(600, size + 39);

            // 애니메이션에 쓰일 데이터들 세팅
            BattleStatusData status = input.status;
            float curAP = 0;

            float[] curStat = new float[Constants.Size.ABILITY_STATUS_COUNT];
            for (int i = 0; i < Constants.Size.ABILITY_STATUS_COUNT; ++i)
            {
                curStat[i] = 0f;
            }

            if (status.AP < 0) // 애니메이션 예외 연출 : 전투력 하강인 경우 TweenScale 재생X
            {
                tweenScale_Power.tweenGroup = 0; // 0 : 재생 안 함
                icon.spriteName = DECREASE_ICON;
            }
            else
            {
                tweenScale_Power.tweenGroup = 1; // 1 : 재생
                icon.spriteName = INCREASE_ICON;
            }

            int maxFrame = Mathf.RoundToInt(PLAY_TIME / UPDATE_INTERVAL);
            for (int frm = 0; frm < maxFrame; ++frm) // 정해진 프레임 수 만큼만 반복.
            {
                // AP 업데이트
                float increasePerFrame_AP = GetIncreasePerFrame(status.AP, maxFrame);
                curAP += increasePerFrame_AP;
                if (status.AP >= 0) // 지정된 수를 넘지 못하도록. (양수인 경우)
                {
                    if (curAP > status.AP)
                        curAP = status.AP;
                }
                else // 음수인 경우
                {
                    if (curAP < status.AP)
                        curAP = status.AP;
                }

                // 8종 능력치 업데이트
                for (int i = 0; i < Constants.Size.ABILITY_STATUS_COUNT; ++i)
                {
                    float increasePerFrame = GetIncreasePerFrame(status.GetStatusByIndex(i), maxFrame);
                    curStat[i] += increasePerFrame;
                    if (status.GetStatusByIndex(i) >= 0) // 지정된 수를 넘지 못하도록. (양수인 경우)
                    {
                        if (curStat[i] > status.GetStatusByIndex(i))
                            curStat[i] = status.GetStatusByIndex(i);
                    }
                    else // 음수인 경우
                    {
                        if (curStat[i] < status.GetStatusByIndex(i))
                            curStat[i] = status.GetStatusByIndex(i);
                    }
                }

                // 라벨에 적용
                int printAP = (int)curAP + (input.showAmountOfChange ? 0 : status.beforeAP);
                labelPower.Text = GetPowerString(printAP, curAP / status.AP, (status.AP >= 0));
                for (int i = 0; i < Constants.Size.ABILITY_STATUS_COUNT; ++i)
                {
                    statusChangeGroup.View[i].SetData(status.GetStatusNameByIndex(i), (int)curStat[i], isPercent: IsPercentStat(i.ToEnum<BattleStatusData.Stat>()));
                }

                // 애니메이션 재생
                foreach (var tween in playTween)
                {
                    tween.Play();
                }

                // 애니메이션 조기 종료 (목표 AP에 일찍 도달했을 경우)
                if ((int)curAP == status.AP)
                    break;

                yield return Timing.WaitForSeconds(UPDATE_INTERVAL);
            }

            // 최종 값 출력
            labelPower.Text = GetPowerString(status.afterAP, 1f, (status.AP >= 0));
            for (int i = 0; i < statusChangeGroup.View.Length; ++i)
            {
                statusChangeGroup.View[i].SetData(status.GetStatusNameByIndex(i), status.GetStatusByIndex(i), isPercent: IsPercentStat(i.ToEnum<BattleStatusData.Stat>()));
            }

            // 일정 시간 뒤에 종료
            yield return Timing.WaitForSeconds(REMAIN_TIME);
            Hide();
        }

        /// <summary>
        /// 프레임당 상승량 반환 + 최소 상승량 보정
        /// </summary>
        float GetIncreasePerFrame(int value, int maxFrame)
        {
            float increasePerFrame = value / (float)maxFrame;
            // 최저 상승량 보정.
            if (value >= 0) // 양수인 경우 최소 1로 보정
            {
                if (increasePerFrame < 1f)
                    increasePerFrame = 1f;
            }
            else // 음수인 경우 최소 -1로 보정
            {
                if (increasePerFrame > -1f)
                    increasePerFrame = -1f;
            }
            return increasePerFrame;
        }

        /// <summary>
        /// 백분율로 표기해야하는 스탯인지.
        /// </summary>
        bool IsPercentStat(BattleStatusData.Stat stat)
        {
            switch (stat)
            {
                case BattleStatusData.Stat.FLEE:
                case BattleStatusData.Stat.CRI:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 라벨의 내용과 색을 설정
        /// </summary>
        /// <param name="t">0.0 ~ 1.0 진행도</param>
        private string GetPowerString(int value, float t, bool isIncrease)
        {
            string colorText = GetColorBBCode(isIncrease, t);
            string nameText = LocalizeKey._48000.ToText(); // 전투력
            string valueText = value.ToString();

            string retText = StringBuilderPool.Get()
                .Append(colorText)
                .Append(nameText)
                .Append(" ")
                .Append(valueText).Release();

            return retText;
        }

        private string GetColorBBCode(bool isIncrease, float t)
        {
            Color32 increaseColor = new Color32(0xFF, 0xB7, 0xC2, 255); // 플러스
            Color32 decreaseColor = new Color32(0xEC, 0xEC, 0xEC, 255); // 마이너스

            Color32 destColor = (isIncrease ? increaseColor : decreaseColor);

            int colorValueR = (int)Mathf.Lerp(255, destColor.r, t);
            int colorValueG = (int)Mathf.Lerp(255, destColor.g, t);
            int colorValueB = (int)Mathf.Lerp(255, destColor.b, t);

            string colorRHexString = colorValueR.ToString("X2");
            string colorGHexString = colorValueG.ToString("X2");
            string colorBHexString = colorValueB.ToString("X2");

            return $"[{colorRHexString}{colorGHexString}{colorBHexString}]";
        }

        private bool IsInvalid()
        {
            return (input == null);
        }
    }
}