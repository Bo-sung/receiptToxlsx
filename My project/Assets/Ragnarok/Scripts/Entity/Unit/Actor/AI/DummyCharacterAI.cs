using Ragnarok.AI;

namespace Ragnarok
{
    public class DummyCharacterAI : UnitAI
    {
        protected override FSMSystem MakeFSM()
        {
            return new FSMSystem()

                /* 전투 대기 */
                .AddState(new ReadyState(actor, StateID.Ready));     
        }
    }
}