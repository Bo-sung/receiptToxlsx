namespace Ragnarok
{
    public class MazeMonsterEffectPlayer : MonsterEffectPlayer
    {
        public PoolObject cube;
        public MazeGold mazeZeny;
        public MazeGold mazeExp;

        protected override bool IsHideFullHP()
        {
            return true;
        }

        protected override bool IsHideHp()
        {
            return cube != null; // 큐브 상태의 경우에는 Hp를 보여주지 않는다.
        }

        public override void ShowName()
        {
            if (entity.Monster.MonsterLevel == 0)
                return;

            if (hudName == null)
                hudName = hudPool.SpawMazeMonsterName(CachedTransform);

            hudName.OnUnitInfo = OnClickedBtnInfo;
            hudName.OnBattleClick = OnBattleClick;
            hudName.Initialize(entity.Monster.MonsterID, entity.Monster.MonsterLevel, entity.type);
            hudName.Show();
        }

        public override void OnRelease()
        {
            if (hudName)
            {
                hudName.OnUnitInfo = null;
                hudName.OnBattleClick = null;
            }
            if (cube)
            {
                cube.Release();
                cube = null;
            }
            if (mazeZeny)
            {
                mazeZeny.Release();
                mazeZeny = null;
            }
            if (mazeExp)
            {
                mazeExp.Release();
                mazeExp = null;
            }

            base.OnRelease();
        }

        public override void ShowBattleHUD()
        {
            if (hudName && entity.battleType == MazeBattleType.IntoBattle)
                hudName.ShowBattleHUD();
        }

        public override void HideBattleHUD()
        {
            if (hudName)
                hudName.HideBattleHUD();
        }

        public override void HideName()
        {
            if (hudName)
                hudName.Hide();
        }

        private void OnClickedBtnInfo()
        {
            OnInfo?.Invoke(actor);
        }

        private void OnClickedBtnBattle()
        {
            OnBattleClick?.Invoke();
        }

        public override void SpawnMonsterCube(MazeBattleType battleType, bool isBoss)
        {
            if (cube == null)
                cube = battlePool.SpawnMonsterCube(actor.CachedTransform.position, battleType, isBoss);
        }

        public void SpawnMazeZeny()
        {
            if (mazeZeny is null)
            {
                mazeZeny = battlePool.SpawnMazeZeny(actor.CachedTransform.position);
                mazeZeny.OnDeSpawn += OnDespawnMazeZeny;
            }
        }

        public void SpawnMazeYellowZeny()
        {
            if (mazeZeny is null)
            {
                mazeZeny = battlePool.SpawnMazeZenyYellow(actor.CachedTransform.position);
                mazeZeny.OnDeSpawn += OnDespawnMazeZeny;
            }
        }

        public void SpawnMazeExp()
        {
            if (mazeExp is null)
            {
                mazeExp = battlePool.SpawnMazeExp(actor.CachedTransform.position);
                mazeExp.OnDeSpawn += OnDespawnMazeExp;
            }
        }

        void OnDespawnMazeZeny(IMazeDropItem item)
        {
            item.OnDeSpawn -= OnDespawnMazeZeny;
            mazeZeny = null;
        }

        void OnDespawnMazeExp(IMazeDropItem item)
        {
            item.OnDeSpawn -= OnDespawnMazeExp;
            mazeExp = null;
        }
    }
}
