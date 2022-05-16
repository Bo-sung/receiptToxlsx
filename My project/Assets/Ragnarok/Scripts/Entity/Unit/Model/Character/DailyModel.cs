using CodeStage.AntiCheat.ObscuredTypes;
using System;

namespace Ragnarok
{
    /// <summary>
    /// 일일 초기화 정보
    /// </summary>
    public class DailyModel : CharacterEntityModel
    {
        private ObscuredBool isNewDaily; // 오늘 첫접속(출석체크 연출에 쓰면됨)
        private LoopingRelativeRemainTime remainTimeToMidnight;

        public bool IsNewDaily => isNewDaily;

        public event Action OnResetDailyEvent;

        public float RemainTimeToMidnight { get { return remainTimeToMidnight; } }

        public DailyModel()
        {
            SetNewDaily(false);
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.AddEvent(OnReceiveCharDailyCalc);
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.RESULT_CHAR_DAILY_CALC.RemoveEvent(OnReceiveCharDailyCalc);
            }
        }

        private void OnReceiveCharDailyCalc(Response response)
        {
            if (response.isSuccess)
            {
                DailyInitPacket data = response.GetPacket<DailyInitPacket>("1");
                Notify(data);
                SetNewDaily(true);

                OnResetDailyEvent?.Invoke();
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public void SetNewDaily(bool value)
        {
            isNewDaily = value;
        }

        public void SetRemainTimeToMidnight(float sec)
        {
            remainTimeToMidnight = new LoopingRelativeRemainTime(sec, 86400f);
        }
    }
}