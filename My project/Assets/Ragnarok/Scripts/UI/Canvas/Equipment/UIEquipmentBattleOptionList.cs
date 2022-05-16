using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIEquipmentBattleOptionList : UIBattleOptionList
    {
       [SerializeField] UILabelValue atk, matk, def, mdef;
       [SerializeField] UICardBattleOption[] card;

        public event System.Action<byte> OnSelectEquip;
        public event System.Action<byte> OnSelectUnEquip;

        private void Start()
        {
            for (int i = 0; i < card.Length; i++)
            {
                card[i].OnSelectEquip += OnEquip;
                card[i].OnSelectUnEquip += OnUnEquip;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < card.Length; i++)
            {
                card[i].OnSelectEquip -= OnEquip;
                card[i].OnSelectUnEquip -= OnUnEquip;
            }
        }        

        public void SetData(ItemInfo info)
        {
            ItemInfo.Status status = info.GetStatus();

            SetValue(atk, LocalizeKey._56000.ToText(), status.atk); // 물리 공격력
            SetValue(matk, LocalizeKey._56001.ToText(), status.matk); // 마법 공격력
            SetValue(def, LocalizeKey._56002.ToText(), status.def); // 물리 방어력
            SetValue(mdef, LocalizeKey._56003.ToText(), status.mdef); // 마법 방어력

            for (int i = 0; i < card.Length; i++)
            {
                card[i].SetOpenSlot(info.IsOpenCardSlot(i));
                card[i].SetData(info);
            }

            base.SetData(info);
        }

        private void SetValue(UILabelValue labelValue, string title, int value)
        {
            bool isValid = value > 0;

            labelValue.Title = title;

            if (!isValid)
            {
                labelValue.Value = "-";
                return;
            }
            
            labelValue.Value = $"{value}";
        }        

        void OnEquip(byte index)
        {
            OnSelectEquip?.Invoke(index);
        }

        void OnUnEquip(byte index)
        {
            OnSelectUnEquip?.Invoke(index);
        }
    } 
}
