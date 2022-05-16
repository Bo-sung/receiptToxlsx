using UnityEngine;
using UnityEditor;

namespace Ragnarok
{
    public sealed class HorizontalSplitter : WindowSplitter
    {
        public HorizontalSplitter(ResizeEvent onResizing)
            : base(onResizing)
        {
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && splitterRect.Contains(Event.current.mousePosition))
                isSplitterResizing = true;

            if (isSplitterResizing)
                SplitPercent = Mathf.Clamp(Event.current.mousePosition.x / rect.width, 0.1f, 0.9f);

            splitterRect.x = rect.width * SplitPercent;
            splitterRect.height = rect.height;

            splitRect[0] = new Rect(0f, 0f, splitterRect.x, splitterRect.height);
            float rightX = splitterRect.x + splitterRect.width;
            splitRect[1] = new Rect(rightX, 0f, rect.width - rightX, splitterRect.height);

            DrawBar();

            if (Event.current.type == EventType.MouseUp)
                isSplitterResizing = false;

            if (isSplitterResizing)
                onResizing?.Invoke();
        }
    }
}