using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Ragnarok
{
    public sealed class EntityLogTreeView : EmptyTreeView<EntityLog>
    {
        protected override MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[2]
            {
                CreateColumn("no", 40f),
                CreateColumn("header"),
            };

            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        protected override void DrawColumn(Rect rect, EntityLog t, int column, ref RowGUIArgs args)
        {
            switch (column)
            {
                case 0:
                    DefaultGUI.Label(rect, t.tick.ToString(), args.selected, args.focused);
                    break;

                case 1:
                    DefaultGUI.Label(rect, t.header, args.selected, args.focused);
                    break;
            }
        }
    }
}