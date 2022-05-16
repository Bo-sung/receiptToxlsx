using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.AI
{
    public class FSMSystem
    {
        private readonly List<FSMState> fsmStates;

        public event System.Action<StateID> OnChangedState;

        public FSMState Current { get; private set; }

        public FSMSystem()
        {
            fsmStates = new List<FSMState>();
        }

        public FSMSystem AddState(FSMState state)
        {
            if (state == null)
                throw new System.ArgumentException("FSM ERROR: Null reference is not allowed");

            if (fsmStates.Count == 0)
            {
                fsmStates.Add(state);
                Current = state;
                return this;
            }

#if UNITY_EDITOR
            FSMState fsmState;
            for (int i = 0; i < fsmStates.Count; i++)
            {
                fsmState = fsmStates[i];

                if (fsmState.id == state.id)
                {
                    Debug.LogWarning($"FSM ERROR: Impossible to add state {state.id} because state has already been added");
                    return this;
                }
            }
#endif

            fsmStates.Add(state);

            // 추가한 State가 현재와 같은경우 경우에 한함
            if (Current != null && Current.id == state.id)
                Current = state;

            return this;
        }

        public void SetState(StateID stateID)
        {
            FSMState fsmState;
            for (int i = 0; i < fsmStates.Count; ++i)
            {
                fsmState = fsmStates[i];
                if (fsmState.id == stateID)
                {
                    Current = fsmState;
                    return;
                }
            }

            throw new System.ArgumentException($"FSM ERROR: Tried to set unavailable state : {stateID}");
        }

        public void DeleteState(StateID id)
        {
            if (id == default)
            {
                Debug.LogError("FSM ERROR: Default StateID is not allowed for a real state");
                return;
            }

            foreach (FSMState state in fsmStates)
            {
                if (state.id == id)
                {
                    state.Clear();
                    fsmStates.Remove(state);
                    return;
                }
            }

            Debug.LogError($"FSM ERROR: Impossible to delete state {id}. It was not on the list of states");
        }

        public void Clear()
        {
            fsmStates.Clear();
        }

        public bool HasTransition(Transition trans)
        {
            StateID currentID = Current.GetOutputStateID(trans);

            for (int i = 0; i < fsmStates.Count; i++)
            {
                if (fsmStates[i].id == currentID)
                    return true;
            }

            return false;
        }

        public bool PerformTransition(Transition trans)
        {
            StateID currentID = Current.GetOutputStateID(trans);

            FSMState fsmState;
            for (int i = 0; i < fsmStates.Count; i++)
            {
                fsmState = fsmStates[i];

                if (fsmState.id == currentID)
                {
                    Current.End();
                    Current = fsmState;
                    Current.Begin();

                    OnChangedState?.Invoke(currentID);
                    return true;
                }
            }

            return false;
        }
    }
}