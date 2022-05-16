using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class ShareFilterPresenter : ViewPresenter
    {
        /******************** Models ********************/
        SharingModel sharingModel;
        CharacterModel characterModel;
        QuestModel questModel;

        /******************** Repositories ********************/
        JobDataManager jobDataRepo;

        /******************** Event ********************/
        public event System.Action OnHideShareFilterUI
        {
            add { sharingModel.OnHideShareFilterUI += value; }
            remove { sharingModel.OnHideShareFilterUI -= value; }
        }

        public ShareFilterPresenter()
        {
            sharingModel = Entity.player.Sharing;
            characterModel = Entity.player.Character;
            questModel = Entity.player.Quest;
            jobDataRepo = JobDataManager.Instance;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {            
        }

        public void UpdateShareFilter(JobFilter[] curShareJobFilterAry)
        {
            sharingModel.RequestShareJobFilter(curShareJobFilterAry).WrapNetworkErrors();
        }

        public int GetOpenedShareSlotCount()
        {
            return sharingModel.GetShareSlotCount(characterModel.JobGrade(), questModel.IsOpenContent(ContentType.ShareHope, false));
        }

        public JobFilter[] GetShareJobFilterAry()
        {
            return sharingModel.GetShareJobFilterAry();
        }

        public Job GetJob(JobFilter jobFilter)
        {
            switch (jobFilter)
            {
                case JobFilter.Knight:
                    return Job.Knight;
                case JobFilter.Crusader:
                    return Job.Crusader;
                case JobFilter.Wizard:
                    return Job.Wizard;
                case JobFilter.Sage:
                    return Job.Sage;
                case JobFilter.Assassin:
                    return Job.Assassin;
                case JobFilter.Rogue:
                    return Job.Rogue;
                case JobFilter.Hunter:
                    return Job.Hunter;
                case JobFilter.Dancer:
                    return Job.Dancer;

                //case JobFilter.None:
                //break;
                default:
                    return default;
            }
        }
    }
}