using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class UIApplyBuffContent : UIInfo<UIApplyBuffContent.IBuffInfo>
    {
        public interface IBuffInfo : IInfo, IEnumerable<BattleOption>
        {
            bool IsEventBuff { get; }
            bool HasRemainTime { get; }
            string IconName { get; }
            string Name { get; }
            string RemainTimeText { get; }
        }

        [SerializeField] GameObject itemBuff, eventBuff;

        //[SerializeField] UISprite background;
        [SerializeField] UITextureHelper buffIcon;
        [SerializeField] UILabelHelper labelName, labelTime;
        [SerializeField] UIBattleOptionList effectList;

        [SerializeField] UILabelHelper eventLabelName, eventLabelTime;
        [SerializeField] UIBattleOptionList eventEffectList;

        private float timer = 0;

        protected override void Refresh()
        {
            if (IsInvalid())
            {
                SetActive(false);
                return;
            }

            SetActive(true);

            itemBuff.SetActive(!info.IsEventBuff);
            eventBuff.SetActive(info.IsEventBuff);

            if (info.IsEventBuff)
            {
                eventLabelName.Text = info.Name;
                eventEffectList.SetData(info);
            }
            else
            {
                buffIcon.SetItem(info.IconName);
                labelName.Text = info.Name;
                effectList.SetData(info);
            }

            eventLabelTime.SetActive(info.HasRemainTime);
            labelTime.SetActive(info.HasRemainTime);

            if (info.HasRemainTime)
            {
                SetRemainTime();
            }
        }

        void Update()
        {
            if (IsInvalid() || !info.HasRemainTime)
                return;

            timer -= Time.deltaTime;

            if (timer > 0)
                return;

            timer = 60;

            SetRemainTime();
        }

        private void SetRemainTime()
        {
            if (info.IsEventBuff)
            {
                eventLabelTime.Text = info.RemainTimeText;
            }
            else
            {
                labelTime.Text = info.RemainTimeText;
            }
        }
    }
}