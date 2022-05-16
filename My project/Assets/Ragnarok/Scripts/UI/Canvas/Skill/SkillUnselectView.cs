using UnityEngine;

namespace Ragnarok.View.Skill
{
    public class SkillUnselectView : UIView<SkillUnselectView.IListener>, IInspectorFinder
    {
        public interface IListener
        {
            void OnUnselect();
        }

        [SerializeField] UIPanel panel;
        [SerializeField] UIEventTrigger unselect;

        public UIPanel Panel => panel;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(unselect.onClick, OnClickedUnselect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(unselect.onClick, OnClickedUnselect);
        }

        void OnClickedUnselect()
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnUnselect();
            }
        }

        protected override void OnLocalize()
        {
        }

        bool IInspectorFinder.Find()
        {
            panel = GetComponent<UIPanel>();
            return true;
        }
    }
}