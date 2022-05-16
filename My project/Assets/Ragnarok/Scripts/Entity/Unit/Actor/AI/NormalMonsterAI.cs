using UnityEngine;

namespace Ragnarok
{
    public class NormalMonsterAI : MonsterAI
    {
        public override void StartAI()
        {
            base.StartAI();

            actor.EffectPlayer.PlayPanelBuffEffect(); // 몬스터 소환 이펙트
        }
    }
}