using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(ParticleSystem))]
    public class MazeUnitCircle : PoolObject
    {
        [SerializeField] ParticleSystem myParticleSystem;

        [SerializeField] Color simpleColor = Color.red;
        [SerializeField] Color clickerColor = Color.magenta;
        [SerializeField] Color clearActionColor = Color.blue;
        [SerializeField] Color actionColor = Color.green;
        [SerializeField] Color intoBattleColor = Color.cyan;

        public void Initialize(MazeBattleType battleType)
        {
            ParticleSystem.MainModule settings = myParticleSystem.main;
            settings.startColor = new ParticleSystem.MinMaxGradient(GetColor(battleType));
        }

        private Color GetColor(MazeBattleType battleType)
        {
            switch (battleType)
            {
                case MazeBattleType.Simple:
                    return simpleColor;

                case MazeBattleType.Clicker:
                    return clickerColor;

                case MazeBattleType.ClearAction:
                    return clearActionColor;

                case MazeBattleType.Action:
                    return actionColor;

                case MazeBattleType.IntoBattle:
                case MazeBattleType.AutoIntoBattle:
                    return intoBattleColor;
            }

            return Color.white;
        }
    }
}