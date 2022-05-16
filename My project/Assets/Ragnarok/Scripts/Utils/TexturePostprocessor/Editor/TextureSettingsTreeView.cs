using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Ragnarok
{
    public sealed class TextureSettingsTreeView : EmptyTreeView<TextureSettingsCollection.Settings>
    {
        protected override MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[1]
            {
                CreateColumn("path"),
            };

            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        protected override void DrawColumn(Rect rect, TextureSettingsCollection.Settings t, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    DefaultGUI.Label(rect, t.preset.name, args.selected, args.focused);
                    break;
            }
        }
    }
}