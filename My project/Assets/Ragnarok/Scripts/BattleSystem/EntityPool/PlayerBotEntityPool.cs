using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 1. Entity Pooling
    /// 2. 생성되기 전 바뀐 행동을 담아서 처리
    /// </summary>
    public sealed class PlayerBotEntityPool : BetterList<PlayerBotEntity>
    {
        private readonly Stack<PlayerBotEntity> pooledStack;
        private readonly BetterList<CreateTask> createQueue;
        private readonly Dictionary<int, int> createQueueIndexDic; // key: cid, value: index

        public PlayerBotEntityPool()
        {
            pooledStack = new Stack<PlayerBotEntity>();
            createQueue = new BetterList<CreateTask>();
            createQueueIndexDic = new Dictionary<int, int>(IntEqualityComparer.Default);
        }

        /// <summary>
        /// 데이터 제거
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            pooledStack.Clear();
            createQueue.Clear();
            createQueueIndexDic.Clear();
        }

        /// <summary>
        /// Entity 생성
        /// </summary>
        public PlayerBotEntity Create(IMultiPlayerInput input)
        {
            PlayerBotEntity entity = pooledStack.Count > 0 ? pooledStack.Pop() : CharacterEntity.Factory.CreatePlayerBot();

            entity.Initialize(input); // 초기화
            Add(entity); // List에 관리

            return entity;
        }

        /// <summary>
        /// 모든 Entity 재활용
        /// </summary>
        public void Recycle()
        {
            while (size > 0)
            {
                Recycle(Pop());
            }
        }

        /// <summary>
        /// Entity 재활용
        /// </summary>
        public void Recycle(PlayerBotEntity entity)
        {
            entity.Initialize(PlayerBotEntity.DEFAULT); // 초기화
            entity.ResetData();
            pooledStack.Push(entity); // Stack에 관리 (Pool)
        }

        /// <summary>
        /// Entity 반환 - 고유 cid 이용
        /// </summary>
        public PlayerBotEntity Find(int cid)
        {
            for (int i = 0; i < size; i++)
            {
                if (buffer[i].Character.Cid == cid)
                    return buffer[i];
            }

            return null;
        }

        /// <summary>
        /// 생성 큐 존재
        /// </summary>
        public bool HasQueue()
        {
            return createQueue.size > 0;
        }

        /// <summary>
        /// 생성 큐 꺼내기
        /// </summary>
        public IMultiPlayerInput Dequeue()
        {
            int removeIndex = createQueue.size - 1;
            IMultiPlayerInput result = createQueue[removeIndex];
            createQueueIndexDic.Remove(result.Cid);
            createQueue.RemoveAt(removeIndex);
            return result;
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void EnqueueRange(IMultiPlayerInput[] input)
        {
            if (input == null)
                return;

            for (int i = 0; i < input.Length; i++)
            {
                Enqueue(input[i]);
            }
        }

        /// <summary>
        /// 생성 큐에 추가
        /// </summary>
        public void Enqueue(IMultiPlayerInput input)
        {
            int cid = input.Cid;
            if (RemoveQueue(cid))
            {
#if UNITY_EDITOR
                Debug.LogError($"이미 생성 스택이 존재: {nameof(cid)} = {cid}");
#endif
            }

            createQueueIndexDic.Add(cid, createQueue.size); // 추가
            createQueue.Add(new CreateTask(input));
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 나감 처리)
        /// </summary>
        public bool UpdateQueueLeave(int cid)
        {
            return RemoveQueue(cid);
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 움직임 처리)
        /// </summary>
        public bool UpdateQueueMove(int cid, Vector3 pos)
        {
            if (createQueueIndexDic.ContainsKey(cid))
            {
                int index = createQueueIndexDic[cid];
                createQueue[index].SetPosition(pos);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 상태변환 처리)
        /// </summary>
        public bool UpdateQueueState(int cid, byte state)
        {
            if (createQueueIndexDic.ContainsKey(cid))
            {
                int index = createQueueIndexDic[cid];
                createQueue[index].SetState(state);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐에서 유저 이름 반환
        /// </summary>
        public string GetUserNameFromQueue(int cid, string defaultValue)
        {
            if (createQueueIndexDic.ContainsKey(cid))
            {
                int index = createQueueIndexDic[cid];
                IMultiPlayerInput task = createQueue[index];
                return task.Name;
            }

#if UNITY_EDITOR
            Debug.LogError($"존재하지 않는 플레이어의 유저 이름 반환 시도: {nameof(cid)} = {cid}");
#endif
            return defaultValue;
        }

        /// <summary>
        /// 생성 큐 업데이트 (중도 상태변환 처리)
        /// </summary>
        public bool UpdateQueueSellingState(int cid, PrivateStoreSellingState state, string privateStoreComment)
        {
            if (createQueueIndexDic.ContainsKey(cid))
            {
                int index = createQueueIndexDic[cid];
                createQueue[index].SetSellingState(state, privateStoreComment);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐 업데이트(중도 무기, 코스튬 변경)
        /// </summary>
        public bool UpdateQueueChar(int cid, int weaponId, string costumeids)
        {
            if (createQueueIndexDic.ContainsKey(cid))
            {
                int index = createQueueIndexDic[cid];
                createQueue[index].SetWeapon(weaponId);
                createQueue[index].SetCostume(costumeids);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐 업데이트 (Hp)
        /// </summary>
        public bool UpdateQueueHp(int cid, int remainHp)
        {
            if (createQueueIndexDic.ContainsKey(cid))
            {
                int index = createQueueIndexDic[cid];
                createQueue[index].SetCurHp(remainHp);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 생성 큐에서 제거
        /// </summary>
        private bool RemoveQueue(int cid)
        {
            if (createQueueIndexDic.ContainsKey(cid))
            {
                int index = createQueueIndexDic[cid];
                createQueue.RemoveAt(index); // 생성 stack 제거
                createQueueIndexDic.Remove(cid); // index 제거
                return true;
            }

            return false;
        }

        private class CreateTask : IMultiPlayerInput
        {
            private bool isExceptEquippedItems;
            private int cid;
            private string name;
            private byte job;
            private byte gender;
            private int jobLevel;
            private int profileId;
            private int weaponItemId;
            private string guildName;
            private float posX;
            private float posY;
            private float posZ;
            private byte state;
            private string privateStoreComment;
            private PrivateStoreSellingState sellingState;
            private int uid;
            private bool hasMaxHp;
            private int maxHp;
            private bool hasCurHp;
            private int curHp;
            private byte tempIndex;
            private ItemInfo.IEquippedItemValue[] equippedItems; // 장착 코스튬 정보 목록

            bool IMultiPlayerInput.IsExceptEquippedItems => isExceptEquippedItems;

            int CharacterModel.IInputValue.Cid => cid;
            string CharacterModel.IInputValue.Name => name;
            byte CharacterModel.IInputValue.Job => job;
            byte CharacterModel.IInputValue.Gender => gender;
            int CharacterModel.IInputValue.Level => 0;
            int CharacterModel.IInputValue.LevelExp => 0;
            int CharacterModel.IInputValue.JobLevel => jobLevel;
            long CharacterModel.IInputValue.JobLevelExp => 0;
            int CharacterModel.IInputValue.RebirthCount => 0;
            int CharacterModel.IInputValue.RebirthAccrueCount => 0;
            int CharacterModel.IInputValue.NameChangeCount => 0;
            string CharacterModel.IInputValue.CidHex => string.Empty;
            int CharacterModel.IInputValue.ProfileId => profileId;

            int StatusModel.IInputValue.Str => 0;
            int StatusModel.IInputValue.Agi => 0;
            int StatusModel.IInputValue.Vit => 0;
            int StatusModel.IInputValue.Int => 0;
            int StatusModel.IInputValue.Dex => 0;
            int StatusModel.IInputValue.Luk => 0;
            int StatusModel.IInputValue.StatPoint => 0;

            int IMultiPlayerInput.WeaponItemId => weaponItemId;
            BattleItemInfo.IValue IMultiPlayerInput.ItemStatusValue => null;
            int IMultiPlayerInput.ArmorItemId => 0;
            ElementType IMultiPlayerInput.WeaponChangedElement => ElementType.None;
            int IMultiPlayerInput.WeaponElementLevel => 0;
            ElementType IMultiPlayerInput.ArmorChangedElement => ElementType.None;
            int IMultiPlayerInput.ArmorElementLevel => 0;

            SkillModel.ISkillValue[] IMultiPlayerInput.Skills => null;
            SkillModel.ISlotValue[] IMultiPlayerInput.Slots => null;

            CupetListModel.IInputValue[] IMultiPlayerInput.Cupets => null;

            IBattleOption[] IMultiPlayerInput.BattleOptions => null;
            IBattleOption[] IMultiPlayerInput.GuildBattleOptions => null;

            int GuildModel.IInputValue.GuildId => 0;
            string GuildModel.IInputValue.GuildName => guildName;
            int GuildModel.IInputValue.GuildEmblem => 0;
            byte GuildModel.IInputValue.GuildPosition => 0;
            int GuildModel.IInputValue.GuildCoin => 0;
            int GuildModel.IInputValue.GuildQuestRewardCount => 0;
            long GuildModel.IInputValue.GuildSkillBuyDateTime => 0L;
            byte GuildModel.IInputValue.GuildSkillBuyCount => 0;
            long GuildModel.IInputValue.GuildRejoinTime => 0L;

            float IMultiPlayerInput.PosX => posX;
            float IMultiPlayerInput.PosY => posY;
            float IMultiPlayerInput.PosZ => posZ;
            int IMultiPlayerInput.UID => uid;
            byte IMultiPlayerInput.State => state;
            string TradeModel.IInputValue.PrivateStoreComment => privateStoreComment;
            PrivateStoreSellingState TradeModel.IInputValue.PrivateStoreSellingState => sellingState;
            bool IMultiPlayerInput.HasMaxHp => hasMaxHp;
            int IMultiPlayerInput.MaxHp => maxHp;
            bool IMultiPlayerInput.HasCurHp => hasCurHp;
            int IMultiPlayerInput.CurHp => curHp;
            byte IMultiPlayerInput.TeamIndex => tempIndex;
            ItemInfo.IEquippedItemValue[] IMultiPlayerInput.GetEquippedItems => equippedItems;

            DamagePacket.UnitKey damageUnitKey;

            public CreateTask(IMultiPlayerInput input)
            {
                isExceptEquippedItems = input.IsExceptEquippedItems;
                cid = input.Cid;
                name = input.Name;
                job = input.Job;
                gender = input.Gender;
                jobLevel = input.JobLevel;
                profileId = input.ProfileId;
                weaponItemId = input.WeaponItemId;
                guildName = input.GuildName;
                posX = input.PosX;
                posZ = input.PosZ;
                state = input.State;
                privateStoreComment = input.PrivateStoreComment;
                sellingState = input.PrivateStoreSellingState;
                uid = input.UID;
                hasMaxHp = input.HasMaxHp;
                maxHp = input.MaxHp;
                hasCurHp = input.HasCurHp;
                curHp = input.CurHp;
                tempIndex = input.TeamIndex;
                equippedItems = input.GetEquippedItems;
                damageUnitKey = input.GetDamageUnitKey();
            }

            public void SetState(byte state)
            {
                this.state = state;
            }

            public void SetPosition(Vector3 pos)
            {
                posX = pos.x;
                posY = pos.y;
                posZ = pos.z;
            }

            public void SetSellingState(PrivateStoreSellingState sellingState, string privateStoreComment)
            {
                this.sellingState = sellingState;
                this.privateStoreComment = privateStoreComment;
            }

            public void SetWeapon(int id)
            {
                weaponItemId = id;
            }

            public void SetCostume(string ids)
            {
                string[] results =  ids.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                equippedItems = new EquipmentValuePacket[results.Length];
                for (int i = 0; i < results.Length; i++)
                {
                    equippedItems[i] = new EquipmentValuePacket(int.Parse(results[i]));
                }
            }

            public void SetCurHp(int curHp)
            {
                this.curHp = curHp;
            }

            DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
            {
                return damageUnitKey;
            }
        }
    }
}