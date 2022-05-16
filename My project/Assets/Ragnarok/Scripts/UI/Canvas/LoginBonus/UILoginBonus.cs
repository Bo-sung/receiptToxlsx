using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UILoginBonus : UICanvas, IInspectorFinder
    {
        public class Input : IUIData
        {
            public int groupToShow = -1;
            public bool showUIDailyCheck = true;
        }

        private const string TAG = nameof(UILoginBonus);

        protected override UIType uiType => UIType.Back | UIType.Destroy;

        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UILabelHelper noticeLabel;
        [SerializeField] UIButton backButton;
        [SerializeField] UIButton backButton2;
        [SerializeField] UILoginBonusSlot[] slots;
        [SerializeField] UILabelHelper lastRewardSubDesc;

        [SerializeField] GameObject[] lastRewardEffectRoots;

        [SerializeField] List<string> titleObjectKey;
        [SerializeField] List<GameObject> titleObjectVal;
        [SerializeField] UILabelHelper[] labelTitle;

        LoginBonusPresenter presenter;

        private List<EventLoginPacket> loginPacketList;
        private int curShowIndex;
        private bool showDailyCheckOnHide = false;

        protected override void OnInit()
        {
            presenter = new LoginBonusPresenter();

            EventDelegate.Add(backButton.onClick, TryCloseUI);
            EventDelegate.Add(backButton2.onClick, TryCloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(backButton.onClick, TryCloseUI);
            EventDelegate.Remove(backButton2.onClick, TryCloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
            Input input = data as Input;
            if (input == null)
                input = new Input();
            showDailyCheckOnHide = input.showUIDailyCheck;

            loginPacketList = new List<EventLoginPacket>();

            var loginPackets = Entity.player.User.EventLoginPackets;

            foreach (var each in loginPackets)
                if (input.groupToShow == -1 || input.groupToShow == each.Group)
                    loginPacketList.Add(each);

            if (loginPacketList.Count == 0)
            {
                TryCloseUI();
                return;
            }

            curShowIndex = 0;
            UpdateTitle();
            UpdateSlots();
        }

        private void TryCloseUI()
        {
            if (curShowIndex + 1 < loginPacketList.Count)
            {
                ++curShowIndex;
                UpdateSlots();
                return;
            }

            TryToShowDailyCheck();
            UI.Close<UILoginBonus>();
        }

        private void UpdateTitle()
        {
            var type = presenter.GetImageLanguageType();
            var index = titleObjectKey.IndexOf(type);

            for (int i = 0; i < titleObjectVal.Count; i++)
            {
                titleObjectVal[i].SetActive(i == index);
            }
        }

        private void UpdateSlots()
        {
            Timing.KillCoroutines(TAG);

            for (int i = 0; i < lastRewardEffectRoots.Length; ++i)
                lastRewardEffectRoots[i].SetActive(false);

            var each = loginPacketList[curShowIndex];

            var bonusDatas = EventLoginBonusDataManager.Instance.Get(each.Group);
            UILoginBonusSlot newSlot = null;
            EventLoginBonusData data = null;
            bool isLast = false;

            for (int i = 0; i < slots.Length; ++i)
            {
                var day = bonusDatas[i].day;
                UILoginBonusSlot.State state;

                if (day < each.Day)
                    state = UILoginBonusSlot.State.Already;
                else if (day == each.Day)
                    state = UILoginBonusSlot.State.New;
                else
                    state = UILoginBonusSlot.State.Not;

                slots[i].Init(bonusDatas[i], state);

                if (state == UILoginBonusSlot.State.New)
                {
                    newSlot = slots[i];
                    data = bonusDatas[i];
                    if (i == slots.Length - 1)
                        isLast = true;
                }
                else
                {
                    slots[i].SetCompleteState(state);
                }
            }

            Timing.RunCoroutine(ShowLastRewardEffect(newSlot, data, isLast), TAG);
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            titleLabel.LocalKey = LocalizeKey._49300; // 매일마다 출석하면 선물을 받을 수 있어
            noticeLabel.LocalKey = LocalizeKey._49301; // 받으신 선물은 우편함에서 수령 하실 수 있습니다.
            lastRewardSubDesc.LocalKey = LocalizeKey._49302; // 랜덤 카드 지급!!
            foreach (var item in labelTitle)
            {
                item.LocalKey = LocalizeKey._49303; // GRAND OPEN
            }
        }

        protected override void OnBack()
        {
            TryCloseUI();
        }

        private void TryToShowDailyCheck()
        {
            if (showDailyCheckOnHide && presenter.IsNewDaily())
            {
                UIDailyCheck.tabType = UIDailyCheck.TabType.DailyCheck;
                UI.Show<UIDailyCheck>();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Timing.KillCoroutines(TAG);
        }

        private IEnumerator<float> ShowLastRewardEffect(UILoginBonusSlot slotToShowNew, EventLoginBonusData data, bool isLast)
        {
            var uiFade = UI.GetUI<UIFade>();

            while (uiFade != null && uiFade.IsVisible)
                yield return 0;

            slotToShowNew.SetCompleteState(UILoginBonusSlot.State.New);

            if (isLast)
            {
                for (int i = 0; i < lastRewardEffectRoots.Length; ++i)
                    lastRewardEffectRoots[i].SetActive(true);
            }
        }

        bool IInspectorFinder.Find()
        {
            titleObjectKey = new List<string>();
            titleObjectVal = new List<GameObject>();

            foreach (var t in GetComponentsInChildren<Transform>())
            {
                var name = t.name;

                foreach (var lc in Enum.GetValues(typeof(LanguageType)))
                {
                    var eName = LanguageConfig.GetBytKey((LanguageType)lc).type;
                    if (string.Equals(name, eName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        titleObjectKey.Add(eName);
                        titleObjectVal.Add(t.gameObject);
                        break;
                    }
                }
            }
            return true;
        }
    }
}