using UnityEditor;
using UnityEngine;

namespace Ragnarok
{
    public class MultiViewer
    {
        public interface IContent
        {
            void SetTitle(string title);
            void SetText(string text);
            void Clear();
        }

        public enum DivisionType
        {
            Horizontal,
            Vertical,
        }

        private readonly DivisionType type;
        private readonly Content[] contents;

        public IContent this[int index]
        {
            get { return contents[index]; }
        }

        Vector2 scrollPosition;

        public MultiViewer(DivisionType type, int multiCount)
        {
            this.type = type;
            contents = new Content[multiCount];

            // Initialize
            for (int i = 0; i < contents.Length; i++)
            {
                contents[i] = new Content();
            }
        }

        public void OnGUI(Rect rect)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                if (i == 0)
                {
                    if (type == DivisionType.Horizontal)
                    {
                        rect.width /= contents.Length;
                    }
                    else
                    {
                        rect.height /= contents.Length;
                    }
                }
                else
                {
                    if (type == DivisionType.Horizontal)
                    {
                        rect.x += rect.width;
                    }
                    else
                    {
                        rect.y += rect.height;
                    }
                }

                contents[i].Draw(rect, ref scrollPosition);
            }
        }

        public void Clear()
        {
            foreach (var item in contents)
            {
                item.Clear();
            }
        }

        private class Content : IContent
        {
            private string title;
            private string text;

            void IContent.SetTitle(string title)
            {
                this.title = title;
            }

            void IContent.SetText(string text)
            {
                this.text = text;
            }

            public void Clear()
            {
                text = null;
            }

            public void Draw(Rect rect, ref Vector2 scrollPosition)
            {
                using (new GUILayout.AreaScope(rect, string.Empty, EditorStyles.helpBox))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        // 타이틀
                        if (!string.IsNullOrEmpty(title))
                            GUILayout.Label(title);

                        GUILayout.FlexibleSpace();

                        // 버튼
                        if (GUILayout.Button("클립보드로 복사", EditorStyles.miniButton))
                            CopyToClipboard();
                    }

                    using (var gui = new GUILayout.ScrollViewScope(scrollPosition))
                    {
                        scrollPosition = gui.scrollPosition;
                        GUILayout.Label(text);
                    }
                }
            }

            /// <summary>
            /// 클립보드에 복사
            /// </summary>
            private void CopyToClipboard()
            {
                GUIUtility.systemCopyBuffer = text; // Copy To Clipboard
            }
        }
    }
}