using System;
using UnityEngine;

namespace Ragnarok
{
    public sealed class AlarmModel : CharacterEntityModel
    {
        private readonly ItemDataManager itemDataRepo;

        private Action<AlarmType> onAlarm;

        public event Action<AlarmType> OnAlarm
        {
            add { onAlarm += value; value(alarmType); }
            remove { onAlarm -= value; }
        }

        private AlarmType alarmType;
        private bool isInitialize;

        public AlarmModel()
        {
            itemDataRepo = ItemDataManager.Instance;
            isInitialize = false;
        }

        public override void AddEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.GAME_ALARM.AddEvent(OnReceiveGameAlarm);
                Protocol.RECEIVE_ITEM_GET_NOTICE.AddEvent(OnReceiveItemGetNotice);
                Protocol.RECEIVE_REFRESH_CUD.AddEvent(OnReceiveRefreshCud);
            }
        }

        public override void RemoveEvent(UnitEntityType type)
        {
            if (type == UnitEntityType.Player)
            {
                Protocol.GAME_ALARM.RemoveEvent(OnReceiveGameAlarm);
                Protocol.RECEIVE_ITEM_GET_NOTICE.RemoveEvent(OnReceiveItemGetNotice);
                Protocol.RECEIVE_REFRESH_CUD.RemoveEvent(OnReceiveRefreshCud);
            }
        }

        internal void Initialize(AlarmType alarmType)
        {
            isInitialize = true;
            this.alarmType = alarmType;
            Debug.Log($"알람 세팅 = {this.alarmType}");
            onAlarm?.Invoke(this.alarmType);
        }

        private void OnReceiveGameAlarm(Response response)
        {
            if (response.isSuccess)
            {
                int alarmValue = response.GetInt("1");
                NotifyAlarm(alarmValue);
            }
            else
            {
                response.ShowResultCode();
            }
        }

        public bool HasAlarm(AlarmType type)
        {
            return alarmType.HasFlag(type);
        }

        public void RemoveAlarm(AlarmType type)
        {
            if (!HasAlarm(type))
                return;

            Debug.Log($"알람 제거 = {type}");

            alarmType.RemoveFlagEnum(type);
            onAlarm?.Invoke(alarmType);
        }

        public void AddAlarm(AlarmType type)
        {
            if (HasAlarm(type))
                return;

            Debug.Log($"알람 추가 = {type}");

            alarmType.AddFlagEnum(type);
            onAlarm?.Invoke(alarmType);
        }

        private void OnReceiveItemGetNotice(Response response)
        {
            if (!response.isSuccess || !isInitialize)
                return;

            var packet = response.GetPacket<ItemGetNoticePacket>();
            string name = packet.name;
            RewardType rewardType = packet.goods_type.ToEnum<RewardType>();
            string itemName;
            if (rewardType == RewardType.Item)
            {
                ItemData itemData = itemDataRepo.Get(packet.item_id);
                if (itemData == null)
                    return;

                itemName = itemData.name_id.ToText();
            }
            else if (rewardType == RewardType.Agent)
            {
                AgentData agentData = AgentDataManager.Instance.Get(packet.item_id);
                if (agentData == null)
                    return;

                itemName = agentData.name_id.ToText();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"정의되지 않은 리워드타입 {nameof(rewardType)} = {rewardType}");
#endif
                return;
            }

            string text = LocalizeKey._90141.ToText()
                .Replace(ReplaceKey.NAME, name)
                .Replace(ReplaceKey.ITEM, itemName); // 유니크 MVP 처치! {NAME}님이 {ITEM}을 획득했습니다.

            UI.Show<UIBattleNotice>().Show(text);
        }

        void OnReceiveRefreshCud(Response response)
        {
            // cud. 캐릭터 업데이트 데이터
            if (response.ContainsKey("cud"))
            {
                CharUpdateData charUpdateData = response.GetPacket<CharUpdateData>("cud");
                Notify(charUpdateData);
            }
        }
    }
}