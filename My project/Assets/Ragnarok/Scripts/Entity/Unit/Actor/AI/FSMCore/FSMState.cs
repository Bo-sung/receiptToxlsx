using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.AI
{
    public abstract class FSMState : IEqualityComparer<Transition>
    {
        public readonly StateID id;
        private readonly Dictionary<Transition, StateID> map;

        protected FSMState(StateID id)
        {
            this.id = id;
            map = new Dictionary<Transition, StateID>(this);
        }

        public FSMState AddTransition(Transition trans, StateID id)
        {
            if (trans == default)
            {
#if UNITY_EDITOR
                Debug.LogError("FSMState ERROR: Default Transition is not allowed for a real transition");
#endif
                return this;
            }

            if (id == default)
            {
#if UNITY_EDITOR
                Debug.LogError("FSMState ERROR: Default StateID is not allowed for a real ID");
#endif
                return this;
            }

            if (map.ContainsKey(trans))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"FSMState ERROR: State {this.id} already has transition {trans} Impossible to assign to another state");
#endif
                return this;
            }

            map.Add(trans, id);
            return this;
        }

        public void DeleteTransition(Transition trans)
        {
            if (trans == default)
            {
#if UNITY_EDITOR
                Debug.LogError("FSMState ERROR: Default StateID is not allowed");
#endif
                return;
            }

            if (map.ContainsKey(trans))
            {
                map.Remove(trans);
                return;
            }

#if UNITY_EDITOR
            Debug.LogWarning($"FSMState ERROR: Transition {trans} passed to {id} was not on the state's transition list");
#endif
        }

        public void Clear()
        {
            map.Clear();
        }

        public StateID GetOutputStateID(Transition trans)
        {
            if (map.ContainsKey(trans))
                return map[trans];

#if UNITY_EDITOR
            Debug.LogWarning($"FSMState ERROR: [{id}] Transition {trans} does not existed");
#endif
            return default;
        }

        public bool HasTransition(Transition trans)
        {
            return map.ContainsKey(trans);
        }

        public abstract void Begin();

        public abstract void End();

        public abstract void Update();

        bool IEqualityComparer<Transition>.Equals(Transition x, Transition y)
        {
            return x == y;
        }

        int IEqualityComparer<Transition>.GetHashCode(Transition obj)
        {
            return obj.GetHashCode();
        }
    }
}