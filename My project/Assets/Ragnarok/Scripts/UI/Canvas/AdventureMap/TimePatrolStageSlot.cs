using UnityEngine;

namespace Ragnarok.View
{
    public class TimePatrolStageSlot : UIView
    {
        public enum State
        {
            Clear,
            LastEnter,
            Lock,
        }

        [SerializeField] UILabelHelper labelClear;
        [SerializeField] UILabelHelper labelLastEnter;
        [SerializeField] GameObject goClear , goLastEnter, goLock;

        protected override void OnLocalize()
        {
        }

        public void SetName(string text)
        {
            labelClear.Text = text;
            labelLastEnter.Text = text;
        }

        public void SetState(State state)
        {
            goClear.SetActive(state == State.Clear);
            goLastEnter.SetActive(state == State.LastEnter);
            goLock.SetActive(state == State.Lock);
        }
    }
}