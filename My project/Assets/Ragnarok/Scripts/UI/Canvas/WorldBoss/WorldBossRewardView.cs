using Ragnarok.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class WorldBossRewardView : UISubCanvasListener<WorldBossRewardView.IListener>
    {
        public interface IListener
        {

        }

        [SerializeField] UILabelHelper labelTitle;
        [SerializeField] UILabelHelper labelParticipation;
        [SerializeField] UIRewardHelper[] rewards;

        WorldBossDungeonElement element;

        protected override void OnInit()
        {

        }

        protected override void OnClose()
        {
            
        }

        protected override void OnShow()
        {
            
        }

        protected override void OnHide()
        {
            
        }                

        protected override void OnLocalize()
        {
            labelTitle.LocalKey = LocalizeKey._7038; // 보상 아이템
            labelParticipation.LocalKey = LocalizeKey._7047; // 참가 보상
        }  

        public void SetData(WorldBossDungeonElement element)
        {
            this.element = element;
            Refresh();
        }

        public void Refresh()
        {
            if (element == null)
                return;

            for (int i = 0; i < element.GetRewardInfos().Length; i++)
            {
                rewards[i].SetData(element.GetRewardInfos()[i].info.data);
            }           
        }
    } 
}
