using UnityEngine;

namespace Ragnarok
{
    public class UIItemBattleOptionList : UIBattleOptionList, IInspectorFinder
    {
        [SerializeField] UILabelValue atk, matk, def, mdef;        

        public void SetData(ItemInfo info)
        {
            ItemInfo.Status status = info.GetStatus();

            SetValue(atk, LocalizeKey._56000.ToText(), status.atk); // 물리 공격력
            SetValue(matk, LocalizeKey._56001.ToText(), status.matk); // 마법 공격력
            SetValue(def, LocalizeKey._56002.ToText(), status.def); // 물리 방어력
            SetValue(mdef, LocalizeKey._56003.ToText(), status.mdef); // 마법 방어력

            base.SetData(info);            
        }

        private void SetValue(UILabelValue labelValue, string title, int value)
        {
            bool isValid = value > 0;
            labelValue.SetActive(isValid);

            if (!isValid)
                return;

            labelValue.Title = title;
            labelValue.Value = $"+{value}";
        }
    }
}