using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Ragnarok
{
    public class PacketSenderTreeView : EmptyTreeView<PacketSenderTuple>
    {
        protected override MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[2]
            {
                CreateColumn("cmd", 32f),
                CreateColumn("text"),
            };

            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        protected override void DrawColumn(Rect rect, PacketSenderTuple t, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    DefaultGUI.Label(rect, t.cmd, args.selected, args.focused);
                    break;

                case 1:
                    DefaultGUI.Label(rect, t.text, args.selected, args.focused);
                    break;
            }
        }
    }
}