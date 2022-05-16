using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 스킬 사용 시 말풍선
    /// </summary>
    public class SkillBalloon : ChatBalloon, IAutoInspectorFinder
    {
        public enum Mode
        {
            DEFAULT = 0,    // 기본 말풍선
            BOSS = 1,       // 보스 전용 말풍선 (보스 체력바 위)
        }

        [SerializeField] UIWidget widgetAnchor;
        [SerializeField] GameObject mode_Default;
        [SerializeField] GameObject mode_Boss;

        public virtual void Set(Mode mode, string iconName)
        {
            switch (mode)
            {
                case Mode.DEFAULT:
                    widgetAnchor.SetAnchor(mode_Default, 0, 0, 0, 0);
                    break;

                case Mode.BOSS:
                    widgetAnchor.SetAnchor(mode_Boss, 0, 0, 0, 0);
                    break;
            }
        }
    }
}