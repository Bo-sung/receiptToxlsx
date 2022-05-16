using UnityEngine;

namespace Ragnarok
{
    public class BossViewTimeline : ViewTimeline
    {
        private const int TOP_VIEW = 0;
        private const int FRONT_VIEW = 1;
        private const int ZOOM_IN = 2;

        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float showBossAlarm = 5f;
        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float showBossTime = 3f;
        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float showBossAniTime = 5f;
        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float hidePlayerTime = 4.6f;

        public event System.Action OnShowBossAlarm;
        public event System.Action OnShowBoss;
        public event System.Action OnShowBossAni;
        public event System.Action OnHidePlayer;

        protected override void OnEnable()
        {
            base.OnEnable();

            Invoke(showBossAlarm, OnShowBossAlarm);
            Invoke(showBossTime, OnShowBoss);
            Invoke(showBossAniTime, OnShowBossAni);
            Invoke(hidePlayerTime, OnHidePlayer);
        }

        public void SetTarget(Transform player, Transform boss)
        {
            ChildCameras[TOP_VIEW].Follow = player;
            ChildCameras[FRONT_VIEW].Follow = boss;
            ChildCameras[ZOOM_IN].Follow = boss;
        }
    }
}