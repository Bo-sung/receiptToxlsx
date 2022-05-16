using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ragnarok
{
    public class SelectPopupPresenter : ViewPresenter
    {
        public interface IView
        {

        }

        private readonly IView view;

        public SelectPopupPresenter(IView view)
        {
            this.view = view;
        }

        public override void AddEvent()
        {

        }

        public override void RemoveEvent()
        {

        }
    }
}
