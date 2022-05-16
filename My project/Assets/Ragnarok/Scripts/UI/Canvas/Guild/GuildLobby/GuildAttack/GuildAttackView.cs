using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok.View
{
    public class GuildAttackView : UIView
    {
        [SerializeField] UILabelHelper labelEmperiumLevel;
        [SerializeField] EmperiumBuffView emperiumBuffView;
        [SerializeField] EmperiumStartTimeView emperiumStartTimeView;
        [SerializeField] EmperiumDonationView emperiumDonationView;
        [SerializeField] EmperiumRemainTimeView emperiumRemainTimeView;
        [SerializeField] UILabelHelper labelNoti;

        public event Action OnChangeStartTime;
        public event Action OnChangeStartTimeHelp;
        public event Action OnDonation;
        public event Action OnDonationHelp;

        protected override void Awake()
        {
            base.Awake();
            emperiumStartTimeView.OnSelect += OnSelectChangeStartTime;
            emperiumStartTimeView.OnSelectHelp += OnSelectChangeStartTimeHelp;
            emperiumDonationView.OnSelect += OnSelectDonation;
            emperiumDonationView.OnSelectHelp += OnSelectDonationHelp;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            emperiumStartTimeView.OnSelect -= OnSelectChangeStartTime;
            emperiumStartTimeView.OnSelectHelp -= OnSelectChangeStartTimeHelp;
            emperiumDonationView.OnSelect -= OnSelectDonation;
            emperiumDonationView.OnSelectHelp -= OnSelectDonationHelp;
        }

        protected override void OnLocalize()
        {
            labelNoti.LocalKey = LocalizeKey._38412; // 몬스터에게서 엠펠리움을 보호하세요.
        }

        private void OnSelectChangeStartTime()
        {
            OnChangeStartTime?.Invoke();
        }

        private void OnSelectChangeStartTimeHelp()
        {
            OnChangeStartTimeHelp?.Invoke();
        }

        private void OnSelectDonation()
        {
            OnDonation?.Invoke();
        }

        private void OnSelectDonationHelp()
        {
            OnDonationHelp?.Invoke();
        }

        public void SetEmperiumLevel(string levelText)
        {
            labelEmperiumLevel.Text = levelText;
        }

        public void SetBuffOption(IEnumerable<BattleOption> collection)
        {
            emperiumBuffView.SetData(collection);
        }

        public void SetActiveBtnChangeStartTime(bool isGuildMaster)
        {
            emperiumStartTimeView.SetActiveBtnChangeStartTime(isGuildMaster);
        }

        public void SetChangeTimeCoin(int count)
        {
            emperiumStartTimeView.SetChangeItemCoin(count);
        }

        public void SetStartTime(DateTime dateTime)
        {
            emperiumStartTimeView.SetStartTime(dateTime);
        }

        public void SetOwnedRefineEmperium(int count)
        {
            emperiumDonationView.SetOwnedCount(count);
        }

        public void SetDonationItem(string iconName, int count)
        {
            emperiumDonationView.SetItem(iconName, count);
        }

        public void SetRemainTime(DateTime startTime)
        {
            emperiumRemainTimeView.SetRemainTime(startTime);
        }
    }
}