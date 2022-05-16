using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Ragnarok
{
    public class LoudSpeakerPopupPresenter : ViewPresenter
    {
        /******************** Models ********************/
        InventoryModel invenModel;

        /******************** Repositories ********************/

        /******************** Event ********************/

        public LoudSpeakerPopupPresenter()
        {
            invenModel = Entity.player.Inventory;
        }

        public override void AddEvent()
        {
        }

        public override void RemoveEvent()
        {
        }

        public ItemInfo GetItemInfo(long itemNo)
        {
            return invenModel.GetItemInfo(itemNo);
        }

        /// <summary>
        /// 확성기 사용
        /// </summary>
        public void RequestUseLoudSpeaker(ItemInfo itemInfo, string str)
        {
            str = FilterUtils.ReplaceChat(str); // 채팅필터링
            invenModel.RequestUseLoudSpeaker(itemInfo, str).WrapNetworkErrors();
        }
    }
}