using UnityEngine;

namespace Ragnarok.AI
{
    public class MazeFollowBindingTargetState : FollowBindingTargetState
    {
        public MazeFollowBindingTargetState(UnitActor actor, StateID id) : base(actor, id)
        {
        }

        protected override Vector3 GetSquareOffset(int pos)
        {
            return Vector3.zero;
        }
    }
}