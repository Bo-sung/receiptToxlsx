namespace Ragnarok
{
    public class GhostPlayerActor : CharacterActor
    {
        public delegate void StartReviveTimerEvent(UnitActor actor, float remainTime);
        public event StartReviveTimerEvent OnStartReviveTimer; // 부활 타이머 시작 이벤트

        /// <summary>
        /// 부활까지 남은 시간
        /// </summary>
        public RelativeRemainTime RemainReviveTime { get; private set; }

        public override void AddEvent()
        {
            base.AddEvent();

            Entity.OnReadyToBattle += ResetReviveTime;
            Entity.OnRebirth += ResetReviveTime;
        }

        public override void RemoveEvent()
        {
            base.RemoveEvent();

            Entity.OnReadyToBattle -= ResetReviveTime;
            Entity.OnRebirth -= ResetReviveTime;
        }

        public void NotifyRemainReviveTime(float time)
        {
            RemainReviveTime = time;
            OnStartReviveTimer?.Invoke(this, time);
        }

        public override bool IsLookTarget(UnitActor other, TargetType targetType)
        {
            // 무시 타겟 타입의 경우 타겟이 될 수 없다.
            if (other.Entity.IsIgnoreTarget)
                return false;

            if (AI.IsLookEnemyContainsFollower)
            {
                // 추격자가 있는 적군 포함
            }
            else
            {
                // 추격자가 있는 적군의 경우
                if (other.Entity.IsEnemy && other.AI.Follower)
                {
                    // 일반 몬스터는 타겟이 될 수 없다.
                    if (other.Entity.type == UnitEntityType.NormalMonster)
                        return false;
                }
            }

            return base.IsLookTarget(other, targetType);
        }

        /// <summary>
        /// 부활타이머 초기화
        /// </summary>
        public void ResetReviveTime()
        {
            NotifyRemainReviveTime(0f);
        }

        protected override void ForceStartCooldown(SkillInfo skillInfo)
        {
            base.ForceStartCooldown(skillInfo);

            if (BattleManager.isUseSkillPoint)
            {
                int remainMp = Entity.CurMp - skillInfo.MpCost; // remainMp 계산
                Entity.SetCurrentMp(remainMp);
            }
        }
    }
}