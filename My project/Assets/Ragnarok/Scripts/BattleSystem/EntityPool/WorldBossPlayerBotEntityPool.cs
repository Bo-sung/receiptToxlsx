using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    /// <summary>
    /// 1. Entity Pooling
    /// 2. 생성되기 전 바뀐 행동을 담아서 처리
    /// </summary>
    public sealed class WorldBossPlayerBotEntityPool : BetterList<GhostPlayerEntity>
    {
        private readonly Stack<GhostPlayerEntity> pooledStack;
        private readonly BetterList<CreateTask> createQueue;
        private readonly Dictionary<int, int> createQueueIndexDic; // key: cid, value: index
        private readonly Dictionary<int, int> uidDic; // key: cid, value: uid

        public WorldBossPlayerBotEntityPool()
        {
            pooledStack = new Stack<GhostPlayerEntity>();
            createQueue = new BetterList<CreateTask>();
            createQueueIndexDic = new Dictionary<int, int>(IntEqualityComparer.Default);
            uidDic = new Dictionary<int, int>(IntEqualityComparer.Default);
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
            uidDic.Clear();
        }

        /// <summary>
        /// Entity 생성
        /// </summary>
        public GhostPlayerEntity Create(IMultiPlayerInput input)
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
        public void Recycle(GhostPlayerEntity entity)
        {
            entity.Initialize(PlayerBotEntity.DEFAULT); // 초기화
            entity.ResetData();
            pooledStack.Push(entity); // Stack에 관리 (Pool)
        }

        /// <summary>
        /// Entity 반환 - 고유 cid 이용
        /// </summary>
        public GhostPlayerEntity Find(int cid)
        {
            for (int i = 0; i < size; i++)
            {
                if (buffer[i].Character.Cid == cid)
                    return buffer[i];
            }

            return null;
        }

        /// <summary>
        /// UID 반환
        /// </summary>
        public int GetUid(int cid)
        {
            if (uidDic.ContainsKey(cid))
                return uidDic[cid];

            return -1;
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

            uidDic[cid] = input.UID;
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
            public bool IsExceptEquippedItems { get; private set; }
            public int WeaponItemId { get; private set; }
            public BattleItemInfo.IValue ItemStatusValue { get; private set; }
            public int ArmorItemId { get; private set; }
            public ElementType WeaponChangedElement { get; private set; }
            public int WeaponElementLevel { get; private set; }
            public ElementType ArmorChangedElement { get; private set; }
            public int ArmorElementLevel { get; private set; }
            public SkillModel.ISkillValue[] Skills { get; private set; }
            public SkillModel.ISlotValue[] Slots { get; private set; }
            public CupetListModel.IInputValue[] Cupets { get; private set; }
            public IBattleOption[] BattleOptions { get; private set; }
            public IBattleOption[] GuildBattleOptions { get; private set; }
            public float PosX { get; private set; }
            public float PosY { get; private set; }
            public float PosZ { get; private set; }
            public byte State { get; private set; }
            public int UID { get; private set; }
            public bool HasMaxHp { get; private set; }
            public int MaxHp { get; private set; }
            public bool HasCurHp { get; private set; }
            public int CurHp { get; private set; }
            public byte TeamIndex { get; private set; }
            public ItemInfo.IEquippedItemValue[] GetEquippedItems { get; private set; }
            public int Cid { get; private set; }
            public string Name { get; private set; }
            public byte Job { get; private set; }
            public byte Gender { get; private set; }
            public int Level { get; private set; }
            public int LevelExp { get; private set; }
            public int JobLevel { get; private set; }
            public long JobLevelExp { get; private set; }
            public int RebirthCount { get; private set; }
            public int RebirthAccrueCount { get; private set; }
            public int NameChangeCount { get; private set; }
            public string CidHex { get; private set; }
            public int ProfileId { get; private set; }
            public int Str { get; private set; }
            public int Agi { get; private set; }
            public int Vit { get; private set; }
            public int Int { get; private set; }
            public int Dex { get; private set; }
            public int Luk { get; private set; }
            public int StatPoint { get; private set; }
            public int GuildId { get; private set; }
            public string GuildName { get; private set; }
            public int GuildEmblem { get; private set; }
            public byte GuildPosition { get; private set; }
            public int GuildCoin { get; private set; }
            public int GuildQuestRewardCount { get; private set; }
            public long GuildSkillBuyDateTime { get; private set; }
            public byte GuildSkillBuyCount { get; private set; }
            public long GuildRejoinTime { get; private set; }
            public string PrivateStoreComment { get; private set; }
            public PrivateStoreSellingState PrivateStoreSellingState { get; private set; }

            private DamagePacket.UnitKey damageUnitKey;

            public CreateTask(IMultiPlayerInput input)
            {
                IsExceptEquippedItems = input.IsExceptEquippedItems;
                WeaponItemId = input.WeaponItemId;
                ItemStatusValue = input.ItemStatusValue;
                ArmorItemId = input.ArmorItemId;
                WeaponChangedElement = input.WeaponChangedElement;
                WeaponElementLevel = input.WeaponElementLevel;
                ArmorChangedElement = input.ArmorChangedElement;
                ArmorElementLevel = input.ArmorElementLevel;
                Skills = input.Skills;
                Slots = input.Slots;
                Cupets = input.Cupets;
                BattleOptions = input.BattleOptions;
                GuildBattleOptions = input.GuildBattleOptions;
                PosX = input.PosX;
                PosY = input.PosY;
                PosZ = input.PosZ;
                State = input.State;
                UID = input.UID;
                HasMaxHp = input.HasMaxHp;
                MaxHp = input.MaxHp;
                HasCurHp = input.HasCurHp;
                CurHp = input.CurHp;
                TeamIndex = input.TeamIndex;
                GetEquippedItems = input.GetEquippedItems;
                Cid = input.Cid;
                Name = input.Name;
                Job = input.Job;
                Gender = input.Gender;
                Level = input.Level;
                LevelExp = input.LevelExp;
                JobLevel = input.JobLevel;
                JobLevelExp = input.JobLevelExp;
                RebirthCount = input.RebirthCount;
                RebirthAccrueCount = input.RebirthAccrueCount;
                NameChangeCount = input.NameChangeCount;
                CidHex = input.CidHex;
                ProfileId = input.ProfileId;
                Str = input.Str;
                Agi = input.Agi;
                Vit = input.Vit;
                Int = input.Int;
                Dex = input.Dex;
                Luk = input.Luk;
                StatPoint = input.StatPoint;
                GuildId = input.GuildId;
                GuildName = input.GuildName;
                GuildEmblem = input.GuildEmblem;
                GuildPosition = input.GuildPosition;
                GuildCoin = input.GuildCoin;
                GuildQuestRewardCount = input.GuildQuestRewardCount;
                GuildSkillBuyDateTime = input.GuildSkillBuyDateTime;
                GuildSkillBuyCount = input.GuildSkillBuyCount;
                GuildRejoinTime = input.GuildRejoinTime;
                PrivateStoreComment = input.PrivateStoreComment;
                PrivateStoreSellingState = input.PrivateStoreSellingState;
                damageUnitKey = input.GetDamageUnitKey();
            }

            public void SetState(byte state)
            {
                State = state;
            }

            public void SetPosition(Vector3 pos)
            {
                PosX = pos.x;
                PosY = pos.y;
                PosZ = pos.z;
            }

            public void SetSellingState(PrivateStoreSellingState sellingState, string privateStoreComment)
            {
                PrivateStoreSellingState = sellingState;
                PrivateStoreComment = privateStoreComment;
            }

            public void SetWeapon(int id)
            {
                WeaponItemId = id;
            }

            public void SetCostume(string ids)
            {
                string[] results = ids.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                GetEquippedItems = new ItemInfo.IEquippedItemValue[results.Length];
                for (int i = 0; i < results.Length; i++)
                {
                    GetEquippedItems[i] = new EquipmentValuePacket(int.Parse(results[i]));
                }
            }

            public void SetCurHp(int curHp)
            {
                CurHp = curHp;
            }

            DamagePacket.UnitKey IMultiPlayerInput.GetDamageUnitKey()
            {
                return damageUnitKey;
            }
        }
    }
}