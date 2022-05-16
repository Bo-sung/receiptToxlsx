using UnityEngine;
using System.Collections;

namespace Ragnarok
{
    public class UIGiveElement : MonoBehaviour
    {
        [SerializeField] UILabelHelper titleLabel;
        [SerializeField] UIRewardHelper target;
        [SerializeField] UIRewardHelper material;
        [SerializeField] UILabelHelper descLabel;
        [SerializeField] UIButtonHelper giveButton;

        private GiveElementPresenter presenter;

        public void OnInit()
        {

        }

        public void OnClose()
        {

        }

        public void OnShow()
        {

        }

        public void OnHide()
        {

        }
    }
}