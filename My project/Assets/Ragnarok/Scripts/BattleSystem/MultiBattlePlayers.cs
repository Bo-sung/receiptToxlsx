using System.Collections.Generic;

namespace Ragnarok
{
    public class MultiBattlePlayers : BetterList<CharacterEntity>
    {
        private readonly Stack<CharacterEntity> pooledStack;

        public int Size => size;

        public MultiBattlePlayers()
        {
            pooledStack = new Stack<CharacterEntity>();
        }

        public new void Clear()
        {
            base.Clear();

            pooledStack.Clear();
        }

        public CharacterEntity Add(IMultiPlayerInput input, UnitEntity.UnitState unitState = UnitEntity.UnitState.Stage)
        {
            CharacterEntity entity = pooledStack.Count > 0 ? pooledStack.Pop() : CharacterEntity.Factory.CreateMultiBattlePlayer(unitState);

            entity.Character.Initialize(input);
            entity.Status.Initialize(input);
            entity.Status.Initialize(input.IsExceptEquippedItems, input.BattleOptions, input.GuildBattleOptions);
            entity.Inventory.Initialize(input.ItemStatusValue, input.WeaponItemId, input.ArmorItemId, input.WeaponChangedElement, input.WeaponElementLevel, input.ArmorChangedElement, input.ArmorElementLevel, input.GetEquippedItems);
            entity.Skill.Initialize(input.IsExceptEquippedItems, input.Skills);
            entity.Skill.Initialize(input.Slots);

            if (unitState == UnitEntity.UnitState.Stage)
                entity.CupetList.Initialize(input.Cupets);

            entity.Guild.Initialize(input);
            entity.Trade.Initialize(input);

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

        public void Recycle(CharacterEntity entity)
        {
            entity.ResetData(); // 데이터 초기화
            pooledStack.Push(entity);
            //Remove(entity);
        }

        public CharacterEntity Find(int cid)
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