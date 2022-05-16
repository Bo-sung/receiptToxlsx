using MEC;
using Ragnarok.View.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public sealed class UICentralLabItemSkill : UICanvas
    {
        protected override UIType uiType => UIType.Fixed | UIType.Hide;

        [SerializeField] UIButton blind;
        [SerializeField] UILabelHelper labelMainTitle;
        [SerializeField] UIItemSkillInfo itemSkill;
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UIButtonHelper btnConfirm;

        protected override void OnInit()
        {
            EventDelegate.Add(blind.onClick, CloseUI);
            EventDelegate.Add(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnClose()
        {
            EventDelegate.Remove(blind.onClick, CloseUI);
            EventDelegate.Remove(btnConfirm.OnClick, CloseUI);
        }

        protected override void OnShow(IUIData data = null)
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnLocalize()
        {
            labelMainTitle.LocalKey = LocalizeKey._48322; // 포링의 축복
            btnConfirm.LocalKey = LocalizeKey._1; // 확인
        }

        public void Show(UIItemSkillInfo.IInput[] infos, UIItemSkillInfo.IInput result)
        {
            Show();

            itemSkill.SetData(result);
            labelDescription.Text = result.SkillDescription;

            //Timing.RunCoroutineSingleton(YieldPlayAnimation(infos, result).CancelWith(gameObject), gameObject, SingletonBehavior.Overwrite);
        }

        private void CloseUI()
        {
            Hide();
        }
    }
}