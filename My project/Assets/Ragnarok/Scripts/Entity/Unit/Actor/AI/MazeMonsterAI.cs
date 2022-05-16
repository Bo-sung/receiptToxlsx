using Ragnarok.AI;
using UnityEngine;

namespace Ragnarok
{
    public class MazeMonsterAI : UnitAI
    {
        Vector3[] patrolPositions;
        float distance;
        bool isPatrol;

        public override void SetPatrolPosition(Vector3[] positions)
        {
            patrolPositions = positions;
        }

        public override Vector3 GetRandomPatrolPosition()
        {
            return patrolPositions[Random.Range(0, patrolPositions.Length)];
        }        

        public override bool IsPatrolPosition()
        {
            return patrolPositions != null && patrolPositions.Length > 0;
        }

        public override void SetFindDistance(float distance)
        {
            this.distance = distance;
        }

        public override float GetFindDistance()
        {
            return distance;
        }

        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투 대기 */
                .AddState(new ReadyState(actor, StateID.Ready)
                    .AddTransition(Transition.StartBattle, StateID.Patrol)) // 전투 시작 => 기본 상태

                /* 전투 종료 */
                .AddState(new EndState(actor, StateID.End)
                    .AddTransition(Transition.StartBattle, StateID.Patrol)) // 전투 시작 => 기본 상태

                ///* 순찰 */
                .AddState(new MazePatrolState(actor, StateID.Patrol)
                    .AddTransition(Transition.MoveAround, StateID.Chase) // [보너스미로 고스트] 
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                // 배회 or 플레이어 쫓기
                .AddState(new MazeGhostChaseState(actor, StateID.Chase)
                    .AddTransition(Transition.Dead, StateID.Die) // 죽음 => 죽음
                    .AddTransition(Transition.EndBattle, StateID.End)) // 전투 종료 => 종료 상태

                /* 죽음 */
                .AddState(new DieState(actor, StateID.Die)
                    .AddTransition(Transition.EndBattle, StateID.End)); // 전투 종료 => 종료 상태
        }
    } 
}
