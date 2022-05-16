using Ragnarok.View;
using Ragnarok.View.Skill;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="UISkill"/>
    /// </summary>
    public interface ISkillCanvas : ICanvas        
        , SkillModelView.IListener
        , SkillListView.IListener
        , SkillEquipView.IListener
        , SkillInfoView.IListener
        , SkillUnselectView.IListener
    {
        /// <summary>
        /// 새로고침
        /// </summary>
        void Refresh();

        /// <summary>
        /// 스킬 선택 해제 (스킬 훔쳐 배우기로 인하여 id가 변경될 수 있음)
        /// </summary>
        void UnSelectSkill();

        /// <summary>
        /// 제니 업데이트
        /// </summary>
        void UpdateZeny(long zeny);

        /// <summary>
        /// 캣코인 업데이트
        /// </summary>
        void UpdateCatCoin(long catCoin);

        /// <summary>
        /// 직업 레벨 업데이트
        /// </summary>
        void UpdateJobLevel(int jobLevel);

        /// <summary>
        /// 스킬 포인트 알림 업데이트
        /// </summary>
        void UpdateHasNewSkillPoint();
    }
}