using UnityEngine;

namespace Ragnarok
{
    public class HudUnitName : SimpleHudUnitName, IAutoInspectorFinder
    {
        [SerializeField] Color playerColor = Color.yellow;
        [SerializeField] Color multiPlayerColor = Color.gray;
        [SerializeField] Color cupetColor = Color.yellow;
        [SerializeField] Color bossMonsterColor = Color.red;
        [SerializeField] Color monsterColor = Color.white;
        [SerializeField] Color NpcColor = new Color(0xDA, 0xA5, 0x20);

        public System.Action OnUnitInfo;
        public System.Action OnBattleClick;

        public void Initialize(string name, int level, UnitEntityType type)
        {
            string text = LocalizeKey._81000.ToText() // Lv.{LEVEL} {NAME}
                .Replace(ReplaceKey.NAME, name)
                .Replace(ReplaceKey.LEVEL, level);

            Initialize(text, type);
        }

        public void Initialize(int nameId, int level, UnitEntityType type)
        {
            this.nameId = nameId;
            this.level = level;

            SetColor(GetColor(type));
            OnLocalize();
        }

        public void Initialize(string text, UnitEntityType type)
        {
            SetColor(GetColor(type));

            SetName(text);
        }
        
        public void Initialize(int nameId, UnitEntityType type)
        {
            this.nameId = nameId;
            level = -1;

            SetColor(GetColor(type));
            OnLocalize();
        }

        private Color GetColor(UnitEntityType type)
        {
            switch (type)
            {
                case UnitEntityType.Player:
                    return playerColor;

                case UnitEntityType.MultiPlayer:
                case UnitEntityType.MultiCupet:
                case UnitEntityType.GhostPlayer:
                case UnitEntityType.GhostCupet:
                case UnitEntityType.Guardian:
                    return multiPlayerColor;

                case UnitEntityType.PlayerCupet:
                    return cupetColor;

                case UnitEntityType.MvpMonster:
                case UnitEntityType.BossMonster:
                case UnitEntityType.WorldBoss:
                case UnitEntityType.TurretBoss:
                    return bossMonsterColor;

                case UnitEntityType.NormalMonster:
                case UnitEntityType.MazeMonster:
                case UnitEntityType.GhostMonster:
                case UnitEntityType.GuardianDestroyer:
                case UnitEntityType.MonsterBot:
                case UnitEntityType.Turret:
                    return monsterColor;

                case UnitEntityType.NPC:
                    return NpcColor;
            }

            return Color.black;
        }

        public virtual void ShowBattleHUD()
        {

        }

        public virtual void HideBattleHUD()
        {

        }
    }
}