using UnityEngine;

namespace Ragnarok
{
    [RequireComponent(typeof(UIWidget))]
    public class UIWidgetResizeGrid : UIGrid
    {
        UIWidget widget;

        [SerializeField] int columnCount;
        [SerializeField] int rowCount;

        protected override void Init()
        {
            base.Init();

            widget = GetComponent<UIWidget>();
            widget.onChange += OnDimensionsChanged;

            SetAutoCellSize();
        }

        protected virtual void OnDestroy()
        {
            if (widget)
            {
                widget.onChange -= OnDimensionsChanged;
            }
        }

        private void SetAutoCellSize()
        {
            columnCount = Mathf.Max(1, columnCount);
            rowCount = Mathf.Max(1, rowCount);

            cellWidth = widget.width / columnCount;
            cellHeight = widget.height / rowCount;
        }

        void OnDimensionsChanged()
        {
            SetAutoCellSize();
            Reposition();
        }

        void OnValidate()
        {
            if (!Application.isPlaying && NGUITools.GetActive(this))
            {
                if (mInitDone)
                {
                    SetAutoCellSize();
                }
                else
                {
                    Init();
                }

                Reposition();
            }
        }
    }
}