using UnityEngine;

namespace Ragnarok.View
{
    public class CharacterShare2nd : UIView
    {
        [SerializeField] UIButtonHelper btnHelp;
        [SerializeField] Share2ndSuitSlot[] suitParts;

        public event System.Action<ShareForceType> OnLockErrorMessage;

        protected override void Awake()
        {
            base.Awake();

            EventDelegate.Add(btnHelp.OnClick, OnClickBtnHelp);

            foreach (var item in suitParts)
            {
                item.OnLockErrorMessage += OnSelectLockErrorMessage;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventDelegate.Remove(btnHelp.OnClick, OnClickBtnHelp);

            foreach (var item in suitParts)
            {
                item.OnLockErrorMessage -= OnSelectLockErrorMessage;
            }
        }

        protected override void OnLocalize()
        {
        }

        void OnClickBtnHelp()
        {
            // 타임패트롤 가이드
            int dungeonInfoId = DungeonInfoType.ShareVice2nd.GetDungeonInfoId();
            UI.Show<UIDungeonInfoPopup>().Show(dungeonInfoId);
        }

        void OnSelectLockErrorMessage(ShareForceType type)
        {
            OnLockErrorMessage?.Invoke(type);
        }

        public void SetData(Share2ndSuitSlot.IInput[] inputs)
        {
            int length = inputs == null ? 0 : inputs.Length;
            for (int i = 0; i < suitParts.Length; i++)
            {
                suitParts[i].SetData(i < length ? inputs[i] : null);
            }
        }
    }
}