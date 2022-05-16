using System.Collections.Generic;

namespace Ragnarok
{
    public class GhostBattlePlayers : BetterList<GhostPlayerEntity>
    {
        private readonly Stack<GhostPlayerEntity> pooledStack;

        public int Size => size;

        public GhostBattlePlayers()
        {
            pooledStack = new Stack<GhostPlayerEntity>();
        }

        public new void Clear()
        {
            base.Clear();

            pooledStack.Clear();
        }

        public GhostPlayerEntity AddGhostPlayer(IMultiPlayerInput input)
        {
            GhostPlayerEntity entity = pooledStack.Count > 0 ? pooledStack.Pop() : CharacterEntity.Factory.CreateGhostPlayer();

            entity.Character.Initialize(input);
            entity.Status.Initialize(input);
            entity.Status.Initialize(input.IsExceptEquippedItems, input.BattleOptions, input.GuildBattleOptions);
            entity.Inventory.Initialize(input.ItemStatusValue, input.WeaponItemId, input.ArmorItemId, input.WeaponChangedElement, input.WeaponElementLevel, input.ArmorChangedElement, input.ArmorElementLevel, input.GetEquippedItems);
            entity.Skill.Initialize(input.IsExceptEquippedItems, input.Skills);
            entity.Skill.Initialize(input.Slots);
            entity.Guild.Initialize(input);
            entity.Trade.Initialize(input);

            entity.SetDamageUnitKey(input.GetDamageUnitKey());

            Add(entity);

            return entity;
        }

        public void Recycle()
        {
            while (size > 0)
            {
                pooledStack.Push(Pop());
            }
        }

        public void Recycle(GhostPlayerEntity entity)
        {
            entity.ResetData();
            pooledStack.Push(entity);
            Remove(entity);
        }

        public GhostPlayerEntity Find(int cid)
        {
            for (int i = 0; i < size; i++)
            {
                if (buffer[i].Character.Cid == cid)
                {
                    return buffer[i];
                }
            }
            return null;
        }
    }
}