using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class BossMonsterEffectPlayer : MonsterEffectPlayer
    {
        private const string TAG = nameof(BossMonsterEffectPlayer);

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Issue.DROP_BOSS_COIN)
                Timing.KillCoroutines(TAG);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (Issue.DROP_BOSS_COIN)
                Timing.KillCoroutines(TAG);
        }

        protected override void OnDie(UnitEntity unit, UnitEntity attacker)
        {
            base.OnDie(unit, attacker);

            if (Issue.DROP_BOSS_COIN)
            {
                Timing.RunCoroutine(SpawnBossBonusZeny(CachedTransform.position), GetInstanceID());
            }
        }

        /// <summary>
        ///	보스 동전 뿌리기 
        /// </summary>
        private IEnumerator<float> SpawnBossBonusZeny(Vector3 position)
        {
            Vector3 spawnPosition = position;

            //	연출1 : 스프링쿨러처럼 첫동전 부터 끝동전까지 9시부터 시계방향으로 원을 그리듯이 쏜다.

            //	각도 간격 = 2PI / 스폰개수
            int spawnCount = BasisType.BOSS_BONUS_ZENY_COUNT.GetInt();
            float RadGap = Mathf.PI * 2f / spawnCount;
            float curRad = 0f;
            for (int i = 0; i < spawnCount; ++i)
            {
                // 높이		 :	15
                // 퍼지는 힘 :	1.25 + random -0.25 ~ 0.25 -> 랜덤 없는게 더 나아서 랜덤은 제거.
                // 시작 방향 :	9시
                // 회전 방향 :	시계방향
                DroppedClickItem coin = battlePool.SpawnGold_ClickItem(spawnPosition);
                float spreadDistance = 1.25f;
                float xDelta = Mathf.Sin(curRad) * -1f;
                float yHeight = 15f;
                float zDelta = Mathf.Cos(curRad + Mathf.PI);
                coin.SetJumpingPower(new Vector3(xDelta * spreadDistance, yHeight, zDelta * spreadDistance));
                curRad += RadGap;

                yield return Timing.WaitForSeconds(0.15f); // 쉬어준다
            }
        }


        protected override void PlayExtraOnFallStart()
        {
            if (actor.Entity.IsEnemy)
            {
                PlayFallShadowResize(Constants.Battle.BOSS_SHADOW_NAME, Constants.Battle.FALL_TIME);
            }
        }

        protected override void PlayExtraOnFallStop(Transform pos)
        {
            if (actor.Entity.IsEnemy)
            {
                // 안개 먼지 효과
                ShowSkillEffect(Constants.Battle.FALL_DUSTWAVE_NAME, pos.position, duration: 0);

                // 카메라 흔들기
                GenerateImpulse(BattlePoolManager.ImpulseType.BossAppearance);
            }
        }


        protected override bool IsHideFullHP()
        {
            return true;
        }
    }
}