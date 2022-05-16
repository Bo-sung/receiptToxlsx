using UnityEngine;

namespace Ragnarok.View
{
    public sealed class GuildBattleBuffMaterialView : LevelUpMaterialSelectView
    {
        [SerializeField] UISkillInfo skillInfo;

        protected override void Awake()
        {
            base.Awake();

            startDisplayLevel = 0; // 버프 레벨은 0부터 보여짐
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();

            labelNoData.LocalKey = LocalizeKey._33902; // 보유한 강화 재료가 없습니다.
            labelNotice.LocalKey = LocalizeKey._33903; // 강화에 필요한 재료를 선택하세요.
        }

        public void SetSkill(UISkillInfo.IInfo info, int curPoint, int maxPoint, int maxLine)
        {
            skillInfo.Show(info);
            SetPoint(curPoint, maxPoint, maxLine);
        }
    }
}