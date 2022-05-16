using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class GoodsViewPresenter : ViewPresenter, IEqualityComparer<GoodsViewData.GoodsViewDataType>
    {
        public interface IView
        {
            void AddSlot(GoodsViewData data);
            void ReleaseSlot(GameObject go);
        }

        public BetterList<GoodsViewSlot> Slots => slots;
        public Transform SpawnTransform { set => tfSpawn = value; get => tfSpawn; }
        public Transform TopTransform { set => tfTop = value; get => tfTop; }
        public Vector3 SpawnPosInterval { set => spawnPosInterval = value; get => spawnPosInterval; }

        GoodsModel goodsModel;
        CharacterModel characterModel;
        Transform tfSpawn;
        Transform tfTop;
        Vector3 spawnPosInterval;
        IView view;

        BetterList<GoodsViewSlot> slots;
        Dictionary<GoodsViewData.GoodsViewDataType, long> storedGoodsValue;
        Dictionary<GoodsViewData.GoodsViewDataType, long> lastGoodsValue;


        public GoodsViewPresenter(IView view)
        {
            this.view = view;

            slots = new BetterList<GoodsViewSlot>();
            goodsModel = Entity.player.Goods;
            characterModel = Entity.player.Character;

            storedGoodsValue = new Dictionary<GoodsViewData.GoodsViewDataType, long>();
            storedGoodsValue[GoodsViewData.GoodsViewDataType.Zeny] = 0;
            storedGoodsValue[GoodsViewData.GoodsViewDataType.LevelExp] = 0;
            storedGoodsValue[GoodsViewData.GoodsViewDataType.JobExp] = 0;

            lastGoodsValue = new Dictionary<GoodsViewData.GoodsViewDataType, long>();
            lastGoodsValue[GoodsViewData.GoodsViewDataType.Zeny] = goodsModel.Zeny;
            lastGoodsValue[GoodsViewData.GoodsViewDataType.LevelExp] = characterModel.LevelExp;
            lastGoodsValue[GoodsViewData.GoodsViewDataType.JobExp] = characterModel.JobLevelExp;
        }

        public override void AddEvent()
        {
            goodsModel.OnUpdateZeny += OnGoodsUpdate_Zeny;
            characterModel.OnUpdateLevelExp += OnGoodsUpdate_LevelExp;
            characterModel.OnUpdateJobExp += OnGoodsUpdate_JobExp;
        }

        public override void RemoveEvent()
        {
            goodsModel.OnUpdateZeny -= OnGoodsUpdate_Zeny;
            characterModel.OnUpdateLevelExp -= OnGoodsUpdate_LevelExp;
            characterModel.OnUpdateJobExp -= OnGoodsUpdate_JobExp;
        }

        public void DeleteSlot(int index)
        {
            GoodsViewData.GoodsViewDataType destGoodsType = slots[index].Data.type;

            var go = slots[index].gameObject;
            slots.RemoveAt(index);
            view.ReleaseSlot(go);

            foreach (var slot in slots)
            {
                int i = slots.IndexOf(slot);
                slot.SetIndex(i);
            }

            AddGoodsData(destGoodsType, storedGoodsValue[destGoodsType]);
            storedGoodsValue[destGoodsType] = 0;
        }

        void OnGoodsUpdate_Zeny(long newZeny)
        {
            OnGoodsUpdate(GoodsViewData.GoodsViewDataType.Zeny, newZeny);
        }

        void OnGoodsUpdate_JobExp(long newJobExp)
        {
            OnGoodsUpdate(GoodsViewData.GoodsViewDataType.JobExp, newJobExp);
        }

        void OnGoodsUpdate_LevelExp(int newExp)
        {
            OnGoodsUpdate(GoodsViewData.GoodsViewDataType.LevelExp, newExp);
        }

        void OnGoodsUpdate(GoodsViewData.GoodsViewDataType type, long newValue)
        {
            long gap = newValue - lastGoodsValue[type];
            lastGoodsValue[type] = newValue;

            if (slots.Find(e => e.Data.type == type) is null)
            {
                AddGoodsData(type, gap);
            }
            else if (gap > 0)
            {
                storedGoodsValue[type] += gap;
            }
        }


        private void AddGoodsData(GoodsViewData.GoodsViewDataType type, long gap)
        {
            if (gap <= 0)
                return;

            view.AddSlot(new GoodsViewData(type, gap));
        }

        public Vector3 GetTopPositionByIndex(int index)
        {
            return this.tfTop.localPosition + (this.spawnPosInterval * index);
        }

        bool IEqualityComparer<GoodsViewData.GoodsViewDataType>.Equals(GoodsViewData.GoodsViewDataType x, GoodsViewData.GoodsViewDataType y)
        {
            return (x == y);
        }

        int IEqualityComparer<GoodsViewData.GoodsViewDataType>.GetHashCode(GoodsViewData.GoodsViewDataType obj)
        {
            return obj.GetHashCode();
        }
    }
}