using Sfs2X.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class HextechViewPresenter : ViewPresenter
    {
        private UIHextechView view;

        public HextechViewPresenter(UIHextechView view)
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
