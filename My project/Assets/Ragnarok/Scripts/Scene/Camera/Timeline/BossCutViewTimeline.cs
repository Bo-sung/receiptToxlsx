using UnityEngine;

namespace Ragnarok
{
    public class BossCutViewTimeline : ViewTimeline
    {
        private const int ZOOM_OUT = 0;
        private const int ZOOM_IN = 1;

        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float showBossAlarm = 0f;
        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float showBossTime = 0f;
        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float showBossAniTime = 1.5f;
        [SerializeField, Tooltip(tooltip: TOOL_TIP)] float hidePlayerTime = 0f;

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

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public void SetTarget(Transform player, Transform boss)
        {
            ChildCameras[ZOOM_OUT].Follow = boss;
            ChildCameras[ZOOM_OUT].LookAt = boss;

            ChildCameras[ZOOM_IN].Follow = boss;
            ChildCameras[ZOOM_IN].LookAt = boss;
        }
    }
}