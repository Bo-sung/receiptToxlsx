using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class Entity
    {
        // 싱글 Entity 목록
        public static readonly PlayerEntity player = Factory.CreatePlayer();

        public static class Factory
        {
            private static readonly UnitEntityPoolManager<ClonePlayerEntity> clonePlayerPoolManager = new UnitEntityPoolManager<ClonePlayerEntity>();

            [System.Obsolete("Use Entity.player instead")]
            public static PlayerEntity CreatePlayer()
            {
                PlayerEntity entity = new PlayerEntity();

                entity.Initialize();

                return entity;
            }

            public static ClonePlayerEntity CreateClonePlayer()
            {
                return clonePlayerPoolManager.Spawn();
            }
        }

#if UNITY_EDITOR
        public readonly List<EntityLog> logStack = new List<EntityLog>();

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void SetHeader(string header)
        {
            int tick = Time.frameCount;
            logStack.Add(new EntityLog(tick, header));
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void AddHeader(string header)
        {
            EntityLog log = GetLog();

            if (log == null)
            {
                SetHeader(header);
            }
            else
            {
                log.header = string.Concat(log.header, header);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void AddLog(string text)
        {
            EntityLog log = GetLog();

            if (log == null)
                return;

            AddText(log, text);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void AddLog(params string[] texts)
        {
            EntityLog log = GetLog();

            if (log == null)
                return;

            foreach (var item in texts)
            {
                AddText(log, item);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void FinishLog()
        {
            EntityLog log = GetLog();

            if (log == null)
                return;

            log.Finish();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void ClearLog()
        {
            logStack.Clear();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void AddText(EntityLog log, string text)
        {
            if (text.StartsWith("["))
                text = text.Insert(0, "\t");

            log.Add(text);
        }

        private EntityLog GetLog()
        {
            int tick = Time.frameCount;

            for (int i = 0; i < logStack.Count; i++)
            {
                if (logStack[i].tick == tick && !logStack[i].isFinished)
                    return logStack[i];
            }

            return null;
        }
#endif
    }
}