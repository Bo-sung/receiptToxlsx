using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class GhostCupetEntityPool : System.IDisposable
    {
        private readonly BetterList<GhostCupetEntity> cupetEntityList; // 사용중
        private readonly Stack<GhostCupetEntity> pooledStatck; // 사용대기중

        public GhostCupetEntityPool()
        {
            cupetEntityList = new BetterList<GhostCupetEntity>();
            pooledStatck = new Stack<GhostCupetEntity>();
        }

        public void Dispose()
        {
            for (int i = cupetEntityList.size - 1; i >= 0; i--)
            {
                cupetEntityList[i].ResetData();
                cupetEntityList[i].Dispose();
            }

            cupetEntityList.Release();
            pooledStatck.Clear();
        }

        public int Size()
        {
            return cupetEntityList.size;
        }

        public GhostCupetEntity Create(IMultiCupetInput input)
        {
            GhostCupetEntity entity = pooledStatck.Count > 0 ? pooledStatck.Pop() : CupetEntity.Factory.CreateGhostCupet();

            entity.Initialize(input); // 초기화
            cupetEntityList.Add(entity); // List에 관리

            return entity;
        }

        public void Recycle(GhostCupetEntity entity)
        {
            if (entity == null)
                return;

            if (cupetEntityList.Remove(entity))
            {
                entity.Initialize(GhostCupetEntity.DEFAULT); // 초기화
                entity.ResetData();

                pooledStatck.Push(entity); // Stack에 관리 (Pool)
            }
        }
    }
}