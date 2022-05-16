using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleStageSkillList : UIBattleSkillList
    {
        public enum Mode
        {
            Type_1,
            Type_2,
        }

        [SerializeField] UIWidget container;
        [SerializeField] GameObject mode1, mode2;

        public void SetMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Type_1:
                    container.SetAnchor(mode1, 0, 0, 0, 0);
                    break;
                case Mode.Type_2:
                    container.SetAnchor(mode2, 0, 0, 0, 0);
                    break;
            }
        }
    }
}