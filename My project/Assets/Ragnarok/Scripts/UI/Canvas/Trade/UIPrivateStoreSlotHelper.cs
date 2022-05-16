using UnityEngine;

namespace Ragnarok
{
    public class UIPrivateStoreSlotHelper : MonoBehaviour, IInspectorFinder
    {
        [SerializeField] UIPrivateStoreSlot[] slots;

        public void SetData(int idx, PrivateStorePresenter presenter, PrivateStoreItemData data)
        {
            slots[idx].SetData(presenter, data);
        }

        bool IInspectorFinder.Find()
        {
            slots = GetComponentsInChildren<UIPrivateStoreSlot>();
            return true;
        }
    }
}