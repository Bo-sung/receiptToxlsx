using UnityEngine;

namespace Ragnarok
{
    public class UIMailOnBuff : UIMail
    {
        private const int TAB_NORMAL = 0;
        private const int TAB_ONBUFF = 1;

        [SerializeField] UITabHelper subTab;

        private bool isSelectSubTab = false;

        protected override void OnInit()
        {
            base.OnInit();
            subTab.OnSelect += OnSelectSubTab;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            subTab.OnSelect -= OnSelectSubTab;
        }

        protected override void OnLocalize()
        {
            base.OnLocalize();
            subTab[TAB_NORMAL].LocalKey = LocalizeKey._12027; // 일반
            subTab[TAB_ONBUFF].LocalKey = LocalizeKey._12028; // OnBuff
        }

        public override void ShowAccountSubView()
        {
            if (!UIToggle.current.value)
                return;

           
            int index = subTab.GetSelectIndex();

            switch (index)
            {
                case TAB_NORMAL:
                    presenter.SetMailType(MailType.Account);
                    break;
                case TAB_ONBUFF:
                    presenter.SetMailType(MailType.OnBuff);
                    break;
            }

            if (isSelectSubTab)
            {
                OnSelectSubTab(index);
            }
            else
            {
                ShowSubCanvas(accountView);
            }
        }

        private async void OnSelectSubTab(int index)
        {
            isSelectSubTab = true;
            switch (index)
            {
                case TAB_NORMAL:
                    await presenter.RequestMailList(MailType.Account);
                    accountView.SetMailType(MailType.Account);
                    break;

                case TAB_ONBUFF:
                    await presenter.RequestMailList(MailType.OnBuff);
                    accountView.SetMailType(MailType.OnBuff);
                    break;
            }

            ShowSubCanvas(accountView);
            accountView.SetInviteSlot();
            accountView.ResetPosition();
        }
      
        public override void SetMailNew(AlarmType alarmType)
        {
            tab[0].SetNotice(alarmType.HasFlag(AlarmType.MailAccount) || alarmType.HasFlag(AlarmType.MailOnBuff));
            tab[1].SetNotice(alarmType.HasFlag(AlarmType.MailCharacter));
            tab[2].SetNotice(alarmType.HasFlag(AlarmType.MailShop));
            tab[3].SetNotice(alarmType.HasFlag(AlarmType.MailTrade));

            subTab[TAB_NORMAL].SetNotice(alarmType.HasFlag(AlarmType.MailAccount));
            subTab[TAB_ONBUFF].SetNotice(alarmType.HasFlag(AlarmType.MailOnBuff));
        }
    }
}