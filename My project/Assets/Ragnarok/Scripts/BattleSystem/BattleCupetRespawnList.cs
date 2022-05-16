using MEC;
using System.Collections.Generic;

namespace Ragnarok
{
    public class BattleCupetRespawnList : BetterList<CupetEntity>
    {
        private readonly string TAG = nameof(BattleCupetRespawnList);

        /// <summary>
        /// 큐펫 부활 대기 목록에 큐펫 추가.
        /// </summary>
        /// <param name="duration">부활 대기 시간 (seconds)</param>
        public void AddCupetToRespawnList(CupetEntity cupetEntity, int duration)
        {
            Add(cupetEntity);
            Timing.RunCoroutine(CoroutineRespawnCupet(cupetEntity, duration), TAG);
        }

        /// <summary>
        /// 부활 대기 목록 초기화, 모든 큐펫 부활 타이머 취소
        /// </summary>
        public void Reset()
        {
            Clear();
            Timing.KillCoroutines(TAG);
        }

        /// <summary>
        /// 지정 시간 뒤에 큐펫을 부활시키는 코루틴
        /// </summary>
        /// <param name="duration">second</param>
        private IEnumerator<float> CoroutineRespawnCupet(CupetEntity cupetEntity, int duration)
        {
            duration /= 1000;
            yield return Timing.WaitForSeconds(duration);

            Remove(cupetEntity);

            RespawnCupet(cupetEntity);

            // 워프
            UnitActor cupetActor = cupetEntity.GetActor();
            cupetActor.Movement.Warp(Entity.player.LastPosition);
        }

        /// <summary>
        /// 큐펫 부활
        /// </summary>
        /// <param name="cupetEntity"></param>
        private void RespawnCupet(CupetEntity cupetEntity)
        {
            UnitActor cupetActor = cupetEntity.SpawnActor(); // 유닛 소환

            // UnitList에는 남아있지만 Actor가 없을 수도 있다. 
            IBattleManagerImpl impl = BattleManager.Instance;
            impl.Add(cupetEntity, isEnemy: false);

            UnitActor characterActor = Entity.player.GetActor();
            cupetActor.AI.SetBindingActor(characterActor); // 바인딩 타겟 설정 (반드시 바인딩을 먼저 할 것!)
            cupetActor.AI.SetHomePosition(Entity.player.LastPosition, isWarp: true); // 위치 세팅

            cupetActor.Show();
            cupetActor.AI.ReadyToBattle();
        }
    }
}