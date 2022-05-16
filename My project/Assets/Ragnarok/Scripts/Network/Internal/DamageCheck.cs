#if UNITY_EDITOR

using UnityEngine;
using System.Collections.Generic;

namespace Ragnarok
{
    public static class DamageCheck
    {
        public static event System.Action<List<DebugDamageTuple>> OnUpdate;

        public readonly static List<DebugDamageTuple> list = new List<DebugDamageTuple>();

        public static void Add(DebugDamageTuple client)
        {
            if (DebugUtils.IsLogDamage)
            {
                Debug.Log($"<color=white>[서버와 대미지 다름]</color>");
            }

            list.Add(client);
            Invoke();
        }

        public static void Clear()
        {
            list.Clear();
            Invoke();
        }

        private static void Invoke()
        {
            OnUpdate?.Invoke(list);
        }
    }
}
#endif