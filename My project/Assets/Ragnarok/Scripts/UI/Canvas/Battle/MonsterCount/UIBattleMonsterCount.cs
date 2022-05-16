using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleMonsterCount : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] MonsterCountView monsterCountView;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            SetMonsterType(MonsterType.Boss); // 기본이 보스
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void UpdateMonsterCount(int remainCount, int maxCount)
        {
            monsterCountView.Show(remainCount, maxCount);
        }

        public void SetMonsterType(MonsterType monsterType)
        {
            monsterCountView.SetMonsterType(monsterType);
        }
    }
}