using Ragnarok.View.BattleStage;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UIBattleMazeAgentList : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;
        public override int layer => Layer.UI_ExceptForCharZoom;

        [SerializeField] UIBattleStageAgentSlot[] shareSlots;
        [SerializeField] UIGrid gridShare;

        public event System.Action<CharacterEntity> OnSelectAgent;

        protected override void OnInit()
        {
            for (int i = 0; i < shareSlots.Length; i++)
            {
                shareSlots[i].OnSelect += OnClickedShareCharacter;
            }
        }

        protected override void OnClose()
        {
            for (int i = 0; i < shareSlots.Length; i++)
            {
                shareSlots[i].OnSelect -= OnClickedShareCharacter;
            }
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        void OnClickedShareCharacter(CharacterEntity entity)
        {
            OnSelectAgent?.Invoke(entity);
        }

        public void SetShareCharacters(CharacterEntity[] array)
        {
            for (int i = 0; i < shareSlots.Length; i++)
            {
                if (i < array.Length)
                {
                    shareSlots[i].Show();
                    shareSlots[i].SetData(array[i]);
                }
                else
                {
                    shareSlots[i].Hide();
                }
            }

            gridShare.Reposition();
        }

        public void SetCloneCharacters(CharacterEntity[] array)
        {
            for (int i = Constants.Size.SHARE_SLOT_SIZE; i < shareSlots.Length; i++)
            {
                if (i < array.Length + Constants.Size.SHARE_SLOT_SIZE)
                {
                    shareSlots[i].Show();
                    shareSlots[i].SetData(array[i - Constants.Size.SHARE_SLOT_SIZE]);
                }
                else
                {
                    shareSlots[i].Hide();
                }
            }

            gridShare.Reposition();
        }

        public override bool Find()
        {
            base.Find();

            shareSlots = GetComponentsInChildren<UIBattleStageAgentSlot>();

#if UNITY_EDITOR
            /*for (int i = shareSlots.Length - 1; i >= 0; i--)
            {
                if (i < Constants.Size.SHARE_SLOT_SIZE)
                    continue;

                UnityEditor.ArrayUtility.RemoveAt(ref shareSlots, i);
            }

            for (int i = cloneSlots.Length - 1; i >= 0; i--)
            {
                if (i >= Constants.Size.SHARE_SLOT_SIZE)
                    continue;

                UnityEditor.ArrayUtility.RemoveAt(ref cloneSlots, i);
            }*/
#endif
            return true;
        }
    }
}