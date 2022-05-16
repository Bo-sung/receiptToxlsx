using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UISelectPropertySlot : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UILabelHelper labelProperty;
        [SerializeField] UILabelHelper labelNone;
        [SerializeField] UIGridHelper grid;
        [SerializeField] UISelectPropertySlotInfo[] slotInfos;

        public void InitData(List<ElementType> types, bool isWeak)
        {
            if (isWeak) labelProperty.LocalKey = LocalizeKey._5202;// 열세
            else labelProperty.LocalKey = LocalizeKey._5201; // 우세

            labelNone.LocalKey = LocalizeKey._5203; // 정보 없음
            labelNone.SetActive(types.Count <= 0);

            grid.SetValue(types.Count);
            for (int i = 0; i < types.Count; i++)
            {
                if (i >= slotInfos.Length) break; // 속성슬롯(4개)보다 많으면 표시하지 않음.

                slotInfos[i].InitData(types[i]);
            }
        }

        bool IInspectorFinder.Find()
        {
            if (grid != null) slotInfos = grid.GetComponentsInChildren<UISelectPropertySlotInfo>();
            return true;
        }
    }
}