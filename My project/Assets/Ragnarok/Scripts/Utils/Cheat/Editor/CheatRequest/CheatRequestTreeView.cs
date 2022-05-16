using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Ragnarok
{
    public sealed class CheatRequestTreeView : EmptyTreeView<CheatRequestDrawer>
    {
        protected override MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[2]
            {
                CreateColumn("No"),
                CreateColumn("Title"),
            };

            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        protected override void DrawColumn(Rect rect, CheatRequestDrawer t, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    DefaultGUI.Label(rect, t.OrderNum.ToString(), args.selected, args.focused);
                    break;

                case 1:
                    DefaultGUI.Label(rect, t.Title, args.selected, args.focused);
                    break;
            }
        }
    }
}