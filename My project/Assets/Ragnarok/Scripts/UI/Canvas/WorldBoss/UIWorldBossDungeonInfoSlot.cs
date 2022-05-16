using MEC;
using Ragnarok.View;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIWorldBossDungeonInfoSlot : UIInfo<WorldBossDungeonElement>
    {
        [SerializeField] UIMonsterIcon monsterIcon;
        [SerializeField] UILabelHelper labelBossName;
        [SerializeField] UILabelHelper labelJoin;
        [SerializeField] UILabelHelper labelRemain;
        [SerializeField] GameObject lockBase;
        [SerializeField] UILabelHelper labelOpen;
        [SerializeField] UIButtonHelper btnSelect;
        [SerializeField] UISlider bossHp;
        [SerializeField] UILabel bossHpLabel;
        [SerializeField] GameObject selection;
        [SerializeField] UIButtonHelper btnMonster;
        [SerializeField] UIButtonHelper btnAlarm;
        [SerializeField] UILabel alarmLabel;

        Action<WorldBossDungeonElement> onSelectElement;
        Action<WorldBossDungeonElement> onAlarmElement;
        int selectId;

        private Color alarmLabelNormal;
        private Color bossNameNormal;
        private Color joinLabelNormal;
        private Color remainLabelNormal;

        protected override void Awake()
        {
            base.Awake();
            EventDelegate.Add(btnSelect.OnClick, OnClickedBtnSelect);
            EventDelegate.Add(btnAlarm.OnClick, OnClickedBtnAlarm);
            EventDelegate.Add(btnMonster.OnClick, OnClickedBtnMonster);

            alarmLabelNormal = alarmLabel.color;
            bossNameNormal = labelBossName.uiLabel.color;
            joinLabelNormal = labelJoin.uiLabel.color;
            remainLabelNormal = labelRemain.uiLabel.color;
        }

        protected override void OnDestroy()
        {
            EventDelegate.Remove(btnSelect.OnClick, OnClickedBtnSelect);
            EventDelegate.Remove(btnAlarm.OnClick, OnClickedBtnAlarm);
            EventDelegate.Remove(btnMonster.OnClick, OnClickedBtnMonster);

            base.OnDestroy();
        }

        void OnClickedBtnSelect()
        {
            if (info == null)
                return;

            if (!info.CanEnter(isShowNotice: true))
                return;

            onSelectElement?.Invoke(info);
        }

        void OnClickedBtnAlarm()
        {
            btnAlarm.SetNotice(!btnAlarm.IsActiveNotice());
            //info.SetAlarm(btnAlarm.IsActiveNotice());
            onAlarmElement?.Invoke(info);
        }

        void OnClickedBtnMonster()
        {
            onSelectElement?.Invoke(info);
            // TODO 몬스터 정보 팝업 추가 필요
        }

        protected override void OnLocalize()
        {
            btnAlarm.LocalKey = LocalizeKey._7046; // 알림

            base.OnLocalize();
        }

        public void Set(Action<WorldBossDungeonElement> onSelectElement, Action<WorldBossDungeonElement> onAlarmElement, int selectId)
        {
            this.onSelectElement = onSelectElement;
            this.onAlarmElement = onAlarmElement;
            this.selectId = selectId;
        }

        protected override void Refresh()
        {
            Timing.KillCoroutines(gameObject);

            if (IsInvalid())
                return;

            bool isSelected = info.IsSelect(selectId);
            selection.gameObject.SetActive(isSelected);

            MonsterInfo monsterInfo = info.GetMonsterInfo();
            monsterIcon.SetData(monsterInfo);
            labelBossName.Text = monsterInfo.Name;

            bool isLock = !info.CanEnter(isShowNotice: false);

            labelJoin.SetActive(false);
            labelRemain.SetActive(false);

            if (isLock)
            {               
                bossHp.value = 1f;
                bossHpLabel.text = "100%";
                lockBase.SetActive(true);
                btnAlarm.SetNotice(false);

                if (info.ConditionValue > info.GetMaxLevel())
                {
                    labelOpen.LocalKey = LocalizeKey._3500; // 업데이트 예정
                }
                else
                {
                    labelOpen.Text = info.GetOpenConditionalSimpleText();
                }
            }
            else
            {
                if (info.IsOpen())
                {
                    labelJoin.SetActive(true);
                    labelJoin.Text = LocalizeKey._7034.ToText() // 진행 인원 : {COUNT}명
                        .Replace(ReplaceKey.COUNT, info.JoinCharCount);
                    labelRemain.SetActive(false);
                    bossHp.value = info.GetBossHpProgress();
                    bossHpLabel.text = $"{Mathf.CeilToInt(info.GetBossHpProgress() * 100)}%";
                }
                else
                {
                    bossHp.value = 1f;
                    bossHpLabel.text = "100%";                   
                    Timing.RunCoroutineSingleton(YieldUpdateTime().CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
                }

                btnAlarm.SetNotice(info.IsAlarmWorldBoss());
                lockBase.SetActive(false);
                labelOpen.Text = string.Empty;
            }
        }

        private IEnumerator<float> YieldUpdateTime()
        {
            while (!info.IsOpen())
            {
                labelJoin.SetActive(false);
                labelRemain.SetActive(true);
                labelRemain.Text = LocalizeKey._7035.ToText() // 부활 : {TIME}
                    .Replace(ReplaceKey.TIME, info.RemainTimeText);
                yield return Timing.WaitForSeconds(1f);
            }
            Refresh();
        }
    }
}
