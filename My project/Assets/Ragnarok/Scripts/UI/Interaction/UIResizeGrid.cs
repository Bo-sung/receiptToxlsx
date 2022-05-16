namespace Ragnarok
{
    public class UIResizeGrid : UIGrid
    {
        public float maxCellWidth;
        public float maxCellHeight;

        protected override void Init()
        {
            base.Init();

            hideInactive = true;
        }

        public override void Reposition()
        {
            int childCount = GetChildList().Count;
            cellWidth = maxCellWidth / childCount;
            cellHeight = maxCellHeight / childCount;

            base.Reposition();
        }
    }
}