using Ragnarok.View;
using UnityEngine;

namespace Ragnarok
{
    public class UIMatchMultiMazeStatus : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIGrid grid;
        [SerializeField] UIMatchPlayerSlot[] slots;
        [SerializeField] UILabel labelRemainCoin;
        [SerializeField] GameObject goRemainCoinBase;
        [SerializeField] UIWidget targetWidget;

        protected override void OnInit()
        {
        }

        protected override void OnClose()
        {
        }

        protected override void OnShow(IUIData data = null)
        {
            SetPlayers(null); // 초기화
            goRemainCoinBase.SetActive(true);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
        }

        public void SetPlayers(CharacterEntity[] players)
        {
            int playerCount = players == null ? 0 : players.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].SetData(i < playerCount ? players[i] : null);
            }

            grid.Reposition();
        }

        public void SetState(int cid, UIMatchPlayerSlot.State state)
        {
            UIMatchPlayerSlot slot = GetSlot(cid);
            if (slot == null)
                return;

            slot.SetState(state);
        }

        public void UpdateHp(int cid, int cur, int max)
        {
            UIMatchPlayerSlot slot = GetSlot(cid);
            if (slot == null)
                return;

            slot.UpdateHp(cur, max);
        }

        public void SetCoin(int cid, int coin)
        {
            UIMatchPlayerSlot slot = GetSlot(cid);
            if (slot == null)
                return;

            slot.SetCoin(coin);
        }

        public void ShowBossBattle(int cid)
        {
            UIMatchPlayerSlot slot = GetSlot(cid);
            if (slot == null)
                return;

            slot.SetActiveBossBattle(true);
        }

        public void SetRemainCoin(int remainCoin, int maxCoin)
        {
            labelRemainCoin.text = string.Concat(remainCoin, "/", maxCoin);
        }

        public void ShowRemainCoin(bool isActive)
        {
            goRemainCoinBase.SetActive(isActive);
        }

        private UIMatchPlayerSlot GetSlot(int cid)
        {
            foreach (var item in slots)
            {
                if (item.Cid == cid)
                    return item;
            }

            return null;
        }

        public override bool Find()
        {
            base.Find();

            slots = GetComponentsInChildren<UIMatchPlayerSlot>();
            return true;
        }

        public UIWidget GetWidGet()
        {
            return targetWidget;
        }

        public UIWidget GetWidget(int cid)
        {
            foreach (var item in slots)
            {
                if (item.Cid == cid)
                    return item.GetWidget();
            }
            return null;
        }
    }
}