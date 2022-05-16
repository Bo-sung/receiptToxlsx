using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    /// <summary>
    /// <see cref="HorizontalSplitter"/>
    /// </summary>
    public abstract class WindowSplitter
    {
        private const float SPLITTER_WIDTH = 3f;

        public delegate void ResizeEvent();

        protected readonly Rect[] splitRect;

        private float splitPercent;

        /// <summary>
        /// 분할 영역 퍼센트
        /// </summary>
        public float SplitPercent
        {
            get
            {
                if (splitPercent == 0f && onResizing.Target != null)
                    splitPercent = EditorPrefs.GetFloat(onResizing.Target.ToString(), 0.5f);

                return splitPercent;
            }
            set
            {
                if (splitPercent == value)
                    return;

                splitPercent = value;

                if (onResizing != null)
                    EditorPrefs.SetFloat(onResizing.Target.ToString(), splitPercent);
            }
        }

        protected Rect splitterRect; // 분할 영역 (왼쪽)
        protected bool isSplitterResizing; // 사이즈 조정 중

        /// <summary>
        /// 사이즈 조정 이벤트
        /// <see cref="EditorWindow.Repaint"/>
        /// </summary>
        public ResizeEvent onResizing;

        /// <summary>
        /// 구분선
        /// </summary>
        public bool isShowBar;

        public Rect this[int index]
        {
            get { return splitRect[index]; }
        }

        protected WindowSplitter(ResizeEvent onResizing)
        {
            this.onResizing = onResizing;

            splitRect = new Rect[2];
            splitterRect.width = SPLITTER_WIDTH;
        }

        public abstract void OnGUI(Rect rect);

        /// <summary>
        /// 구분선 그리기
        /// </summary>
        protected void DrawBar()
        {
            if (isShowBar)
                GUI.Box(splitterRect, string.Empty);
        }
    }
}