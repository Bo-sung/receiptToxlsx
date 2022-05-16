#if UNITY_EDITOR

namespace Ragnarok.AI
{
    public class TestState : UnitFsmState
    {
        public TestState(UnitActor actor, StateID id) : base(actor, id)
        {
        }
    }
}

#endif