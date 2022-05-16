using UnityEngine;

namespace Ragnarok
{
    public class HudUnitInfo : HUDObject, IAutoInspectorFinder
    {
        [SerializeField] UIButtonHelper btnInfo;

        public event System.Action OnSelect;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnInfo.OnClick, OnClickedBtnInfo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnInfo.OnClick, OnClickedBtnInfo);
        }

        void OnClickedBtnInfo()
        {
            OnSelect?.Invoke();
        }
    }
}