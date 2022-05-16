using UnityEngine;

namespace Ragnarok
{
    public class UIDungeonButton : UIButtonHelper
    {
        [SerializeField] UILabelHelper labelDescription;
        [SerializeField] UILabelValue freeCount;

        public int DescriptionLocalKey
        {
            set { labelDescription.LocalKey = value; }
        }

        public int FreeCountLocalKey
        {
            set { freeCount.TitleKey = value; }
        }

        public void SetCount(int free, int max)
        {
            freeCount.Value = LocalizeKey._7022.ToText() // {COUNT}/{MAX}
                .Replace(ReplaceKey.COUNT, free)
                .Replace(ReplaceKey.MAX, max);

            SetNotice(free > 0);
        }
    }
}