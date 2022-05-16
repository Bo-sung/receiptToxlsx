using UnityEngine;
using Ragnarok.View.BattleLeague;

namespace Ragnarok.View
{
    public class BattleDuelView : UIView
    {
        [SerializeField] UIBattleDuelStatus player, enemy;

        protected override void OnLocalize()
        {
        }

        public void ResetData()
        {
            player.ResetData();
            enemy.ResetData();
        }

        public void SetPlayer(string iconRankName, CharacterEntity entity)
        {
            player.SetData(iconRankName, entity);
        }

        public void SetPlayerAgents(CharacterEntity[] agents)
        {
            player.SetAgents(agents);
        }

        public void SetEnemy(string iconRankName, CharacterEntity entity)
        {
            enemy.SetData(iconRankName, entity);
        }

        public void SetEnemyAgents(CharacterEntity[] agents)
        {
            enemy.SetAgents(agents);
        }
    }
}